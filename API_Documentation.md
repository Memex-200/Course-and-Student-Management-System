# AI Robotics Academy Management System - API Documentation

## Overview

The AI Robotics Academy Management System is a comprehensive .NET Core Web API designed to manage a robotics academy with two branches: Abu Tig (courses only) and Assiut (courses + individual workspace + shared workspace). The system supports full Arabic language localization and includes course management, student registration, individual workspace billing, shared workspace system with multi-user support, equipment tracking, and comprehensive financial management.

### Key Features

- **Multi-Branch Support**: Abu Tig (courses only) and Assiut (full features)
- **Shared Workspace System**: Multiple users can book and use the same workspace simultaneously
- **Arabic Language Support**: All enum values returned in both English and Arabic
- **Real-time Occupancy Tracking**: Live monitoring of workspace usage
- **Flexible Booking System**: Support for individual and group bookings

## System Architecture

- **Framework**: .NET 8.0 Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer Token
- **Language Support**: Arabic (RTL)
- **API Documentation**: Swagger/OpenAPI

## Base Configuration

- **Base URL**: `http://localhost:5227`
- **Swagger UI**: `http://localhost:5227/swagger`
- **Database**: SQL Server (MEMEX\SQLSERVER)

## Authentication & Authorization

### Login

**POST** `/api/auth/login`

**Request Body:**

```json
{
  "username": "string",
  "password": "string"
}
```

**Response:**

```json
{
  "token": "jwt_token_here",
  "user": {
    "id": 1,
    "username": "admin",
    "fullName": "Administrator",
    "email": "admin@example.com",
    "userRole": "Admin",
    "branch": "أسيوط",
    "branchId": 1
  }
}
```

### User Roles

- **Admin**: Full system access
- **Employee**: Limited administrative access
- **Student**: Read-only access to personal data

## Core Entities

### Branches

- **Abu Tig (أبو تيج)**: Courses only
- **Assiut (أسيوط)**: Courses + Workspace + 3 rooms for scheduling

### User Management

- Users, Students, Employees
- Branch-based access control
- Role-based permissions

### Shared Workspace System (Assiut Branch Only)

The shared workspace system allows multiple users to book and use the same workspace simultaneously with individual billing and capacity management.

**Key Features:**

- **Multi-user Support**: Multiple people can share the same workspace
- **Real-time Occupancy**: Live tracking of current occupancy vs. capacity
- **Flexible Booking Types**: Individual, Group, Study, Meeting, Project
- **Equipment Management**: WiFi, Printer, Projector, Whiteboard tracking
- **Individual Billing**: Each user pays separately even when sharing space
- **Check-in/Check-out System**: Real-time entry and exit tracking

**Workspace Status:**

- `Available` (متاح): Space available for booking
- `Occupied` (مشغول): Currently in use but has available capacity
- `Full` (ممتلئ): At maximum capacity
- `Maintenance` (صيانة): Temporarily unavailable

**Booking Types:**

- `Individual` (فردي): Single person booking
- `Group` (جماعي): Group booking
- `Study` (دراسة): Study session
- `Meeting` (اجتماع): Meeting or presentation
- `Project` (مشروع): Project work

## Enum Localization

All API responses include enum values in both English and Arabic:

```json
{
  "status": "Active",
  "statusArabic": "نشط",
  "paymentMethod": "InstaPay",
  "paymentMethodArabic": "انستا باي"
}
```

## Data Models

### Shared Workspace Models

#### SharedWorkspace

```json
{
  "id": 1,
  "name": "مساحة العمل المشتركة الأولى",
  "description": "مساحة مفتوحة للدراسة والعمل الجماعي",
  "maxCapacity": 12,
  "currentOccupancy": 5,
  "hourlyRatePerPerson": 5.0,
  "hasWifi": true,
  "hasPrinter": true,
  "hasProjector": false,
  "hasWhiteboard": true,
  "equipment": "واي فاي، طابعة، سبورة، طاولات دراسة",
  "status": "Available",
  "statusArabic": "متاح",
  "branchId": 1,
  "isActive": true,
  "createdAt": "2024-01-15T10:00:00Z"
}
```

#### SharedWorkspaceBooking

