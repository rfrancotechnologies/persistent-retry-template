# PersistentRetryTemplate

.NET library for retrying operations with persistence.

[![Build History](https://buildstats.info/travisci/chart/mediatechsolutions/persistent-retry-template?branch=master)](https://travis-ci.org/mediatechsolutions/persistent-retry-template)
[![NuGet Version](https://buildstats.info/nuget/persistentretrytemplate?includePreReleases=true)](https://www.nuget.org/packages/PersistentRetryTemplate)
[![Build Status](https://travis-ci.org/mediatechsolutions/persistent-retry-template.svg?branch=master)](https://travis-ci.org/mediatechsolutions/persistent-retry-template)


## Why PersistentRetryTemplate

It is very common to face situations in which you need to retry a specific operation while it is not successful, according to some certain criteria. Typical criteria are retrying a maximum number of attempts, retrying until a timeout is exceeded or waiting a fixed or exponential amount of time before each retry. In order to address this need, the Spring Team released the amazing [Spring Retry project](https://github.com/spring-projects/spring-retry).

A more specific situation is the need to be able to retry some operation with persistence. That is, we need the operation to be retried even if our software or the device in which it runs stops for some time and starts again.

Finally, a probably even more specific situation is the need to handle batch operations that involve separate pieces of information usually behaving like events (they contain a piece of data and are received asynchronously). Let's imagine an application that has to react to incoming events, and it needs to execute some operation relying on many of those events. The application would have to record those events and analyze/process them to come up with a resulting action. This operation becomes more complex when we need that event processing to be persistent (we need to be able to process the incoming events even if they were received before our application restarted). In this cases, if the application decides the event series ends up in a failure, it seems within reason that the application will want to run a recovery routine with persistent retries.

*PersistentRetryTemplate* has been created with two main features in mind, to help solve these problems:

* A Spring-Retry-like RetryTemplate, for retrying operations with persistence capabilities.
* A batch operation template, for being able to track and recover batch operations which involve separate asynchronous pieces of information.

*PersistentRetryTemplate* uses [LiteDB](https://github.com/mbdavid/LiteDB) for persistence, making it specially appropriate for embedded devices and standalone applications. It is not appropriate for server applications involving high concurrency or multiple instances of an application that must share the persistence mechanism.

## How to use it

### RetryTemplate: Retrying an Operation With Persistence

#### Saving an Operation for Retrying

*PersistentRetryTemplate* provides a `RetryTemplate` class, that allows to retry operations and provides persistence features. In order to create an instance of `RetryTemplate`, you need to provide an instance of `LiteDatabase`, which is the handle of the [LiteDB](https://github.com/mbdavid/LiteDB) which will hold the information about your pending retries.

In order to save an operation for retrying, `RetryTemplate` provides the `PendingRetry<T> SaveForRetry<T>(string operationId, T argument)` function. You can use the same instance of `RetryTemplate` to manage several operations, with different argument types. In order to identify the specific operation you need to retry, you have to specify a string identifier for it. This identifier will allow you to know what callback you have to execute for that pending retry. 

In addition, when saving an operation to be retried you can also save an argument that has to be passed to the operation when being retried:

```csharp
using (var database = new LiteDatabase(fileName))
{
	RetryTemplate retryTemplate = new RetryTemplate(database );
	var pendingRetry = retryTemplate.SaveForRetry<string>("SendNotification", "Your login was successful!");
}
```

#### Retrying an Operation

The `RetryTemplate` class provides the `DoExecute` function for retrying operations. 
```csharp
R DoExecute<T, R>(PendingRetry<T> pendingRetry, Func<T, R> retryCallback, Func<T, R> recoveryCallback, CancellationToken cancellationToken)
```
`DoExecute` will execute the provided retry callback. In case the callback is successful, the returned value of type `R` will be returned. In case of failure (the retry callback throws an exception), the operation will be retried while the specified retry policy allows it. It is possible to wait for some time before each retry is performed, according to the specified back-off policy. Read the next sections in order to learn more about retry and back-off policies.

When the retry policy does not allow more retries for some exception, a RetryExhaustedException will be thrown (containing a reference to the exception that caused the failure in the retry callback).

The `DoExecute` function accepts the following arguments:

* A pending retry, which is the handle to an operation pending for retries as returned by `SaveForRetry`.
* A retry callback, which is the function that will be invoked in each retry. 
* An optional recovery callback, which is a function that will be invoked when the retries over the operation have been exhausted according the specified RetryPolicy . If null, the recovery callback will be ignored.
* A cancellation token, which allows to manually indicate that the retries should stop.

Example:
```csharp
var pendingRetry = retryTemplate.SaveForRetry<string>("SendNotification", "Your login was successful!");
CancellationToken cancellationToken = new CancellationToken(false);

retryTemplate.DoExecute(pendingRetry, (arg) => SendNotification(arg), null, cancellationToken);
```

#### Retry Policies: Specifying Whether an Operation Should Be Retried or Not

Retry policies specify whether an operation should be retried or not. The `RetryTemplate` accepts a retry policy in its `RetryPolicy` property.

Some retry policies are provided out-of-the-box:

`AlwaysRetryPolicy` allows retrying an operation until it is successful. A `NeverRetryPolicy` with the opposite behavior is also available (mainly for testing purposes).

`SimpleRetryPolicy` allows to retry an operation while a maximum number of attempts is not exceeded. By default the maximum number of attempts is 3, though any maximum attemps can be specified. For instance, for a maximum of 5 total attempts:
```csharp
RetryTemplate retryTemplate = new RetryTemplate(database);
retryTemplate.RetryPolicy = new SimpleRetryPolicy(5);
```

`TimeoutRetryPolicy` allows to retry an operation until a specified timeout is exceeded:
```csharp
RetryTemplate retryTemplate = new RetryTemplate(database);
retryTemplate.RetryPolicy = new TimeoutRetryPolicy(TimeSpan.FromSeconds(30));
```

In both, the simple and timeout retry policies, it is also possible to specify some Exception types and whether they should be retried or not: 
```csharp
RetryTemplate retryTemplate = new RetryTemplate(database);

var specifiedExceptions = new Dictionary<Type, bool>();
specifiedExceptions.Add(typeof(Exception1), true);
specifiedExceptions.Add(typeof(Exception2), false);

retryTemplate.RetryPolicy = new SimpleRetryPolicy(5, specifiedExceptions);
```
In the previous example, occurrences of `Exception1` or any of its children will be retried, while occurrences of `Exception2` or any of its children will not be retried. By default, if no specific exceptions are provided, every .NET Exception will be retriable.

Finally, in addition to specific exceptions, it is possible to specify a default retriability for all those exceptions that are not included in the specific exceptions (by default, false):
```csharp
RetryTemplate retryTemplate = new RetryTemplate(database);

var specifiedExceptions = new Dictionary<Type, bool>();
specifiedExceptions.Add(typeof(Exception2), false);

retryTemplate.RetryPolicy = new SimpleRetryPolicy(5, specifiedExceptions, true);
```
In the previous example, occurrences of exceptions of type `Exception2` and any of its children will not be retried, while any other exception will.

In addition to the retry policies provided out-of-the-box, you can also implement your own by implementing the `IRetryPolicy` interface, or by extending the `AbstractSubclassRetryPolicy`.

If no retry policy is explicitly specified for a `RetryTemplate`, a `SimpleRetryPolicy` with 3 maximum attempts will be used by a default.

#### Back-off Policies: Specifying a Wait Time Before Each Retry

Back-off policies define an optional wait time before each retry. Retry templates accept a back-off policy via the `BackOffPolicy` property. 

*PersistentRetryTemplate* provides three out-of-the-box back-off policies:

* `NoBackOffPolicy`, which provides a 0 wait between retries.
* `FixedBackOffPolicy`, that allows to define a fixed wait time between each retry.
```csharp
RetryTemplate retryTemplate = new RetryTemplate(database);
retryTemplate.BackOffPolicy = new FixedBackOffPolicy(TimeSpan.FromMilliseconds(100));
```
* `ExponentialBackOffPolicy`, that implements an exponencially incrementing wait. 
```csharp
RetryTemplate retryTemplate = new RetryTemplate(database);
ExponentialBackOffPolicy backOffPolicy = new ExponentialBackOffPolicy();
backOffPolicy.InitialInterval = TimeSpan.FromMilliseconds(100);
backOffPolicy.MaxInterval = TimeSpan.FromMilliseconds(300);
backOffPolicy.Multiplier = 2;
retryTemplate.BackOffPolicy = backOffPolicy;
```
`ExponentialBackOffPolicy` accepts three configuration properties:

* `InitialInterval`: the amount of time that will be waited in the first retry.
* `Multiplier`: the amount that multiplies the current interval in each retry. In the first retry the wait interval will be `InitialInterval` and in subsequent retries the wait interval will be the previous interval multiplied by `Multiplier`.
* `MaxInterval`: the maximum amount of time that will be waited in any retry. 


#### Listing Pending Retries

`RetryTemplate` allows retrieving all the pending operations belonging to a specified operation identifier via the `GetPendingRetries` function:
```csharp
IEnumerable<PendingRetry<T>> GetPendingRetries<T>(string operationId)
```

It is also possible to steadily take pending retries in a blocking fashion, via the `TakePendingRetry` function:
```csharp
PendingRetry<T> TakePendingRetry<T>(string operationId)
```
This operation will block the caller until an operation for that `operationId` is available. If a pending retry operation is available at the moment of invoking `TakePendingRetry` the function will return immediately.

Once a retry finishes, either because the callback was successfully executed or because the retries were exhausted, it will not be retrieved anymore by `GetPendingRetries` or `TakePendingRetry`.

### Managing Batch Operations

*PersistentRetryTemplate* provides a `BatchOperationTemplate` class, that provides features for handling batch operations that involve several event-like pieces of information. Similar to retry templates, the `BatchOperationTemplate` requires an instance of `LiteDatabase` in the constructor (a handle to the [LiteDB](https://github.com/mbdavid/LiteDB) file database that will hold the information about the batch operations).

You can invoke the `StartBatchOperation` function in order to start one of such batch operations:
```csharp
using (var database = new LiteDatabase(fileName))
{
	BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(new LiteDatabase(Path.GetTempFileName()));

	var batchOperation = batchOperationTemplate.StartBatchOperation<string>("operation.identifier");
}       
```

`StartBatchOperation` accepts a string identifier, that allows you to identify the batch operation, and returns a handle of batch operation that can subsequently used to store data or complete the batch operation.

As your application receives or generates events, they can be persistently stored in the batch operation via the `AddBatchOperationData` function:
```csharp
using (var database = new LiteDatabase(fileName))
{
	BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(new LiteDatabase(Path.GetTempFileName()));

	var batchOperation = batchOperationTemplate.StartBatchOperation<string>("SendCompositeNotification");
    
    batchOperationTemplate.AddBatchOperationData(batchOperation, "Piece of notification data");
}       
```
The added pieces of data must be of type `T`, being `T` the generic type specified when creating the batch operation with `StartBatchOperation`.

It is possible to retrieve all the ongoing batch operations with a specific operation identifier by means of the `GetPendingBatchOperations` operation:
```csharp
IEnumerable<BatchOperation<string>> ongoingBatchOperations = batchOperationTemplate.GetPendingBatchOperations("SendCompositeNotification");
```	

When a batch operation is finished, according to your application logic, it is possible to notify *PesistentRetryTemplate* via the `Complete` function:
```csharp
batchOperationTemplate.Complete(batchOperation);
```

It is also possible to execute some function, with persistent retries, when the batch operation is finished via the `CompleteWithFinishingCallback` function:
```csharp
using (var database = new LiteDatabase(fileName))
{
	BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(database);
	RetryTemplate retryTemplate = new RetryTemplate(database);

    var batchOperation = batchOperationTemplate.StartBatchOperation<string>(testOperationId);
    
    var pendingRetry = batchOperationTemplate.CompleteWithFinishingCallback(retryTemplate, batchOperation);
}
```
`CompleteWithFinishingCallback` will mark the batch operation as completed and will create a new pending retry for the finishing callback (using the same operation identifier that was used when creating the batch operation). That pending retry can be handled using a `RetryTemplate`.

Completed batch operations, either via `Complete` or `CompleteWithFinishingCallback`, will not be returned by the `GetPendingBatchOperations` function.
