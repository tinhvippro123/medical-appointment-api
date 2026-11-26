SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID('MedicalAppointmentDb') IS NULL
    CREATE DATABASE MedicalAppointmentDb;
GO

USE MedicalAppointmentDb;
GO

-- 1. Bảng Roles
IF OBJECT_ID('Roles', 'U') IS NULL
BEGIN
    CREATE TABLE Roles (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        [Key]       NVARCHAR(50)  NOT NULL,
        Name        NVARCHAR(100) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt   DATETIME2     NULL
    );

    CREATE UNIQUE INDEX IX_Roles_Key ON Roles ([Key]);
END
GO

-- 2. Bảng Users
IF OBJECT_ID('Users', 'U') IS NULL
BEGIN
    CREATE TABLE Users (
        Id         INT IDENTITY(1,1) PRIMARY KEY,
        FullName   NVARCHAR(100) NOT NULL,
        Email      NVARCHAR(256) NULL,
        Phone      NVARCHAR(20)  NOT NULL DEFAULT '',
        Password   NVARCHAR(MAX) NOT NULL,
        AvatarUrl  NVARCHAR(MAX) NULL,
        RoleId     INT           NOT NULL,
        GoogleId   NVARCHAR(MAX) NULL,
        FacebookId NVARCHAR(MAX) NULL,
        ZaloId     NVARCHAR(MAX) NULL,
        IsActive   BIT           NOT NULL DEFAULT 1,
        CreatedAt  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt  DATETIME2     NULL,
        CONSTRAINT FK_Users_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES Roles(Id)
    );

    CREATE UNIQUE INDEX IX_Users_Email ON Users (Email) WHERE Email IS NOT NULL;
    CREATE UNIQUE INDEX IX_Users_Phone ON Users (Phone);
    CREATE INDEX IX_Users_RoleId ON Users (RoleId);
END
GO

-- 3. Bảng Departments
IF OBJECT_ID('Departments', 'U') IS NULL
BEGIN
    CREATE TABLE Departments (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Name            NVARCHAR(200)  NOT NULL,
        Description     NVARCHAR(MAX)  NULL,
        ImageUrl        NVARCHAR(MAX)  NULL,
        ConsultationFee DECIMAL(18, 2) NOT NULL DEFAULT 50000,
        IsActive        BIT            NOT NULL DEFAULT 1,
        CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt       DATETIME2      NULL
    );

    CREATE UNIQUE INDEX IX_Departments_Name ON Departments (Name);
END
GO

-- 4. Bảng Rooms
IF OBJECT_ID('Rooms', 'U') IS NULL
BEGIN
    CREATE TABLE Rooms (
        Id           INT IDENTITY(1,1) PRIMARY KEY,
        RoomNumber   NVARCHAR(20) NOT NULL,
        Floor        INT          NOT NULL,
        DepartmentId INT          NOT NULL,
        IsActive     BIT          NOT NULL DEFAULT 1,
        CreatedAt    DATETIME2    NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt    DATETIME2    NULL,
        CONSTRAINT FK_Rooms_Departments_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
    );

    CREATE INDEX IX_Rooms_DepartmentId ON Rooms (DepartmentId);
END
GO

