-- AI Robotics Course and Student Management System Database Dump
-- Generated for VPS hosting setup
-- Character Set: utf8mb4 with unicode_ci collation for Arabic support

-- =============================================
-- DATABASE CREATION AND USER SETUP
-- =============================================

-- Create the database with proper character set for Arabic content
CREATE DATABASE IF NOT EXISTS airobotics_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create a dedicated user for the application
CREATE USER IF NOT EXISTS 'aiuser'@'localhost' IDENTIFIED BY 'StrongPasswordHere!';

-- Grant all privileges on the database to the user
GRANT ALL PRIVILEGES ON airobotics_db.* TO 'aiuser'@'localhost';

-- If you want to allow remote connections (for VPS), also create user for remote access
CREATE USER IF NOT EXISTS 'aiuser'@'%' IDENTIFIED BY 'StrongPasswordHere!';
GRANT ALL PRIVILEGES ON airobotics_db.* TO 'aiuser'@'%';

-- Apply the privilege changes
FLUSH PRIVILEGES;

-- Switch to the created database
USE airobotics_db;

-- =============================================
-- TABLE CREATION
-- =============================================

-- 1. Branches table
CREATE TABLE Branches (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,
    Address VARCHAR(200) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    HasWorkspace BOOLEAN NOT NULL DEFAULT FALSE,
    HasSharedWorkspace BOOLEAN NOT NULL DEFAULT FALSE,
    HasRooms BOOLEAN NOT NULL DEFAULT FALSE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 2. CourseCategories table
CREATE TABLE CourseCategories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    MinAge INT,
    MaxAge INT,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 3. Users table
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Username VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Phone VARCHAR(20) NOT NULL,
    Address VARCHAR(200) NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role INT NOT NULL,
    UserRole INT NOT NULL,
    BranchId INT NOT NULL,
    StudentId INT,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 4. Students table
CREATE TABLE Students (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    StudentNumber VARCHAR(50),
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Address VARCHAR(200),
    DateOfBirth DATE,
    ParentName VARCHAR(100),
    ParentPhone VARCHAR(20),
    ParentEmail VARCHAR(100),
    EmergencyContact VARCHAR(100),
    EmergencyPhone VARCHAR(20),
    MedicalInfo TEXT,
    Notes TEXT,
    BranchId INT NOT NULL,
    UserId INT,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_Students_BranchId (BranchId),
    INDEX IX_Students_UserId (UserId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 5. Employees table
CREATE TABLE Employees (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeNumber VARCHAR(50),
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Address VARCHAR(200),
    Position VARCHAR(100),
    Department VARCHAR(100),
    HireDate DATE,
    Salary DECIMAL(18,2),
    BranchId INT NOT NULL,
    UserId INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_Employees_BranchId (BranchId),
    INDEX IX_Employees_UserId (UserId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 6. Labs table
CREATE TABLE Labs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    Capacity INT,
    Equipment TEXT,
    BranchId INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    INDEX IX_Labs_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 7. Rooms table
CREATE TABLE Rooms (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    RoomNumber VARCHAR(50) NOT NULL,
    Description TEXT,
    RoomType INT NOT NULL,
    Capacity INT,
    Equipment TEXT,
    Location VARCHAR(100),
    BranchId INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    UNIQUE INDEX IX_Rooms_BranchId_RoomNumber (BranchId, RoomNumber)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 8. Courses table
CREATE TABLE Courses (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    Price DECIMAL(18,2) NOT NULL,
    Duration INT,
    StartDate DATE,
    EndDate DATE,
    CourseDays VARCHAR(200),
    StartTime VARCHAR(50),
    EndTime VARCHAR(50),
    MaxStudents INT,
    CurrentStudents INT DEFAULT 0,
    CourseCategoryId INT NOT NULL,
    BranchId INT NOT NULL,
    InstructorId INT,
    LabId INT,
    RoomId INT,
    DriveLink TEXT,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (CourseCategoryId) REFERENCES CourseCategories(Id) ON DELETE RESTRICT,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (InstructorId) REFERENCES Employees(Id) ON DELETE SET NULL,
    FOREIGN KEY (LabId) REFERENCES Labs(Id) ON DELETE SET NULL,
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE SET NULL,
    INDEX IX_Courses_CourseCategoryId (CourseCategoryId),
    INDEX IX_Courses_BranchId (BranchId),
    INDEX IX_Courses_InstructorId (InstructorId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 9. CourseRegistrations table
CREATE TABLE CourseRegistrations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    RegistrationDate DATETIME(6) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    PaymentStatus INT NOT NULL DEFAULT 0,
    Notes TEXT,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    INDEX IX_CourseRegistrations_StudentId (StudentId),
    INDEX IX_CourseRegistrations_CourseId (CourseId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 10. Payments table
CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME(6) NOT NULL,
    PaymentMethod INT NOT NULL,
    PaymentType INT NOT NULL,
    Notes TEXT,
    CourseRegistrationId INT,
    WorkspaceBookingId INT,
    SharedWorkspaceBookingId INT,
    StudentId INT,
    BranchId INT NOT NULL,
    ProcessedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (CourseRegistrationId) REFERENCES CourseRegistrations(Id) ON DELETE CASCADE,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ProcessedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_Payments_CourseRegistrationId (CourseRegistrationId),
    INDEX IX_Payments_BranchId (BranchId),
    INDEX IX_Payments_ProcessedByUserId (ProcessedByUserId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 11. Attendances table
CREATE TABLE Attendances (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AttendanceDate DATE NOT NULL,
    IsPresent BOOLEAN NOT NULL DEFAULT TRUE,
    Notes TEXT,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    RecordedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    FOREIGN KEY (RecordedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_Attendances_StudentId (StudentId),
    INDEX IX_Attendances_CourseId (CourseId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 12. StudentGrades table
CREATE TABLE StudentGrades (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Grade DECIMAL(5,2),
    Notes VARCHAR(500),
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    UpdatedByUserId INT,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    UNIQUE INDEX IX_StudentGrades_StudentId_CourseId (StudentId, CourseId),
    INDEX IX_StudentGrades_CreatedByUserId (CreatedByUserId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 13. Equipment table
CREATE TABLE Equipment (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    SerialNumber VARCHAR(100) NOT NULL UNIQUE,
    Model VARCHAR(100) NOT NULL,
    Brand VARCHAR(100) NOT NULL,
    Manufacturer VARCHAR(100) NOT NULL,
    Category VARCHAR(50) NOT NULL,
    Description VARCHAR(1000) NOT NULL,
    PurchasePrice DECIMAL(18,2) NOT NULL,
    PurchaseDate DATETIME(6) NOT NULL,
    WarrantyExpiry DATETIME(6),
    Status INT NOT NULL,
    Condition INT NOT NULL,
    BranchId INT NOT NULL,
    RoomId INT,
    AssignedToUserId INT,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE SET NULL,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_Equipment_SerialNumber (SerialNumber),
    INDEX IX_Equipment_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 14. Expenses table
CREATE TABLE Expenses (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    Amount DECIMAL(18,2) NOT NULL,
    ExpenseDate DATE NOT NULL,
    Category VARCHAR(100),
    Status INT NOT NULL DEFAULT 0,
    BranchId INT NOT NULL,
    RequestedByUserId INT NOT NULL,
    ApprovedByUserId INT,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (RequestedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_Expenses_BranchId (BranchId),
    INDEX IX_Expenses_RequestedByUserId (RequestedByUserId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 15. Certificates table
CREATE TABLE Certificates (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CertificateNumber VARCHAR(50) NOT NULL UNIQUE,
    StudentName VARCHAR(100) NOT NULL,
    CourseName VARCHAR(200) NOT NULL,
    CompletionDate DATE NOT NULL,
    Grade DECIMAL(5,2),
    IssuedDate DATE NOT NULL,
    BranchId INT NOT NULL,
    StudentId INT,
    CourseId INT,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE SET NULL,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE SET NULL,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_Certificates_CertificateNumber (CertificateNumber),
    INDEX IX_Certificates_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 16. WorkspaceDesks table
CREATE TABLE WorkspaceDesks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DeskNumber VARCHAR(50) NOT NULL,
    Description TEXT,
    HourlyRate DECIMAL(18,2),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    BranchId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    UNIQUE INDEX IX_WorkspaceDesks_BranchId_DeskNumber (BranchId, DeskNumber)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 17. SharedWorkspaces table
CREATE TABLE SharedWorkspaces (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    MaxCapacity INT NOT NULL,
    HourlyRatePerPerson DECIMAL(18,2),
    HasWifi BOOLEAN NOT NULL DEFAULT FALSE,
    HasPrinter BOOLEAN NOT NULL DEFAULT FALSE,
    HasProjector BOOLEAN NOT NULL DEFAULT FALSE,
    HasWhiteboard BOOLEAN NOT NULL DEFAULT FALSE,
    Equipment VARCHAR(500),
    BranchId INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    INDEX IX_SharedWorkspaces_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 18. WorkspaceBookings table
CREATE TABLE WorkspaceBookings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BookingDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    HourlyRate DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    RemainingAmount DECIMAL(18,2),
    Status INT NOT NULL DEFAULT 0,
    StudentId INT NOT NULL,
    RoomId INT,
    BranchId INT NOT NULL,
    BookedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE RESTRICT,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (BookedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_WorkspaceBookings_StudentId (StudentId),
    INDEX IX_WorkspaceBookings_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 19. SharedWorkspaceBookings table
CREATE TABLE SharedWorkspaceBookings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BookingDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    CustomerName VARCHAR(100),
    CustomerPhone VARCHAR(20),
    CustomerEmail VARCHAR(100),
    HourlyRate DECIMAL(18,2) NOT NULL,
    TotalHours DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    Status INT NOT NULL DEFAULT 0,
    Purpose VARCHAR(500),
    Notes VARCHAR(1000),
    RequiredEquipment VARCHAR(500),
    SharedWorkspaceId INT NOT NULL,
    StudentId INT,
    BranchId INT NOT NULL,
    BookedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (SharedWorkspaceId) REFERENCES SharedWorkspaces(Id) ON DELETE RESTRICT,
    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE SET NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (BookedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_SharedWorkspaceBookings_SharedWorkspaceId (SharedWorkspaceId),
    INDEX IX_SharedWorkspaceBookings_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 20. CafeteriaItems table
CREATE TABLE CafeteriaItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    Price DECIMAL(18,2) NOT NULL,
    Cost DECIMAL(18,2),
    Category VARCHAR(100),
    IsAvailable BOOLEAN NOT NULL DEFAULT TRUE,
    BranchId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    INDEX IX_CafeteriaItems_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 21. CafeteriaOrders table
CREATE TABLE CafeteriaOrders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderNumber VARCHAR(50) NOT NULL UNIQUE,
    OrderDate DATETIME(6) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    Status INT NOT NULL DEFAULT 0,
    StudentId INT,
    EmployeeId INT,
    BranchId INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE SET NULL,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE SET NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_CafeteriaOrders_OrderNumber (OrderNumber),
    INDEX IX_CafeteriaOrders_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 22. CafeteriaOrderItems table
CREATE TABLE CafeteriaOrderItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    CafeteriaOrderId INT NOT NULL,
    CafeteriaItemId INT NOT NULL,
    FOREIGN KEY (CafeteriaOrderId) REFERENCES CafeteriaOrders(Id) ON DELETE CASCADE,
    FOREIGN KEY (CafeteriaItemId) REFERENCES CafeteriaItems(Id) ON DELETE RESTRICT,
    INDEX IX_CafeteriaOrderItems_CafeteriaOrderId (CafeteriaOrderId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 23. WorkspaceSessions table
CREATE TABLE WorkspaceSessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SessionDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    InternetCost DECIMAL(18,2) DEFAULT 0,
    LaptopCost DECIMAL(18,2) DEFAULT 0,
    CafeteriaCost DECIMAL(18,2) DEFAULT 0,
    TotalCost DECIMAL(18,2) NOT NULL,
    WorkspaceDeskId INT NOT NULL,
    StudentId INT,
    BranchId INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (WorkspaceDeskId) REFERENCES WorkspaceDesks(Id) ON DELETE CASCADE,
    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE SET NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_WorkspaceSessions_WorkspaceDeskId (WorkspaceDeskId),
    INDEX IX_WorkspaceSessions_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 24. WorkspaceInvoices table
CREATE TABLE WorkspaceInvoices (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InvoiceNumber VARCHAR(50) NOT NULL UNIQUE,
    InvoiceDate DATE NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    WorkspaceSessionId INT NOT NULL,
    BranchId INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (WorkspaceSessionId) REFERENCES WorkspaceSessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id) ON DELETE RESTRICT,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_WorkspaceInvoices_InvoiceNumber (InvoiceNumber),
    INDEX IX_WorkspaceInvoices_BranchId (BranchId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 25. WorkspaceInvoiceItems table
CREATE TABLE WorkspaceInvoiceItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Description VARCHAR(200) NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    WorkspaceInvoiceId INT NOT NULL,
    WorkspaceSessionId INT NOT NULL,
    FOREIGN KEY (WorkspaceInvoiceId) REFERENCES WorkspaceInvoices(Id) ON DELETE CASCADE,
    FOREIGN KEY (WorkspaceSessionId) REFERENCES WorkspaceSessions(Id) ON DELETE NO ACTION,
    INDEX IX_WorkspaceInvoiceItems_WorkspaceInvoiceId (WorkspaceInvoiceId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 26. WorkspaceOccupancies table
CREATE TABLE WorkspaceOccupancies (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OccupancyDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME,
    Notes VARCHAR(500),
    SharedWorkspaceId INT NOT NULL,
    SharedWorkspaceBookingId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (SharedWorkspaceId) REFERENCES SharedWorkspaces(Id) ON DELETE RESTRICT,
    FOREIGN KEY (SharedWorkspaceBookingId) REFERENCES SharedWorkspaceBookings(Id) ON DELETE CASCADE,
    INDEX IX_WorkspaceOccupancies_SharedWorkspaceId (SharedWorkspaceId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 27. LabReservations table
CREATE TABLE LabReservations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReservationDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Purpose VARCHAR(200),
    Notes TEXT,
    LabId INT NOT NULL,
    CourseId INT,
    ReservedByUserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (LabId) REFERENCES Labs(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE SET NULL,
    FOREIGN KEY (ReservedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX IX_LabReservations_LabId (LabId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 28. RoomReservations table
CREATE TABLE RoomReservations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    ReservationDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Purpose VARCHAR(200),
    Notes TEXT,
    RoomId INT NOT NULL,
    CourseId INT,
    ReservedByUserId INT NOT NULL,
    InstructorId INT,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE SET NULL,
    FOREIGN KEY (ReservedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (InstructorId) REFERENCES Employees(Id) ON DELETE SET NULL,
    INDEX IX_RoomReservations_RoomId (RoomId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 29. EquipmentReservations table
CREATE TABLE EquipmentReservations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReservationDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Purpose VARCHAR(200) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    DamageCost DECIMAL(18,2),
    Notes TEXT,
    EquipmentId INT NOT NULL,
    ReservedByUserId INT NOT NULL,
    ApprovedByUserId INT,
    CourseId INT,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(Id) ON DELETE CASCADE,
    FOREIGN KEY (ReservedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE SET NULL,
    INDEX IX_EquipmentReservations_EquipmentId (EquipmentId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- =============================================
-- SEED DATA INSERTION
-- =============================================

-- Insert initial seed data
-- Branches
INSERT INTO Branches (Id, Name, Address, Phone, HasWorkspace, HasSharedWorkspace, HasRooms, IsActive, CreatedAt) VALUES
(1, 'فرع أسيوط', 'أسيوط، مصر', '0882123456', TRUE, TRUE, TRUE, TRUE, NOW()),
(2, 'فرع أبو تيج', 'أبو تيج، أسيوط، مصر', '0882654321', FALSE, FALSE, TRUE, TRUE, NOW());

-- Course Categories
INSERT INTO CourseCategories (Id, Name, Description, MinAge, MaxAge, IsActive, CreatedAt) VALUES
(1, 'أساسيات الروبوتيكس والتفكير الصحيح', 'للأطفال من 4-6 سنوات', 4, 6, TRUE, NOW()),
(2, 'المستوى الأول (7-12 سنة)', 'أساسيات الروبوتيكس والذكاء الاصطناعي، الكمبيوتر والبرمجة، الهندسة الكهربية والميكاترونكس', 7, 12, TRUE, NOW()),
(3, 'المستوى الأول (13-17 سنة)', 'أساسيات الروبوتيكس والذكاء الاصطناعي، البرمجة بـ Python و C++، الهندسة الكهربية والميكاترونكس', 13, 17, TRUE, NOW()),
(4, 'كورسات التخصص (18+ سنة)', 'تطوير المواقع، تطبيقات الهواتف، تطبيقات سطح المكتب، تحليل البيانات، الروبوتات، الميكاترونيكس', 18, 100, TRUE, NOW());

-- Test User (Password: Test123!)
INSERT INTO Users (Id, FullName, Username, Email, Phone, Address, PasswordHash, Role, UserRole, BranchId, IsActive, CreatedAt) VALUES
(1, 'Test User', 'testuser', 'test@example.com', '0123456789', 'Test Address', '9Dpj3YyGsMQPKJvPXhvtqhsGnXmyJLwx4m2TJgZx3jM=', 1, 1, 1, TRUE, NOW());

-- Rooms for Assiut Branch
INSERT INTO Rooms (Id, Name, RoomNumber, Description, RoomType, Capacity, Equipment, Location, BranchId, IsActive, CreatedAt) VALUES
(1, 'قاعة الروبوتيكس الأولى', 'R101', 'قاعة مخصصة لكورسات الروبوتيكس والذكاء الاصطناعي', 1, 20, 'أجهزة كمبيوتر، مجموعات روبوتيكس، شاشة عرض', 'الطابق الأول', 1, TRUE, NOW()),
(2, 'قاعة البرمجة', 'R102', 'قاعة مخصصة لكورسات البرمجة وتطوير التطبيقات', 0, 25, 'أجهزة كمبيوتر، شاشة عرض، سبورة ذكية', 'الطابق الأول', 1, TRUE, NOW()),
(3, 'ورشة الميكاترونيكس', 'R103', 'ورشة مخصصة للأعمال العملية والمشاريع', 2, 15, 'طاولات عمل، أدوات هندسية، معدات إلكترونية', 'الطابق الأول', 1, TRUE, NOW());

-- Shared Workspaces for Assiut Branch
INSERT INTO SharedWorkspaces (Id, Name, Description, MaxCapacity, HourlyRatePerPerson, HasWifi, HasPrinter, HasProjector, HasWhiteboard, Equipment, BranchId, IsActive, CreatedAt) VALUES
(1, 'مساحة العمل المشتركة الأولى', 'مساحة مفتوحة للدراسة والعمل الجماعي', 12, 5.00, TRUE, TRUE, FALSE, TRUE, 'واي فاي، طابعة، سبورة، طاولات دراسة', 1, TRUE, NOW()),
(2, 'مساحة العمل المشتركة الثانية', 'مساحة هادئة للدراسة الفردية والجماعية', 8, 4.00, TRUE, FALSE, TRUE, TRUE, 'واي فاي، بروجكتر، سبورة، طاولات دراسة', 1, TRUE, NOW());

-- =============================================
-- END OF DUMP FILE
-- =============================================
