# Khung backend

ASP.NET Core 10, 4 project, phụ thuộc một chiều:

```
congThongTinf.API
        │
        ▼
congThongTinf.Service
        │
        ▼
congThongTinf.Infrastructure
        │
        ▼
congThongTinf.Domain
```

`Domain` không tham chiếu `Infrastructure` hay `API`. Controller không gọi database. Mọi project: `net10.0`, `Nullable` bật, `ImplicitUsings` bật. Solution: `congThongTinf.slnx`.

Luồng một request:

```
Controller
  → I{Feature}Service
    → IUnitOfWork / IGenericRepository<T>
      → congThongTinfDbContext
        → PostgreSQL
```

Lỗi nghiệp vụ ném từ Service (`ErrorHelper`). Controller bắt `Exception`, đọc status code, trả `ApiResult<T>`.

File dưới đây là khung. Mỗi tính năng chỉ thêm file theo slot ở cuối mỗi layer (`{Feature}`), không nằm trong danh sách khung.

---

## 1. `congThongTinf.API`

HTTP, auth, DI, swagger, rate limit. Không chứa rule nghiệp vụ.

### Thư viện

| Package | Version |
| --- | --- |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.12 |
| `Microsoft.AspNetCore.OpenApi` | 10.0.12 |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 |
| `Swashbuckle.AspNetCore` | 10.2.3 |
| `Swashbuckle.AspNetCore.Swagger` | 10.2.3 |
| `Swashbuckle.AspNetCore.SwaggerUI` | 10.2.3 |
| `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` | 1.24.2-preview.1 |

Project reference: `congThongTinf.Service`.

### Cây khung

```
congThongTinf.API/
  Program.cs
  appsettings.json
  Dockerfile
  Properties/launchSettings.json
  Architecture/
    IOCContainer.cs
    MigrationExtensions.cs
  Middleware/
    RateLimitingExtensions.cs
    RateLimitingOptions.cs
  Controllers/
    {Feature}Controller.cs
```

### `Program.cs`

Thứ tự:

1. `AddControllers`
2. CORS policy `AllowFrontend` (`AllowAnyOrigin` / `AllowAnyHeader` / `AllowAnyMethod`)
3. `AddEndpointsApiExplorer` + `AddSwaggerGen` (security scheme `Bearer`, HTTP bearer JWT)
4. `SetupIocContainer()`
5. `AddApiRateLimiting(configuration)`
6. Pipeline: `UseCors` → `UseRouting` → `UseRateLimiter` → Swagger (Development và Production, UI ở `/`) → `ApplyMigrations` → `UseAuthentication` → `UseAuthorization` → `MapControllers`

### `Architecture/IOCContainer.cs`

`IocContainer.SetupIocContainer` là extension trên `IServiceCollection`. Các bước khung:

| Method | Việc |
| --- | --- |
| `SetupDbContext` | `AddDbContext<congThongTinfDbContext>` + `UseNpgsql`. Connection string `DefaultConnection`. `MigrationsAssembly` = assembly của DbContext. `CommandTimeout(300)`. `EnableRetryOnFailure(5, 10s)`. |
| `SetupBusinessServicesLayer` | Đăng ký scoped: `IUnitOfWork`, từng `I{Feature}Service`, `IBlobService`, `IClaimsService`, `AddHttpContextAccessor`. |
| `SetupJwt` | `AddAuthentication` / `AddJwtBearer`. Validate issuer, audience, lifetime. Key `JWT:SecretKey`, `JWT:Issuer`, `JWT:Audience`. `NameClaimType` = `ClaimTypes.NameIdentifier`, `RoleClaimType` = `ClaimTypes.Role`. |
| `SetupObjectStorage` | Bind `ObjectStorage`, tạo `IAmazonS3` singleton (`ForcePathStyle`, `AuthenticationRegion`). Tạo bucket nếu chưa có. |

Lifetime: service nghiệp vụ và `IUnitOfWork` là **scoped**. Client S3 là **singleton**.

### `Architecture/MigrationExtensions.cs`

`ApplyMigrations` tạo scope, lấy `congThongTinfDbContext`, gọi `Database.Migrate()`. API tự apply migration lúc startup. Không chạy `dotnet ef` trong luồng này.

### `Middleware/`

- `RateLimitingOptions`: section `RateLimiting`. `PermitLimit`, `WindowSeconds`, `LoginPermitLimit`, `LoginWindowSeconds`. Hằng `LoginPolicy = "login"`.
- `RateLimitingExtensions.AddApiRateLimiting`: sliding window theo IP cho path `/api`. Policy `login` riêng. Reject trả `429` với body `ApiResult`.