```json
{
  "id": 1,
  "sharedWorkspaceId": 1,
  "studentId": 123,
  "customerName": "أحمد محمد",
  "customerPhone": "01234567890",
  "customerEmail": "ahmed@example.com",
  "startTime": "2024-01-15T09:00:00Z",
  "endTime": "2024-01-15T12:00:00Z",
  "numberOfPeople": 3,
  "hourlyRate": 5.0,
  "totalHours": 3.0,
  "totalAmount": 45.0,
  "paidAmount": 45.0,
  "paymentStatus": "Paid",
  "paymentStatusArabic": "مدفوع",
  "paymentMethod": "InstaPay",
  "paymentMethodArabic": "انستا باي",
  "bookingType": "Study",
  "bookingTypeArabic": "دراسة",
  "status": "Active",
  "statusArabic": "نشط",
  "purpose": "مراجعة مشروع التخرج",
  "requiredEquipment": "بروجكتر، واي فاي",
  "checkInTime": "2024-01-15T09:05:00Z",
  "checkOutTime": null
}
```

### Enum Values with Arabic Translation

#### SharedWorkspaceStatus

- `Available` → `متاح`
- `Occupied` → `مشغول`
- `Full` → `ممتلئ`
- `Maintenance` → `صيانة`

#### BookingType

- `Individual` → `فردي`
- `Group` → `جماعي`
- `Study` → `دراسة`
- `Meeting` → `اجتماع`
- `Project` → `مشروع`

#### PaymentMethod

- `Cash` → `نقدي`
- `InstaPay` → `انستا باي`
- `Fawry` → `فوري`

#### PaymentStatus

- `Paid` → `مدفوع`
- `Unpaid` → `غير مدفوع`
- `PartiallyPaid` → `مدفوع جزئياً`

## API Endpoints

## 1. Authentication Controller (`/api/auth`)

| Method | Endpoint    | Description       | Auth Required |
| ------ | ----------- | ----------------- | ------------- |
| POST   | `/login`    | User login        | No            |
| POST   | `/register` | User registration | No            |

## 2. Students Controller (`/api/students`)

| Method | Endpoint                | Description                 | Auth Required  |
| ------ | ----------------------- | --------------------------- | -------------- |
| GET    | `/`                     | Get all students            | Yes            |
| GET    | `/{id}`                 | Get student by ID           | Yes            |
| POST   | `/`                     | Create new student          | Admin/Employee |
| PUT    | `/{id}`                 | Update student              | Admin/Employee |
| DELETE | `/{id}`                 | Delete student              | Admin          |
| POST   | `/{id}/register-course` | Register student for course | Admin/Employee |

**Query Parameters for GET /:**

- `branchId` (optional): Filter by branch
- `search` (optional): Search by name or phone

**Create Student Request:**

```json
{
  "fullName": "أحمد محمد علي",
  "phone": "01234567890",
  "email": "ahmed@example.com",
  "age": 15,
  "gender": "Male",
  "address": "شارع النيل، أسيوط",
  "parentName": "محمد علي",
  "parentPhone": "01098765432",
  "parentEmail": "parent@example.com",
  "emergencyContact": "فاطمة علي",
  "emergencyPhone": "01123456789",
  "medicalConditions": "لا يوجد",
  "preferredTransportation": "والدين"
}
```

## 3. Courses Controller (`/api/courses`)

| Method | Endpoint      | Description                 | Auth Required  |
| ------ | ------------- | --------------------------- | -------------- |
| GET    | `/`           | Get all courses             | Yes            |
| GET    | `/{id}`       | Get course by ID            | Yes            |
| POST   | `/`           | Create new course           | Admin/Employee |
| PUT    | `/{id}`       | Update course               | Admin/Employee |
| DELETE | `/{id}`       | Delete course               | Admin          |
| GET    | `/categories` | Get course categories       | Yes            |
| GET    | `/test-data`  | Get test data for dropdowns | Yes            |

**Query Parameters for GET /:**

- `categoryId` (optional): Filter by category
- `status` (optional): Filter by status
- `branchId` (optional): Filter by branch

**Create Course Request:**

```json
{
  "name": "الروبوتات الأساسية",
  "description": "مقدمة في علم الروبوتات للمبتدئين",
  "price": 1200.0,
  "maxStudents": 15,
  "startDate": "2025-08-01T09:00:00.000Z",
  "endDate": "2025-09-30T11:00:00.000Z",
  "schedule": "الأحد والثلاثاء 9:00-11:00 ص",
  "courseCategoryId": 1,
  "instructorId": 1,
  "roomId": 1,
  "labId": 1
}
```

## 4. Equipment Controller (`/api/equipment`)

