import { Student, Course as BaseCourse, Branch } from './index';

export interface EnrolledStudent {
  Id: number;
  RegistrationId: number;
  FullName: string;
  PaidAmount: number;
  RemainingAmount: number;
  PaymentStatus: string;
  PaymentStatusArabic: string;
  Phone: string;
  PaymentMethod?: string;
  PaymentMethodArabic?: string;
}

export interface CourseWithEnrollments extends BaseCourse {
  enrolledStudents?: EnrolledStudent[];
}

export interface CourseEnrollment {
  id: number;
  courseId: number;
  studentId: number;
  enrollmentDate: string;
  status: 'active' | 'completed' | 'dropped';
  course?: CourseWithEnrollments;
  student?: Student;
}

export interface CourseFormData {
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  schedule?: string;
  sessionsCount: number;
  maxStudents: number;
  price: number;
  instructorId?: number | null;
  courseCategoryId?: number | null;
  branchId?: number | null;
  roomId?: number | null;
  labId?: number | null;
  content: string;
  prerequisites: string;
  courseDays?: string;
  startTime?: string;
  endTime?: string;
  scheduleDetails: {
    days: ('Sunday' | 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday')[];
    startTime: string;
    endTime: string;
  };
} 