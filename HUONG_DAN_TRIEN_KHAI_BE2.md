# HƯỚNG DẪN CHI TIẾT TRIỂN KHAI BACKEND BE-2 (LEON)
> **Dành cho:** Thành viên BE-2 (Leon) – Đồ án Giữa kỳ môn Thực tập An toàn Thông tin & Lập trình Web API.  
> **Kiến trúc:** Clean Architecture + Modular Monolith (.NET 10).  
> **Mục đích:** Tài liệu hướng dẫn từng bước từ con số 0, giải thích rõ lý do từng file, kèm code hoàn chỉnh để copy & đối chiếu.

---

## MỤC LỤC
1. [Bản đồ phạm vi công việc của BE-2](#1-bản-đồ-phạm-vi-công-việc-của-be-2)
2. [Cấu trúc 4 tầng trong Project](#2-cấu-trúc-4-tầng-trong-project)
3. [Danh sách tất cả các File cần tạo & Đường dẫn (File Checklist)](#3-danh-sách-tất-cả-các-file-cần-tạo--đường-dẫn-file-checklist)
4. [Quy trình 6 bước triển khai chuẩn Clean Architecture](#4-quy-trình-6-bước-triển-khai-chuẩn-clean-architecture)
5. [Chi tiết code từng Module](#5-chi-tiết-code-từng-module)
   - [Module A: Quản lý Email Whitelist](#module-a-quản-lý-email-whitelist)
   - [Module B: User Profile & Avatar Upload (Bảo mật IDOR & File)](#module-b-user-profile--avatar-upload-bảo-mật-idor--file)
   - [Module C: Admin User Management (Quản trị người dùng)](#module-c-admin-user-management-quản-trị-người-dùng)
6. [Đăng ký Dependency Injection & Static Files trong Program.cs](#6-đăng-ký-dependency-injection--static-files-trong-programcs)
7. [Bảng kiểm tra An toàn Thông tin (Security Checklist)](#7-bảng-kiểm-tra-an-toàn-thông-tin-security-checklist)

---

## 1. BẢN ĐỒ PHẠM VI CÔNG VIỆC CỦA BE-2

BE-2 phụ trách 3 mảng chức năng chính với các API Endpoint sau:

| STT | Chức năng | Method | Endpoint | Quyền hạn |
| :--- | :--- | :--- | :--- | :--- |
| **1** | Xem thông tin cá nhân | `GET` | `/api/users/me` | Người dùng đã đăng nhập |
| **2** | Cập nhật thông tin cá nhân | `PUT` | `/api/users/me` | Người dùng đã đăng nhập |
| **3** | Tải lên ảnh đại diện (Avatar) | `POST` / `PATCH` | `/api/users/me/avatar` | Người dùng đã đăng nhập |
| **4** | Admin xem danh sách người dùng | `GET` | `/api/admin/users` | Chỉ `Admin` (Có lọc & phân trang) |
| **5** | Admin cập nhật Role/Status User | `PUT` | `/api/admin/users/{id}` | Chỉ `Admin` |
| **6** | Admin xem danh sách Whitelist | `GET` | `/api/admin/whitelist` | Chỉ `Admin` |
| **7** | Admin thêm email vào Whitelist | `POST` | `/api/admin/whitelist` | Chỉ `Admin` |
| **8** | Admin xóa email khỏi Whitelist | `DELETE` | `/api/admin/whitelist/{id}`| Chỉ `Admin` |

---

## 2. CẤU TRÚC 4 TẦNG TRONG PROJECT

Dự án chia làm 4 Project con (từ trong lõi ra ngoài):

```
[1. FirstAPIProject.Domain]        <- Lõi: Chứa Entity (Các bảng DB)
       ▲
[2. FirstAPIProject.Application]   <- Nghiệp vụ: DTOs, Services, Interfaces, Validators
       ▲
[3. FirstAPIProject.Infrastructure]<- Hạ tầng: AppDbContext, Lưu file, SQL Server
       ▲
[4. FirstAPIProject]               <- Web API: Controllers, Program.cs, Cấu hình
```

---

## 3. DANH SÁCH TẤT CẢ CÁC FILE CẦN TẠO & ĐƯỜNG DẪN (FILE CHECKLIST)

Dưới đây là bảng tổng hợp toàn bộ các file bạn cần thao tác khi thực hiện vai trò **BE-2 (Leon)**. Bạn hãy đối chiếu từng file theo đúng đường dẫn này:

### Bảng tra cứu nhanh theo tầng:

| Tầng | Thao tác | Đường dẫn file chính xác | Mục đích / Chức năng |
| :--- | :--- | :--- | :--- |
| **Domain** | `[TẠO MỚI]` | `FirstAPIProject.Domain/Entities/EmailWhitelist.cs` | Thực thể CSDL cho bảng Whitelist (Id, Email, Reason, CreatedAt) |
| **Domain** | `[CHỈNH SỬA]` | `FirstAPIProject.Domain/Entities/User.cs` | Bổ sung các cột: `FullName`, `PhoneNumber`, `AvatarUrl`, `Role`, `IsActive` |
| **Infrastructure** | `[CHỈNH SỬA]` | `FirstAPIProject.Infrastructure/Persistence/AppDbContext.cs` | Khai báo `public DbSet<EmailWhitelist> EmailWhitelists` |
| **Infrastructure** | `[TẠO MỚI]` | `FirstAPIProject.Infrastructure/Services/LocalFileStorageService.cs` | Dịch vụ lưu file ảnh, kiểm tra Magic Bytes, đổi tên bằng GUID |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Common/Interfaces/IFileStorageService.cs` | Interface cho dịch vụ lưu file ảnh |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/Whitelist/DTOs/WhitelistRequests.cs` | Các DTO hứng/trả dữ liệu Whitelist (`AddWhitelistRequest`, `WhitelistItemResponse`) |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/Whitelist/Validators/WhitelistValidators.cs` | FluentValidation kiểm tra email hợp lệ cho Whitelist |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/Whitelist/Interfaces/IEmailWhitelistService.cs` | Interface khai báo các hàm nghiệp vụ Whitelist |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/Whitelist/Services/EmailWhitelistService.cs` | Xử lý logic thêm, xóa, kiểm tra trùng lặp email Whitelist |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/User/DTOs/UserProfileDTOs.cs` | Các DTO cá nhân (`UserProfileResponse`, `UpdateProfileRequest`) |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/User/DTOs/AdminUserDTOs.cs` | Các DTO quản trị (`AdminUserFilterRequest`, `AdminUpdateUserRequest`) |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/User/Interfaces/IUserService.cs` | Interface xem/sửa hồ sơ cá nhân và đổi avatar |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/User/Services/UserService.cs` | Logic cập nhật thông tin người dùng và lưu avatar |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/User/Interfaces/IAdminUserService.cs` | Interface quản trị danh sách người dùng cho Admin |
| **Application** | `[TẠO MỚI]` | `FirstAPIProject.Application/Modules/User/Services/AdminUserService.cs` | Logic tìm kiếm, lọc theo Role/Status, phân trang `Skip/Take` |
| **Web API** | `[TẠO MỚI]` | `FirstAPIProject/Controllers/UserProfileController.cs` | Controller `/api/users/me` (Chống IDOR lấy ID từ JWT Claim) |
| **Web API** | `[TẠO MỚI]` | `FirstAPIProject/Controllers/AdminWhitelistController.cs` | Controller `/api/admin/whitelist` (Phân quyền Admin) |
| **Web API** | `[TẠO MỚI]` | `FirstAPIProject/Controllers/AdminUserController.cs` | Controller `/api/admin/users` (Phân quyền Admin) |
| **Web API** | `[TẠO THƯ MỤC]`| `FirstAPIProject/wwwroot/uploads/avatars/` | Thư mục lưu trữ vật lý các file ảnh đại diện được upload |
| **Web API** | `[CHỈNH SỬA]` | `FirstAPIProject/Program.cs` | Đăng ký `AddScoped` 4 dịch vụ và bật `app.UseStaticFiles()` |

---

### Sơ đồ cấu trúc cây thư mục:
```text
FirstAPIProject/
├── FirstAPIProject.Domain/
│   └── Entities/
│       ├── EmailWhitelist.cs                         <-- [TẠO MỚI]
│       └── User.cs                                   <-- [CHỈNH SỬA]
│
├── FirstAPIProject.Infrastructure/
│   ├── Persistence/
│   │   └── AppDbContext.cs                           <-- [CHỈNH SỬA]
│   └── Services/
│       └── LocalFileStorageService.cs                <-- [TẠO MỚI]
│
├── FirstAPIProject.Application/
│   ├── Common/Interfaces/
│   │   └── IFileStorageService.cs                    <-- [TẠO MỚI]
│   └── Modules/
│       ├── Whitelist/
│       │   ├── DTOs/WhitelistRequests.cs             <-- [TẠO MỚI]
│       │   ├── Validators/WhitelistValidators.cs     <-- [TẠO MỚI]
│       │   ├── Interfaces/IEmailWhitelistService.cs  <-- [TẠO MỚI]
│       │   └── Services/EmailWhitelistService.cs     <-- [TẠO MỚI]
│       └── User/
│           ├── DTOs/UserProfileDTOs.cs               <-- [TẠO MỚI]
│           ├── DTOs/AdminUserDTOs.cs                 <-- [TẠO MỚI]
│           ├── Interfaces/IUserService.cs            <-- [TẠO MỚI]
│           ├── Services/UserService.cs               <-- [TẠO MỚI]
│           ├── Interfaces/IAdminUserService.cs       <-- [TẠO MỚI]
│           └── Services/AdminUserService.cs          <-- [TẠO MỚI]
│
└── FirstAPIProject/
    ├── Controllers/
    │   ├── UserProfileController.cs                  <-- [TẠO MỚI]
    │   ├── AdminWhitelistController.cs               <-- [TẠO MỚI]
    │   └── AdminUserController.cs                    <-- [TẠO MỚI]
    ├── wwwroot/uploads/avatars/                      <-- [TẠO THƯ MỤC]
    └── Program.cs                                    <-- [CHỈNH SỬA]
```

---

## 4. QUY TRÌNH 6 BƯỚC TRIỂN KHAI CHUẨN

Mỗi khi code một tính năng mới, bạn hãy đi theo đúng thứ tự 6 bước:
1. **Bước 1 (Domain):** Tạo hoặc bổ sung `Entity` (Đại diện cho bảng/cột trong DB).
2. **Bước 2 (Infrastructure):** Khai báo `DbSet<...>` trong `AppDbContext.cs`.
3. **Bước 3 (Application - DTO & Validator):** Tạo DTO Request/Response và viết quy tắc kiểm tra dữ liệu bằng FluentValidation.
4. **Bước 4 (Application - Service):** Tạo `Interface` và viết class `Service` thực thi logic.
5. **Bước 5 (API - Controller):** Tạo `Controller` để hứng HTTP Request và phân quyền (`[Authorize]`).
6. **Bước 6 (API - Program.cs):** Đăng ký Service vào container `builder.Services.AddScoped<...>()`.

---

## 5. CHI TIẾT CODE TỪNG MODULE

---

### MODULE A: QUẢN LÝ EMAIL WHITELIST

#### Bước 1: Tạo Entity
* **File:** `FirstAPIProject.Domain/Entities/EmailWhitelist.cs`
```csharp
using System;

namespace FirstAPIProject.Domain.Entities
{
    public class EmailWhitelist
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### Bước 2: Khai báo vào AppDbContext
* **File:** `FirstAPIProject.Infrastructure/Persistence/AppDbContext.cs`
```csharp
public DbSet<EmailWhitelist> EmailWhitelists => Set<EmailWhitelist>();
```

#### Bước 3: Tạo DTOs & Validator
* **File:** `FirstAPIProject.Application/Modules/Whitelist/DTOs/WhitelistRequests.cs`
```csharp
namespace FirstAPIProject.Application.Modules.Whitelist.DTOs
{
    public record AddWhitelistRequest(string Email, string? Reason);
    public record WhitelistItemResponse(Guid Id, string Email, string? Reason, DateTime CreatedAt);
}
```

* **File:** `FirstAPIProject.Application/Modules/Whitelist/Validators/WhitelistValidators.cs`
```csharp
using FluentValidation;
using FirstAPIProject.Application.Modules.Whitelist.DTOs;

namespace FirstAPIProject.Application.Modules.Whitelist.Validators
{
    public class AddWhitelistRequestValidator : AbstractValidator<AddWhitelistRequest>
    {
        public AddWhitelistRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ")
                .MaximumLength(150);

            RuleFor(x => x.Reason)
                .MaximumLength(255);
        }
    }
}
```

#### Bước 4: Tạo Interface & Service
* **File:** `FirstAPIProject.Application/Modules/Whitelist/Interfaces/IEmailWhitelistService.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirstAPIProject.Application.Modules.Whitelist.DTOs;

namespace FirstAPIProject.Application.Modules.Whitelist.Interfaces
{
    public interface IEmailWhitelistService
    {
        Task<List<WhitelistItemResponse>> GetAllAsync();
        Task<WhitelistItemResponse> AddAsync(AddWhitelistRequest request);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> IsEmailWhitelistedAsync(string email);
    }
}
```

* **File:** `FirstAPIProject.Application/Modules/Whitelist/Services/EmailWhitelistService.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstAPIProject.Application.Modules.Whitelist.DTOs;
using FirstAPIProject.Application.Modules.Whitelist.Interfaces;
using FirstAPIProject.Domain.Entities;
using FirstAPIProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirstAPIProject.Application.Modules.Whitelist.Services
{
    public class EmailWhitelistService : IEmailWhitelistService
    {
        private readonly AppDbContext _db;

        public EmailWhitelistService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<WhitelistItemResponse>> GetAllAsync()
        {
            return await _db.EmailWhitelists
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new WhitelistItemResponse(x.Id, x.Email, x.Reason, x.CreatedAt))
                .ToListAsync();
        }

        public async Task<WhitelistItemResponse> AddAsync(AddWhitelistRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var exists = await _db.EmailWhitelists.AnyAsync(x => x.Email.ToLower() == normalizedEmail);
            if (exists)
            {
                throw new InvalidOperationException("Email này đã tồn tại trong danh sách Whitelist.");
            }

            var item = new EmailWhitelist
            {
                Email = normalizedEmail,
                Reason = request.Reason
            };

            _db.EmailWhitelists.Add(item);
            await _db.SaveChangesAsync();

            return new WhitelistItemResponse(item.Id, item.Email, item.Reason, item.CreatedAt);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _db.EmailWhitelists.FindAsync(id);
            if (item == null) return false;

            _db.EmailWhitelists.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsEmailWhitelistedAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var normalized = email.Trim().ToLowerInvariant();
            return await _db.EmailWhitelists.AnyAsync(x => x.Email.ToLower() == normalized);
        }
    }
}
```

#### Bước 5: Tạo Controller
* **File:** `FirstAPIProject/Controllers/AdminWhitelistController.cs`
```csharp
using System;
using System.Threading.Tasks;
using FirstAPIProject.Application.Modules.Whitelist.DTOs;
using FirstAPIProject.Application.Modules.Whitelist.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstAPIProject.Controllers
{
    [ApiController]
    [Route("api/admin/whitelist")]
    [Authorize(Roles = "Admin")] // BẮT BUỘC ROLE ADMIN
    public class AdminWhitelistController : ControllerBase
    {
        private readonly IEmailWhitelistService _whitelistService;

        public AdminWhitelistController(IEmailWhitelistService whitelistService)
        {
            _whitelistService = whitelistService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _whitelistService.GetAllAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddWhitelistRequest request)
        {
            try
            {
                var result = await _whitelistService.AddAsync(request);
                return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _whitelistService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy email trong whitelist." });
            return NoContent();
        }
    }
}
```

---

### MODULE B: USER PROFILE & AVATAR UPLOAD (BẢO MẬT IDOR & FILE)

#### 1. Dịch vụ lưu file an toàn (Tầng Infrastructure)
Kiểm tra Magic Bytes (chữ ký nhị phân) để ngăn chặn kẻ tấn công đổi đuôi file `.exe`/`.php` thành `.jpg`.

* **Interface:** `FirstAPIProject.Application/Common/Interfaces/IFileStorageService.cs`
```csharp
using System.IO;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string subFolder);
        void DeleteFile(string relativeUrl);
    }
}
```

* **Service:** `FirstAPIProject.Infrastructure/Services/LocalFileStorageService.cs`
```csharp
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FirstAPIProject.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace FirstAPIProject.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string subFolder)
        {
            var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                throw new ArgumentException("Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .webp");
            }

            // KIỂM TRA MAGIC BYTES (CHỐNG GIẢ MẠO FILE HEADER)
            var header = new byte[8];
            fileStream.Position = 0;
            await fileStream.ReadExactlyAsync(header, 0, 8);
            fileStream.Position = 0;

            if (!IsValidImageHeader(header, ext))
            {
                throw new ArgumentException("Nội dung file không khớp với định dạng ảnh hợp lệ.");
            }

            // ĐỔI TÊN BẰNG GUID NGẪU NHIÊN ĐỂ TRÁNH TRÙNG & GHI ĐÈ FILE HỆ THỐNG
            var safeFileName = $"{Guid.NewGuid()}{ext}";
            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), subFolder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, safeFileName);
            using (var destinationStream = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            return $"/{subFolder}/{safeFileName}".Replace("\\", "/");
        }

        public void DeleteFile(string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) return;
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webRoot, relativeUrl.TrimStart('/').Replace("/", "\\"));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private static bool IsValidImageHeader(byte[] header, string ext)
        {
            // JPG: FF D8 FF
            if (ext is ".jpg" or ".jpeg") return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            // PNG: 89 50 4E 47
            if (ext == ".png") return header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            // WEBP: RIFF...WEBP
            if (ext == ".webp") return header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46;
            return false;
        }
    }
}
```

#### 2. DTOs & Service User Profile
* **File:** `FirstAPIProject.Application/Modules/User/DTOs/UserProfileDTOs.cs`
```csharp
using System;