| Method | Endpoint                    | Description         | Auth Required  |
| ------ | --------------------------- | ------------------- | -------------- |
| GET    | `/`                         | Get all equipment   | Yes            |
| GET    | `/{id}`                     | Get equipment by ID | Yes            |
| POST   | `/`                         | Add new equipment   | Admin/Employee |
| PUT    | `/{id}`                     | Update equipment    | Admin/Employee |
| POST   | `/{id}/reserve`             | Reserve equipment   | Admin/Employee |
| PUT    | `/reservations/{id}/return` | Return equipment    | Admin/Employee |

**Create Equipment Request:**

```json
{
  "name": "Arduino Uno R3",
  "serialNumber": "ARD-001-2025",
  "model": "Arduino Uno R3",
  "manufacturer": "Arduino",
  "purchaseDate": "2025-01-15T00:00:00.000Z",
  "purchasePrice": 450.0,
  "warrantyExpiry": "2027-01-15T00:00:00.000Z",
  "roomId": 1,
  "condition": "Excellent",
  "status": "Available"
}
```

## 5. Expenses Controller (`/api/expenses`)

| Method | Endpoint        | Description            | Auth Required  |
| ------ | --------------- | ---------------------- | -------------- |
| GET    | `/`             | Get all expenses       | Yes            |
| GET    | `/{id}`         | Get expense by ID      | Yes            |
| POST   | `/`             | Create new expense     | Admin/Employee |
| PUT    | `/{id}`         | Update expense         | Admin/Employee |
| PUT    | `/{id}/approve` | Approve expense        | Admin          |
| PUT    | `/{id}/reject`  | Reject expense         | Admin          |
| GET    | `/statistics`   | Get expense statistics | Yes            |

**Create Expense Request:**

```json
{
  "title": "فاتورة كهرباء الفرع الرئيسي",
  "description": "فاتورة استهلاك الكهرباء لشهر يونيو 2025",
  "category": 1,
  "amount": 850.5,
  "expenseDate": "2025-07-02T12:05:32.954Z",
  "priority": 2,
  "vendor": "شركة الكهرباء المصرية",
  "paymentMethod": 2,
  "receiptNumber": "ELE-2025-001234",
  "notes": "دفع في الموعد المحدد - خصم 5% للدفع المبكر",
  "isRecurring": true,
  "recurrencePattern": "Monthly",
  "nextRecurrenceDate": "2025-08-02T12:05:32.954Z"
}
```

## 6. Workspace Controller (`/api/workspace`) - Assiut Branch Only

| Method | Endpoint                 | Description              | Auth Required  |
| ------ | ------------------------ | ------------------------ | -------------- |
| GET    | `/bookings`              | Get workspace bookings   | Yes            |
| POST   | `/book`                  | Create workspace booking | Admin/Employee |
| PUT    | `/bookings/{id}/end`     | End workspace booking    | Admin/Employee |
| POST   | `/bookings/{id}/payment` | Add payment to booking   | Admin/Employee |

**Create Workspace Booking Request:**

```json
{
  "studentId": 1,
  "employeeId": null,
  "customerName": "",
  "customerPhone": "",
  "startTime": "2025-07-02T14:00:00.000Z",
  "endTime": "2025-07-02T16:00:00.000Z",
  "hourlyRate": 25.0,
  "paidAmount": 50.0,
  "paymentMethod": 1,
  "purpose": "العمل على مشروع الروبوتات",
  "notes": "حجز لمدة ساعتين"
}
```

## 7. SharedWorkspace Controller (`/api/SharedWorkspace`) - Assiut Branch Only

| Method | Endpoint                | Description                      | Auth Required  |
| ------ | ----------------------- | -------------------------------- | -------------- |
| GET    | `/spaces`               | Get available shared workspaces  | Yes            |
| POST   | `/book`                 | Create shared workspace booking  | Admin/Employee |
| GET    | `/bookings`             | Get shared workspace bookings    | Yes            |
| POST   | `/checkin/{bookingId}`  | Check-in to shared workspace     | Admin/Employee |
| POST   | `/checkout/{bookingId}` | Check-out from shared workspace  | Admin/Employee |
| GET    | `/occupancy`            | Get current occupancy status     | Yes            |
| POST   | `/payment/{bookingId}`  | Process shared workspace payment | Admin/Employee |
| GET    | `/statistics`           | Get shared workspace statistics  | Yes            |

**Example Request - Create Shared Workspace Booking:**

