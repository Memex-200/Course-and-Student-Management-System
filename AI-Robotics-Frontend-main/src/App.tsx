//import React from "react";
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { AuthProvider } from "./contexts/AuthContext";
import { useAuth } from "./contexts/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import Layout from "./components/layout/Layout";
import LandingPage from "./components/portfolio/LandingPage";
import LoginForm from "./components/auth/LoginForm";
import RegisterForm from "./components/auth/RegisterForm";
import ChangePasswordForm from "./components/auth/ChangePasswordForm";
import Dashboard from "./components/dashboard/Dashboard";
import StudentList from "./components/students/StudentList";
import StudentForm from "./components/students/StudentForm";
import StudentView from "./components/students/StudentView";
import StudentDashboard from "./components/student/StudentDashboard";
import EnrolledCourses from "./components/student/EnrolledCourses";
import AvailableCourses from "./components/student/AvailableCourses";
import Certificates from "./components/student/Certificates";
import StudentAccount from "./components/students/StudentAccount";
import EmployeeDashboard from "./components/employee/EmployeeDashboard";
import { CourseList, CourseForm, CourseDetails } from "./components/courses";
import { UserRole } from "./types";
import AddTrainerForm from "./components/employee/AddTrainerForm";
import ExpensesPage from "./components/dashboard/ExpensesPage";
import PaymentManagement from "./components/payments/PaymentManagement";
import CafeteriaManagement from "./components/cafeteria/CafeteriaManagement";
import ProductsManagement from "./components/cafeteria/ProductsManagement";
import PointOfSale from "./components/cafeteria/PointOfSale";
import OrdersManagement from "./components/cafeteria/OrdersManagement";
import CafeteriaReports from "./components/cafeteria/CafeteriaReports";
import AttendanceManagement from "./components/dashboard/AttendanceManagement";

// Default redirect component based on user role
const DefaultRedirect = () => {
  const { user } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  switch (user.role) {
    case UserRole.Student:
      return <Navigate to="/student" replace />;
    case UserRole.Employee:
      return <Navigate to="/employee/dashboard" replace />;
    default:
      return <Navigate to="/dashboard" replace />;
  }
};

function App() {
  return (
    <AuthProvider>
      <Router future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <div className="App">
          <Toaster
            position="top-center"
            toastOptions={{
              duration: 4000,
              style: {
                background: "#363636",
                color: "#fff",
                direction: "rtl",
                fontSize: "16px",
                fontWeight: "500",
                padding: "16px",
                borderRadius: "8px",
              },
              success: {
                style: {
                  background: "#10B981",
                  color: "#FFFFFF",
                },
                iconTheme: {
                  primary: '#FFFFFF',
                  secondary: '#10B981',
                },
              },
              error: {
                style: {
                  background: "#EF4444",
                  color: "#FFFFFF",
                },
                iconTheme: {
                  primary: '#FFFFFF',
                  secondary: '#EF4444',
                },
              },
            }}
          />

          <Routes>
            {/* Public Routes */}
            <Route path="/" element={<LandingPage />} />
            <Route path="/login" element={<LoginForm />} />
            <Route path="/register" element={<RegisterForm />} />
            <Route path="/expenses" element={<ExpensesPage />} />
            <Route
              path="/payments"
              element={
                <ProtectedRoute
                  requiredRoles={[UserRole.Admin, UserRole.Employee]}
                >
                  <PaymentManagement />
                </ProtectedRoute>
              }
            />

            {/* Protected Routes */}
            <Route
              path="/change-password"
              element={
                <ProtectedRoute>
                  <ChangePasswordForm />
                </ProtectedRoute>
              }
            />

            {/* Default redirect based on user role */}
            <Route
              path="/dashboard/*"
              element={
                <ProtectedRoute
                  requiredRoles={[UserRole.Admin, UserRole.Employee]}
                >
                  <DefaultRedirect />
                </ProtectedRoute>
              }
            />

            <Route
              path="/employee/*"
              element={
                <ProtectedRoute
                  requiredRoles={[UserRole.Employee, UserRole.Admin]}
                >
                  <DefaultRedirect />
                </ProtectedRoute>
              }
            />

            <Route
              path="/student/*"
              element={
                <ProtectedRoute requiredRoles={[UserRole.Student]}>
                  <DefaultRedirect />
                </ProtectedRoute>
              }
            />

            {/* Admin Routes */}
            <Route element={<Layout />}>
              <Route
                path="/dashboard"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <Dashboard />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/dashboard/add-trainer"
                element={
                  <ProtectedRoute requiredRoles={[UserRole.Admin]}>
                    <AddTrainerForm />
                  </ProtectedRoute>
                }
              />
            </Route>

            {/* Employee Routes */}
            <Route element={<Layout />}>
              <Route
                path="/employee/dashboard"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Employee, UserRole.Admin]}
                  >
                    <EmployeeDashboard />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/students"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <StudentList />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/students/new"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <StudentForm />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/students/:id/edit"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <StudentForm />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/students/:id/view"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <StudentView />
                  </ProtectedRoute>
                }
              />

              {/* Course Routes */}
              <Route
                path="/courses"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <CourseList />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/courses/new"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <CourseForm />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/courses/:id"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <CourseDetails />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/courses/:id/edit"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <CourseForm />
                  </ProtectedRoute>
                }
              />
              
              {/* Cafeteria Routes */}
              <Route
                path="/cafeteria"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <CafeteriaManagement />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/cafeteria/products"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <ProductsManagement />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/cafeteria/point-of-sale"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <PointOfSale />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/cafeteria/orders"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <OrdersManagement />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/cafeteria/reports"
                element={
                  <ProtectedRoute
                    requiredRoles={[UserRole.Admin, UserRole.Employee]}
                  >
                    <CafeteriaReports />
                  </ProtectedRoute>
                }
              />
            </Route>

            {/* Student Routes */}
            <Route element={<Layout />}>
              <Route
                path="/student"
                element={
                  <ProtectedRoute requiredRoles={[UserRole.Student]}>
                    <StudentDashboard />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/student/account"
                element={
                  <ProtectedRoute requiredRoles={[UserRole.Student]}>
                    <StudentAccount />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/student/courses"
                element={
                  <ProtectedRoute requiredRoles={[UserRole.Student]}>
                    <EnrolledCourses />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/student/available-courses"
                element={
                  <ProtectedRoute requiredRoles={[UserRole.Student]}>
                    <AvailableCourses />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/student/certificates"
                element={
                  <ProtectedRoute requiredRoles={[UserRole.Student]}>
                    <Certificates />
                  </ProtectedRoute>
                }
              />
            </Route>

            {/* Attendance Routes */}
            <Route element={<Layout />}>
              <Route
                path="/attendance"
                element={
                  <ProtectedRoute requiredRoles={[UserRole.Admin, UserRole.Employee]}>
                    <AttendanceManagement />
                  </ProtectedRoute>
                }
              />
            </Route>

            {/* Catch-all redirect */}
            <Route path="*" element={<DefaultRedirect />} />
          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