### Controller — slot `{Feature}Controller`

```csharp
[Route("api/{feature}")]
[ApiController]
public class {Feature}Controller : ControllerBase
{
    // ctor: I{Feature}Service, và khi cần IClaimsService, IConfiguration

    [HttpPost]
    [Authorize(Roles = "...")]          // bỏ nếu endpoint public
    [ProducesResponseType(typeof(ApiResult<TDto>), 200)]
    public async Task<IActionResult> Action([FromBody] TRequestDto dto)
    {
        try
        {
            var result = await _service.Action(dto);
            return Ok(ApiResult<TDto>.Success(result, "200", "..."));
        }
        catch (Exception ex)
        {
            var statusCode = ExceptionUtils.ExtractStatusCode(ex);
            var errorResponse = ExceptionUtils.CreateErrorResponse<TDto>(ex);
            return StatusCode(statusCode, errorResponse);
        }
    }
}
```

Controller chỉ: nhận HTTP, gọi một method service, bọc `ApiResult`, map exception sang status code. Không `new` repository, không `DbContext`.

### `appsettings.json` — các section khung

`Logging`, `ConnectionStrings:DefaultConnection`, `ConnectionStrings:Redis`, `AllowedHosts`, `RateLimiting`, `JWT`, `ObjectStorage` (`ServiceUrl`, `BucketName`, `PublicUrl`, `AccessKey`, `SecretKey`, `Region`, `StorageQuotaBytes`).

`Redis` có connection string. Đăng ký Redis trong `IOCContainer` đang để comment, chưa gắn vào request.

---

## 2. `congThongTinf.Service`

Use case, DTO, map entity → DTO, lỗi nghiệp vụ, object storage.

### Thư viện

| Package | Version |
| --- | --- |
| `Microsoft.AspNetCore.App` | FrameworkReference |
| `AWSSDK.S3` | 4.0.104.2 |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 |
| `SixLabors.ImageSharp` | 3.1.12 |

Project reference: `congThongTinf.Infrastructure`.

### Cây khung

```
congThongTinf.Service/
  Interfaces/
    I{Feature}Service.cs
    Commons/
      ILoggerService.cs
      IBlobService.cs
      IMapperService.cs
  Services/
    {Feature}Service.cs
    Commons/
      LoggerService.cs
      BlobService.cs          // chứa luôn class ObjectStorageOptions
      MapperService.cs
  DTOs/
    {Feature}Dtos.cs
    BlobDtos.cs               // UploadFileResultDto, BlobObjectDto, BlobDownloadResult
  Mappers/
    {Feature}Mapper.cs
  Utils/
    ApiResult.cs
    ErrorHelper.cs
    ExceptionUtils.cs
    ErrorMessages.cs
    PasswordHasher.cs
    JwtUtils.cs
    DateTimeUtc.cs
    OtpGenerator.cs
    ImageFiles.cs
    SharedCacheKeys/
```

### Interface service

`congThongTinf.Service.Interfaces.I{Feature}Service`: chỉ method async, nhận DTO / id, trả DTO hoặc `bool`. Không nhận `HttpContext`.

### `{Feature}Service`

```csharp
public class {Feature}Service : I{Feature}Service
{
    private readonly ILoggerService _logger;
    private readonly IUnitOfWork _unitOfWork;

    // đọc/ghi qua _unitOfWork.{Entities}
    // fail → throw ErrorHelper.*(message)
    // xong → await _unitOfWork.SaveChangesAsync()
    // return DTO
}
```

Service không trả `IActionResult`. Không mở `DbContext`. Log qua `ILoggerService` (`Info`, `Warn`, `Error`, `Success`).

### Commons

`ILoggerService` không có namespace. `LoggerService` ghi ra console, có màu.

```csharp
public interface ILoggerService
{
    void Success(string msg);
    void Error(string msg);
    void Warn(string msg);
    void Info(string msg);
}

public class LoggerService : ILoggerService
{
    public void Info(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[Logger Info] - {DateTime.UtcNow.AddHours(7)} - " + msg);
        Console.ResetColor();
    }
    // Success = Green, Warn = Yellow, Error = Red. Cùng format dòng.
}
```

`IMapperService` không có namespace. `MapperService` copy property public cùng tên, cùng kiểu.