```json
POST /api/SharedWorkspace/book
{
  "sharedWorkspaceId": 1,
  "studentId": 123,
  "customerName": "أحمد محمد",
  "customerPhone": "01234567890",
  "customerEmail": "ahmed@example.com",
  "startTime": "2024-01-15T09:00:00",
  "endTime": "2024-01-15T12:00:00",
  "numberOfPeople": 3,
  "bookingType": "Study",
  "purpose": "مراجعة مشروع التخرج",
  "requiredEquipment": "بروجكتر، واي فاي",
  "needsInternet": true,
  "needsPrinter": false,
  "needsProjector": true,
  "paymentMethod": "InstaPay"
}
```

**Example Response - Get Shared Workspaces:**

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "مساحة العمل المشتركة الأولى",
      "description": "مساحة مفتوحة للدراسة والعمل الجماعي",
      "maxCapacity": 12,
      "currentOccupancy": 5,
      "availableCapacity": 7,
      "hourlyRatePerPerson": 5.0,
      "hasWifi": true,
      "hasPrinter": true,
      "hasProjector": false,
      "hasWhiteboard": true,
      "equipment": "واي فاي، طابعة، سبورة، طاولات دراسة",
      "status": "Occupied",
      "statusArabic": "مشغول",
      "isActive": true
    }
  ]
}
```

## 8. Payments Controller (`/api/payments`)

| Method | Endpoint                             | Description                          | Auth Required  |
| ------ | ------------------------------------ | ------------------------------------ | -------------- |
| GET    | `/course-registrations`              | Get course payment records           | Yes            |
| GET    | `/workspace-bookings`                | Get workspace payment records        | Yes            |
| GET    | `/shared-workspace-bookings`         | Get shared workspace payment records | Yes            |
| GET    | `/cafeteria-orders`                  | Get cafeteria payment records        | Yes            |
| POST   | `/course-registrations/{id}/payment` | Process course payment               | Admin/Employee |
| POST   | `/workspace-bookings/{id}/payment`   | Process workspace payment            | Admin/Employee |
| POST   | `/cafeteria-orders/{id}/payment`     | Process cafeteria payment            | Admin/Employee |
| GET    | `/statistics`                        | Get payment statistics               | Yes            |

**Process Payment Request:**

```json
{
  "amount": 500.0,
  "paymentMethod": 1,
  "notes": "دفعة رسوم التسجيل في كورس الروبوتات الأساسي"
}
```

## 9. Cafeteria Controller (`/api/cafeteria`)

| Method | Endpoint              | Description          | Auth Required  |
| ------ | --------------------- | -------------------- | -------------- |
| GET    | `/items`              | Get cafeteria items  | Yes            |
| GET    | `/items/{id}`         | Get item by ID       | Yes            |
| POST   | `/items`              | Add new item         | Admin/Employee |
| PUT    | `/items/{id}`         | Update item          | Admin/Employee |
| GET    | `/orders`             | Get cafeteria orders | Yes            |
| GET    | `/orders/{id}`        | Get order by ID      | Yes            |
| POST   | `/orders`             | Create new order     | Admin/Employee |
| PUT    | `/orders/{id}/status` | Update order status  | Admin/Employee |

**Create Cafeteria Item Request:**

```json
{
  "name": "شاي بالنعناع",
  "description": "شاي طبيعي بالنعناع الطازج",
  "price": 15.0,
  "category": "مشروبات ساخنة",
  "stockQuantity": 50,
  "minimumStock": 10,
  "isAvailable": true,
  "isActive": true
}
```

## 10. Rooms Controller (`/api/rooms`) - Assiut Branch Only

| Method | Endpoint             | Description             | Auth Required  |
| ------ | -------------------- | ----------------------- | -------------- |
| GET    | `/`                  | Get all rooms           | Yes            |
| GET    | `/{id}`              | Get room by ID          | Yes            |
| POST   | `/{id}/reserve`      | Create room reservation | Admin/Employee |
| GET    | `/{id}/reservations` | Get room reservations   | Yes            |
| PUT    | `/reservations/{id}` | Update reservation      | Admin/Employee |
| DELETE | `/reservations/{id}` | Cancel reservation      | Admin/Employee |

**Create Room Reservation Request:**

```json
{
  "title": "محاضرة الروبوتات المتقدمة",
  "description": "محاضرة تعريفية بالروبوتات المتقدمة",
  "reservationType": 1,
  "startDateTime": "2025-07-05T10:00:00.000Z",
  "endDateTime": "2025-07-05T12:00:00.000Z",
  "expectedAttendees": 20,
  "courseId": 1,
  "instructorId": 1,
  "requiredEquipment": "بروجكتر، سبورة ذكية",
  "specialRequirements": "تكييف قوي",
  "notes": "محاضرة مهمة للطلاب المتقدمين"
}
```

## 11. Attendance Controller (`/api/attendance`)

| Method | Endpoint               | Description              | Auth Required  |
| ------ | ---------------------- | ------------------------ | -------------- |
| GET    | `/course/{courseId}`   | Get course attendance    | Yes            |
| GET    | `/student/{studentId}` | Get student attendance   | Yes            |
| POST   | `/`                    | Record attendance        | Admin/Employee |
| PUT    | `/{id}`                | Update attendance record | Admin/Employee |
| GET    | `/sessions/{courseId}` | Get course sessions      | Yes            |

**Record Attendance Request:**

```json
{
  "courseId": 1,
  "studentId": 1,
  "sessionDate": "2025-07-02T10:00:00.000Z",
  "status": 1,
  "checkInTime": "2025-07-02T10:05:00.000Z",
  "checkOutTime": "2025-07-02T12:00:00.000Z",
  "notes": "حضر في الوقت المحدد"
}
```

## 12. Reports Controller (`/api/reports`)

| Method | Endpoint              | Description                   | Auth Required |
| ------ | --------------------- | ----------------------------- | ------------- |
| GET    | `/course-performance` | Get course performance report | Yes           |
| GET    | `/student-attendance` | Get student attendance report | Yes           |
| GET    | `/financial-summary`  | Get financial summary report  | Yes           |
| GET    | `/dashboard`          | Get dashboard data            | Yes           |

**Query Parameters:**

- `startDate`: Start date for report period
- `endDate`: End date for report period
- `branchId`: Filter by branch (optional)
- `courseId`: Filter by course (optional)
- `studentId`: Filter by student (optional)

## Data Models & Enums

### Payment Methods

```csharp
public enum PaymentMethod
{
    Cash = 1,           // نقدي
    BankTransfer = 2,   // تحويل بنكي
    CreditCard = 3,     // بطاقة ائتمان
    Check = 4           // شيك
}
```

### Payment Status

```csharp
public enum PaymentStatus
{
    Pending = 1,        // في الانتظار
    PartiallyPaid = 2,  // مدفوع جزئياً
    FullyPaid = 3,      // مدفوع بالكامل
    Overdue = 4,        // متأخر
    Cancelled = 5       // ملغي
}
```

### Course Status

```csharp
public enum CourseStatus
{
    Planned = 1,        // مخطط
    Active = 2,         // نشط
    Completed = 3,      // مكتمل
    Cancelled = 4,      // ملغي
    Suspended = 5       // معلق
}
```

### Attendance Status

```csharp
public enum AttendanceStatus
{
    Present = 1,        // حاضر
    Absent = 2,         // غائب
    Late = 3,           // متأخر
    Excused = 4         // معذور
}
```

### Equipment Status

```csharp
public enum EquipmentStatus
{
    Available = 1,      // متاح
    InUse = 2,          // قيد الاستخدام
    Maintenance = 3,    // صيانة
    Damaged = 4,        // تالف
    Retired = 5         // خارج الخدمة
}
```

## Error Handling

All endpoints return standardized error responses:

**Success Response (200):**

```json
{
  "message": "تم بنجاح",
  "data": { ... }
}
```

**Error Response (400/404/500):**

```json
{
  "message": "رسالة الخطأ بالعربية",
  "error": "Technical error details (English)"
}
```

## Authentication Headers

For protected endpoints, include the JWT token in the Authorization header:

```
Authorization: Bearer your_jwt_token_here
```

## CORS Configuration

The API supports CORS for frontend integration with the following headers:

- `Access-Control-Allow-Origin: *`
- `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`
- `Access-Control-Allow-Headers: Content-Type, Authorization`

## Database Schema

### Key Tables:

- **Branches**: Abu Tig, Assiut
- **Users**: System users with roles
- **Students**: Student information and registration
- **Employees**: Staff members
- **Courses**: Course catalog and management
- **CourseRegistrations**: Student course enrollments
- **Payments**: All payment transactions
- **WorkspaceBookings**: Workspace reservations (Assiut only)
- **Equipment**: Equipment inventory and tracking
- **Expenses**: Financial expense tracking
- **Attendance**: Student attendance records
- **Rooms**: Room management and reservations
- **CafeteriaItems/Orders**: Cafeteria management

## Testing the API

### Using Swagger UI:

1. Navigate to `http://localhost:5227/swagger`
2. Click "Authorize" and enter your JWT token
3. Test endpoints with sample data provided in this documentation

