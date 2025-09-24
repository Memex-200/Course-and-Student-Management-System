import React, { useState, useEffect } from "react";
import { useAuth } from "../../contexts/AuthContext";
import {
  Calendar,
  Clock,
  User,
  Phone,
  Mail,
  CheckCircle,
  XCircle,
  Download,
} from "lucide-react";
import toast from "react-hot-toast";
import axios from "../../lib/api/axios.ts";

interface StudentCourse {
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
  driveLink?: string | null;
}

interface StudentDashboardData {
  studentId: number;
  studentName: string;
  phone: string;
  email: string;
  totalPaid: number;
  outstandingBalance: number;
  paymentHistory: any[];
  courses: StudentCourse[];
}

const StudentDashboard: React.FC = () => {
  const { user } = useAuth();
  const [studentInfo, setStudentInfo] = useState<StudentDashboardData | null>(
    null
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchStudentDashboard();
  }, []);

  const fetchStudentDashboard = async () => {
    try {
      setError(null);
      const response = await axios.get("/students/my-dashboard");
      if (response.data.success) {
        setStudentInfo(response.data.data);
      } else {
        setError(response.data.message || "فشل في تحميل بيانات الطالب");
        toast.error(response.data.message || "فشل في تحميل بيانات الطالب");
      }
    } catch (error: any) {
      console.error("Error fetching student dashboard:", error);
      const errorMessage =
        error.response?.data?.message || "حدث خطأ في تحميل البيانات";
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const getProgressColor = (progress: number) => {
    if (progress >= 80) return "text-green-400";
    if (progress >= 60) return "text-yellow-400";
    return "text-red-400";
  };

  const getProgressBgColor = (progress: number) => {
    if (progress >= 80) return "bg-green-500";
    if (progress >= 60) return "bg-yellow-500";
    return "bg-red-500";
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-secondary-900 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-primary-400 mx-auto mb-4"></div>
          <p className="text-primary-300 text-lg">جاري تحميل بياناتك...</p>
        </div>
      </div>
    );
  }

  if (!studentInfo && !loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-secondary-900 flex items-center justify-center">
        <div className="text-center">
          <XCircle className="w-16 h-16 text-red-400 mx-auto mb-4" />
          <p className="text-red-400 text-lg">
            {error || "لا توجد بيانات للطالب"}
          </p>
          <button
            onClick={fetchStudentDashboard}
            className="mt-4 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors"
          >
            إعادة المحاولة
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-secondary-900 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="bg-gradient-to-r from-primary-600 to-primary-700 rounded-3xl p-8 mb-8 shadow-2xl">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-white mb-2">
                مرحباً، {studentInfo?.studentName}
              </h1>
              <p className="text-primary-100">لوحة تحكم الطالب</p>
            </div>
            <div className="text-right">
              <div className="flex items-center gap-2 text-primary-100 mb-2">
                <Phone className="w-4 h-4" />
                <span>{studentInfo?.phone}</span>
              </div>
              {studentInfo?.email && (
                <div className="flex items-center gap-2 text-primary-100">
                  <Mail className="w-4 h-4" />
                  <span>{studentInfo.email}</span>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-6 gap-6 mb-8">
          <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-primary-300 text-sm">إجمالي الكورسات</p>
                <p className="text-2xl font-bold text-white">
                  {studentInfo?.courses?.length || 0}
                </p>
              </div>
              <span className="w-8 h-8 text-primary-400 text-2xl">📚</span>
            </div>
          </div>

          <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-primary-300 text-sm">الكورسات المكتملة</p>
                <p className="text-2xl font-bold text-white">
                  {studentInfo?.courses?.filter((c) => c.isCompleted).length ||
                    0}
                </p>
              </div>
              <span className="w-8 h-8 text-green-400 text-2xl">🏆</span>
            </div>
          </div>

          <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-primary-300 text-sm">إجمالي الحضور</p>
                <p className="text-2xl font-bold text-white">
                  {studentInfo?.courses?.reduce(
                    (sum, c) => sum + c.attendedSessions,
                    0
                  ) || 0}
                </p>
              </div>
              <CheckCircle className="w-8 h-8 text-green-400" />
            </div>
          </div>

          <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-primary-300 text-sm">إجمالي الغياب</p>
                <p className="text-2xl font-bold text-white">
                  {studentInfo?.courses?.reduce(
                    (sum, c) => sum + c.absentSessions,
                    0
                  ) || 0}
                </p>
              </div>
              <XCircle className="w-8 h-8 text-red-400" />
            </div>
          </div>

          <div className="bg-green-600/20 backdrop-blur-sm rounded-2xl p-6 border border-green-500/30">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-green-300 text-sm">إجمالي المدفوع</p>
                <p className="text-2xl font-bold text-green-400">
                  {studentInfo?.totalPaid?.toLocaleString() || 0} جنيه
                </p>
              </div>
              <span className="w-8 h-8 text-green-400 text-2xl">💰</span>
            </div>
          </div>

          <div
            className={`backdrop-blur-sm rounded-2xl p-6 border ${
              (studentInfo?.outstandingBalance || 0) > 0
                ? "bg-red-600/20 border-red-500/30"
                : "bg-green-600/20 border-green-500/30"
            }`}
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm">الرصيد المستحق</p>
                <p
                  className={`text-2xl font-bold ${
                    (studentInfo?.outstandingBalance || 0) > 0
                      ? "text-red-400"
                      : "text-green-400"
                  }`}
                >
                  {(studentInfo?.outstandingBalance || 0).toLocaleString()} جنيه
                </p>
              </div>
              <span
                className={`w-8 h-8 text-2xl ${
                  (studentInfo?.outstandingBalance || 0) > 0
                    ? "text-red-400"
                    : "text-green-400"
                }`}
              >
                {(studentInfo?.outstandingBalance || 0) > 0 ? "⚠️" : "✅"}
              </span>
            </div>
          </div>
        </div>

        {/* Notice */}
        <div className="mb-6 p-4 bg-blue-500/20 border border-blue-500/30 rounded-lg text-blue-300 text-center font-medium">
          أول جلسة من كل كورس مجانية. عند حضور الجلسة الثانية يتم تفعيل الدفع
          تلقائيًا لهذا الكورس.
        </div>

        {/* Payment History */}
        {(studentInfo?.paymentHistory?.length || 0) > 0 && (
          <div className="mb-8">
            <h2 className="text-2xl font-bold text-white mb-4 flex items-center gap-2">
              <span className="text-2xl">💰</span>
              سجل المدفوعات
            </h2>
            <div className="bg-secondary-800/30 backdrop-blur-sm rounded-2xl border border-primary-500/20 overflow-hidden">
              <div className="overflow-x-auto">
                <table className="min-w-full">
                  <thead className="bg-secondary-700/50">
                    <tr>
                      <th className="px-6 py-3 text-right text-sm font-semibold text-primary-300">
                        المبلغ
                      </th>
                      <th className="px-6 py-3 text-right text-sm font-semibold text-primary-300">
                        نوع الدفع
                      </th>
                      <th className="px-6 py-3 text-right text-sm font-semibold text-primary-300">
                        طريقة الدفع
                      </th>
                      <th className="px-6 py-3 text-right text-sm font-semibold text-primary-300">
                        التاريخ
                      </th>
                      <th className="px-6 py-3 text-right text-sm font-semibold text-primary-300">
                        الكورس
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-primary-500/20">
                    {studentInfo?.paymentHistory?.map((payment, index) => (
                      <tr key={index} className="hover:bg-secondary-700/30">
                        <td className="px-6 py-4 font-bold text-green-400">
                          {payment.amount.toLocaleString()} جنيه
                        </td>
                        <td className="px-6 py-4 text-primary-200">
                          {payment.paymentTypeArabic}
                        </td>
                        <td className="px-6 py-4 text-primary-200">
                          {payment.paymentMethodArabic}
                        </td>
                        <td className="px-6 py-4 text-primary-200">
                          {new Date(payment.paymentDate).toLocaleDateString(
                            "ar-EG"
                          )}
                        </td>
                        <td className="px-6 py-4 text-primary-200">
                          {payment.courseName || "-"}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}

        {/* Courses Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {studentInfo?.courses?.map((course) => (
            <div
              key={course.courseId}
              className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20"
            >
              <div className="flex items-start justify-between mb-4">
                <h3 className="text-lg font-semibold text-white">
                  {course.courseName}
                </h3>
                <div
                  className={`px-3 py-1 rounded-full text-xs font-medium ${
                    course.isCompleted
                      ? "bg-green-500/20 text-green-400 border border-green-500/30"
                      : "bg-blue-500/20 text-blue-400 border border-blue-500/30"
                  }`}
                >
                  {course.isCompleted ? "مكتمل" : "جاري"}
                </div>
              </div>

              {course.driveLink && (
                <a
                  href={course.driveLink}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-2 px-3 py-2 bg-primary-500/20 text-primary-300 rounded-lg hover:bg-primary-500/30 transition-colors text-sm mb-3"
                >
                  <span>📂</span>
                  <span>مواد الكورس (Drive)</span>
                  <span>↗</span>
                </a>
              )}

              {/* Progress Bar */}
              <div className="mb-4">
                <div className="flex justify-between items-center mb-2">
                  <span className="text-primary-300 text-sm">التقدم</span>
                  <span
                    className={`text-sm font-medium ${getProgressColor(
                      course.progress
                    )}`}
                  >
                    {course.progress}%
                  </span>
                </div>
                <div className="w-full bg-secondary-700 rounded-full h-2">
                  <div
                    className={`h-2 rounded-full transition-all duration-300 ${getProgressBgColor(
                      course.progress
                    )}`}
                    style={{ width: `${course.progress}%` }}
                  ></div>
                </div>
              </div>

              {/* Course Details */}
              <div className="space-y-3">
                <div className="flex items-center gap-2 text-primary-200">
                  <User className="w-4 h-4 text-primary-400" />
                  <span className="text-sm">
                    المدرب: {course.instructorName}
                  </span>
                </div>

                <div className="flex items-center gap-2 text-primary-200">
                  <Clock className="w-4 h-4 text-primary-400" />
                  <span className="text-sm">{course.schedule}</span>
                </div>

                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div className="flex items-center gap-2">
                    <CheckCircle className="w-4 h-4 text-green-400" />
                    <span className="text-primary-200">
                      حضر: {course.attendedSessions}
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <XCircle className="w-4 h-4 text-red-400" />
                    <span className="text-primary-200">
                      غاب: {course.absentSessions}
                    </span>
                  </div>
                </div>

                <div className="flex items-center gap-2 text-primary-200">
                  <span className="text-sm">
                    إجمالي الجلسات: {course.totalSessions}
                  </span>
                </div>

                {course.nextSessionDate && (
                  <div className="flex items-center gap-2 text-primary-200">
                    <Calendar className="w-4 h-4 text-yellow-400" />
                    <span className="text-sm">
                      الجلسة القادمة:{" "}
                      {new Date(course.nextSessionDate).toLocaleDateString(
                        "ar-EG"
                      )}
                    </span>
                  </div>
                )}

                {/* Payment Status */}
                <div className="flex items-center gap-2">
                  <span className="text-sm text-primary-300">حالة الدفع:</span>
                  <span
                    className={`px-2 py-1 rounded-full text-xs font-medium ${
                      course.paymentStatus === "FullyPaid"
                        ? "bg-green-500/20 text-green-400 border border-green-500/30"
                        : course.paymentStatus === "PartiallyPaid"
                        ? "bg-yellow-500/20 text-yellow-400 border border-yellow-500/30"
                        : "bg-red-500/20 text-red-400 border border-red-500/30"
                    }`}
                  >
                    {course.paymentStatusArabic}
                  </span>
                </div>

                {/* Certificate and Exam Score */}
                {course.isCompleted && (
                  <div className="pt-3 border-t border-primary-500/20">
                    <div className="flex items-center justify-between">
                      {course.certificateUrl && (
                        <button className="flex items-center gap-2 px-3 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors text-sm">
                          <Download className="w-4 h-4" />
                          تحميل الشهادة
                        </button>
                      )}
                      {course.examScore && (
                        <div className="flex items-center gap-2 text-primary-200">
                          <span className="w-4 h-4 text-yellow-400 text-2xl">
                            🏆
                          </span>
                          <span className="text-sm">
                            درجة الامتحان: {course.examScore}%
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>

        {(!studentInfo?.courses || studentInfo.courses.length === 0) && (
          <div className="text-center py-12">
            <span className="w-16 h-16 text-primary-400/50 text-2xl">📚</span>
            <h3 className="text-lg font-medium text-primary-300 mb-2">
              لا توجد كورسات مسجلة
            </h3>
            <p className="text-primary-400">
              تواصل مع الإدارة للتسجيل في الكورسات المتاحة
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default StudentDashboard;