```csharp
public interface IMapperService
{
    TDestination Map<TSource, TDestination>(TSource source)
        where TSource : class
        where TDestination : class, new();
}

public class MapperService : IMapperService
{
    public TDestination Map<TSource, TDestination>(TSource source)
        where TSource : class
        where TDestination : class, new()
    {
        var destination = new TDestination();
        var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destProps = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceProp in sourceProps)
        {
            var destProp = destProps.FirstOrDefault(p =>
                p.Name == sourceProp.Name && p.PropertyType == sourceProp.PropertyType);
            if (destProp != null && destProp.CanWrite)
                destProp.SetValue(destination, sourceProp.GetValue(source));
        }

        return destination;
    }
}
```

`IBlobService` là hợp đồng object storage. `ObjectStorageOptions` nằm cùng file `BlobService.cs`, bind section `ObjectStorage`.

```csharp
public interface IBlobService
{
    Task<UploadFileResultDto> UploadFileAsync(IFormFile file, string folder = "uploads",
        CancellationToken cancellationToken = default, string? objectFileName = null);
    Task DeleteFileAsync(string key, CancellationToken cancellationToken = default);
    string GetPublicUrl(string key);
    Task<BlobDownloadResult> DownloadFileAsync(string key, CancellationToken cancellationToken = default);
    string GetKeyFromPublicUrl(string url);
    Task<List<BlobObjectDto>> ListImagesAsync(string folder, CancellationToken cancellationToken = default);
}

public class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";
    public string ServiceUrl { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public long StorageQuotaBytes { get; set; } = 10L * 1024 * 1024 * 1024;
}

public class BlobService : IBlobService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int MaxImageEdgePixels = 8192;
    private const int WebpQuality = 82;

    public BlobService(IAmazonS3 s3Client, IOptions<ObjectStorageOptions> options, ILogger<BlobService> logger) { }

    public async Task<UploadFileResultDto> UploadFileAsync(...)
    {
        // reject file rỗng hoặc > 10 MB
        // ảnh: ImageSharp → WebP quality 82, cạnh tối đa 8192
        // PutObject (DisablePayloadSigning khi endpoint là HTTPS)
        // return { Key, Url = GetPublicUrl(key), ContentType, Size }
    }
}
```

### DTO

POCO trong `congThongTinf.Service.DTOs`. Một file theo feature: request, response, list item. Không kế thừa entity.

### Mapper

`{Feature}Mapper` là **static class**, method `To{Feature}Dto(Entity)`. Dùng khi map không phải copy 1-1 (navigation, đổi tên field). `MapperService` dùng khi property trùng tên.

### `Utils` — hợp đồng lỗi và response

`ApiResult` và `ApiResult<T>` (global namespace trong file này):

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": { "code": "200", "message": "...", "data": {} },
  "error": null
}
```

Failure: `value` null (hoặc có data nếu truyền vào), `error: { code, message }`.

`ApiResult` / `ApiResult<T>` nằm ở global namespace.

```csharp
public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public bool IsFailure => !IsSuccess;
    public ResponseDataContent<T> Value { get; set; }
    public ErrorContent Error { get; set; }

    public static ApiResult<T> Success(T data, string code = "200", string message = "Operation successful.")
        => new()
        {
            IsSuccess = true,
            Value = new ResponseDataContent<T> { Code = code, Message = message, Data = data },
            Error = null
        };

    public static ApiResult<T> Failure(string code = "400", string message = "Operation failed.", T value = default)
        => new()
        {
            IsSuccess = false,
            Value = value == null ? null : new ResponseDataContent<T> { Code = code, Message = message, Data = value },
            Error = new ErrorContent { Code = code, Message = message }
        };
}

public class ApiResult   // cùng shape, Value không có Data
{
    public static ApiResult Success(string code = "200", string message = "Operation successful.");
    public static ApiResult Failure(string code = "400", string message = "Operation failed.");
}
```

`ErrorHelper` gắn status vào `Exception.Data`. Service ném, controller không tự chọn status.

```csharp
public static class ErrorHelper
{
    public static Exception WithStatus(int statusCode, string message)
    {
        var ex = new Exception(message);
        ex.Data["StatusCode"] = statusCode;
        return ex;
    }

