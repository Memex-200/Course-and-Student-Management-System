# AI Robotics Company Management System

## Table of Contents
- [Project Overview](#project-overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Backend (API)](#backend-api)
  - [API Endpoints](#api-endpoints)
  - [Database Schema](#database-schema)
  - [How to Run the Backend](#how-to-run-the-backend)
- [Frontend (Web App)](#frontend-web-app)
  - [Main Routes & Pages](#main-routes--pages)
  - [How to Run the Frontend](#how-to-run-the-frontend)
- [Authentication & Authorization](#authentication--authorization)
- [Project Structure](#project-structure)
- [Screenshots](#screenshots)
- [License](#license)

---

## Project Overview
This project is a full management system for an AI Robotics Academy/Company. It covers student registration, course management, employee and admin dashboards, workspace and equipment booking, cafeteria, and more. The system is built with a React frontend and a .NET 8 (C#) backend API, using MySQL for data storage.

## Features
- User authentication (Admin, Employee, Student roles)
- Student management (CRUD, registration, attendance)
- Course management (CRUD, categories, registration)
- Employee dashboard
- Workspace and equipment booking
- Cafeteria orders
- Payments and expenses
- Reporting and statistics

## Tech Stack
- **Frontend:** React, TypeScript, TailwindCSS
- **Backend:** .NET 8 (ASP.NET Core Web API)
- **Database:** MySQL
- **Authentication:** JWT (JSON Web Tokens)

---

## Backend (API)

### API Endpoints
Below are some of the main API endpoints (all under `/api/`):

#### Auth
- `POST /api/auth/login` — User login (returns JWT)
- `POST /api/auth/register` — Register new user
- `PUT /api/auth/update` — Update user profile (auth required)
- `PUT /api/auth/update-role/{userId}` — Update user role (admin only)

#### Students
- `GET /api/students` — List students (filter by branch/search)
- `GET /api/students/{id}` — Get student details
- `POST /api/students` — Create new student
- `PUT /api/students/{id}` — Update student
- `DELETE /api/students/{id}` — Delete student
- `POST /api/students/{studentId}/register-course` — Register student to course

#### Courses
- `GET /api/courses` — List courses (filter by branch/category)
- `GET /api/courses/{id}` — Get course details
- `POST /api/courses` — Create new course
- `PUT /api/courses/{id}` — Update course
- `DELETE /api/courses/{id}` — Delete course
- `GET /api/courses/categories` — List course categories

#### Other Modules
- `GET /api/attendance` — Attendance management
- `GET /api/equipment` — Equipment management
- `GET /api/expenses` — Expenses management
- `GET /api/payments` — Payments management
- `GET /api/reports` — Reports and statistics
- `GET /api/rooms` — Room management
- `GET /api/sharedworkspace` — Shared workspace booking
- `GET /api/cafeteria` — Cafeteria orders

> **Note:** All endpoints require JWT authentication except for login/register.

### Database Schema
The main tables and their relationships:
- **User**: (Id, FullName, Username, Email, Phone, PasswordHash, Role, BranchId, ...)
- **Student**: (Id, FullName, Age, Phone, Email, Gender, School, Grade, Parent info, Emergency info, BranchId, ...)
- **Course**: (Id, Name, Description, Category, Price, SessionsCount, MaxStudents, Dates, Status, BranchId, ...)
- **CourseRegistration**: (Id, StudentId, CourseId, PaymentStatus, ...)
- **Attendance**: (Id, StudentId, CourseId, Status, ...)
- **Employee, Equipment, Room, Lab, WorkspaceBooking, CafeteriaOrder, Payment, Expense, Branch, ...**

> See `/Api/Models/` for full model definitions.

#### Example: Student Table
```csharp
public class Student {
  public int Id { get; set; }
  public string FullName { get; set; }
  public int Age { get; set; }
  public string Phone { get; set; }
  public string Email { get; set; }
  public string Gender { get; set; }
  public string School { get; set; }
  public string Grade { get; set; }
  public string ParentPhone { get; set; }
  public string ParentName { get; set; }
  public string ParentEmail { get; set; }
  public string EmergencyContact { get; set; }
  public string EmergencyPhone { get; set; }
  public string MedicalConditions { get; set; }
  public string PreferredTransportation { get; set; }
  public int BranchId { get; set; }
  public bool IsActive { get; set; }
  public DateTime CreatedAt { get; set; }
  // ...
}
```

### How to Run the Backend
1. Install [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. Install [MySQL](https://dev.mysql.com/downloads/installer/) and create a database named `AIRoboticsCompanyDB` (see `Api/appsettings.json` for connection string)
3. Update the connection string in `Api/appsettings.json` if needed
4. Run migrations (if needed):
   ```bash
   cd Api
   dotnet ef database update
   ```
5. Start the API:
   ```bash
   dotnet run
   ```
6. The API will be available at `https://localhost:5001` (or as configured)
7. Swagger docs available at `/swagger` in development mode

---

## Frontend (Web App)

### Main Routes & Pages
The frontend is built with React and TypeScript. Main routes (see `src/App.tsx`):

- `/` — Landing Page
- `/login` — Login
- `/register` — Register
- `/dashboard` — Admin/Employee dashboard
- `/employee/dashboard` — Employee dashboard
- `/students` — Student list (admin/employee)
- `/students/new` — Add student
- `/students/:id/edit` — Edit student
- `/students/:id/view` — Student details
- `/courses` — Course list
- `/courses/new` — Add course
- `/courses/:id` — Course details
- `/courses/:id/edit` — Edit course
- `/student` — Student home
- `/student/account` — Student account
- `/student/courses` — Enrolled courses
- `/student/available-courses` — Available courses
- `/student/certificates` — Certificates

### How to Run the Frontend
1. Install [Node.js](https://nodejs.org/)
2. Go to the frontend directory:
   ```bash
   cd AI-Robotics-Frontend-main
   ```
3. Install dependencies:
   ```bash
   npm install
   ```
4. Start the development server:
   ```bash
   npm run dev
   ```
5. The app will be available at `http://localhost:5173`

---

## Authentication & Authorization
- Uses JWT for secure authentication
- Roles: Admin, Employee, Student
- Protected routes in both backend and frontend
- Role-based access control for API and UI

---

## Project Structure
```
AI-Robotics-Company-Management-System-main/
├── Api/                # Backend (.NET 8 Web API)
│   ├── Controllers/    # API endpoints
│   ├── Models/         # Database models
│   ├── Data/           # DbContext
│   ├── DTOs/           # Data Transfer Objects
│   ├── Migrations/     # EF Core migrations
│   └── ...
├── AI-Robotics-Frontend-main/  # Frontend (React)
│   ├── src/
│   │   ├── components/ # React components
│   │   ├── contexts/   # React context (Auth, etc.)
│   │   ├── lib/        # API calls
│   │   └── types/      # TypeScript types
│   └── ...
└── ...
```

---

## Screenshots
Add screenshots from the `AI Robotics UI- Old/` folder here to illustrate the main features and UI.

---

## License
Specify your license here (MIT, Apache, etc.) 