import React, { useState, useEffect } from "react";
import axios from "../../lib/api/axios.ts";
import toast from "react-hot-toast";
import { BookOpen, Award, TrendingUp, Calendar } from "lucide-react";
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
  const [studentId, setStudentId] = useState<number | null>(null);

  useEffect(() => {
    fetchStudentData();
  }, []);

  const fetchStudentData = async () => {
    try {
      // أولاً نحصل على بيانات الطالب
      const studentResponse = await axios.get("/students/my-dashboard");
      if (studentResponse.data.success && studentResponse.data.data) {
        const studentData = studentResponse.data.data;
        setStudentId(studentData.id);

        // ثم نحصل على درجات الطالب
        const gradesResponse = await axios.get(
          `/studentgrades/student/${studentData.id}`
        );
        if (gradesResponse.data.success) {
          setGrades(gradesResponse.data.data);
        }
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || "حدث خطأ في جلب البيانات");
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
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  const average = calculateAverage();
  const highestGrade = getHighestGrade();
  const lowestGrade = getLowestGrade();

  return (
    <div className="p-6 max-w-7xl mx-auto rtl">
      <div className="bg-white rounded-xl shadow-sm border border-gray-200">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <Award className="w-8 h-8 text-primary-600" />
            <h1 className="text-2xl font-bold text-gray-900">درجاتي</h1>
          </div>
          <div className="text-sm text-gray-500">
            آخر تحديث: {new Date().toLocaleDateString("ar-EG")}
          </div>
        </div>

        {/* Statistics Cards */}
        <div className="p-6 border-b border-gray-200 bg-gray-50">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <div className="bg-white rounded-lg p-4 border border-gray-200">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">المعدل العام</p>
                  <p className="text-2xl font-bold text-primary-600">
                    {average.toFixed(1)}%
                  </p>
                </div>
                <TrendingUp className="w-8 h-8 text-primary-600" />
              </div>
            </div>

            <div className="bg-white rounded-lg p-4 border border-gray-200">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">عدد الكورسات</p>
                  <p className="text-2xl font-bold text-blue-600">
                    {grades.length}
                  </p>
                </div>
                <BookOpen className="w-8 h-8 text-blue-600" />
              </div>
            </div>

            {highestGrade && (
              <div className="bg-white rounded-lg p-4 border border-gray-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">أعلى درجة</p>
                    <p className="text-2xl font-bold text-green-600">
                      {highestGrade.grade}%
                    </p>
                    <p className="text-xs text-gray-500">
                      {highestGrade.courseName}
                    </p>
                  </div>
                  <div className="text-2xl">🏆</div>
                </div>
              </div>
            )}

            {lowestGrade && (
              <div className="bg-white rounded-lg p-4 border border-gray-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">أدنى درجة</p>
                    <p className="text-2xl font-bold text-orange-600">
                      {lowestGrade.grade}%
                    </p>
                    <p className="text-xs text-gray-500">
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
              <BookOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                لا توجد درجات متاحة
              </h3>
              <p className="text-gray-500">
                لم يتم تسجيل أي درجات لك بعد. ستظهر هنا بمجرد إضافة درجات من قبل
                المدرب.
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {grades.map((grade) => (
                <div
                  key={grade.id}
                  className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4">
                      <div className="text-3xl">
                        {getGradeIcon(grade.grade)}
                      </div>
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">
                          {grade.courseName}
                        </h3>
                        <div className="flex items-center gap-4 mt-1">
                          <span className="text-sm text-gray-500">
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
                      <div className="text-xs text-gray-500 mt-1">
                        {getGradeText(grade.grade)}
                      </div>
                    </div>
                  </div>

                  {grade.notes && (
                    <div className="mt-4 p-3 bg-gray-50 rounded-lg">
                      <p className="text-sm text-gray-700">
                        <strong>ملاحظات:</strong> {grade.notes}
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
          <div className="p-6 border-t border-gray-200 bg-gray-50">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              ملخص الأداء
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="bg-white rounded-lg p-4 border border-gray-200">
                <div className="text-center">
                  <div className="text-2xl font-bold text-green-600 mb-1">
                    {grades.filter((g) => g.grade >= 90).length}
                  </div>
                  <div className="text-sm text-gray-600">درجات ممتازة</div>
                </div>
              </div>
              <div className="bg-white rounded-lg p-4 border border-gray-200">
                <div className="text-center">
                  <div className="text-2xl font-bold text-blue-600 mb-1">
                    {grades.filter((g) => g.grade >= 80 && g.grade < 90).length}
                  </div>
                  <div className="text-sm text-gray-600">درجات جيد جداً</div>
                </div>
              </div>
              <div className="bg-white rounded-lg p-4 border border-gray-200">
                <div className="text-center">
                  <div className="text-2xl font-bold text-yellow-600 mb-1">
                    {grades.filter((g) => g.grade >= 70 && g.grade < 80).length}
                  </div>
                  <div className="text-sm text-gray-600">درجات جيد</div>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default StudentGrades;
