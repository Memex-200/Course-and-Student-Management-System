import React from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { Award, Calendar, User, ArrowRight, Bot, Brain } from "lucide-react";

const StudentHome: React.FC = () => {
  const { user } = useAuth();

  // لا بيانات وهمية هنا؛ سيتم جلبها من صفحات متخصصة
  const enrolledCourses: Array<{
    id: number;
    name: string;
    progress: number;
    nextClass: string;
  }> = [];

  const availableCourses: Array<{
    id: number;
    name: string;
    startDate: string;
    price: number;
  }> = [];

  const certificates: Array<{ id: number; name: string; date: string }> = [];

  return (
    <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900">
      {/* Welcome Section */}
      <div className="bg-gradient-to-r from-primary-900/50 to-secondary-900/50 backdrop-blur-xl border-b border-primary-500/20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="text-center">
            <h1 className="text-4xl font-bold text-white mb-4">
              مرحباً بك، {user?.fullName}
            </h1>
            <p className="text-xl text-gray-300">
              استمر في رحلة التعلم مع شركة الذكاء الاصطناعي والروبوتات
            </p>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
          <div className="bg-gradient-to-br from-primary-800/50 to-primary-900/50 backdrop-blur-xl rounded-2xl p-6 border border-primary-500/20">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 mb-1">الكورسات المسجل فيها</p>
                <h3 className="text-3xl font-bold text-white">
                  {enrolledCourses.length}
                </h3>
              </div>
              <User className="w-12 h-12 text-primary-400" />
            </div>
          </div>

          <div className="bg-gradient-to-br from-accent-800/50 to-accent-900/50 backdrop-blur-xl rounded-2xl p-6 border border-accent-500/20">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 mb-1">الشهادات المحصلة</p>
                <h3 className="text-3xl font-bold text-white">
                  {certificates.length}
                </h3>
              </div>
              <Award className="w-12 h-12 text-accent-400" />
            </div>
          </div>

          <div className="bg-gradient-to-br from-electric-800/50 to-electric-900/50 backdrop-blur-xl rounded-2xl p-6 border border-electric-500/20">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 mb-1">المحاضرات القادمة</p>
                <h3 className="text-3xl font-bold text-white">2</h3>
              </div>
              <Calendar className="w-12 h-12 text-electric-400" />
            </div>
          </div>
        </div>

        {/* Enrolled Courses */}
        <section className="mb-12">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-white">
              الكورسات المسجل فيها
            </h2>
            <Link
              to="/student/courses"
              className="text-primary-400 hover:text-primary-300 flex items-center"
            >
              عرض الكل
              <ArrowRight className="w-4 h-4 mr-2" />
            </Link>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {enrolledCourses.map((course) => (
              <div
                key={course.id}
                className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-6 border border-primary-500/20"
              >
                <div className="flex items-start justify-between mb-4">
                  <h3 className="text-xl font-semibold text-white">
                    {course.name}
                  </h3>
                  <Bot className="w-6 h-6 text-primary-400" />
                </div>

                <div className="mb-4">
                  <div className="flex justify-between text-sm text-gray-400 mb-2">
                    <span>التقدم</span>
                    <span>{course.progress}%</span>
                  </div>
                  <div className="w-full h-2 bg-gray-700 rounded-full">
                    <div
                      className="h-full bg-primary-500 rounded-full"
                      style={{ width: `${course.progress}%` }}
                    />
                  </div>
                </div>

                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-400">المحاضرة القادمة:</span>
                  <span className="text-primary-400">
                    {new Date(course.nextClass).toLocaleDateString("ar-EG", {
                      weekday: "long",
                      hour: "numeric",
                      minute: "numeric",
                    })}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </section>

        {/* Enrolled Courses shortcut */}
        <section className="mb-12">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-white">كورساتي</h2>
            <Link
              to="/student/courses"
              className="text-primary-400 hover:text-primary-300 flex items-center"
            >
              إدارة الكورسات
              <ArrowRight className="w-4 h-4 mr-2" />
            </Link>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {enrolledCourses.length === 0 && (
              <div className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-6 border border-primary-500/20 text-center text-gray-300">
                لا توجد كورسات مسجلة بعد
              </div>
            )}
          </div>
        </section>

        {/* Latest Certificates */}
        <section>
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-white">الشهادات</h2>
            <Link
              to="/student/certificates"
              className="text-primary-400 hover:text-primary-300 flex items-center"
            >
              عرض الكل
              <ArrowRight className="w-4 h-4 mr-2" />
            </Link>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {certificates.map((cert) => (
              <div
                key={cert.id}
                className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-6 border border-primary-500/20"
              >
                <div className="flex items-start justify-between mb-4">
                  <h3 className="text-xl font-semibold text-white">
                    {cert.name}
                  </h3>
                  <Award className="w-6 h-6 text-warning-400" />
                </div>

                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-400">تاريخ الإصدار:</span>
                  <span className="text-white">
                    {new Date(cert.date).toLocaleDateString("ar-EG")}
                  </span>
                </div>

                <button className="w-full mt-4 bg-warning-500 hover:bg-warning-600 text-white rounded-xl py-2 transition-colors">
                  تحميل الشهادة
                </button>
              </div>
            ))}
          </div>
        </section>
      </div>
    </div>
  );
};

export default StudentHome;
