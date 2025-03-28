# DremIO Client Library

## Introduction
DremIO Client Library is a C# library that helps you execute SQL queries on DremIO via API. The library provides classes that support connection management, query execution, job status tracking, and result retrieval efficiently.

## Installation

1. Clone the repository to your machine:
   ```sh
   git clone <repository_url>
   cd <repository_folder>
   ```
2. Add the library to your project:
   ```sh
   dotnet add package Dino.DremIO
   ```

### NuGet Repository
You can download the library from NuGet using the following command:
```sh
 dotnet add package Dino.DremIO --version <latest_version>
```
Or visit: [NuGet Gallery](https://www.nuget.org/packages/Dino.DremIO/)

## Configuring DremIOOption
The `DremIOOption` class contains connection configuration details for DremIO.

### Structure
```csharp
var options = new DremIOOption
{
    UserName = "your_username",
    Password = "your_password",
    EndpointUrl = "https://your-dremio-endpoint.com"
};
```

- `UserName`: Username to log into DremIO.
- `Password`: Password.
- `TokenStore`: Path for storing the token (default is `AppData\Local\DremIOtokenStore`).
- `EndpointUrl`: DremIO API endpoint URL.

## Using DremIOService
The `DremIOService` class is the main class for sending queries to DremIO.

### Initializing the Service
```csharp
var client = new DremIOClient(options);
var service = new DremIOService(client);
```

### Sending an SQL Query
```csharp
var payload = new PayloadSqlRequest { Sql = "SELECT * FROM my_table" };
var result = await service.QueryAsync<Dictionary<string, object>>(payload);
```

## Managing Context with DremIOContext
The `DremIOContext` class enables executing SQL queries within a specific context.

### Creating a Context
```csharp
var context = service.CreateContext("my_space", "my_folder");
```

### Sending a Query and Getting Job ID
```csharp
var jobId = await context.QueryAsync("SELECT * FROM my_table");
```

### Waiting for Job Completion and Retrieving Results
```csharp
await foreach (var row in context.QueryWaitAsync<Dictionary<string, object>>("SELECT * FROM my_table"))
{
    Console.WriteLine(row);
}
```

## Managing Jobs with DremIOJob
The `DremIOJob` class allows monitoring job status and retrieving results.

### Creating a Job
```csharp
var job = service.CreateJob();
```

### Checking Job Status
```csharp
var jobResponse = await job.GetAsync(jobId);
Console.WriteLine($"Job State: {jobResponse?.JobState}");
```

### Waiting for Job Completion
```csharp
var completedJob = await job.WaitAsync(jobId);
```

### Retrieving All Job Results
```csharp
await foreach (var row in job.ResultAllAsync<Dictionary<string, object>>(jobId))
{
    Console.WriteLine(row);
}
```

## Error Handling and Debugging
- If the query fails, an exception will be thrown.
- If the job does not complete within the timeout period, a `TimeoutException` will occur.
- Check `JobState` to determine whether the job was successful.

## License
MIT License. Please refer to the LICENSE file for more details.