    public static Exception Unauthorized(string message = "Không được phép.") => WithStatus(401, message);
    public static Exception BadRequest(string message = "Dữ liệu không hợp lệ.") => WithStatus(400, message);
    public static Exception Forbidden(string message = "Truy cập bị từ chối.") => WithStatus(403, message);
    public static Exception NotFound(string message = "Không tìm thấy tài nguyên.") => WithStatus(404, message);
    public static Exception Conflict(string message = "Xung đột dữ liệu.") => WithStatus(409, message);
    public static Exception Internal(string message = "Lỗi hệ thống.") => WithStatus(500, message);
}
```

```csharp
public static class ExceptionUtils
{
    public static int ExtractStatusCode(Exception ex)
    {
        if (ex.Data.Contains("StatusCode") && int.TryParse(ex.Data["StatusCode"]?.ToString(), out var code))
            return code;
        return 500;
    }

    public static ApiResult<T> CreateErrorResponse<T>(Exception ex)
        => ApiResult<T>.Failure(ExtractStatusCode(ex).ToString(), ex.Message ?? "Lỗi không xác định.");
}
```

`ErrorMessages` là `static class` chứa `const string`, nhóm bằng `#region`. Service truyền hằng này vào `ErrorHelper`, không hard-code lại câu đã có.

```csharp
public static class ErrorMessages
{
    public const string AccountNotFound = "Không tìm thấy tài khoản với email này.";
    public const string AccountEmailAlreadyRegistered = "Email này đã được đăng ký.";
    public const string CategoryNotFound = "Không tìm thấy danh mục.";
}
```

`PasswordHasher`: salt 16 byte + HMAC-SHA256, lưu `base64(salt):base64(hash)`.

```csharp
public class PasswordHasher
{
    public string? HashPassword(string password);          // "{salt}:{hash}"
    public bool VerifyPassword(string password, string? storedHash);  // FixedTimeEquals
}
```

`JwtUtils.GenerateJwtToken` ký HMAC-SHA256. Claim: `sub`, `NameIdentifier` (user id), `email`, `Role`, `jti`, `iat`. Issuer/audience/secret lấy từ `JWT:*`. Secret ngắn hơn 32 ký tự thì ném `ArgumentException`.

