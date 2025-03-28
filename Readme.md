# DremIO Client Library

## Giới thiệu
DremIO Client Library là một thư viện C# giúp bạn thực hiện các truy vấn SQL trên DremIO thông qua API. Thư viện cung cấp các lớp hỗ trợ việc quản lý kết nối, gửi truy vấn, theo dõi trạng thái job và lấy kết quả một cách tiện lợi.

## Cài đặt

1. Clone repository về máy của bạn:
   ```sh
   git clone <repository_url>
   cd <repository_folder>
   ```
2. Thêm thư viện vào project của bạn:
   ```sh
   dotnet add package Dino.DremIO
   ```

### Kho NuGet
Bạn có thể tải thư viện từ NuGet bằng cách sử dụng lệnh sau:
```sh
 dotnet add package Dino.DremIO --version <latest_version>
```
Hoặc truy cập: [NuGet Gallery](https://www.nuget.org/packages/Dino.DremIO/)

## Cấu hình DremIOOption
Lớp `DremIOOption` chứa các thông tin cấu hình kết nối tới DremIO.

### Cấu trúc
```csharp
var options = new DremIOOption
{
    UserName = "your_username",
    Password = "your_password",
    EndpointUrl = "https://your-dremio-endpoint.com"
};
```

- `UserName`: Tên người dùng đăng nhập vào DremIO.
- `Password`: Mật khẩu.
- `TokenStore`: Đường dẫn lưu trữ token (mặc định là `AppData\Local\DremIOtokenStore`).
- `EndpointUrl`: Địa chỉ API của DremIO.

## Sử dụng DremIOService
Lớp `DremIOService` là lớp chính giúp gửi truy vấn đến DremIO.

### Khởi tạo Service
```csharp
var client = new DremIOClient(options);
var service = new DremIOService(client);
```

### Gửi truy vấn SQL
```csharp
var payload = new PayloadSqlRequest { Sql = "SELECT * FROM my_table" };
var result = await service.QueryAsync<Dictionary<string, object>>(payload);
```

## Quản lý Context với DremIOContext
Lớp `DremIOContext` giúp thực hiện truy vấn SQL với ngữ cảnh (context) cụ thể.

### Tạo Context
```csharp
var context = service.CreateContext("my_space", "my_folder");
```

### Gửi truy vấn và lấy ID của job
```csharp
var jobId = await context.QueryAsync("SELECT * FROM my_table");
```

### Chờ Job hoàn thành và lấy kết quả
```csharp
await foreach (var row in context.QueryWaitAsync<Dictionary<string, object>>("SELECT * FROM my_table"))
{
    Console.WriteLine(row);
}
```

## Quản lý Job với DremIOJob
Lớp `DremIOJob` giúp theo dõi trạng thái job và lấy kết quả.

### Tạo Job
```csharp
var job = service.CreateJob();
```

### Kiểm tra trạng thái Job
```csharp
var jobResponse = await job.GetAsync(jobId);
Console.WriteLine($"Job State: {jobResponse?.JobState}");
```

### Chờ Job hoàn thành
```csharp
var completedJob = await job.WaitAsync(jobId);
```

### Lấy tất cả kết quả của Job
```csharp
await foreach (var row in job.ResultAllAsync<Dictionary<string, object>>(jobId))
{
    Console.WriteLine(row);
}
```

## Xử lý lỗi và Debug
- Nếu truy vấn không thành công, một ngoại lệ sẽ được ném.
- Nếu job không hoàn thành trong thời gian chờ, `TimeoutException` sẽ xảy ra.
- Kiểm tra trạng thái `JobState` để biết job có thành công hay không.

## License
MIT License. Vui lòng tham khảo tệp LICENSE để biết thêm chi tiết.