### Sample Test Data:

**Login Credentials:**

```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Test Student:**

```json
{
  "fullName": "أحمد محمد علي",
  "phone": "01234567890",
  "email": "ahmed@test.com",
  "age": 16,
  "gender": "Male",
  "parentName": "محمد علي",
  "parentPhone": "01098765432"
}
```

## System Features

### Multi-Branch Support:

- **Abu Tig Branch**: Course management only
- **Assiut Branch**: Full features including workspace and room scheduling

### Arabic Language Support:

- All user-facing messages in Arabic
- RTL text support
- Arabic date/time formatting

### Financial Management:

- Course registration payments
- Workspace billing (hourly rates)
- Expense tracking and approval workflow
- Comprehensive financial reporting

### Equipment Management:

- Inventory tracking
- Reservation system
- Maintenance scheduling
- Damage cost tracking

### Reporting & Analytics:

- Course performance metrics
- Student attendance tracking
- Financial summaries
- Dashboard with key metrics

## Deployment Notes

### Requirements:

- .NET 8.0 Runtime
- SQL Server 2019 or later
- IIS or Kestrel web server

### Configuration:

- Update connection string in `appsettings.json`
- Configure JWT settings
- Set CORS origins for production

### Database Migration:

```bash
dotnet ef database update
```

### Connection String Format:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=MEMEX\\SQLSERVER;Database=AIRoboticsCompanyDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

## API Testing Examples

### 1. Login and Get Token:

```bash
curl -X POST "http://localhost:5227/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### 2. Create Student (with token):

