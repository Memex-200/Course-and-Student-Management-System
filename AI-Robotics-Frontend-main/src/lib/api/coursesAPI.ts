import axios from "./axios";
import {
  CourseWithEnrollments,
  CourseFormData,
  CourseEnrollment,
} from "../../types/course";
import { Student } from "../../types";

interface TestDataResponse {
  branches: Array<{ id: number; name: string }>;
  categories: Array<{ id: number; name: string }>;
  rooms: Array<{ id: number; name: string; branchId: number }>;
  labs: Array<{ id: number; name: string; branchId: number }>;
  employees: Array<{ id: number; fullName: string; branchId: number }>;
}

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

const coursesAPI = {
  // Course Management
  getAll: (params?: any) =>
    axios.get<ApiResponse<CourseWithEnrollments[]>>("/courses", { params }),
  getById: (id: number) =>
    axios.get<ApiResponse<CourseWithEnrollments>>(`/courses/${id}`),
  create: (data: CourseFormData) =>
    axios.post<ApiResponse<{ courseId: number }>>("/courses", data),
  update: (id: number, data: Partial<CourseFormData>) =>
    axios.put<ApiResponse<void>>(`/courses/${id}`, data),
  delete: (id: number) => axios.delete<ApiResponse<void>>(`/courses/${id}`),
  getTestData: () => axios.get<TestDataResponse>("/courses/test-data"),

  // Course Categories
  getCategories: () =>
    axios.get<ApiResponse<Array<{ id: number; name: string }>>>(
      "/courses/categories"
    ),

  // Course Enrollment
  getEnrollments: (courseId: number) =>
    axios.get<ApiResponse<CourseEnrollment[]>>(
      `/courses/${courseId}/enrollments`
    ),
  enrollStudent: (courseId: number, studentId: number, enrollmentData: any) =>
    axios.post<ApiResponse<void>>(`/students/${studentId}/register-course`, {
      courseId,
      ...enrollmentData,
    }),
  updateEnrollment: (registrationId: number, status: string) =>
    axios.put<ApiResponse<void>>(
      `/students/course-registrations/${registrationId}/update-status`,
      { status }
    ),
  removeStudent: (courseId: number, studentId: number) =>
    axios.delete<ApiResponse<void>>(
      `/courses/${courseId}/enrollments/${studentId}`
    ),
  removeStudentByRegistrationId: (registrationId: number) =>
    axios.delete<ApiResponse<void>>(
      `/payments/course-registrations/${registrationId}`
    ),

  // Student's Courses
  getStudentCourses: (studentId: number) =>
    axios.get<ApiResponse<CourseWithEnrollments[]>>(
      `/students/${studentId}/courses`
    ),
  getAvailableCourses: (studentId: number) =>
    axios.get<ApiResponse<CourseWithEnrollments[]>>(
      `/students/${studentId}/available-courses`
    ),
  getAvailableStudents: (courseId: number) =>
    axios.get<ApiResponse<Student[]>>(
      `/courses/${courseId}/available-students`
    ),

  // Payment Management
  updateCoursePayment: (
    registrationId: number,
    data: {
      paidAmount: number;
      paymentMethod: number;
      notes?: string;
    }
  ) =>
    axios.put<
      ApiResponse<{
        message: string;
        paidAmount: number;
        remainingAmount: number;
        paymentStatus: string;
        paymentStatusArabic: string;
      }>
    >(`/students/course-registrations/${registrationId}/update-payment`, data),

  // Student Account Management
  createStudentAccount: (registrationId: number) =>
    axios.post<
      ApiResponse<{
        success: boolean;
        message: string;
        username: string;
        password: string;
        studentName: string;
        courseName: string;
      }>
    >(`/students/course-registrations/${registrationId}/create-account`),

  // Student Dashboard
  getStudentDashboard: () =>
    axios.get<
      ApiResponse<{
        studentId: number;
        studentName: string;
        phone: string;
        email: string;
        courses: Array<{
          courseId: number;
          courseName: string;
          startDate: string;
          endDate: string;
          totalSessions: number;
          attendedSessions: number;
          absentSessions: number;
          isCompleted: boolean;
          paymentStatus: string;
          paymentStatusArabic: string;
          nextSessionDate?: string;
          schedule: string;
          instructorName: string;
          progress: number;
          certificateUrl?: string;
          examScore?: number;
        }>;
      }>
    >(`/students/my-dashboard`),

  // Certificate Management
  issueCertificate: (
    registrationId: number,
    data: {
      examScore?: number;
      notes?: string;
    }
  ) =>
    axios.post<
      ApiResponse<{
        success: boolean;
        message: string;
        certificateNumber: string;
        certificateUrl: string;
        examScore?: number;
      }>
    >(
      `/students/course-registrations/${registrationId}/issue-certificate`,
      data
    ),
};

export default coursesAPI;
export { coursesAPI };
