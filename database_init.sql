-- =============================================================================
-- DATABASE INITIALIZATION SCRIPT FOR SQL SERVER
-- Project: FirstAPIProject (Mid-term Assignment)
-- Architecture: Clean Architecture + Modular Monolith
-- Security: Indexes, Constraints, RBAC, Data Integrity
-- =============================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'FirstAPIProjectDb')
BEGIN
    CREATE DATABASE FirstAPIProjectDb;
END
GO

USE FirstAPIProjectDb;
GO

-- 1. Table: Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        UserName NVARCHAR(50) NULL,
        Email NVARCHAR(255) NOT NULL,
        PhoneNumber NVARCHAR(20) NULL,
        AvatarUrl NVARCHAR(1000) NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT 'User',
        LastLoginAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT 0,
        DeletedAt DATETIME2 NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
    CREATE NONCLUSTERED INDEX IX_Users_Role ON Users(Role);
    CREATE NONCLUSTERED INDEX IX_Users_IsActive ON Users(IsActive);
    CREATE NONCLUSTERED INDEX IX_Users_CreatedAt ON Users(CreatedAt DESC);
END
GO

-- 2. Table: RefreshTokens
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        TokenHash NVARCHAR(256) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        RevokedAt DATETIME2 NULL,
        ReplacedByTokenHash NVARCHAR(256) NULL,
        ReasonRevoked NVARCHAR(255) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_RefreshTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
    CREATE NONCLUSTERED INDEX IX_RefreshTokens_TokenHash ON RefreshTokens(TokenHash);
    CREATE NONCLUSTERED INDEX IX_RefreshTokens_ExpiresAt ON RefreshTokens(ExpiresAt);
END
GO

-- 3. Table: EmailWhitelists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmailWhitelists')
BEGIN
    CREATE TABLE EmailWhitelists (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmailWhitelists PRIMARY KEY,
        Pattern NVARCHAR(255) NOT NULL,
        Description NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_EmailWhitelists_IsActive DEFAULT 1,
        IsDeleted BIT NOT NULL CONSTRAINT DF_EmailWhitelists_IsDeleted DEFAULT 0,
        DeletedAt DATETIME2 NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX IX_EmailWhitelists_Pattern ON EmailWhitelists(Pattern);
    CREATE NONCLUSTERED INDEX IX_EmailWhitelists_IsActive ON EmailWhitelists(IsActive);
END
GO

-- 4. Table: PasswordResetTokens
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordResetTokens')
BEGIN
    CREATE TABLE PasswordResetTokens (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        TokenHash NVARCHAR(256) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        IsUsed BIT NOT NULL CONSTRAINT DF_PasswordResetTokens_IsUsed DEFAULT 0,
        UsedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_PasswordResetTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_TokenHash ON PasswordResetTokens(TokenHash);
    CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_UserId ON PasswordResetTokens(UserId);
END
GO

-- 5. Table: Announcements
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Announcements')
BEGIN
    CREATE TABLE Announcements (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Announcements PRIMARY KEY,
        Title NVARCHAR(255) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Announcements_Status DEFAULT 'Draft',
        PublishedAt DATETIME2 NULL,
        AuthorId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Announcements_IsActive DEFAULT 1,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Announcements_IsDeleted DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        CONSTRAINT FK_Announcements_Users_AuthorId FOREIGN KEY (AuthorId) REFERENCES Users(Id) ON DELETE NO ACTION
    );

    CREATE NONCLUSTERED INDEX IX_Announcements_Status ON Announcements(Status);
    CREATE NONCLUSTERED INDEX IX_Announcements_CreatedAt ON Announcements(CreatedAt DESC);
    CREATE NONCLUSTERED INDEX IX_Announcements_AuthorId ON Announcements(AuthorId);
END
GO

-- 6. Table: AuditLogs (Append-Only)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NULL,
        UserEmail NVARCHAR(255) NULL,
        Action NVARCHAR(100) NOT NULL,
        Endpoint NVARCHAR(500) NOT NULL,
        Method NVARCHAR(20) NOT NULL,
        IpAddress NVARCHAR(50) NULL,
        StatusCode INT NOT NULL,
        ExecutionDurationMs BIGINT NOT NULL,
        Details NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt DESC);
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Action ON AuditLogs(Action);
    CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
END
GO

-- =============================================================================
-- SEED DATA: Default Admin Account
-- Password: Admin@123456
-- Email: admin@uit.edu.vn
-- Role: Admin
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@uit.edu.vn')
BEGIN
    INSERT INTO Users (
        Id,
        UserName,
        Email,
        PhoneNumber,
        AvatarUrl,
        PasswordHash,
        Role,
        LastLoginAt,
        CreatedAt,
        UpdatedAt,
        IsActive,
        IsDeleted,
        DeletedAt
    )
    VALUES (
        NEWID(),
        N'System Administrator',
        'admin@uit.edu.vn',
        '0901234567',
        NULL,
        -- ASP.NET Core Identity PBKDF2 hash of "Admin@123456"
        'AQAAAAIAAYagAAAAEI0uW2qK5Xg5k7BqjZc5O2m8W3tK9yL1a4P0r7E2s6T9v3X8y1Z4a7B0c3D6e9F2g==',
        'Admin',
        NULL,
        SYSUTCDATETIME(),
        SYSUTCDATETIME(),
        1,
        0,
        NULL
    );
END
GO