namespace FirstAPIProject.Application.Modules.User.DTOs
{
    public record UserProfileResponse(Guid Id, string Email, string? FullName, string? PhoneNumber, string? AvatarUrl, string Role, bool IsActive, DateTime CreatedAt);
    public record UpdateProfileRequest(string? FullName, string? PhoneNumber);
}
```

* **File:** `FirstAPIProject.Application/Modules/User/Interfaces/IUserService.cs`
```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using FirstAPIProject.Application.Modules.User.DTOs;

namespace FirstAPIProject.Application.Modules.User.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileResponse?> GetProfileAsync(Guid userId);
        Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<string> UploadAvatarAsync(Guid userId, Stream fileStream, string fileName);
    }
}
```

* **File:** `FirstAPIProject.Application/Modules/User/Services/UserService.cs`
```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Modules.User.DTOs;
using FirstAPIProject.Application.Modules.User.Interfaces;
using FirstAPIProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirstAPIProject.Application.Modules.User.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IFileStorageService _fileStorage;

        public UserService(AppDbContext db, IFileStorageService fileStorage)
        {
            _db = db;
            _fileStorage = fileStorage;
        }

        public async Task<UserProfileResponse?> GetProfileAsync(Guid userId)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) return null;

            return new UserProfileResponse(u.Id, u.Email, u.FullName, u.PhoneNumber, u.AvatarUrl, u.Role, u.IsActive, u.CreatedAt);
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) throw new KeyNotFoundException("Không tìm thấy người dùng.");

            u.FullName = request.FullName;
            u.PhoneNumber = request.PhoneNumber;
            u.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return new UserProfileResponse(u.Id, u.Email, u.FullName, u.PhoneNumber, u.AvatarUrl, u.Role, u.IsActive, u.CreatedAt);
        }

        public async Task<string> UploadAvatarAsync(Guid userId, Stream fileStream, string fileName)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) throw new KeyNotFoundException("Không tìm thấy người dùng.");

            // Lưu file mới
            var avatarUrl = await _fileStorage.SaveFileAsync(fileStream, fileName, "uploads/avatars");

            // Xóa ảnh cũ nếu có
            if (!string.IsNullOrEmpty(u.AvatarUrl))
            {
                _fileStorage.DeleteFile(u.AvatarUrl);
            }

            u.AvatarUrl = avatarUrl;
            u.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return avatarUrl;
        }
    }
}
```

#### 3. Controller User Profile (Chống lỗi IDOR tuyệt đối)
> [!IMPORTANT]
> **Chống IDOR:** Tuyệt đối **KHÔNG** nhận `userId` từ URL (ví dụ `/api/users/{id}`) hoặc body của client. Phải rút `userId` trực tiếp từ **Claims của JWT Token**!

* **File:** `FirstAPIProject/Controllers/UserProfileController.cs`
```csharp
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FirstAPIProject.Application.Modules.User.DTOs;
using FirstAPIProject.Application.Modules.User.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FirstAPIProject.Controllers
{
    [ApiController]
    [Route("api/users/me")]
    [Authorize] // BẮT BUỘC ĐÃ ĐĂNG NHẬP
    public class UserProfileController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserProfileController(IUserService userService)
        {
            _userService = userService;
        }

        // HÀM TIỆN ÍCH LẤY USER ID TỪ JWT TOKEN
        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var profile = await _userService.GetProfileAsync(GetCurrentUserId());
            if (profile == null) return NotFound(new { message = "Không tìm thấy thông tin tài khoản." });
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var updated = await _userService.UpdateProfileAsync(GetCurrentUserId(), request);
            return Ok(updated);
        }

        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(2 * 1024 * 1024)] // GIỚI HẠN TỐI ĐA 2MB (CHỐNG DOS)
        public async Task<IActionResult> UploadAvatar([FromForm] AvatarUploadRequest request)
        {
            if (request?.File == null || request.File.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file ảnh." });

            if (request.File.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "Dung lượng ảnh không được vượt quá 2MB." });

            try
            {
                using var stream = request.File.OpenReadStream();
                var avatarUrl = await _userService.UploadAvatarAsync(GetCurrentUserId(), stream, request.File.FileName);
                return Ok(new { avatarUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // Class bao bọc file upload để Swagger sinh schema chuẩn, tránh lỗi Swagger 500
    public class AvatarUploadRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}
```

---

### MODULE C: ADMIN USER MANAGEMENT (QUẢN TRỊ NGƯỜI DÙNG)

#### 1. DTOs & Service Quản trị
* **File:** `FirstAPIProject.Application/Modules/User/DTOs/AdminUserDTOs.cs`
```csharp
namespace FirstAPIProject.Application.Modules.User.DTOs
{
    public record AdminUserFilterRequest(string? Search, string? Role, bool? IsActive, int Page = 1, int PageSize = 10);
    public record AdminUpdateUserRequest(string? Role, bool? IsActive);
}
```

* **File:** `FirstAPIProject.Application/Modules/User/Interfaces/IAdminUserService.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirstAPIProject.Application.Modules.User.DTOs;

namespace FirstAPIProject.Application.Modules.User.Interfaces
{
    public interface IAdminUserService
    {
        Task<(List<UserProfileResponse> Items, int TotalCount)> GetUsersAsync(AdminUserFilterRequest filter);
        Task<UserProfileResponse> UpdateUserAsync(Guid id, AdminUpdateUserRequest request);
    }
}
```

* **File:** `FirstAPIProject.Application/Modules/User/Services/AdminUserService.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstAPIProject.Application.Modules.User.DTOs;
using FirstAPIProject.Application.Modules.User.Interfaces;
using FirstAPIProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirstAPIProject.Application.Modules.User.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AppDbContext _db;

        public AdminUserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(List<UserProfileResponse> Items, int TotalCount)> GetUsersAsync(AdminUserFilterRequest filter)
        {
            var query = _db.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(u => u.Email.ToLower().Contains(s) || (u.FullName != null && u.FullName.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                query = query.Where(u => u.Role == filter.Role);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new UserProfileResponse(u.Id, u.Email, u.FullName, u.PhoneNumber, u.AvatarUrl, u.Role, u.IsActive, u.CreatedAt))
                .ToListAsync();

            return (items, total);
        }

        public async Task<UserProfileResponse> UpdateUserAsync(Guid id, AdminUpdateUserRequest request)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy người dùng.");

            if (!string.IsNullOrEmpty(request.Role))
            {
                user.Role = request.Role;
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new UserProfileResponse(user.Id, user.Email, user.FullName, user.PhoneNumber, user.AvatarUrl, user.Role, user.IsActive, user.CreatedAt);
        }
    }
}
```

#### 2. Controller Admin User
* **File:** `FirstAPIProject/Controllers/AdminUserController.cs`
```csharp
using System;
using System.Threading.Tasks;
using FirstAPIProject.Application.Modules.User.DTOs;
using FirstAPIProject.Application.Modules.User.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstAPIProject.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")] // CHỈ DÀNH CHO ADMIN
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] AdminUserFilterRequest filter)
        {
            var (items, totalCount) = await _adminUserService.GetUsersAsync(filter);
            return Ok(new
            {
                data = items,
                total = totalCount,
                page = filter.Page,
                pageSize = filter.PageSize
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserRequest request)
        {
            try
            {
                var updated = await _adminUserService.UpdateUserAsync(id, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
```

---

## 6. ĐĂNG KÝ DEPENDENCY INJECTION & STATIC FILES TRONG PROGRAM.CS

Sau khi tạo xong tất cả các service và controller, mở file `FirstAPIProject/Program.cs` và thêm các dòng đăng ký sau:

```csharp
// 1. ĐĂNG KÝ CÁC DỊCH VỤ CỦA BE-2 VÀO CONTAINER DI
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IEmailWhitelistService, EmailWhitelistService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();

// 2. CHO PHÉP XEM ẢNH STATIC FILE TRONG THƯ MỤC WWWROOT (ĐẶT SAU app.UseRouting())
app.UseStaticFiles();
```

---

## 7. BẢNG KIỂM TRA AN TOÀN THÔNG TIN (SECURITY CHECKLIST)

Khi nộp bài hoặc bảo vệ với thầy cô, bạn có thể tự tin trình bày các điểm sáng bảo mật sau đây do chính bạn thiết kế:

| Mục tiêu bảo mật | Nguy cơ tiềm ẩn | Giải pháp kỹ thuật BE-2 đã thực hiện |
| :--- | :--- | :--- |
| **Chống IDOR** | Hacker đổi ID trên URL `/api/users/123` để xem/sửa hồ sơ của người khác. | Tuyệt đối dùng endpoint `/api/users/me` và lấy `userId` từ **JWT Claim** (`User.FindFirstValue(ClaimTypes.NameIdentifier)`), không bao giờ nhận `id` từ client. |
| **Chống File Giả mạo** | Hacker đổi tên file `shell.php` thành `avatar.jpg` để upload mã độc. | Kiểm tra **Magic Bytes** (chữ ký nhị phân ở 8 byte đầu) của ảnh thay vì chỉ tin tưởng phần mở rộng `.jpg`. |
| **Chống Path Traversal** | Hacker đặt tên file `../../etc/passwd` để ghi đè file hệ thống. | Luôn đổi tên file thành chuỗi **`Guid.NewGuid()` ngẫu nhiên**. |
| **Chống DoS Upload** | Hacker gửi file nặng 10GB làm tràn bộ nhớ máy chủ. | Đặt thuộc tính `[RequestSizeLimit(2 * 1024 * 1024)]` (giới hạn cứng 2MB) ngay trên Controller. |
| **Phân quyền chặt chẽ (RBAC)** | Người dùng thường gọi API quản trị. | Đặt `[Authorize(Roles = "Admin")]` nghiêm ngặt trên toàn bộ `AdminWhitelistController` và `AdminUserController`. |