```bash
curl -X POST "http://localhost:5227/api/students" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"fullName":"أحمد محمد","phone":"01234567890","age":16,"gender":"Male"}'
```

### 3. Create Expense:

```bash
curl -X POST "http://localhost:5227/api/expenses" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"title":"فاتورة كهرباء","amount":500.00,"category":1,"paymentMethod":1}'
```

## Recent Updates (Latest Version)

### ✅ Enum Localization Enhancement

All API responses now include enum values in both English and Arabic for better user experience:

- Status fields show both `"status": "Active"` and `"statusArabic": "نشط"`
- Payment methods, course statuses, and all other enums are fully localized
- Consistent across all controllers and endpoints

### ✅ Shared Workspace System (New Feature)

Complete implementation of shared workspace functionality for Assiut branch:

- **Multi-user Support**: Multiple people can book and use the same workspace simultaneously
- **Real-time Occupancy Tracking**: Live monitoring of current vs. maximum capacity
- **Flexible Booking Types**: Individual, Group, Study, Meeting, Project bookings
- **Equipment Management**: WiFi, Printer, Projector, Whiteboard availability tracking
- **Individual Billing**: Each user pays separately even when sharing workspace
- **Check-in/Check-out System**: Real-time entry and exit management
- **Comprehensive Statistics**: Usage reports and occupancy analytics

### 🔧 Database Schema Updates

- New tables: `SharedWorkspaces`, `SharedWorkspaceBookings`, `WorkspaceOccupancies`
- Enhanced `Payments` table to support shared workspace billing
- Seed data for 2 shared workspaces in Assiut branch

## Conclusion

This AI Robotics Academy Management System provides a comprehensive solution for managing a multi-branch robotics academy with full Arabic language support. The system includes all necessary features for course management, student registration, individual workspace billing, shared workspace system, equipment tracking, and financial management.

**Key Highlights:**

- ✅ Multi-branch architecture (Abu Tig & Assiut)
- ✅ Complete Arabic language support with enum localization
- ✅ JWT-based authentication & authorization
- ✅ Comprehensive financial management
- ✅ Equipment inventory & reservation system
- ✅ Individual workspace billing (Assiut branch)
- ✅ **NEW**: Shared workspace system with multi-user support
- ✅ Room scheduling system
- ✅ Real-time occupancy tracking
- ✅ Detailed reporting & analytics
- ✅ RESTful API design with Swagger documentation

The system is production-ready and can be easily extended to support additional features as needed. The latest updates significantly enhance the user experience with proper Arabic localization and provide advanced workspace management capabilities for collaborative learning environments.