-- 5. Bảng Doctors
IF OBJECT_ID('Doctors', 'U') IS NULL
BEGIN
    CREATE TABLE Doctors (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        FullName        NVARCHAR(100) NOT NULL,
        Qualification   NVARCHAR(100) NULL,
        Email           NVARCHAR(MAX) NULL,
        Phone           NVARCHAR(MAX) NULL,
        DepartmentId    INT           NOT NULL,
        RoomId          INT           NULL,
        Description     NVARCHAR(MAX) NULL,
        AvatarUrl       NVARCHAR(MAX) NULL,
        ExperienceYears INT           NOT NULL,
        IsActive        BIT           NOT NULL DEFAULT 1,
        CreatedAt       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt       DATETIME2     NULL,
        CONSTRAINT FK_Doctors_Departments_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
        CONSTRAINT FK_Doctors_Rooms_RoomId FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_Doctors_DepartmentId ON Doctors (DepartmentId);
    CREATE INDEX IX_Doctors_RoomId ON Doctors (RoomId);
END
GO

-- 6. Bảng DoctorSchedules
IF OBJECT_ID('DoctorSchedules', 'U') IS NULL
BEGIN
    CREATE TABLE DoctorSchedules (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        DoctorId    INT       NOT NULL,
        DayOfWeek   INT       NOT NULL,
        StartTime   TIME      NOT NULL,
        EndTime     TIME      NOT NULL,
        MaxPatients INT       NOT NULL DEFAULT 20,
        CreatedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt   DATETIME2 NULL,
        CONSTRAINT FK_DoctorSchedules_Doctors_DoctorId FOREIGN KEY (DoctorId) REFERENCES Doctors(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_DoctorSchedules_DoctorId ON DoctorSchedules (DoctorId);
END
GO

-- 7. Bảng DoctorDayOffs
IF OBJECT_ID('DoctorDayOffs', 'U') IS NULL
BEGIN
    CREATE TABLE DoctorDayOffs (
        Id        INT IDENTITY(1,1) PRIMARY KEY,
        DoctorId  INT           NOT NULL,
        Date      DATE          NOT NULL,
        Reason    NVARCHAR(200) NULL,
        CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2     NULL,
        CONSTRAINT FK_DoctorDayOffs_Doctors_DoctorId FOREIGN KEY (DoctorId) REFERENCES Doctors(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_DoctorDayOffs_DoctorId ON DoctorDayOffs (DoctorId);
END
GO

-- 8. Bảng PatientProfiles
IF OBJECT_ID('PatientProfiles', 'U') IS NULL
BEGIN
    CREATE TABLE PatientProfiles (
        Id                    INT IDENTITY(1,1) PRIMARY KEY,
        UserId                INT           NOT NULL,
        FullName              NVARCHAR(100) NOT NULL,
        DateOfBirth           DATETIME2     NOT NULL,
        Gender                NVARCHAR(10)  NOT NULL,
        Ethnicity             NVARCHAR(50)  NULL,
        Occupation            NVARCHAR(100) NULL,
        Phone                 NVARCHAR(MAX) NULL,
        Address               NVARCHAR(MAX) NULL,
        IdentityCardNumber    NVARCHAR(20)  NULL,
        HealthInsuranceNumber NVARCHAR(50)  NULL,
        Relationship          NVARCHAR(50)  NOT NULL,
        CreatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt             DATETIME2     NULL,
        CONSTRAINT FK_PatientProfiles_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_PatientProfiles_UserId ON PatientProfiles (UserId);
END
GO

-- 9. Bảng CartItems
IF OBJECT_ID('CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE CartItems (
        Id               INT IDENTITY(1,1) PRIMARY KEY,
        UserId           INT           NOT NULL,
        DoctorId         INT           NOT NULL,
        PatientProfileId INT           NOT NULL,
        AppointmentDate  DATETIME2     NOT NULL,
        TimeSlot         NVARCHAR(20)  NOT NULL,
        Note             NVARCHAR(MAX) NULL,
        CreatedAt        DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt        DATETIME2     NULL,
        CONSTRAINT FK_CartItems_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CartItems_Doctors_DoctorId FOREIGN KEY (DoctorId) REFERENCES Doctors(Id),
        CONSTRAINT FK_CartItems_PatientProfiles_PatientProfileId FOREIGN KEY (PatientProfileId) REFERENCES PatientProfiles(Id)
    );

    CREATE INDEX IX_CartItems_UserId ON CartItems (UserId);
    CREATE INDEX IX_CartItems_DoctorId ON CartItems (DoctorId);
    CREATE INDEX IX_CartItems_PatientProfileId ON CartItems (PatientProfileId);
END
GO

-- 10. Bảng Orders
IF OBJECT_ID('Orders', 'U') IS NULL
BEGIN
    CREATE TABLE Orders (
        Id                 INT IDENTITY(1,1) PRIMARY KEY,
        UserId             INT            NOT NULL,
        TotalAmount        DECIMAL(18, 2) NOT NULL,
        Status             NVARCHAR(20)   NOT NULL,
        CancellationReason NVARCHAR(500)  NULL,
        PaymentMethod      NVARCHAR(20)   NOT NULL,
        PaymentStatus      NVARCHAR(20)   NOT NULL,
        TransactionId      NVARCHAR(100)  NULL,
        CreatedAt          DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt          DATETIME2      NULL,
        CONSTRAINT FK_Orders_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_Orders_UserId ON Orders (UserId);
END
GO

-- 11. Bảng OrderDetails
IF OBJECT_ID('OrderDetails', 'U') IS NULL
BEGIN
    CREATE TABLE OrderDetails (
        Id               INT IDENTITY(1,1) PRIMARY KEY,
        OrderId          INT            NOT NULL,
        DoctorId         INT            NOT NULL,
        PatientProfileId INT            NOT NULL,
        AppointmentDate  DATETIME2      NOT NULL,
        TimeSlot         NVARCHAR(20)   NOT NULL,
        QueueNumber      INT            NOT NULL,
        Fee              DECIMAL(18, 2) NOT NULL,
        Note             NVARCHAR(MAX)  NULL,
        Status           NVARCHAR(20)   NOT NULL,
        CreatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt        DATETIME2      NULL,
        CONSTRAINT FK_OrderDetails_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderDetails_Doctors_DoctorId FOREIGN KEY (DoctorId) REFERENCES Doctors(Id),
        CONSTRAINT FK_OrderDetails_PatientProfiles_PatientProfileId FOREIGN KEY (PatientProfileId) REFERENCES PatientProfiles(Id)
    );

    CREATE INDEX IX_OrderDetails_OrderId ON OrderDetails (OrderId);
    CREATE INDEX IX_OrderDetails_DoctorId ON OrderDetails (DoctorId);
    CREATE INDEX IX_OrderDetails_PatientProfileId ON OrderDetails (PatientProfileId);
END
GO

-- Dữ liệu mẫu: Vai trò
IF NOT EXISTS (SELECT 1 FROM Roles)
BEGIN
    SET IDENTITY_INSERT Roles ON;
    INSERT INTO Roles (Id, [Key], Name, Description, CreatedAt)
    VALUES
        (1, 'ADMIN',   N'Quản trị viên', N'Toàn quyền hệ thống',  '2026-09-25T00:00:00'),
        (2, 'PATIENT', N'Bệnh nhân',     N'Người dùng đặt lịch', '2026-09-25T00:00:00');
    SET IDENTITY_INSERT Roles OFF;
END
GO

-- Dữ liệu mẫu: Tài khoản Admin & Bệnh nhân (mật khẩu mặc định: 123456)
DECLARE @AdminRoleId   INT = (SELECT TOP 1 Id FROM Roles WHERE [Key] = 'ADMIN');
DECLARE @PatientRoleId INT = (SELECT TOP 1 Id FROM Roles WHERE [Key] = 'PATIENT');

IF @AdminRoleId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM Users WHERE RoleId = @AdminRoleId AND IsActive = 1)
   AND NOT EXISTS (SELECT 1 FROM Users WHERE Phone = '0987654321')
BEGIN
    INSERT INTO Users (FullName, Email, Phone, Password, RoleId, IsActive, CreatedAt)
    VALUES
        (N'Quản trị viên', 'admin@ytehiepphuoc.vn', '0987654321', '$2a$11$u67TsHzwfoyrNEGpT6sy9O56sRdCsPEbImw2UCuF.RH40SyfFU5mK', @AdminRoleId, 1, GETUTCDATE());
END

IF @PatientRoleId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM Users WHERE RoleId = @PatientRoleId)
   AND NOT EXISTS (SELECT 1 FROM Users WHERE Phone = '0912345678')
BEGIN
    INSERT INTO Users (FullName, Email, Phone, Password, RoleId, IsActive, CreatedAt)
    VALUES
        (N'Nguyễn Minh An', 'benhnhan@ytehiepphuoc.vn', '0912345678', '$2a$11$u67TsHzwfoyrNEGpT6sy9O56sRdCsPEbImw2UCuF.RH40SyfFU5mK', @PatientRoleId, 1, GETUTCDATE());

    DECLARE @NewPatientUserId INT = SCOPE_IDENTITY();
    INSERT INTO PatientProfiles
        (UserId, FullName, DateOfBirth, Gender, Ethnicity, Occupation, Address, IdentityCardNumber, HealthInsuranceNumber, Relationship, CreatedAt)
    VALUES
        (@NewPatientUserId, N'Nguyễn Minh An', '1998-05-15', N'Nam', N'Kinh', N'Nhân viên văn phòng', N'Xã Hiệp Phước, Huyện Nhà Bè, TP.HCM', '079098012345', 'DN4791234567890', N'Bản thân', GETUTCDATE());
END
GO

-- Dữ liệu mẫu: Chuyên khoa, Phòng khám, Bác sĩ và Lịch làm việc
IF NOT EXISTS (SELECT 1 FROM Departments)
   AND NOT EXISTS (SELECT 1 FROM Rooms)
   AND NOT EXISTS (SELECT 1 FROM Doctors)
BEGIN
    SET IDENTITY_INSERT Departments ON;
    INSERT INTO Departments (Id, Name, Description, ConsultationFee, ImageUrl, IsActive, CreatedAt)
    VALUES
        (1, N'Nội tổng quát', N'Khám, tư vấn và điều trị các bệnh lý nội khoa tổng quát, tầm soát sức khỏe định kỳ.', 150000, '/images/departments/dept-noi-tong-quat.jpg', 1, GETUTCDATE()),
        (2, N'Tim mạch',      N'Chẩn đoán và điều trị tăng huyết áp, rối loạn nhịp tim, bệnh mạch vành và suy tim.',    220000, '/images/departments/dept-tim-mach.jpg',      1, GETUTCDATE()),
        (3, N'Nhi khoa',      N'Chăm sóc sức khỏe toàn diện cho trẻ sơ sinh, trẻ nhỏ và tư vấn dinh dưỡng, tiêm chủng.', 160000, '/images/departments/dept-nhi-khoa.jpg',      1, GETUTCDATE()),
        (4, N'Tai Mũi Họng',  N'Nội soi và điều trị viêm mũi xoang, viêm họng, viêm tai giữa và rối loạn tiền đình.',    180000, '/images/departments/dept-tai-mui-hong.jpg',  1, GETUTCDATE()),
        (5, N'Da liễu',       N'Khám và điều trị các bệnh lý da, dị ứng, viêm da cơ địa, mụn và chăm sóc da chuyên sâu.', 180000, '/images/departments/dept-da-lieu.jpg',       1, GETUTCDATE()),
        (6, N'Sản Phụ khoa',  N'Khám thai định kỳ, siêu âm sản khoa và chăm sóc sức khỏe sinh sản nữ giới.',               200000, '/images/departments/dept-san-phu-khoa.jpg',  1, GETUTCDATE());
    SET IDENTITY_INSERT Departments OFF;

    SET IDENTITY_INSERT Rooms ON;
    INSERT INTO Rooms (Id, RoomNumber, Floor, DepartmentId, IsActive, CreatedAt)
    VALUES
        (1,  'P.101', 1, 1, 1, GETUTCDATE()),
        (2,  'P.102', 1, 1, 1, GETUTCDATE()),
        (3,  'P.103', 1, 2, 1, GETUTCDATE()),
        (4,  'P.104', 1, 2, 1, GETUTCDATE()),
        (5,  'P.201', 2, 3, 1, GETUTCDATE()),
        (6,  'P.202', 2, 3, 1, GETUTCDATE()),
        (7,  'P.203', 2, 4, 1, GETUTCDATE()),
        (8,  'P.204', 2, 4, 1, GETUTCDATE()),
        (9,  'P.301', 3, 5, 1, GETUTCDATE()),
        (10, 'P.302', 3, 5, 1, GETUTCDATE()),
        (11, 'P.303', 3, 6, 1, GETUTCDATE()),
        (12, 'P.304', 3, 6, 1, GETUTCDATE());
    SET IDENTITY_INSERT Rooms OFF;

    SET IDENTITY_INSERT Doctors ON;
    INSERT INTO Doctors (Id, FullName, Qualification, Email, Phone, DepartmentId, RoomId, ExperienceYears, AvatarUrl, Description, IsActive, CreatedAt)
    VALUES
        (1,  N'Nguyễn Minh Quân',    'TS.BS',     'nmquan@ytehiepphuoc.vn',   '0901000001', 1, 1,  18, '/images/doctors/doc-01.jpg', N'Trưởng khoa Nội tổng quát, chuyên sâu bệnh lý chuyển hóa, đái tháo đường và tiêu hóa.',      1, GETUTCDATE()),
        (2,  N'Trần Thị Thanh Hà',   'BS.CKII',   'tttha@ytehiepphuoc.vn',    '0901000002', 1, 1,  15, '/images/doctors/doc-07.jpg', N'Chuyên khám tầm soát sức khỏe tổng quát, bệnh lý hô hấp và nội tiết.',                       1, GETUTCDATE()),
        (3,  N'Lê Hoàng Phúc',       'ThS.BS',    'lhphuc@ytehiepphuoc.vn',   '0901000003', 1, 2,  11, '/images/doctors/doc-02.jpg', N'Bác sĩ nội khoa giàu kinh nghiệm điều trị bệnh dạ dày, gan mật và tăng huyết áp.',            1, GETUTCDATE()),
        (4,  N'Phạm Ngọc Mai',       'BS.CKI',    'pnmai@ytehiepphuoc.vn',    '0901000004', 1, 2,  9,  '/images/doctors/doc-08.jpg', N'Tận tâm tư vấn chế độ dinh dưỡng và điều trị bệnh mạn tính cho người cao tuổi.',              1, GETUTCDATE()),
        (5,  N'Võ Quốc Bảo',         'BS.CKII',   'vqbao@ytehiepphuoc.vn',    '0901000005', 1, 1,  20, '/images/doctors/doc-03.jpg', N'Hơn 20 năm công tác tại các bệnh viện tuyến đầu về Nội tổng quát và Cơ xương khớp.',         1, GETUTCDATE()),
        (6,  N'Đặng Thu Thảo',       'ThS.BS',    'dtthao@ytehiepphuoc.vn',   '0901000006', 1, 2,  7,  '/images/doctors/doc-09.jpg', N'Chuyên khám sức khỏe định kỳ, tư vấn phòng bệnh và điều trị rối loạn giấc ngủ.',             1, GETUTCDATE()),
        (7,  N'Bùi Đức Anh',         'BS',        'bdanh@ytehiepphuoc.vn',    '0901000007', 1, 2,  6,  '/images/doctors/doc-04.jpg', N'Bác sĩ trẻ nhiệt huyết, chuyên theo dõi sức khỏe ban đầu và khám bảo hiểm y tế.',            1, GETUTCDATE()),
        (8,  N'Huỳnh Gia Huy',       'PGS.TS.BS', 'hghuy@ytehiepphuoc.vn',    '0901000008', 2, 3,  22, '/images/doctors/doc-05.jpg', N'Chuyên gia đầu ngành Tim mạch, siêu âm tim và điều trị rối loạn nhịp tim.',                  1, GETUTCDATE()),
        (9,  N'Nguyễn Thị Kim Ngân', 'BS.CKII',   'ntkngan@ytehiepphuoc.vn',  '0901000009', 3, 5,  14, '/images/doctors/doc-10.jpg', N'Bác sĩ Nhi khoa chuyên điều trị hô hấp nhi, dị ứng và tư vấn tăng trưởng cho trẻ.',          1, GETUTCDATE()),
        (10, N'Trương Tấn Tài',      'ThS.BS',    'tttai@ytehiepphuoc.vn',    '0901000010', 4, 7,  10, '/images/doctors/doc-06.jpg', N'Chuyên nội soi Tai Mũi Họng không đau và điều trị viêm xoang mạn tính.',                     1, GETUTCDATE()),
        (11, N'Lý Mỹ Linh',          'BS.CKI',    'lmlinh@ytehiepphuoc.vn',   '0901000011', 5, 9,  8,  '/images/doctors/doc-11.jpg', N'Chuyên điều trị mụn trứng cá, viêm da dị ứng và phục hồi da thẩm mỹ y khoa.',               1, GETUTCDATE()),
        (12, N'Đỗ Thị Hồng Nhung',   'BS.CKII',   'dthnhung@ytehiepphuoc.vn', '0901000012', 6, 11, 17, '/images/doctors/doc-12.jpg', N'Chuyên theo dõi thai kỳ nguy cơ cao, siêu âm hình thái và khám phụ khoa định kỳ.',           1, GETUTCDATE());
    SET IDENTITY_INSERT Doctors OFF;

    DECLARE @DocId INT = 1;
    DECLARE @Day   INT;
    WHILE @DocId <= 12
    BEGIN
        SET @Day = 0;
        WHILE @Day <= 6
        BEGIN
            INSERT INTO DoctorSchedules (DoctorId, DayOfWeek, StartTime, EndTime, MaxPatients, CreatedAt)
            VALUES
                (@DocId, @Day, '07:00:00', '11:30:00', 20, GETUTCDATE()),
                (@DocId, @Day, '13:30:00', '17:00:00', 20, GETUTCDATE());
            SET @Day += 1;
        END
        SET @DocId += 1;
    END
END
GO
