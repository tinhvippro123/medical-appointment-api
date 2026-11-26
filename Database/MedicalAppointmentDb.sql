/*
    Cơ sở dữ liệu : MedicalAppointmentDb (Hệ thống Đặt lịch Khám bệnh)
    Hệ quản trị    : Microsoft SQL Server
    Cách chạy      : Mở file bằng SQL Server Management Studio (SSMS) và nhấn F5.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'MedicalAppointmentDb') IS NULL
BEGIN
    CREATE DATABASE [MedicalAppointmentDb];
END
GO

USE [MedicalAppointmentDb];
GO

-- ============================================================================
-- PHẦN 1: TẠO BẢNG, KHÓA CHÍNH, KHÓA NGOẠI VÀ CHỈ MỤC
-- ============================================================================

-- 1. Bảng Vai trò (Roles)
IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Roles]
    (
        [Id]          INT            IDENTITY(1,1) NOT NULL,
        [Key]         NVARCHAR(50)   NOT NULL,
        [Name]        NVARCHAR(100)  NOT NULL,
        [Description] NVARCHAR(MAX)  NULL,
        [CreatedAt]   DATETIME2      NOT NULL CONSTRAINT [DF_Roles_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]   DATETIME2      NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Roles_Key] ON [dbo].[Roles] ([Key]);
END
GO

-- 2. Bảng Người dùng (Users)
IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id]         INT            IDENTITY(1,1) NOT NULL,
        [FullName]   NVARCHAR(100)  NOT NULL,
        [Email]      NVARCHAR(256)  NULL,
        [Phone]      NVARCHAR(20)   NOT NULL CONSTRAINT [DF_Users_Phone] DEFAULT (N''),
        [Password]   NVARCHAR(MAX)  NOT NULL,
        [AvatarUrl]  NVARCHAR(MAX)  NULL,
        [RoleId]     INT            NOT NULL,
        [GoogleId]   NVARCHAR(MAX)  NULL,
        [FacebookId] NVARCHAR(MAX)  NULL,
        [ZaloId]     NVARCHAR(MAX)  NULL,
        [IsActive]   BIT            NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT (1),
        [CreatedAt]  DATETIME2      NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]  DATETIME2      NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id])
    );

    CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]) WHERE [Email] IS NOT NULL;
    CREATE UNIQUE INDEX [IX_Users_Phone] ON [dbo].[Users] ([Phone]);
    CREATE INDEX [IX_Users_RoleId] ON [dbo].[Users] ([RoleId]);
END
GO

-- 3. Bảng Chuyên khoa (Departments)
IF OBJECT_ID(N'[dbo].[Departments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Departments]
    (
        [Id]              INT             IDENTITY(1,1) NOT NULL,
        [Name]            NVARCHAR(200)   NOT NULL,
        [Description]     NVARCHAR(MAX)   NULL,
        [ImageUrl]        NVARCHAR(MAX)   NULL,
        [ConsultationFee] DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_Departments_ConsultationFee] DEFAULT (50000),
        [IsActive]        BIT             NOT NULL CONSTRAINT [DF_Departments_IsActive] DEFAULT (1),
        [CreatedAt]       DATETIME2       NOT NULL CONSTRAINT [DF_Departments_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]       DATETIME2       NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Departments_Name] ON [dbo].[Departments] ([Name]);
END
GO

-- 4. Bảng Phòng khám (Rooms)
IF OBJECT_ID(N'[dbo].[Rooms]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Rooms]
    (
        [Id]           INT           IDENTITY(1,1) NOT NULL,
        [RoomNumber]   NVARCHAR(20)  NOT NULL,
        [Floor]        INT           NOT NULL,
        [DepartmentId] INT           NOT NULL,
        [IsActive]     BIT           NOT NULL CONSTRAINT [DF_Rooms_IsActive] DEFAULT (1),
        [CreatedAt]    DATETIME2     NOT NULL CONSTRAINT [DF_Rooms_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]    DATETIME2     NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Rooms_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments] ([Id])
    );

    CREATE INDEX [IX_Rooms_DepartmentId] ON [dbo].[Rooms] ([DepartmentId]);
END
GO

-- 5. Bảng Bác sĩ (Doctors)
IF OBJECT_ID(N'[dbo].[Doctors]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Doctors]
    (
        [Id]              INT            IDENTITY(1,1) NOT NULL,
        [FullName]        NVARCHAR(100)  NOT NULL,
        [Qualification]   NVARCHAR(100)  NULL,
        [Email]           NVARCHAR(MAX)  NULL,
        [Phone]           NVARCHAR(MAX)  NULL,
        [DepartmentId]    INT            NOT NULL,
        [RoomId]          INT            NULL,
        [Description]     NVARCHAR(MAX)  NULL,
        [AvatarUrl]       NVARCHAR(MAX)  NULL,
        [ExperienceYears] INT            NOT NULL,
        [IsActive]        BIT            NOT NULL CONSTRAINT [DF_Doctors_IsActive] DEFAULT (1),
        [CreatedAt]       DATETIME2      NOT NULL CONSTRAINT [DF_Doctors_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]       DATETIME2      NULL,
        CONSTRAINT [PK_Doctors] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Doctors_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments] ([Id]),
        CONSTRAINT [FK_Doctors_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id]) ON DELETE SET NULL
    );

    CREATE INDEX [IX_Doctors_DepartmentId] ON [dbo].[Doctors] ([DepartmentId]);
    CREATE INDEX [IX_Doctors_RoomId] ON [dbo].[Doctors] ([RoomId]);
END
GO

-- 6. Bảng Lịch làm việc của Bác sĩ (DoctorSchedules)
IF OBJECT_ID(N'[dbo].[DoctorSchedules]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DoctorSchedules]
    (
        [Id]          INT        IDENTITY(1,1) NOT NULL,
        [DoctorId]    INT        NOT NULL,
        [DayOfWeek]   INT        NOT NULL, -- 0 = Chủ nhật, 1 = Thứ 2 .. 6 = Thứ 7
        [StartTime]   TIME       NOT NULL,
        [EndTime]     TIME       NOT NULL,
        [MaxPatients] INT        NOT NULL CONSTRAINT [DF_DoctorSchedules_MaxPatients] DEFAULT (20),
        [CreatedAt]   DATETIME2  NOT NULL CONSTRAINT [DF_DoctorSchedules_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]   DATETIME2  NULL,
        CONSTRAINT [PK_DoctorSchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DoctorSchedules_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [dbo].[Doctors] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_DoctorSchedules_DoctorId] ON [dbo].[DoctorSchedules] ([DoctorId]);
END
GO

-- 7. Bảng Ngày nghỉ phép của Bác sĩ (DoctorDayOffs)
IF OBJECT_ID(N'[dbo].[DoctorDayOffs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DoctorDayOffs]
    (
        [Id]        INT            IDENTITY(1,1) NOT NULL,
        [DoctorId]  INT            NOT NULL,
        [Date]      DATE           NOT NULL,
        [Reason]    NVARCHAR(200)  NULL,
        [CreatedAt] DATETIME2      NOT NULL CONSTRAINT [DF_DoctorDayOffs_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2      NULL,
        CONSTRAINT [PK_DoctorDayOffs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DoctorDayOffs_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [dbo].[Doctors] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_DoctorDayOffs_DoctorId] ON [dbo].[DoctorDayOffs] ([DoctorId]);
END
GO

-- 8. Bảng Hồ sơ Bệnh nhân (PatientProfiles)
IF OBJECT_ID(N'[dbo].[PatientProfiles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PatientProfiles]
    (
        [Id]                    INT            IDENTITY(1,1) NOT NULL,
        [UserId]                INT            NOT NULL,
        [FullName]              NVARCHAR(100)  NOT NULL,
        [DateOfBirth]           DATETIME2      NOT NULL,
        [Gender]                NVARCHAR(10)   NOT NULL,
        [Ethnicity]             NVARCHAR(50)   NULL,
        [Occupation]            NVARCHAR(100)  NULL,
        [Phone]                 NVARCHAR(MAX)  NULL,
        [Address]               NVARCHAR(MAX)  NULL,
        [IdentityCardNumber]    NVARCHAR(20)   NULL,
        [HealthInsuranceNumber] NVARCHAR(50)   NULL,
        [Relationship]          NVARCHAR(50)   NOT NULL,
        [CreatedAt]             DATETIME2      NOT NULL CONSTRAINT [DF_PatientProfiles_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]             DATETIME2      NULL,
        CONSTRAINT [PK_PatientProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PatientProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_PatientProfiles_UserId] ON [dbo].[PatientProfiles] ([UserId]);
END
GO

-- 9. Bảng Giỏ đặt lịch (CartItems)
IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CartItems]
    (
        [Id]               INT            IDENTITY(1,1) NOT NULL,
        [UserId]           INT            NOT NULL,
        [DoctorId]         INT            NOT NULL,
        [PatientProfileId] INT            NOT NULL,
        [AppointmentDate]  DATETIME2      NOT NULL,
        [TimeSlot]         NVARCHAR(20)   NOT NULL,
        [Note]             NVARCHAR(MAX)  NULL,
        [CreatedAt]        DATETIME2      NOT NULL CONSTRAINT [DF_CartItems_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]        DATETIME2      NULL,
        CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItems_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [dbo].[Doctors] ([Id]),
        CONSTRAINT [FK_CartItems_PatientProfiles_PatientProfileId] FOREIGN KEY ([PatientProfileId]) REFERENCES [dbo].[PatientProfiles] ([Id])
    );

    CREATE INDEX [IX_CartItems_UserId] ON [dbo].[CartItems] ([UserId]);
    CREATE INDEX [IX_CartItems_DoctorId] ON [dbo].[CartItems] ([DoctorId]);
    CREATE INDEX [IX_CartItems_PatientProfileId] ON [dbo].[CartItems] ([PatientProfileId]);
END
GO

-- 10. Bảng Đơn đặt lịch (Orders)
IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Orders]
    (
        [Id]                 INT             IDENTITY(1,1) NOT NULL,
        [UserId]             INT             NOT NULL,
        [TotalAmount]        DECIMAL(18, 2)  NOT NULL,
        [Status]             NVARCHAR(20)    NOT NULL,
        [CancellationReason] NVARCHAR(500)   NULL,
        [PaymentMethod]      NVARCHAR(20)    NOT NULL,
        [PaymentStatus]      NVARCHAR(20)    NOT NULL,
        [TransactionId]      NVARCHAR(100)   NULL,
        [CreatedAt]          DATETIME2       NOT NULL CONSTRAINT [DF_Orders_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]          DATETIME2       NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
    );

    CREATE INDEX [IX_Orders_UserId] ON [dbo].[Orders] ([UserId]);
END
GO

-- 11. Bảng Chi tiết Đơn đặt lịch (OrderDetails)
IF OBJECT_ID(N'[dbo].[OrderDetails]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderDetails]
    (
        [Id]               INT             IDENTITY(1,1) NOT NULL,
        [OrderId]          INT             NOT NULL,
        [DoctorId]         INT             NOT NULL,
        [PatientProfileId] INT             NOT NULL,
        [AppointmentDate]  DATETIME2       NOT NULL,
        [TimeSlot]         NVARCHAR(20)    NOT NULL,
        [QueueNumber]      INT             NOT NULL,
        [Fee]              DECIMAL(18, 2)  NOT NULL,
        [Note]             NVARCHAR(MAX)   NULL,
        [Status]           NVARCHAR(20)    NOT NULL,
        [CreatedAt]        DATETIME2       NOT NULL CONSTRAINT [DF_OrderDetails_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt]        DATETIME2       NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDetails_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [dbo].[Doctors] ([Id]),
        CONSTRAINT [FK_OrderDetails_PatientProfiles_PatientProfileId] FOREIGN KEY ([PatientProfileId]) REFERENCES [dbo].[PatientProfiles] ([Id])
    );

    CREATE INDEX [IX_OrderDetails_OrderId] ON [dbo].[OrderDetails] ([OrderId]);
    CREATE INDEX [IX_OrderDetails_DoctorId] ON [dbo].[OrderDetails] ([DoctorId]);
    CREATE INDEX [IX_OrderDetails_PatientProfileId] ON [dbo].[OrderDetails] ([PatientProfileId]);
END
GO

-- ============================================================================
-- PHẦN 2: DỮ LIỆU MẪU BAN ĐẦU (TƯƠNG ỨNG DbSeeder)
-- ============================================================================

-- 1. Vai trò mặc định
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles])
BEGIN
    SET IDENTITY_INSERT [dbo].[Roles] ON;
    INSERT INTO [dbo].[Roles] ([Id], [Key], [Name], [Description], [CreatedAt])
    VALUES
        (1, N'ADMIN',   N'Quản trị viên', N'Toàn quyền hệ thống',  '2026-09-25T00:00:00'),
        (2, N'PATIENT', N'Bệnh nhân',     N'Người dùng đặt lịch', '2026-09-25T00:00:00');
    SET IDENTITY_INSERT [dbo].[Roles] OFF;
END
GO

-- 2. Tài khoản Admin & Bệnh nhân mẫu (mật khẩu mặc định: 123456 - đã hash BCrypt)
DECLARE @AdminRoleId   INT = (SELECT TOP 1 [Id] FROM [dbo].[Roles] WHERE [Key] = N'ADMIN');
DECLARE @PatientRoleId INT = (SELECT TOP 1 [Id] FROM [dbo].[Roles] WHERE [Key] = N'PATIENT');

IF @AdminRoleId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [RoleId] = @AdminRoleId AND [IsActive] = 1)
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Phone] = N'0987654321')
BEGIN
    INSERT INTO [dbo].[Users] ([FullName], [Email], [Phone], [Password], [RoleId], [IsActive], [CreatedAt])
    VALUES
        (N'Quản trị viên', N'admin@ytehiepphuoc.vn', N'0987654321', N'$2a$11$u67TsHzwfoyrNEGpT6sy9O56sRdCsPEbImw2UCuF.RH40SyfFU5mK', @AdminRoleId, 1, GETUTCDATE());
END

IF @PatientRoleId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [RoleId] = @PatientRoleId)
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Phone] = N'0912345678')
BEGIN
    INSERT INTO [dbo].[Users] ([FullName], [Email], [Phone], [Password], [RoleId], [IsActive], [CreatedAt])
    VALUES
        (N'Nguyễn Minh An', N'benhnhan@ytehiepphuoc.vn', N'0912345678', N'$2a$11$u67TsHzwfoyrNEGpT6sy9O56sRdCsPEbImw2UCuF.RH40SyfFU5mK', @PatientRoleId, 1, GETUTCDATE());

    DECLARE @NewPatientUserId INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[PatientProfiles]
        ([UserId], [FullName], [DateOfBirth], [Gender], [Ethnicity], [Occupation], [Address], [IdentityCardNumber], [HealthInsuranceNumber], [Relationship], [CreatedAt])
    VALUES
        (@NewPatientUserId, N'Nguyễn Minh An', '1998-05-15', N'Nam', N'Kinh', N'Nhân viên văn phòng', N'Xã Hiệp Phước, Huyện Nhà Bè, TP.HCM', N'079098012345', N'DN4791234567890', N'Bản thân', GETUTCDATE());
END
GO

-- 3. Chuyên khoa, Phòng khám, Bác sĩ và Lịch làm việc mẫu
IF NOT EXISTS (SELECT 1 FROM [dbo].[Departments])
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Rooms])
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Doctors])
BEGIN
    SET IDENTITY_INSERT [dbo].[Departments] ON;
    INSERT INTO [dbo].[Departments] ([Id], [Name], [Description], [ConsultationFee], [ImageUrl], [IsActive], [CreatedAt])
    VALUES
        (1, N'Nội tổng quát', N'Khám, tư vấn và điều trị các bệnh lý nội khoa tổng quát, tầm soát sức khỏe định kỳ.', 150000, N'/images/departments/dept-noi-tong-quat.jpg', 1, GETUTCDATE()),
        (2, N'Tim mạch',      N'Chẩn đoán và điều trị tăng huyết áp, rối loạn nhịp tim, bệnh mạch vành và suy tim.',    220000, N'/images/departments/dept-tim-mach.jpg',      1, GETUTCDATE()),
        (3, N'Nhi khoa',      N'Chăm sóc sức khỏe toàn diện cho trẻ sơ sinh, trẻ nhỏ và tư vấn dinh dưỡng, tiêm chủng.', 160000, N'/images/departments/dept-nhi-khoa.jpg',      1, GETUTCDATE()),
        (4, N'Tai Mũi Họng',  N'Nội soi và điều trị viêm mũi xoang, viêm họng, viêm tai giữa và rối loạn tiền đình.',    180000, N'/images/departments/dept-tai-mui-hong.jpg',  1, GETUTCDATE()),
        (5, N'Da liễu',       N'Khám và điều trị các bệnh lý da, dị ứng, viêm da cơ địa, mụn và chăm sóc da chuyên sâu.', 180000, N'/images/departments/dept-da-lieu.jpg',       1, GETUTCDATE()),
        (6, N'Sản Phụ khoa',  N'Khám thai định kỳ, siêu âm sản khoa và chăm sóc sức khỏe sinh sản nữ giới.',               200000, N'/images/departments/dept-san-phu-khoa.jpg',  1, GETUTCDATE());
    SET IDENTITY_INSERT [dbo].[Departments] OFF;

    SET IDENTITY_INSERT [dbo].[Rooms] ON;
    INSERT INTO [dbo].[Rooms] ([Id], [RoomNumber], [Floor], [DepartmentId], [IsActive], [CreatedAt])
    VALUES
        (1,  N'P.101', 1, 1, 1, GETUTCDATE()),
        (2,  N'P.102', 1, 1, 1, GETUTCDATE()),
        (3,  N'P.103', 1, 2, 1, GETUTCDATE()),
        (4,  N'P.104', 1, 2, 1, GETUTCDATE()),
        (5,  N'P.201', 2, 3, 1, GETUTCDATE()),
        (6,  N'P.202', 2, 3, 1, GETUTCDATE()),
        (7,  N'P.203', 2, 4, 1, GETUTCDATE()),
        (8,  N'P.204', 2, 4, 1, GETUTCDATE()),
        (9,  N'P.301', 3, 5, 1, GETUTCDATE()),
        (10, N'P.302', 3, 5, 1, GETUTCDATE()),
        (11, N'P.303', 3, 6, 1, GETUTCDATE()),
        (12, N'P.304', 3, 6, 1, GETUTCDATE());
    SET IDENTITY_INSERT [dbo].[Rooms] OFF;

    SET IDENTITY_INSERT [dbo].[Doctors] ON;
    INSERT INTO [dbo].[Doctors] ([Id], [FullName], [Qualification], [Email], [Phone], [DepartmentId], [RoomId], [ExperienceYears], [AvatarUrl], [Description], [IsActive], [CreatedAt])
    VALUES
        (1,  N'Nguyễn Minh Quân',  N'TS.BS',     N'nmquan@ytehiepphuoc.vn',  N'0901000001', 1, 1,  18, N'/images/doctors/doc-01.jpg', N'Trưởng khoa Nội tổng quát, chuyên sâu bệnh lý chuyển hóa, đái tháo đường và tiêu hóa.',      1, GETUTCDATE()),
        (2,  N'Trần Thị Thanh Hà', N'BS.CKII',   N'tttha@ytehiepphuoc.vn',   N'0901000002', 1, 1,  15, N'/images/doctors/doc-07.jpg', N'Chuyên khám tầm soát sức khỏe tổng quát, bệnh lý hô hấp và nội tiết.',                       1, GETUTCDATE()),
        (3,  N'Lê Hoàng Phúc',     N'ThS.BS',    N'lhphuc@ytehiepphuoc.vn',  N'0901000003', 1, 2,  11, N'/images/doctors/doc-02.jpg', N'Bác sĩ nội khoa giàu kinh nghiệm điều trị bệnh dạ dày, gan mật và tăng huyết áp.',            1, GETUTCDATE()),
        (4,  N'Phạm Ngọc Mai',     N'BS.CKI',    N'pnmai@ytehiepphuoc.vn',   N'0901000004', 1, 2,  9,  N'/images/doctors/doc-08.jpg', N'Tận tâm tư vấn chế độ dinh dưỡng và điều trị bệnh mạn tính cho người cao tuổi.',              1, GETUTCDATE()),
        (5,  N'Võ Quốc Bảo',       N'BS.CKII',   N'vqbao@ytehiepphuoc.vn',   N'0901000005', 1, 1,  20, N'/images/doctors/doc-03.jpg', N'Hơn 20 năm công tác tại các bệnh viện tuyến đầu về Nội tổng quát và Cơ xương khớp.',         1, GETUTCDATE()),
        (6,  N'Đặng Thu Thảo',     N'ThS.BS',    N'dtthao@ytehiepphuoc.vn',  N'0901000006', 1, 2,  7,  N'/images/doctors/doc-09.jpg', N'Chuyên khám sức khỏe định kỳ, tư vấn phòng bệnh và điều trị rối loạn giấc ngủ.',             1, GETUTCDATE()),
        (7,  N'Bùi Đức Anh',       N'BS',        N'bdanh@ytehiepphuoc.vn',   N'0901000007', 1, 2,  6,  N'/images/doctors/doc-04.jpg', N'Bác sĩ trẻ nhiệt huyết, chuyên theo dõi sức khỏe ban đầu và khám bảo hiểm y tế.',            1, GETUTCDATE()),
        (8,  N'Huỳnh Gia Huy',     N'PGS.TS.BS', N'hghuy@ytehiepphuoc.vn',   N'0901000008', 2, 3,  22, N'/images/doctors/doc-05.jpg', N'Chuyên gia đầu ngành Tim mạch, siêu âm tim và điều trị rối loạn nhịp tim.',                  1, GETUTCDATE()),
        (9,  N'Nguyễn Thị Kim Ngân', N'BS.CKII', N'ntkngan@ytehiepphuoc.vn', N'0901000009', 3, 5,  14, N'/images/doctors/doc-10.jpg', N'Bác sĩ Nhi khoa chuyên điều trị hô hấp nhi, dị ứng và tư vấn tăng trưởng cho trẻ.',          1, GETUTCDATE()),
        (10, N'Trương Tấn Tài',    N'ThS.BS',    N'tttai@ytehiepphuoc.vn',   N'0901000010', 4, 7,  10, N'/images/doctors/doc-06.jpg', N'Chuyên nội soi Tai Mũi Họng không đau và điều trị viêm xoang mạn tính.',                     1, GETUTCDATE()),
        (11, N'Lý Mỹ Linh',        N'BS.CKI',    N'lmlinh@ytehiepphuoc.vn',  N'0901000011', 5, 9,  8,  N'/images/doctors/doc-11.jpg', N'Chuyên điều trị mụn trứng cá, viêm da dị ứng và phục hồi da thẩm mỹ y khoa.',               1, GETUTCDATE()),
        (12, N'Đỗ Thị Hồng Nhung', N'BS.CKII',   N'dthnhung@ytehiepphuoc.vn', N'0901000012', 6, 11, 17, N'/images/doctors/doc-12.jpg', N'Chuyên theo dõi thai kỳ nguy cơ cao, siêu âm hình thái và khám phụ khoa định kỳ.',           1, GETUTCDATE());
    SET IDENTITY_INSERT [dbo].[Doctors] OFF;

    -- Tạo lịch khám Thứ 2 -> Chủ nhật (ca sáng 07:00-11:30, ca chiều 13:30-17:00) cho 12 bác sĩ
    DECLARE @DocId INT = 1;
    DECLARE @Day   INT;
    WHILE @DocId <= 12
    BEGIN
        SET @Day = 0;
        WHILE @Day <= 6
        BEGIN
            INSERT INTO [dbo].[DoctorSchedules] ([DoctorId], [DayOfWeek], [StartTime], [EndTime], [MaxPatients], [CreatedAt])
            VALUES
                (@DocId, @Day, '07:00:00', '11:30:00', 20, GETUTCDATE()),
                (@DocId, @Day, '13:30:00', '17:00:00', 20, GETUTCDATE());
            SET @Day += 1;
        END
        SET @DocId += 1;
    END
END
GO
