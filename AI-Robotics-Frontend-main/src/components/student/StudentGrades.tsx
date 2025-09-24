import React, { useState, useEffect } from "react";
import axios from "../../lib/api/axios.ts";
import toast from "react-hot-toast";
import {
  BookOpen,
  Award,
  TrendingUp,
  Calendar,
  AlertCircle,
} from "lucide-react";
import { useAuth } from "../../contexts/AuthContext";

interface Grade {
  id: number;
  courseId: number;
  courseName: string;
  grade: number;
  notes: string;
  updatedAt: string;
}

const StudentGrades: React.FC = () => {
  const { user } = useAuth();
  const [grades, setGrades] = useState<Grade[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [studentId, setStudentId] = useState<number | null>(null);

  useEffect(() => {
    fetchStudentData();
  }, []);

  const fetchStudentData = async () => {
    try {
      setError(null);
      // أولاً نحصل على بيانات الطالب
      const studentResponse = await axios.get("/students/my-dashboard");
      if (studentResponse.data.success && studentResponse.data.data) {
        const studentData = studentResponse.data.data;
        const sid = studentData.studentId ?? studentData.id; // توافق مع DTO
        setStudentId(sid);

        if (!sid) {
          setGrades([]);
          setError("لم يتم العثور على معرف الطالب");
          return;
        }

        // ثم نحصل على درجات الطالب
        const gradesResponse = await axios.get(`/studentgrades/student/${sid}`);
        if (gradesResponse.data.success) {
          setGrades(gradesResponse.data.data);
        } else {
          setError(gradesResponse.data.message || "فشل في تحميل الدرجات");
        }
      } else {
        setError("فشل في تحميل بيانات الطالب");
      }
    } catch (error: any) {
      console.error(
        "Grades fetch error",
        error.response?.data || error.message
      );
      const errorMessage =
        error.response?.data?.message || "حدث خطأ في تحميل الدرجات";
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const getGradeColor = (grade: number) => {
    if (grade >= 90) return "text-green-600 bg-green-100 border-green-300";
    if (grade >= 80) return "text-blue-600 bg-blue-100 border-blue-300";
    if (grade >= 70) return "text-yellow-600 bg-yellow-100 border-yellow-300";
    if (grade >= 60) return "text-orange-600 bg-orange-100 border-orange-300";
    return "text-red-600 bg-red-100 border-red-300";
  };

  const getGradeText = (grade: number) => {
    if (grade >= 90) return "ممتاز";
    if (grade >= 80) return "جيد جداً";
    if (grade >= 70) return "جيد";
    if (grade >= 60) return "مقبول";
    return "ضعيف";
  };

  const getGradeIcon = (grade: number) => {
    if (grade >= 90) return "🏆";
    if (grade >= 80) return "🥇";
    if (grade >= 70) return "🥈";
    if (grade >= 60) return "🥉";
    return "📚";
  };

  const calculateAverage = () => {
    if (grades.length === 0) return 0;
    const total = grades.reduce((sum, grade) => sum + grade.grade, 0);
    return total / grades.length;
  };

  const getHighestGrade = () => {
    if (grades.length === 0) return null;
    return grades.reduce((highest, grade) =>
      grade.grade > highest.grade ? grade : highest
    );
  };

  const getLowestGrade = () => {
    if (grades.length === 0) return null;
    return grades.reduce((lowest, grade) =>
      grade.grade < lowest.grade ? grade : lowest
    );
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-secondary-900 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-primary-400 mx-auto mb-4"></div>
          <p className="text-primary-300 text-lg">جاري تحميل درجاتك...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-secondary-900 flex items-center justify-center">
        <div className="text-center">
          <AlertCircle className="w-16 h-16 text-red-400 mx-auto mb-4" />
          <p className="text-red-400 text-lg mb-4">{error}</p>
          <button
            onClick={fetchStudentData}
            className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors"
          >
            إعادة المحاولة
          </button>
        </div>
      </div>
    );
  }

  const average = calculateAverage();
  const highestGrade = getHighestGrade();
  const lowestGrade = getLowestGrade();

  return (
    <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-secondary-900 p-6">
      <div className="max-w-7xl mx-auto">
        <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl border border-primary-500/20">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-primary-500/20">
            <div className="flex items-center gap-3">
              <Award className="w-8 h-8 text-primary-400" />
              <h1 className="text-2xl font-bold text-white">درجاتي</h1>
            </div>
            <div className="text-sm text-primary-300">
              آخر تحديث: {new Date().toLocaleDateString("ar-EG")}
            </div>
          </div>

          {/* Statistics Cards */}
          <div className="p-6 border-b border-primary-500/20 bg-secondary-700/30">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
              <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-primary-300">المعدل العام</p>
                    <p className="text-2xl font-bold text-primary-400">
                      {average.toFixed(1)}%
                    </p>
                  </div>
                  <TrendingUp className="w-8 h-8 text-primary-400" />
                </div>
              </div>

              <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-primary-300">عدد الكورسات</p>
                    <p className="text-2xl font-bold text-blue-400">
                      {grades.length}
                    </p>
                  </div>
                  <BookOpen className="w-8 h-8 text-blue-400" />
                </div>
              </div>

              {highestGrade && (
                <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-primary-300">أعلى درجة</p>
                      <p className="text-2xl font-bold text-green-400">
                        {highestGrade.grade}%
                      </p>
                      <p className="text-xs text-primary-400">
                        {highestGrade.courseName}
                      </p>
                    </div>
                    <div className="text-2xl">🏆</div>
                  </div>
                </div>
              )}

              {lowestGrade && (
                <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-primary-300">أدنى درجة</p>
                      <p className="text-2xl font-bold text-orange-400">
                        {lowestGrade.grade}%
                      </p>
                      <p className="text-xs text-primary-400">
                        {lowestGrade.courseName}
                      </p>
                    </div>
                    <div className="text-2xl">📚</div>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Grades List */}
          <div className="p-6">
            {grades.length === 0 ? (
              <div className="text-center py-12">
                <BookOpen className="w-16 h-16 text-primary-400/50 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-primary-300 mb-2">
                  لا توجد درجات متاحة
                </h3>
                <p className="text-primary-400">
                  لم يتم تسجيل أي درجات لك بعد. ستظهر هنا بمجرد إضافة درجات من
                  قبل المدرب.
                </p>
              </div>
            ) : (
              <div className="space-y-4">
                {grades.map((grade) => (
                  <div
                    key={grade.id}
                    className="bg-secondary-800/30 backdrop-blur-sm border border-primary-500/20 rounded-2xl p-6 hover:bg-secondary-700/30 transition-colors"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-4">
                        <div className="text-3xl">
                          {getGradeIcon(grade.grade)}
                        </div>
                        <div>
                          <h3 className="text-lg font-semibold text-white">
                            {grade.courseName}
                          </h3>
                          <div className="flex items-center gap-4 mt-1">
                            <span className="text-sm text-primary-300">
                              <Calendar className="w-4 h-4 inline mr-1" />
                              {new Date(grade.updatedAt).toLocaleDateString(
                                "ar-EG"
                              )}
                            </span>
                          </div>
                        </div>
                      </div>

                      <div className="text-right">
                        <div
                          className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium border ${getGradeColor(
                            grade.grade
                          )}`}
                        >
                          {grade.grade}%
                        </div>
                        <div className="text-xs text-primary-400 mt-1">
                          {getGradeText(grade.grade)}
                        </div>
                      </div>
                    </div>

                    {grade.notes && (
                      <div className="mt-4 p-3 bg-secondary-700/50 rounded-lg border border-primary-500/10">
                        <p className="text-sm text-primary-200">
                          <strong className="text-primary-300">ملاحظات:</strong>{" "}
                          {grade.notes}
                        </p>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Performance Summary */}
          {grades.length > 0 && (
            <div className="p-6 border-t border-primary-500/20 bg-secondary-700/30">
              <h3 className="text-lg font-semibold text-white mb-4">
                ملخص الأداء
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
                  <div className="text-center">
                    <div className="text-2xl font-bold text-green-400 mb-1">
                      {grades.filter((g) => g.grade >= 90).length}
                    </div>
                    <div className="text-sm text-primary-300">درجات ممتازة</div>
                  </div>
                </div>
                <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
                  <div className="text-center">
                    <div className="text-2xl font-bold text-blue-400 mb-1">
                      {
                        grades.filter((g) => g.grade >= 80 && g.grade < 90)
                          .length
                      }
                    </div>
                    <div className="text-sm text-primary-300">
                      درجات جيد جداً
                    </div>
                  </div>
                </div>
                <div className="bg-secondary-800/50 backdrop-blur-sm rounded-2xl p-6 border border-primary-500/20">
                  <div className="text-center">
                    <div className="text-2xl font-bold text-yellow-400 mb-1">
                      {
                        grades.filter((g) => g.grade >= 70 && g.grade < 80)
                          .length
                      }
                    </div>
                    <div className="text-sm text-primary-300">درجات جيد</div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default StudentGrades;
