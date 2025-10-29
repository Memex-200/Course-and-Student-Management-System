import React, { useState, useEffect } from "react";
import toast from "react-hot-toast";
import {
  BookOpen,
  User,
  Edit,
  Trash2,
  Plus,
  Search,
  RefreshCw,
} from "lucide-react";
import gradesAPI, {
  Student,
  Course,
  Grade,
  GradeFormData,
} from "../../lib/api/gradesAPI.ts";

const GradesManagement: React.FC = () => {
  const [grades, setGrades] = useState<Grade[]>([]);
  const [students, setStudents] = useState<Student[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingGrade, setEditingGrade] = useState<Grade | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedStudent, setSelectedStudent] = useState("");
  const [selectedCourse, setSelectedCourse] = useState("");
  const [error, setError] = useState<string | null>(null);

  const [form, setForm] = useState({
    studentId: "",
    courseId: "",
    grade: "",
    notes: "",
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    try {
      // Fetch all data using the API service
      console.log("[GradesManagement] Fetching grades, students, courses...");
      const [grades, students, courses] = await Promise.all([
        gradesAPI.getGrades(),
        gradesAPI.getStudents(),
        gradesAPI.getCourses(),
      ]);
      console.log("[GradesManagement] Received:", {
        gradesCount: grades?.length ?? 0,
        studentsCount: students?.length ?? 0,
        coursesCount: courses?.length ?? 0,
        sampleGrade: grades?.[0],
      });
      setGrades(grades);
      setStudents(students);
      setCourses(courses);
    } catch (error: any) {
      console.error("Error fetching data:", error);
      setError("حدث خطأ في جلب البيانات - يرجى المحاولة مرة أخرى");
      toast.error("حدث خطأ في جلب البيانات - يرجى المحاولة مرة أخرى");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    console.log("=== Grade Form Submit ===");
    console.log("Form data:", form);

    if (!form.studentId || !form.courseId || !form.grade) {
      console.log("Missing required fields");
      toast.error("جميع الحقول مطلوبة");
      return;
    }

    const gradeValue = parseFloat(form.grade);
    console.log("Parsed grade value:", gradeValue);

    const validation = gradesAPI.validateGrade(gradeValue);
    if (!validation.isValid) {
      console.log("Grade validation failed:", validation.message);
      toast.error(validation.message || "الدرجة يجب أن تكون بين 0 و 100");
      return;
    }

    try {
      let result;
      if (editingGrade) {
        console.log("Updating existing grade:", editingGrade.id);
        result = await gradesAPI.updateGrade(editingGrade.id, {
          grade: gradeValue,
          notes: form.notes,
        });
      } else {
        console.log("Creating new grade with data:", {
          studentId: parseInt(form.studentId),
          courseId: parseInt(form.courseId),
          grade: gradeValue,
          notes: form.notes,
        });
        result = await gradesAPI.createGrade({
          studentId: parseInt(form.studentId),
          courseId: parseInt(form.courseId),
          grade: gradeValue,
          notes: form.notes,
        });
      }

      console.log("API result:", result);

      if (result.success) {
        toast.success(result.message);
        setShowForm(false);
        setEditingGrade(null);
        resetForm();
        fetchData();
      } else {
        console.error("API returned error:", result.message);
        toast.error(result.message);
      }
    } catch (error: any) {
      console.error("Error saving grade:", error);
      console.error("Error details:", {
        message: error.message,
        response: error.response?.data,
        status: error.response?.status,
      });
      toast.error("حدث خطأ في حفظ الدرجة - يرجى المحاولة مرة أخرى");
    }
  };

  const handleEdit = (grade: Grade) => {
    setEditingGrade(grade);
    setForm({
      studentId: grade.studentId.toString(),
      courseId: grade.courseId.toString(),
      grade: grade.grade.toString(),
      notes: grade.notes,
    });
    setShowForm(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm("هل أنت متأكد من حذف هذه الدرجة؟")) return;

    const result = await gradesAPI.deleteGrade(id);
    if (result.success) {
      toast.success(result.message);
      fetchData();
    } else {
      toast.error(result.message);
    }
  };

  const resetForm = () => {
    setForm({
      studentId: "",
      courseId: "",
      grade: "",
      notes: "",
    });
  };

  const filteredGrades = grades.filter((grade) => {
    const matchesSearch =
      (grade.studentName || "")
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      (grade.courseName || "").toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStudent =
      !selectedStudent || grade.studentId.toString() === selectedStudent;
    const matchesCourse =
      !selectedCourse || grade.courseId.toString() === selectedCourse;

    return matchesSearch && matchesStudent && matchesCourse;
  });

  const getGradeColor = (grade: number) => {
    return gradesAPI.getGradeInfo(grade).color;
  };

  const getGradeText = (grade: number) => {
    return gradesAPI.getGradeInfo(grade).text;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-7xl mx-auto rtl" dir="rtl">
      <div className="bg-white rounded-xl shadow-sm border border-gray-200">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <BookOpen className="w-8 h-8 text-primary-600" />
            <h1 className="text-2xl font-bold text-gray-900">
              إدارة درجات الطلاب
            </h1>
          </div>
          <div className="flex gap-2">
            <button
              onClick={fetchData}
              disabled={loading}
              className="flex items-center gap-2 px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors disabled:opacity-50"
            >
              <RefreshCw
                className={`w-4 h-4 ${loading ? "animate-spin" : ""}`}
              />
              تحديث
            </button>
            <button
              onClick={() => {
                setShowForm(true);
                setEditingGrade(null);
                resetForm();
              }}
              className="flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors"
            >
              <Plus className="w-4 h-4" />
              إضافة درجة جديدة
            </button>
          </div>
        </div>

        {/* Error Message */}
        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 mx-6 mt-4 rounded">
            {error}
          </div>
        )}

        {/* Filters */}
        <div className="p-6 border-b border-gray-200 bg-gray-50">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                البحث
              </label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="text"
                  placeholder="البحث في الطلاب أو الكورسات..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                الطالب
              </label>
              <select
                value={selectedStudent}
                onChange={(e) => setSelectedStudent(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">جميع الطلاب</option>
                {students.map((student) => (
                  <option key={student.id} value={student.id}>
                    {student.fullName}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                الكورس
              </label>
              <select
                value={selectedCourse}
                onChange={(e) => setSelectedCourse(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">جميع الكورسات</option>
                {courses.map((course) => (
                  <option key={course.id} value={course.id}>
                    {course.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex items-end">
              <button
                onClick={() => {
                  setSearchTerm("");
                  setSelectedStudent("");
                  setSelectedCourse("");
                }}
                className="w-full px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
              >
                مسح الفلاتر
              </button>
            </div>
          </div>
        </div>

        {/* Grades Table */}
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  الطالب
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  الكورس
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  الدرجة
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  الملاحظات
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  آخر تحديث
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  الإجراءات
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredGrades.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    <div className="flex flex-col items-center gap-2">
                      <BookOpen className="w-8 h-8 text-gray-400" />
                      لا توجد درجات لعرضها
                    </div>
                  </td>
                </tr>
              ) : (
                filteredGrades.map((grade) => (
                  <tr key={grade.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <User className="w-4 h-4 text-gray-400 mr-2" />
                        <span className="text-sm font-medium text-gray-900">
                          {grade.studentName}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <BookOpen className="w-4 h-4 text-gray-400 mr-2" />
                        <span className="text-sm text-gray-900">
                          {grade.courseName}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-2">
                        <span
                          className={`px-2 py-1 rounded-full text-xs font-medium ${getGradeColor(
                            grade.grade
                          )}`}
                        >
                          {grade.grade}%
                        </span>
                        <span className="text-xs text-gray-500">
                          {getGradeText(grade.grade)}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span className="text-sm text-gray-900 max-w-xs truncate block">
                        {grade.notes || "-"}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {new Date(grade.updatedAt).toLocaleDateString("ar-EG")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleEdit(grade)}
                          className="text-primary-600 hover:text-primary-900"
                        >
                          <Edit className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(grade.id)}
                          className="text-red-600 hover:text-red-900"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Add/Edit Form Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md mx-4">
            <h2 className="text-xl font-bold mb-4">
              {editingGrade ? "تعديل الدرجة" : "إضافة درجة جديدة"}
            </h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  الطالب
                </label>
                <select
                  value={form.studentId}
                  onChange={(e) =>
                    setForm({ ...form, studentId: e.target.value })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  required
                  disabled={!!editingGrade}
                >
                  <option value="">اختر الطالب</option>
                  {students.map((student) => (
                    <option key={student.id} value={student.id}>
                      {student.fullName}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  الكورس
                </label>
                <select
                  value={form.courseId}
                  onChange={(e) =>
                    setForm({ ...form, courseId: e.target.value })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  required
                  disabled={!!editingGrade}
                >
                  <option value="">اختر الكورس</option>
                  {courses.map((course) => (
                    <option key={course.id} value={course.id}>
                      {course.name}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  الدرجة (0-100)
                </label>
                <input
                  type="number"
                  min="0"
                  max="100"
                  step="0.01"
                  value={form.grade}
                  onChange={(e) => setForm({ ...form, grade: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  الملاحظات
                </label>
                <textarea
                  value={form.notes}
                  onChange={(e) => setForm({ ...form, notes: e.target.value })}
                  rows={3}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors"
                >
                  {editingGrade ? "تحديث" : "إضافة"}
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowForm(false);
                    setEditingGrade(null);
                    resetForm();
                  }}
                  className="flex-1 px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
                >
                  إلغاء
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default GradesManagement;