```csharp
public static class JwtUtils
{
    public static string GenerateJwtToken(
        Guid userId, string email, string role,
        IConfiguration configuration, TimeSpan validityPeriod)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(validityPeriod),
            Issuer = configuration["JWT:Issuer"],
            Audience = configuration["JWT:Audience"],
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        });
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

`DateTimeUtc` giữ `DateTimeKind.Utc` vì Npgsql từ chối `timestamp with time zone` khi Kind không phải Utc. `DateTime.Date` luôn ra `Unspecified`.

```csharp
public static class DateTimeUtc
{
    public static DateTime StartOfDay(DateTime date) =>
        new(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime ExclusiveEndOfDay(DateTime date) => StartOfDay(date).AddDays(1);

    public static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : new DateTime(value.Ticks, DateTimeKind.Utc);
}
```

```csharp
public static class OtpGenerator
{
    public static string GenerateNumeric(int length = 6);
    public static string GenerateAlphanumeric(int length = 8);
    public static OtpToken GenerateToken(int length = 6, TimeSpan? validFor = null); // mặc định 5 phút
}

public class OtpToken
{
    public string Code { get; }
    public DateTime GeneratedAtUtc { get; }
    public DateTime ExpiresAtUtc { get; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;
}
```

`ImageFiles` chỉ nhận diện ảnh. Chuyển WebP nằm trong `BlobService`.

```csharp
public static class ImageFiles
{
    // .jpg .jpeg .jfif .png .gif .webp .bmp .tif .tiff .tga .pbm .pgm .ppm .qoi
    public static bool IsImage(IFormFile file);       // Content-Type image/* hoặc đuôi trên
    public static bool HasImageExtension(string? key);
}
```

`ListingSharedCacheKeys` là static builder cho key Redis. Mỗi key một method, trả chuỗi `prefix:{id}`.

```csharp
public static class ListingSharedCacheKeys
{
    public static string GetInventoryItem(Guid inventoryItemId) => $"inventoryitem:{inventoryItemId}";
    public static string GetUserAvailableItems(Guid userId) => $"listing:user-items:{userId}";
    public static string GetListingDetail(Guid listingId) => $"listing:detail:{listingId}";
}
```

---

## 3. `congThongTinf.Domain`

Entity, `DbContext`, migration. Không có use case, không có DTO.

### Thư viện

| Package | Version |
| --- | --- |
| `Microsoft.EntityFrameworkCore` | 10.0.0 |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.0 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.0 |
| `Microsoft.Extensions.Configuration` | 10.0.0 |
| `Microsoft.Extensions.Configuration.Json` | 10.0.0 |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.0 |
| `System.Security.Cryptography.Xml` | 10.0.12 |

Không project reference.

### Cây khung

```
congThongTinf.Domain/
  congThongTinfDbContext.cs
  Entities/
    BaseEntity.cs
    {Entity}.cs
  Migrations/                 // sinh bởi EF, không sửa tay trong agent
    {timestamp}_{name}.cs
    {timestamp}_{name}.Designer.cs
    congThongTinfDbContextModelSnapshot.cs
```

### `BaseEntity`

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

Mọi entity kế thừa `BaseEntity`. `GenericRepository` gán `Id`, `CreatedAt`, `UpdatedAt` lúc add/update.

### `{Entity}`

Class trong `congThongTinf.Domain.Entities`. Property scalar + navigation. Không attribute validation của API. Quan hệ và ràng buộc cột khai báo trong `OnModelCreating`, không dùng data annotation làm nguồn chính.

### `congThongTinfDbContext`

- `DbContext`
- Constructor rỗng và constructor `DbContextOptions<congThongTinfDbContext>`
- `DbSet<{Entity}> {Entities}`
- `OnModelCreating`: `HasKey`, `IsRequired`, `HasMaxLength`, `HasIndex`, `HasOne` / `WithMany` / `HasForeignKey` / `OnDelete`

Thêm entity = thêm `DbSet` + block cấu hình trong `OnModelCreating`. Migration để người generate ngoài agent. Startup gọi `Database.Migrate()`.

---

## 4. `congThongTinf.Infrastructure`

Repository generic, unit of work, đọc user hiện tại, helper không thuộc use case.

### Thư viện

| Package | Version |
| --- | --- |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 |

Project reference: `congThongTinf.Domain`. EF Core và Npgsql đi theo `Domain`.

### Cây khung

```
congThongTinf.Infrastructure/
  UnitOfWork.cs
  Interfaces/
    IGenericRepository.cs
    IUnitOfWork.cs
    IClaimsService.cs
    ICurrentTime.cs
  Repositories/
    GenericRepository.cs
  Commons/
    ClaimsService.cs
    CurrentTime.cs
    Pagination.cs
  Utils/
    AuthenTools.cs
    TokenTools.cs
    GenerateJWTToken.cs
    HashHelper.cs
    StringTools.cs
    StringExtensions.cs
    ResourceHelper.cs
```

`IGenericRepository`, `GenericRepository`, `IUnitOfWork`, `UnitOfWork`, `ICurrentTime` đang ở global namespace (không khai báo `namespace`). `IClaimsService` / `ClaimsService` / `Pagination` có namespace `congThongTinf.Infrastructure.*`.

### `IGenericRepository<TEntity> where TEntity : BaseEntity`

| Method | Việc |
| --- | --- |
| `GetAllAsync(predicate, includes)` | List, filter, include |
| `GetByIdAsync(id, includes)` | Một bản ghi theo `Id` |
| `FirstOrDefaultAsync(predicate, includes)` | Một bản ghi theo điều kiện |
| `GetQueryable()` | `IQueryable` để service tự compose |
| `CountAsync(predicate)` | Đếm |
| `AddAsync` / `AddRangeAsync` | Gán `Id` nếu empty, set `CreatedAt` / `UpdatedAt` = `DateTime.UtcNow` |
| `Update` / `UpdateRange` | Set `UpdatedAt`, `DbSet.Update` |
| `HardRemove` / `HardRemoveRange` | Xóa cứng |
| `SoftRemove*` | Khai báo trên interface, implementation ném `NotSupportedException` |

Không có repository riêng theo entity. Mọi entity đi qua `GenericRepository<T>`.

### `IUnitOfWork`

Một property `IGenericRepository<{Entity}>` cho mỗi aggregate. `SaveChangesAsync()`, `Dispose()`, và transaction: `BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`.

`UnitOfWork` nhận `congThongTinfDbContext`, tạo `new GenericRepository<T>(dbContext)` trong ctor. Thêm entity = thêm một property ở interface và gán trong ctor.

### Commons

`IClaimsService` có namespace. `CurrentTime` và `ICurrentTime` không có namespace.

```csharp
public interface IClaimsService
{
    Guid CurrentUserId { get; }
    string? IpAddress { get; }
}

public class ClaimsService : IClaimsService
{
    public ClaimsService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
        CurrentUserId = Guid.TryParse(AuthenTools.GetCurrentUserId(identity), out var id) ? id : Guid.Empty;
        IpAddress = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    public Guid CurrentUserId { get; }
    public string? IpAddress { get; }
}
```

```csharp
public interface ICurrentTime
{
    DateTime GetCurrentTime();
}

public class CurrentTime : ICurrentTime
{
    public DateTime GetCurrentTime() => DateTime.UtcNow.ToUniversalTime();
}
```

```csharp
public class Pagination<T>
{
    public Pagination(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 0;
    }

