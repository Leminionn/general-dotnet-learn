# FirstAPIProject - Clean Architecture + Modular Monolith Documentation

## 1. Tổng quan dự án

Đây là một ASP.NET Core Web API được tổ chức theo hướng:

-   **Clean Architecture**
-   **Modular Monolith**
-   **Dependency Injection**
-   **Repository Pattern**
-   **Unit Of Work Pattern**
-   **Entity Framework Core**
-   **JWT Authentication**

Mục tiêu kiến trúc:

    API Layer
        ↓
    Application Layer
        ↓
    Domain Layer

    Infrastructure Layer
        ↓
    Database / External Services

Nguyên tắc quan trọng:

-   Domain không phụ thuộc project nào.
-   Application chỉ biết abstraction (interface).
-   Infrastructure triển khai database, repository, external service.
-   API chỉ chịu trách nhiệm HTTP request/response.

------------------------------------------------------------------------

# 2. Cấu trúc Solution

    FirstAPIProject.slnx

    ├── FirstAPIProject.API
    │
    ├── FirstAPIProject.Application
    │
    ├── FirstAPIProject.Domain
    │
    └── FirstAPIProject.Infrastructure

------------------------------------------------------------------------

# 3. Dependency giữa các project

## Domain

Reference:

    Không reference project nào

Vai trò:

-   Entity
-   Enum
-   Business rule cốt lõi
-   Domain interface

Ví dụ:

    User.cs
    BaseEntity.cs
    LifecycleEntity.cs
    UserRole.cs

Domain là lớp trung tâm.

------------------------------------------------------------------------

## Application

Reference:

    Application
        ↓
    Domain

Chứa:

-   DTO
-   Interface
-   Business use case
-   Service abstraction
-   Application exception

Ví dụ:

    Modules/Auth

    DTOs
        LoginRequest.cs
        RegisterRequest.cs

    Interfaces
        IAuthService.cs

    Services
        AuthService.cs

Application không biết database dùng gì.

Nó chỉ biết:

    IUserRepository

không biết:

    UserRepository

------------------------------------------------------------------------

## Infrastructure

Reference:

    Infrastructure
        ↓
    Application
và
    Infrastructure
        ↓
    Domain

Chứa phần triển khai:

-   EF Core
-   DbContext
-   Repository
-   Authentication
-   External service

Ví dụ:

    Persistence

    AppDbContext.cs

    Repositories

    UserRepository.cs
    GenericRepository.cs

    Authentication

    JwtService.cs

Infrastructure implement interface của Application.

Ví dụ:

Application:

``` csharp
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}
```

Infrastructure:

``` csharp
public class UserRepository : IUserRepository
{
}
```

------------------------------------------------------------------------

## API

Reference:

    API
     ↓
    Application
và
    API
     ↓
    Infrastructure

Chứa:

-   Controller
-   Middleware
-   Program.cs
-   Configuration

Ví dụ:

    Controllers/AuthController.cs

    Program.cs

    Middlewares
        GlobalExceptionHandler.cs

------------------------------------------------------------------------

# 4. Modular Monolith Structure

Hiện tại module:

    Application

    Modules

    ├── Auth

    ├── User

Mỗi module nên tự chứa:

    ModuleName

    ├── DTOs

    ├── Interfaces

    ├── Services

    ├── Validators

    ├── Mappings

Ví dụ module Course:

    Modules

    └── Course

        ├── DTOs

        │   ├── CreateCourseRequest.cs
        │   ├── UpdateCourseRequest.cs
        │   └── CourseResponse.cs


        ├── Interfaces

        │   ├── ICourseService.cs
        │   └── ICourseRepository.cs


        ├── Services

        │   └── CourseService.cs

------------------------------------------------------------------------

# 5. Tạo Module mới: Course

Ví dụ nghiệp vụ:

Quản lý khóa học:

    Course

    Id
    Name
    Description
    Price
    CreatedDate

------------------------------------------------------------------------

# Bước 1: Tạo Entity

Location:

    Domain/Entities/Course.cs

Ví dụ:

``` csharp
public class Course : BaseEntity
{
    public string Name {get;set;}

    public string Description {get;set;}

    public decimal Price {get;set;}
}
```

------------------------------------------------------------------------

# Bước 2: Tạo DTO

Location:

    Application/Modules/Course/DTOs

Bao gồm:

## CreateCourseRequest

Input từ client:

``` csharp
public class CreateCourseRequest
{
    public string Name {get;set;}
    public decimal Price {get;set;}
}
```

