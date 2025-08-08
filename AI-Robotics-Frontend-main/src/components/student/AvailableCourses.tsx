import React, { useState, useEffect } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { coursesAPI } from '../../lib/api';
import { Calendar, Clock, Users, DollarSign } from 'lucide-react';

interface AvailableCourse {
  id: number;
  name: string;
  description: string;
  courseCategoryName: string;
  price: number;
  sessionsCount: number;
  maxStudents: number;
  currentStudents: number;
  availableSeats: number;
  startDate: string;
  endDate: string;
  status: number;
  branchName: string;
  instructorName: string;
  schedule: string;
  content: string;
  prerequisites: string;
  courseDays: string;
  startTime: string;
  endTime: string;
  createdAt: string;
}

const AvailableCourses: React.FC = () => {
  const { user } = useAuth();
  const [expandedCourse, setExpandedCourse] = useState<number | null>(null);
  const [courses, setCourses] = useState<AvailableCourse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchAvailableCourses();
  }, []);

  const fetchAvailableCourses = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await coursesAPI.getAvailable();
      
      if (response.data.success) {
        setCourses(response.data.data);
      } else {
        setError(response.data.message || 'فشل في جلب بيانات الكورسات');
      }
    } catch (err: any) {
      console.error('Error fetching available courses:', err);
      setError(err.response?.data?.message || 'حدث خطأ في جلب البيانات');
    } finally {
      setLoading(false);
    }
  };

  const toggleCourse = (courseId: number) => {
    setExpandedCourse(expandedCourse === courseId ? null : courseId);
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('ar-EG', {
      style: 'currency',
      currency: 'EGP',
      minimumFractionDigits: 0
    }).format(amount);
  };

  const getCourseStatus = (startDate: string, endDate: string) => {
    const now = new Date();
    const start = new Date(startDate);
    const end = new Date(endDate);

    if (now < start) {
      return { text: 'قريباً', color: 'bg-blue-500', emoji: '🔜' };
    } else if (now >= start && now <= end) {
      return { text: 'جاري الآن', color: 'bg-green-500', emoji: '▶️' };
    } else {
      return { text: 'انتهى', color: 'bg-gray-500', emoji: '✅' };
    }
  };

  const parsePrerequisites = (prerequisites: string): string[] => {
    if (!prerequisites || prerequisites.trim() === '') return ['لا توجد متطلبات سابقة'];
    
    // Try to split by common delimiters
    const delimiters = ['\n', '•', '-', '*', '،', ','];
    for (const delimiter of delimiters) {
      if (prerequisites.includes(delimiter)) {
        return prerequisites.split(delimiter)
          .map(item => item.trim())
          .filter(item => item.length > 0);
      }
    }
    
    // If no delimiters found, return as single item
    return [prerequisites.trim()];
  };

  const parseTopics = (content: string): string[] => {
    if (!content || content.trim() === '') return ['سيتم تحديد المحتوى قريباً'];
    
    // Try to split by common delimiters
    const delimiters = ['\n', '•', '-', '*', '،', ','];
    for (const delimiter of delimiters) {
      if (content.includes(delimiter)) {
        return content.split(delimiter)
          .map(item => item.trim())
          .filter(item => item.length > 0);
      }
    }
    
    // If no delimiters found, return as single item
    return [content.trim()];
  };

  const getSkillsFromCategory = (categoryName: string): string[] => {
    // Generate skills based on category
    const skillsMap: { [key: string]: string[] } = {
      'الروبوتيكس': ['Arduino', 'البرمجة المدمجة', 'التحكم الآلي', 'المستشعرات', 'المحركات'],
      'البرمجة': ['Programming', 'Problem Solving', 'Algorithm Design', 'Code Optimization'],
      'الذكاء الاصطناعي': ['Machine Learning', 'Deep Learning', 'Python', 'TensorFlow', 'Data Analysis'],
      'تطوير التطبيقات': ['Mobile Development', 'UI/UX Design', 'API Integration', 'Database Management']
    };

    // Find matching skills or return default
    for (const [key, skills] of Object.entries(skillsMap)) {
      if (categoryName.includes(key) || categoryName.toLowerCase().includes(key.toLowerCase())) {
        return skills;
      }
    }

    return ['مهارات تقنية', 'حل المشكلات', 'العمل الجماعي', 'التفكير الإبداعي'];
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-500"></div>
            <span className="mr-3 text-white">جاري تحميل الكورسات المتاحة...</span>
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
              onClick={fetchAvailableCourses}
              className="mt-2 px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 transition-colors"
            >
              إعادة المحاولة
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (courses.length === 0) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 p-6">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-3xl font-bold text-white mb-2">الكورسات المتاحة</h1>
          <div className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-8 border border-primary-500/20 text-center">
            <div className="w-16 h-16 bg-primary-400 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">📚</span>
            </div>
            <h3 className="text-xl font-semibold text-white mb-2">لا توجد كورسات متاحة</h3>
            <p className="text-gray-400">لا توجد كورسات متاحة للتسجيل في الوقت الحالي. تحقق مرة أخرى لاحقاً.</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-white mb-2">الكورسات المتاحة</h1>
          <p className="text-gray-400">اكتشف الكورسات الجديدة وابدأ رحلة التعلم</p>
          <div className="mt-4 flex items-center space-x-4 rtl:space-x-reverse">
            <span className="text-sm text-gray-300">إجمالي الكورسات المتاحة: {courses.length}</span>
          </div>
        </div>

        {/* Course List */}
        <div className="space-y-6">
          {courses.map(course => (
            <div
              key={course.id}
              className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl border border-primary-500/20 overflow-hidden"
            >
              {/* Course Header */}
              <div 
                className="p-6 cursor-pointer"
                onClick={() => toggleCourse(course.id)}
              >
                <div className="flex items-start justify-between mb-4">
                  <div>
                    <h2 className="text-2xl font-bold text-white mb-2">{course.name}</h2>
                    <p className="text-gray-300">{course.description}</p>
                    <div className="flex items-center gap-2 mt-2">
                      {course.courseCategoryName && (
                        <span className="inline-block px-3 py-1 bg-primary-500/20 text-primary-300 rounded-full text-sm">
                          {course.courseCategoryName}
                        </span>
                      )}
                      {(() => {
                        const status = getCourseStatus(course.startDate, course.endDate);
                        return (
                          <span className={`inline-flex items-center gap-1 px-3 py-1 ${status.color} text-white rounded-full text-sm`}>
                            <span>{status.emoji}</span>
                            {status.text}
                          </span>
                        );
                      })()}
                    </div>
                  </div>
                  <div className="w-8 h-8 bg-primary-400 rounded-full flex items-center justify-center">
                    <span className="text-lg">🎓</span>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
                  <div className="flex items-center space-x-3 rtl:space-x-reverse">
                    <Users className="w-5 h-5 text-primary-400" />
                    <div>
                      <p className="text-gray-400 text-sm">المدرب</p>
                      <p className="text-white">{course.instructorName || 'سيتم تحديده'}</p>
                    </div>
                  </div>

                  <div className="flex items-center space-x-3 rtl:space-x-reverse">
                    <Calendar className="w-5 h-5 text-primary-400" />
                    <div>
                      <p className="text-gray-400 text-sm">تاريخ البدء</p>
                      <p className="text-white">
                        {new Date(course.startDate).toLocaleDateString('ar-EG')}
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center space-x-3 rtl:space-x-reverse">
                    <Clock className="w-5 h-5 text-primary-400" />
                    <div>
                      <p className="text-gray-400 text-sm">عدد الجلسات</p>
                      <p className="text-white">{course.sessionsCount} جلسة</p>
                    </div>
                  </div>

                  <div className="flex items-center space-x-3 rtl:space-x-reverse">
                    <DollarSign className="w-5 h-5 text-primary-400" />
                    <div>
                      <p className="text-gray-400 text-sm">السعر</p>
                      <p className="text-white">{formatCurrency(course.price)}</p>
                    </div>
                  </div>
                </div>

                <div className="flex justify-center mt-6">
                  {expandedCourse === course.id ? (
                    <span className="text-primary-400 text-xl">▲</span>
                  ) : (
                    <span className="text-primary-400 text-xl">▼</span>
                  )}
                </div>
              </div>

              {/* Course Details */}
              {expandedCourse === course.id && (
                <div className="border-t border-primary-500/20 p-6">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {/* Prerequisites */}
                    <div>
                      <h3 className="text-xl font-semibold text-white mb-4 flex items-center">
                        <span className="text-primary-400 text-lg mr-2">📋</span>
                        المتطلبات الأساسية
                      </h3>
                      <ul className="space-y-2">
                        {parsePrerequisites(course.prerequisites).map((prereq, index) => (
                          <li key={index} className="text-gray-300 flex items-center">
                            <div className="w-2 h-2 bg-primary-400 rounded-full ml-2" />
                            {prereq}
                          </li>
                        ))}
                      </ul>
                    </div>

                    {/* Topics */}
                    <div>
                      <h3 className="text-xl font-semibold text-white mb-4 flex items-center">
                        <span className="text-primary-400 text-lg mr-2">📖</span>
                        محتوى الكورس
                      </h3>
                      <ul className="space-y-2">
                        {parseTopics(course.content).slice(0, 6).map((topic, index) => (
                          <li key={index} className="text-gray-300 flex items-center">
                            <div className="w-2 h-2 bg-primary-400 rounded-full ml-2" />
                            {topic}
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>

                  {/* Course Info */}
                  <div className="mt-6 grid grid-cols-1 md:grid-cols-3 gap-4 p-4 bg-secondary-800/30 rounded-lg">
                    <div className="text-center">
                      <p className="text-gray-400 text-sm">الجدول الزمني</p>
                      <p className="text-white font-semibold">{course.schedule || `${course.courseDays} ${course.startTime}-${course.endTime}`}</p>
                    </div>
                    <div className="text-center">
                      <p className="text-gray-400 text-sm">مدة الكورس</p>
                      <p className="text-white font-semibold">
                        {Math.ceil((new Date(course.endDate).getTime() - new Date(course.startDate).getTime()) / (1000 * 60 * 60 * 24 * 7))} أسابيع
                      </p>
                    </div>
                    <div className="text-center">
                      <p className="text-gray-400 text-sm">الفرع</p>
                      <p className="text-white font-semibold">{course.branchName}</p>
                    </div>
                  </div>

                  {/* Skills & Registration */}
                  <div className="mt-6 pt-6 border-t border-primary-500/20">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 items-center">
                      <div>
                        <h3 className="text-xl font-semibold text-white mb-4">المهارات المكتسبة</h3>
                        <div className="flex flex-wrap gap-2">
                          {getSkillsFromCategory(course.courseCategoryName).map((skill, index) => (
                            <span
                              key={index}
                              className="px-3 py-1 bg-primary-500/20 text-primary-300 rounded-full text-sm"
                            >
                              {skill}
                            </span>
                          ))}
                        </div>
                      </div>

                      <div className="text-center md:text-right">
                        <div className="mb-4">
                          <p className="text-gray-400 text-sm">
                            المقاعد المتاحة: {course.availableSeats} من {course.maxStudents}
                          </p>
                          <div className="w-full h-2 bg-gray-700 rounded-full mt-2">
                            <div 
                              className={`h-full rounded-full ${
                                course.availableSeats > course.maxStudents * 0.5 
                                  ? 'bg-green-500' 
                                  : course.availableSeats > 0 
                                    ? 'bg-yellow-500' 
                                    : 'bg-red-500'
                              }`}
                              style={{ width: `${(course.currentStudents / course.maxStudents) * 100}%` }}
                            />
                          </div>
                        </div>
                        <button 
                          className={`rounded-xl px-8 py-3 transition-colors ${
                            course.availableSeats > 0 && new Date(course.endDate) > new Date()
                              ? 'bg-primary-500 hover:bg-primary-600 text-white'
                              : 'bg-gray-600 text-gray-400 cursor-not-allowed'
                          }`}
                          disabled={course.availableSeats === 0 || new Date(course.endDate) <= new Date()}
                        >
                          {course.availableSeats === 0 
                            ? 'مكتمل العدد' 
                            : new Date(course.endDate) <= new Date()
                              ? 'انتهى الكورس'
                              : 'سجل الآن'
                          }
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default AvailableCourses; 