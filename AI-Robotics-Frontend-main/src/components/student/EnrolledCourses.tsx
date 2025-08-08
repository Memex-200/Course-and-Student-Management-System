import React, { useState, useEffect } from 'react';
//import { useAuth } from '../../contexts/AuthContext';
import { studentsAPI } from '../../lib/api';
import { Calendar, Clock, Users, CheckCircle } from 'lucide-react';

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
  nextSessionDate: string | null;
  schedule: string;
  instructorName: string;
  progress: number;
  certificateUrl?: string;
  examScore?: number;
}

interface StudentDashboard {
  studentId: number;
  studentName: string;
  phone: string;
  email: string;
  courses: StudentCourse[];
}

const EnrolledCourses: React.FC = () => {
  //const { user } = useAuth();
  const [selectedCourse, setSelectedCourse] = useState<number | null>(null);
  const [dashboardData, setDashboardData] = useState<StudentDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchEnrolledCourses();
  }, []);

  const fetchEnrolledCourses = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await studentsAPI.getMyDashboard();
      
      if (response.data.success) {
        setDashboardData(response.data.data);
        // Set first course as selected by default if exists
        if (response.data.data.courses.length > 0) {
          setSelectedCourse(response.data.data.courses[0].courseId);
        }
      } else {
        setError('فشل في جلب بيانات الكورسات');
      }
    } catch (err: any) {
      console.error('Error fetching enrolled courses:', err);
      setError(err.response?.data?.message || 'حدث خطأ في جلب البيانات');
    } finally {
      setLoading(false);
    }
  };

  const getProgressColor = (progress: number) => {
    if (progress >= 80) return 'bg-green-500';
    if (progress >= 60) return 'bg-blue-500';
    if (progress >= 40) return 'bg-yellow-500';
    return 'bg-red-500';
  };

  const getPaymentStatusColor = (status: string) => {
    switch (status) {
      case 'FullyPaid': return 'text-green-400';
      case 'PartiallyPaid': return 'text-yellow-400';
      case 'Unpaid': return 'text-red-400';
      case 'Pending': return 'text-blue-400';
      default: return 'text-gray-400';
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-500"></div>
            <span className="mr-3 text-white">جاري تحميل الكورسات...</span>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="bg-red-500/20 border border-red-500 rounded-lg p-4 text-center">
            <p className="text-red-300">{error}</p>
            <button 
              onClick={fetchEnrolledCourses}
              className="mt-2 px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 transition-colors"
            >
              إعادة المحاولة
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (!dashboardData || dashboardData.courses.length === 0) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 p-6">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-3xl font-bold text-white mb-2">الكورسات المسجل فيها</h1>
          <div className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-8 border border-primary-500/20 text-center">
            <div className="w-16 h-16 bg-primary-400 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">📚</span>
            </div>
            <h3 className="text-xl font-semibold text-white mb-2">لا توجد كورسات مسجلة</h3>
            <p className="text-gray-400">لم تقم بالتسجيل في أي كورس بعد. تواصل مع الإدارة للتسجيل في الكورسات المتاحة.</p>
          </div>
        </div>
      </div>
    );
  }

  const selectedCourseData = dashboardData.courses.find(c => c.courseId === selectedCourse);

  return (
    <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-white mb-2">الكورسات المسجل فيها</h1>
          <p className="text-gray-400">تابع تقدمك في الكورسات وشاهد المحاضرات القادمة</p>
          <div className="mt-4 flex items-center space-x-4 rtl:space-x-reverse">
            <span className="text-sm text-gray-300">الطالب: {dashboardData.studentName}</span>
            <span className="text-sm text-gray-400">•</span>
            <span className="text-sm text-gray-300">إجمالي الكورسات: {dashboardData.courses.length}</span>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Course List */}
          <div className="lg:col-span-1 space-y-4">
            {dashboardData.courses.map(course => (
              <div
                key={course.courseId}
                className={`bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-6 border cursor-pointer transition-all duration-300 ${
                  selectedCourse === course.courseId
                    ? 'border-primary-500 shadow-lg shadow-primary-500/20'
                    : 'border-primary-500/20 hover:border-primary-500/50'
                }`}
                onClick={() => setSelectedCourse(course.courseId)}
              >
                <div className="flex items-start justify-between mb-4">
                  <h3 className="text-xl font-semibold text-white">{course.courseName}</h3>
                  <div className="flex items-center space-x-2 rtl:space-x-reverse">
                    {course.isCompleted && course.certificateUrl && (
                      <div className="w-5 h-5 bg-yellow-400 rounded-full flex items-center justify-center">
                        <span className="text-xs">🏆</span>
                      </div>
                    )}
                    <div className="w-6 h-6 bg-primary-400 rounded-full flex items-center justify-center">
                      <span className="text-sm">🤖</span>
                    </div>
                  </div>
                </div>
                
                <div className="mb-4">
                  <div className="flex justify-between text-sm text-gray-400 mb-2">
                    <span>التقدم</span>
                    <span>{course.progress}%</span>
                  </div>
                  <div className="w-full h-2 bg-gray-700 rounded-full">
                    <div 
                      className={`h-full rounded-full ${getProgressColor(course.progress)}`}
                      style={{ width: `${course.progress}%` }}
                    />
                  </div>
                </div>
                
                <div className="flex items-center justify-between text-sm mb-2">
                  <span className="text-gray-400">حالة الدفع:</span>
                  <span className={getPaymentStatusColor(course.paymentStatus)}>
                    {course.paymentStatusArabic}
                  </span>
                </div>
                
                {course.nextSessionDate && (
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-400">المحاضرة القادمة:</span>
                  <span className="text-primary-400">
                      {new Date(course.nextSessionDate).toLocaleDateString('ar-EG', {
                        weekday: 'short',
                        month: 'short',
                        day: 'numeric'
                    })}
                  </span>
                </div>
                )}
              </div>
            ))}
          </div>

          {/* Course Details */}
          <div className="lg:col-span-2">
            {selectedCourseData ? (
              <div className="space-y-6">
                    {/* Course Overview */}
                <div className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-6 border border-primary-500/20">
                  <div className="flex items-start justify-between mb-4">
                    <h2 className="text-2xl font-bold text-white">{selectedCourseData.courseName}</h2>
                    {selectedCourseData.isCompleted && selectedCourseData.certificateUrl && (
                      <a
                        href={selectedCourseData.certificateUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center space-x-2 rtl:space-x-reverse bg-yellow-500/20 text-yellow-400 px-3 py-2 rounded-lg hover:bg-yellow-500/30 transition-colors"
                      >
                        <span>🏆</span>
                        <span>عرض الشهادة</span>
                        <span>↗</span>
                      </a>
                    )}
                  </div>
                      
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                        <div className="flex items-center space-x-3 rtl:space-x-reverse">
                          <Users className="w-5 h-5 text-primary-400" />
                          <div>
                            <p className="text-gray-400 text-sm">المدرب</p>
                        <p className="text-white">{selectedCourseData.instructorName}</p>
                          </div>
                        </div>
                        
                        <div className="flex items-center space-x-3 rtl:space-x-reverse">
                          <Calendar className="w-5 h-5 text-primary-400" />
                          <div>
                            <p className="text-gray-400 text-sm">المواعيد</p>
                        <p className="text-white">{selectedCourseData.schedule}</p>
                          </div>
                        </div>
                        
                        <div className="flex items-center space-x-3 rtl:space-x-reverse">
                      <span className="text-primary-400 text-lg">📖</span>
                          <div>
                            <p className="text-gray-400 text-sm">الدروس المكتملة</p>
                        <p className="text-white">{selectedCourseData.attendedSessions} من {selectedCourseData.totalSessions}</p>
                          </div>
                        </div>
                        
                        <div className="flex items-center space-x-3 rtl:space-x-reverse">
                      <span className="text-primary-400 text-lg">📺</span>
                          <div>
                        <p className="text-gray-400 text-sm">حالة الكورس</p>
                        <p className="text-white">{selectedCourseData.isCompleted ? 'مكتمل' : 'جاري'}</p>
                          </div>
                        </div>
                      </div>

                      {/* Progress Bar */}
                      <div className="mb-6">
                        <div className="flex justify-between text-sm text-gray-400 mb-2">
                          <span>التقدم الكلي</span>
                      <span>{selectedCourseData.progress}%</span>
                        </div>
                        <div className="w-full h-3 bg-gray-700 rounded-full">
                          <div 
                        className={`h-full rounded-full ${getProgressColor(selectedCourseData.progress)}`}
                        style={{ width: `${selectedCourseData.progress}%` }}
                          />
                        </div>
                  </div>

                  {/* Certificate Section */}
                  {selectedCourseData.isCompleted && (
                    <div className="bg-yellow-500/10 border border-yellow-500/30 rounded-lg p-4">
                      <div className="flex items-center space-x-3 rtl:space-x-reverse mb-2">
                        <span className="text-yellow-400 text-lg">🏆</span>
                        <h3 className="text-lg font-semibold text-yellow-400">تم إكمال الكورس!</h3>
                      </div>
                      {selectedCourseData.examScore && (
                        <p className="text-yellow-300 mb-2">درجة الامتحان: {selectedCourseData.examScore}%</p>
                      )}
                      {selectedCourseData.certificateUrl ? (
                        <a
                          href={selectedCourseData.certificateUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="inline-flex items-center space-x-2 rtl:space-x-reverse text-yellow-400 hover:text-yellow-300"
                        >
                          <span>تحميل الشهادة</span>
                          <span>↗</span>
                        </a>
                      ) : (
                        <p className="text-yellow-300">الشهادة قيد الإعداد...</p>
                      )}
                    </div>
                  )}
                    </div>

                    {/* Course Content */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {/* Attendance Stats */}
                      <div className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-6 border border-primary-500/20">
                        <h3 className="text-xl font-semibold text-white mb-4 flex items-center">
                      <CheckCircle className="w-5 h-5 ml-2 text-primary-400" />
                      إحصائيات الحضور
                        </h3>
                    <div className="space-y-3">
                      <div className="flex justify-between items-center">
                        <span className="text-gray-300">الجلسات الحاضرة</span>
                        <span className="text-green-400 font-semibold">{selectedCourseData.attendedSessions}</span>
                      </div>
                      <div className="flex justify-between items-center">
                        <span className="text-gray-300">الجلسات الغائبة</span>
                        <span className="text-red-400 font-semibold">{selectedCourseData.absentSessions}</span>
                      </div>
                      <div className="flex justify-between items-center">
                        <span className="text-gray-300">إجمالي الجلسات</span>
                        <span className="text-blue-400 font-semibold">{selectedCourseData.totalSessions}</span>
                      </div>
                      <div className="flex justify-between items-center pt-2 border-t border-gray-600">
                        <span className="text-gray-300">معدل الحضور</span>
                        <span className={`font-semibold ${selectedCourseData.progress > 70 ? 'text-green-400' : selectedCourseData.progress > 50 ? 'text-yellow-400' : 'text-red-400'}`}>
                          {selectedCourseData.progress}%
                        </span>
                      </div>
                    </div>
                      </div>

                  {/* Course Timeline */}
                      <div className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-6 border border-primary-500/20">
                        <h3 className="text-xl font-semibold text-white mb-4 flex items-center">
                          <Clock className="w-5 h-5 ml-2 text-primary-400" />
                      الجدول الزمني
                        </h3>
                    <div className="space-y-3">
                      <div className="flex justify-between items-center">
                        <span className="text-gray-300">تاريخ البداية</span>
                        <span className="text-blue-400">
                          {new Date(selectedCourseData.startDate).toLocaleDateString('ar-EG')}
                        </span>
                      </div>
                      <div className="flex justify-between items-center">
                        <span className="text-gray-300">تاريخ النهاية</span>
                        <span className="text-blue-400">
                          {new Date(selectedCourseData.endDate).toLocaleDateString('ar-EG')}
                        </span>
                      </div>
                      <div className="flex justify-between items-center">
                        <span className="text-gray-300">حالة الدفع</span>
                        <span className={getPaymentStatusColor(selectedCourseData.paymentStatus)}>
                          {selectedCourseData.paymentStatusArabic}
                        </span>
                      </div>
                      {selectedCourseData.nextSessionDate && (
                        <div className="flex justify-between items-center pt-2 border-t border-gray-600">
                          <span className="text-gray-300">الجلسة القادمة</span>
                          <span className="text-primary-400">
                            {new Date(selectedCourseData.nextSessionDate).toLocaleDateString('ar-EG', {
                              weekday: 'long',
                              month: 'short',
                              day: 'numeric'
                            })}
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            ) : (
              <div className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-8 border border-primary-500/20 text-center">
                <div className="w-16 h-16 bg-primary-400 rounded-full flex items-center justify-center mx-auto mb-4">
                  <span className="text-2xl">📚</span>
                </div>
                <h3 className="text-xl font-semibold text-white mb-2">اختر كورس</h3>
                <p className="text-gray-400">اختر كورس من القائمة لعرض التفاصيل</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default EnrolledCourses; 