------------------------------------------------------------------------

## CourseResponse

Output:

``` csharp
public class CourseResponse
{
    public Guid Id {get;set;}
    public string Name {get;set;}
}
```

Không trả Entity trực tiếp ra API.

------------------------------------------------------------------------

# Bước 3: Repository Interface

Location:

    Application/Modules/Course/Interfaces/ICourseRepository.cs

``` csharp
public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);

    Task AddAsync(Course course);

    Task<IEnumerable<Course>> GetAllAsync();
}
```

Application chỉ định nghĩa contract.

------------------------------------------------------------------------

# Bước 4: Repository Implementation

Location:

    Infrastructure/Persistence/Repositories/CourseRepository.cs

``` csharp
public class CourseRepository 
    : GenericRepository<Course>,
      ICourseRepository
{

}
```

Nơi này làm việc với:

    DbContext
    EF Core
    SQL

------------------------------------------------------------------------

# Bước 5: Service Interface

Location:

    Application/Modules/Course/Interfaces/ICourseService.cs

``` csharp
public interface ICourseService
{
    Task<CourseResponse> CreateAsync(
        CreateCourseRequest request);
}
```

------------------------------------------------------------------------

# Bước 6: Service Implementation

Location:

    Application/Modules/Course/Services/CourseService.cs

Nhiệm vụ:

-   Validate business rule
-   Gọi repository
-   Mapping DTO

Flow:

    Controller

    ↓

    CourseService

    ↓

    ICourseRepository

    ↓

    Database

------------------------------------------------------------------------

# Bước 7: Controller

Location:

    API/Controllers/CourseController.cs

Controller chỉ xử lý HTTP.

Không viết:

    SQL
    Business logic
    Mapping phức tạp

------------------------------------------------------------------------

# 6. Flow một request hoàn chỉnh

Ví dụ:

POST

    /api/courses

Request:

    CreateCourseRequest

Flow:

    Client

     ↓

    CourseController

     ↓

    ICourseService

     ↓

    CourseService

     ↓

    ICourseRepository

     ↓

    EF Core

     ↓

    SQL Database

Response:

    CourseResponse

------------------------------------------------------------------------

# 7. Dependency Injection

Các module phải đăng ký dependency tại:

    DependencyInjection.cs

Ví dụ:

Application:

``` csharp
services.AddScoped<ICourseService, CourseService>();
```

Infrastructure:

``` csharp
services.AddScoped<ICourseRepository, CourseRepository>();
```

------------------------------------------------------------------------

# 8. Naming Convention

Entity:

    Course.cs

DTO:

    CreateCourseRequest.cs

    UpdateCourseRequest.cs

    CourseResponse.cs

Interface:

    ICourseService.cs

    ICourseRepository.cs

Implementation:

    CourseService.cs

    CourseRepository.cs

------------------------------------------------------------------------

# 9. Checklist khi tạo Module mới

Ví dụ Course:

## Domain

\[x\] Course Entity

\[x\] Enum nếu cần

## Application

\[x\] DTO

\[x\] Service Interface

\[x\] Repository Interface

\[x\] Service Implementation

\[x\] Validation

## Infrastructure

\[x\] Repository Implementation

\[x\] EF Configuration

\[x\] Migration

## API

\[x\] Controller

\[x\] Route

\[x\] Swagger test

------------------------------------------------------------------------

# 10. Quy tắc không phá kiến trúc

Không làm:

    Controller
        ↓
    DbContext

Không làm:

    Controller
        ↓
    Repository trực tiếp

Không trả:

    Entity

ra API.

Đúng:

    Controller

    ↓

    Service

    ↓

    Repository Interface

    ↓

    Repository Implementation

    ↓

    Database

------------------------------------------------------------------------

# 11. Nhận xét kiến trúc hiện tại

Điểm tốt:

-   Đã phân tách Domain/Application/Infrastructure/API.
-   Có Generic Repository.
-   Có UnitOfWork.
-   Có JWT Authentication.
-   Có Module folder.

Điểm cần tiếp tục hoàn thiện:

-   Mỗi module nên chứa đầy đủ DTO + Service + Repository interface.
-   Có thể bổ sung Mapping layer (AutoMapper hoặc Mapster).
-   Có thể bổ sung FluentValidation.
-   Có thể tách Command/Query nếu hệ thống lớn.
-   Nên chuẩn hóa DI registration theo từng module.

------------------------------------------------------------------------

Tài liệu này dùng làm guideline khi phát triển thêm module mới trong
FirstAPIProject.
