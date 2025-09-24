import React, { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { studentsAPI } from "../../lib/api";
import { Student } from "../../types";
import {
  ArrowRight,
  User,
  Phone,
  Mail,
  Users,
  Calendar,
  GraduationCap,
  Edit,
  Plus,
} from "lucide-react";
import toast from "react-hot-toast";
import { Link } from "react-router-dom";

// Course Registration Modal Component
const CourseRegistrationModal: React.FC<{
  isOpen: boolean;
  onClose: () => void;
  studentId: number;
  studentName: string;
}> = ({ isOpen, onClose, studentId, studentName }) => {
  const [courses, setCourses] = useState<any[]>([]);
  const [selectedCourse, setSelectedCourse] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    courseId: "",
    totalAmount: 0,
    paidAmount: 0, // يبدأ بصفر دائماً
    paymentMethod: 1,
    notes: "",
  });

  useEffect(() => {
    if (isOpen) {
      fetchCourses();
    }
  }, [isOpen]);

  const fetchCourses = async () => {
    try {
      const token = localStorage.getItem("token");
      const response = await fetch("/api/courses", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      const data = await response.json();
      if (data.success) {
        setCourses(data.data);
      }
    } catch (error) {
      console.error("Error fetching courses:", error);
      toast.error("خطأ في تحميل الكورسات");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await fetch(
        `/api/students/${studentId}/register-course`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
          body: JSON.stringify({
            courseId: parseInt(formData.courseId),
            totalAmount: formData.totalAmount,
            paidAmount: formData.paidAmount,
            paymentMethod: formData.paymentMethod,
            notes: formData.notes,
          }),
        }
      );

      const data = await response.json();

      if (response.ok) {
        console.log("Success response received:", response.status);
        console.log("Response data:", data);

        // استخدام إعدادات صريحة للون الأخضر
        toast.success("تم تسجيل الطالب في الكورس بنجاح", {
          className: "toast-success-green",
          style: {
            background: "#10B981",
            color: "#ffffff",
            border: "none",
            fontSize: "16px",
            fontWeight: "500",
            direction: "rtl",
          },
          duration: 4000,
        });
        console.log("Toast success called");
        onClose();
        // Refresh the page to show updated data
        window.location.reload();
      } else {
        toast.error(data.message || "حدث خطأ في تسجيل الطالب");
      }
    } catch (error) {
      console.error("Error registering student:", error);
      toast.error("حدث خطأ في تسجيل الطالب");
    } finally {
      setLoading(false);
    }
  };

  const handleCourseChange = (courseId: string) => {
    const course = courses.find((c) => c.id === parseInt(courseId));
    setSelectedCourse(course);
    setFormData((prev) => ({
      ...prev,
      courseId,
      totalAmount: course?.price || 0,
      paidAmount: 0, // دائماً صفر عند اختيار كورس جديد
    }));
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl p-6 w-full max-w-2xl mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold text-gray-900">
            تسجيل طالب في كورس
          </h2>
          <button
            onClick={onClose}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <span className="w-5 h-5">✕</span>
          </button>
        </div>

        <div className="mb-4 p-4 bg-blue-50 rounded-lg">
          <p className="text-blue-800">
            <strong>الطالب:</strong> {studentName}
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Course Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              اختر الكورس <span className="text-red-500">*</span>
            </label>
            <select
              value={formData.courseId}
              onChange={(e) => handleCourseChange(e.target.value)}
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              required
            >
              <option value="">اختر الكورس</option>
              {courses.map((course) => (
                <option key={course.id} value={course.id}>
                  {course.name} - {course.price} جنيه
                </option>
              ))}
            </select>
          </div>

          {/* Course Details */}
          {selectedCourse && (
            <div className="p-4 bg-gray-50 rounded-lg">
              <h3 className="font-semibold text-gray-900 mb-2">
                تفاصيل الكورس
              </h3>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-gray-600">الاسم:</span>
                  <span className="mr-2 font-medium">
                    {selectedCourse.name}
                  </span>
                </div>
                <div>
                  <span className="text-gray-600">السعر:</span>
                  <span className="mr-2 font-medium">
                    {selectedCourse.price} جنيه
                  </span>
                </div>
                <div>
                  <span className="text-gray-600">عدد الجلسات:</span>
                  <span className="mr-2 font-medium">
                    {selectedCourse.sessionsCount}
                  </span>
                </div>
                <div>
                  <span className="text-gray-600">الطلاب الحاليين:</span>
                  <span className="mr-2 font-medium">
                    {selectedCourse.currentStudents}/
                    {selectedCourse.maxStudents}
                  </span>
                </div>
              </div>
            </div>
          )}

          {/* Payment Information */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                المبلغ الإجمالي
              </label>
              <input
                type="number"
                value={formData.totalAmount}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    totalAmount: parseFloat(e.target.value) || 0,
                  }))
                }
                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                min="0"
                step="0.01"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                المبلغ المدفوع <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                value={formData.paidAmount}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    paidAmount: parseFloat(e.target.value) || 0,
                  }))
                }
                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                min="0"
                step="0.01"
                required
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              طريقة الدفع <span className="text-red-500">*</span>
            </label>
            <select
              value={formData.paymentMethod}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  paymentMethod: parseInt(e.target.value),
                }))
              }
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              required
            >
              <option value="1">كاش</option>
              <option value="2">انستا باي</option>
              <option value="3">فوري</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              ملاحظات
            </label>
            <textarea
              value={formData.notes}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, notes: e.target.value }))
              }
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              rows={3}
              placeholder="ملاحظات إضافية..."
            />
          </div>

          {/* Remaining Amount */}
          {formData.totalAmount > 0 && (
            <div className="p-4 bg-yellow-50 rounded-lg">
              <p className="text-yellow-800">
                <strong>المبلغ المتبقي:</strong>{" "}
                {formData.totalAmount - formData.paidAmount} جنيه
              </p>
            </div>
          )}

          <div className="flex items-center justify-end space-x-4 rtl:space-x-reverse">
            <button
              type="button"
              onClick={onClose}
              className="px-6 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
            >
              إلغاء
            </button>
            <button
              type="submit"
              disabled={loading || !formData.courseId}
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
            >
              {loading ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                  جاري التسجيل...
                </>
              ) : (
                <>
                  <Plus className="w-4 h-4 ml-2" />
                  تسجيل الطالب
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

const StudentView: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [student, setStudent] = useState<Student | null>(null);
  const [loading, setLoading] = useState(true);
  const [showRegistrationModal, setShowRegistrationModal] = useState(false);

  useEffect(() => {
    const fetchStudent = async () => {
      try {
        const response = await studentsAPI.getById(parseInt(id!));
        const apiResult = response.data as any;
        // API responses are shaped as { success, message, data }
        setStudent(apiResult.data ?? apiResult);
      } catch (error) {
        console.error("Error fetching student:", error);
        toast.error("خطأ في تحميل بيانات الطالب");
        navigate("/students");
      } finally {
        setLoading(false);
      }
    };

    fetchStudent();
  }, [id, navigate]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!student) {
    return null;
  }

  return (
    <div className="p-6 max-w-4xl mx-auto rtl">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center space-x-4 rtl:space-x-reverse">
          <button
            onClick={() => navigate("/students")}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <ArrowRight className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">بيانات الطالب</h1>
            <p className="text-gray-600">{student.fullName}</p>
          </div>
        </div>
        <div className="flex items-center space-x-4 rtl:space-x-reverse">
          <button
            onClick={() => setShowRegistrationModal(true)}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
          >
            <Plus className="w-4 h-4" />
            <span>تسجيل في كورس</span>
          </button>
          <Link
            to={`/students/${student.id}/edit`}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <Edit className="w-4 h-4" />
            <span>تعديل البيانات</span>
          </Link>
        </div>
      </div>

      {/* Personal Information */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center">
          <User className="w-5 h-5 ml-2 text-blue-600" />
          المعلومات الشخصية
        </h2>

        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <label className="text-sm font-medium text-gray-500">
              الاسم الكامل
            </label>
            <p className="mt-1 text-gray-900">{student.fullName}</p>
          </div>

          <div>
            <label className="text-sm font-medium text-gray-500">العمر</label>
            <p className="mt-1 text-gray-900">{student.age} سنة</p>
          </div>

          <div>
            <label className="text-sm font-medium text-gray-500">الجنس</label>
            <p className="mt-1 text-gray-900">
              {student.gender === "Male" ? "ذكر" : "أنثى"}
            </p>
          </div>

          <div>
            <label className="text-sm font-medium text-gray-500">
              رقم الهاتف
            </label>
            <p className="mt-1 text-gray-900 flex items-center">
              <Phone className="w-4 h-4 ml-1 text-gray-400" />
              {student.phone}
            </p>
          </div>

          {student.email && (
            <div>
              <label className="text-sm font-medium text-gray-500">
                البريد الإلكتروني
              </label>
              <p className="mt-1 text-gray-900 flex items-center">
                <Mail className="w-4 h-4 ml-1 text-gray-400" />
                {student.email}
              </p>
            </div>
          )}

          {student.address && (
            <div className="md:col-span-2">
              <label className="text-sm font-medium text-gray-500">
                العنوان
              </label>
              <p className="mt-1 text-gray-900 flex items-center">
                <span className="w-4 h-4 ml-1 text-gray-400">📍</span>
                {student.address}
              </p>
            </div>
          )}

          <div>
            <label className="text-sm font-medium text-gray-500">الفرع</label>
            <p className="mt-1 text-gray-900">{student.branch?.name}</p>
          </div>

          <div>
            <label className="text-sm font-medium text-gray-500">الحالة</label>
            <p className="mt-1">
              {student.isActive ? (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                  نشط
                </span>
              ) : (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                  غير نشط
                </span>
              )}
            </p>
          </div>
        </div>
      </div>

      {/* Parent Information */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center">
          <Users className="w-5 h-5 ml-2 text-green-600" />
          بيانات ولي الأمر
        </h2>

        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <label className="text-sm font-medium text-gray-500">
              اسم ولي الأمر
            </label>
            <p className="mt-1 text-gray-900">{student.parentName}</p>
          </div>

          <div>
            <label className="text-sm font-medium text-gray-500">
              هاتف ولي الأمر
            </label>
            <p className="mt-1 text-gray-900 flex items-center">
              <Phone className="w-4 h-4 ml-1 text-gray-400" />
              {student.parentPhone}
            </p>
          </div>

          {student.parentEmail && (
            <div>
              <label className="text-sm font-medium text-gray-500">
                بريد ولي الأمر
              </label>
              <p className="mt-1 text-gray-900 flex items-center">
                <Mail className="w-4 h-4 ml-1 text-gray-400" />
                {student.parentEmail}
              </p>
            </div>
          )}

          {student.preferredTransportation && (
            <div>
              <label className="text-sm font-medium text-gray-500">
                وسيلة النقل المفضلة
              </label>
              <p className="mt-1 text-gray-900 flex items-center">
                <span className="w-4 h-4 ml-1 text-gray-400">🚗</span>
                {student.preferredTransportation}
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Emergency Contact */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center">
          <span className="w-5 h-5 ml-2 text-red-600">⚠️</span>
          معلومات الطوارئ
        </h2>

        <div className="grid md:grid-cols-2 gap-6">
          {student.emergencyContact && (
            <div>
              <label className="text-sm font-medium text-gray-500">
                جهة الاتصال في الطوارئ
              </label>
              <p className="mt-1 text-gray-900">{student.emergencyContact}</p>
            </div>
          )}

          {student.emergencyPhone && (
            <div>
              <label className="text-sm font-medium text-gray-500">
                هاتف الطوارئ
              </label>
              <p className="mt-1 text-gray-900 flex items-center">
                <Phone className="w-4 h-4 ml-1 text-gray-400" />
                {student.emergencyPhone}
              </p>
            </div>
          )}

          {student.medicalConditions && (
            <div className="md:col-span-2">
              <label className="text-sm font-medium text-gray-500">
                الحالة الطبية
              </label>
              <p className="mt-1 text-gray-900 flex items-center">
                <span className="w-4 h-4 ml-1 text-gray-400">🏥</span>
                {student.medicalConditions}
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Course Information */}
      <div className="bg-white rounded-xl shadow-sm p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center">
          <GraduationCap className="w-5 h-5 ml-2 text-purple-600" />
          معلومات الكورسات
        </h2>

        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <label className="text-sm font-medium text-gray-500">
              عدد الكورسات المسجلة
            </label>
            <p className="mt-1 text-gray-900">
              {student.registeredCourses || 0} كورس
            </p>
          </div>

          <div>
            <label className="text-sm font-medium text-gray-500">
              تاريخ التسجيل
            </label>
            <p className="mt-1 text-gray-900 flex items-center">
              <Calendar className="w-4 h-4 ml-1 text-gray-400" />
              {new Date(student.createdAt).toLocaleDateString("ar-EG")}
            </p>
          </div>
        </div>
      </div>

      {/* Course Registration Modal */}
      <CourseRegistrationModal
        isOpen={showRegistrationModal}
        onClose={() => setShowRegistrationModal(false)}
        studentId={student.id}
        studentName={student.fullName}
      />
    </div>
  );
};

export default StudentView;