    public List<T> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
```

### `Utils`

`AuthenTools` đọc `ClaimTypes.NameIdentifier`. `ClaimsService` gọi method này.

```csharp
public static class AuthenTools
{
    public static string? GetCurrentUserId(ClaimsIdentity? identity)
        => identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
```

`TokenTools.GetPrincipalFromExpiredToken` kiểm tra chữ ký HMAC-SHA256 (`ValidateIssuer` và `ValidateAudience` tắt, `ValidateLifetime` bật). `GenerateRefreshToken` trả 64 byte ngẫu nhiên dạng Base64.

```csharp
public static class TokenTools
{
    public static ClaimsPrincipal GetPrincipalFromExpiredToken(string? token, IConfiguration configuration);
    public static string GenerateRefreshToken();   // 64 byte ngẫu nhiên, Base64
}
```

`GenerateJWTToken` là bản ký JWT thứ hai, nhận sẵn `List<Claim>`. Key config là `JWT:ValidIssuer`, `JWT:ValidAudience`, `JWT:TokenValidityInMinutes` — khác `JwtUtils` (`JWT:Issuer`, `JWT:Audience`, `TimeSpan` truyền vào).

```csharp
public static class GenerateJWTToken
{
    public static JwtSecurityToken CreateToken(List<Claim> authClaims, IConfiguration configuration, DateTime currentTime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]));
        int.TryParse(configuration["JWT:TokenValidityInMinutes"], out var minutes);
        return new JwtSecurityToken(
            configuration["JWT:ValidIssuer"],
            configuration["JWT:ValidAudience"],
            expires: currentTime.AddMinutes(minutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
    }
}
```

```csharp
public class HashHelper
{
    public static string HmacSHA512(string key, string inputData);   // hex thường
    public static string HmacSHA256(string inputData, string key);   // hex thường, không dấu '-'
}
```

```csharp
public static class StringTools
{
    public static string ConvertToUnSign(string input);  // bỏ dấu tiếng Việt, đ/Đ → d/D
}

public static class StringExtensions
{
    public static string StripJsonWhitespace(this string json);  // giữ nguyên chuỗi trong dấu ngoặc kép
}
```

`ResourceHelper` đọc embedded resource theo `{AssemblyName}.{path.dấu.chấm}`.

```csharp
public static class ResourceHelper
{
    public static string ReadResource(string relativePath, Assembly fromAssembly);
    public static string ReadJsonResource(string relativePath, Assembly fromAssembly, bool stripWhitespace = false);
    public static int DateTimeValidate(DateTime startDate, DateTime endDate);  // số ngày, gồm cả ngày bắt đầu
}
```

---

## Thêm một feature

Chỉ đụng các slot sau. Không thêm project, không thêm repository class.

1. **Domain** — `{Entity} : BaseEntity`. `DbSet` + cấu hình trong `congThongTinfDbContext.OnModelCreating`. Migration do người generate.
2. **Infrastructure** — property `IGenericRepository<{Entity}>` trên `IUnitOfWork` và `UnitOfWork`.
3. **Service** — `I{Feature}Service`, `{Feature}Service` (`IUnitOfWork` + `ILoggerService`), `{Feature}Dtos.cs`, `{Feature}Mapper.cs` nếu map không 1-1.
4. **API** — `{Feature}Controller` bọc `ApiResult` / `ExceptionUtils`. Một dòng `AddScoped<I{Feature}Service, {Feature}Service>()` trong `SetupBusinessServicesLayer`.

Đọc dữ liệu: `_unitOfWork.{Entities}.GetQueryable()` hoặc `FirstOrDefaultAsync` / `GetByIdAsync`. Ghi: `AddAsync` hoặc `Update`, rồi một lần `SaveChangesAsync`. Lỗi: `throw ErrorHelper.NotFound|BadRequest|Forbidden|Conflict|Unauthorized(...)`.
