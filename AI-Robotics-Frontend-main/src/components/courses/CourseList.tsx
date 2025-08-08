import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { coursesAPI } from "../../lib/api";
import { CourseWithEnrollments } from "../../types/course";
import { useAuth } from "../../contexts/AuthContext";
import toast from "react-hot-toast";
import {
  Search,
  Plus,
  Edit,
  Trash2,
  Calendar,
  Users,
  Filter,
  Download,
  RotateCw,
  Eye,
  GraduationCap,
  Clock,
  DollarSign,
} from "lucide-react";
import * as XLSX from "xlsx";

const CourseList: React.FC = () => {
  const { user } = useAuth();
  const [courses, setCourses] = useState<CourseWithEnrollments[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [filteredCourses, setFilteredCourses] = useState<
    CourseWithEnrollments[]
  >([]);
  const [filterStatus, setFilterStatus] = useState<string>("all");

  useEffect(() => {
    fetchCourses();
  }, []);

  useEffect(() => {
    const filtered = courses.filter(
      (course) =>
        (course.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
          course.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
          (course.instructor?.fullName?.toLowerCase() || "").includes(
            searchTerm.toLowerCase()
          )) &&
        (filterStatus === "all" || course.status.toLowerCase() === filterStatus)
    );
    setFilteredCourses(filtered);
  }, [courses, searchTerm, filterStatus]);

  const fetchCourses = async () => {
    try {
      setLoading(true);
      const params = user?.branchId ? { branchId: user.branchId } : {};
      const response = await coursesAPI.getAll(params);

      if (response.data.success) {
        setCourses(response.data.data || []);
        if (response.data.data.length === 0) {
          toast("لا توجد دورات مسجلة حالياً", { icon: "ℹ️" });
        }
      } else {
        toast.error(response.data.message || "خطأ في تحميل بيانات الدورات");
        setCourses([]);
      }
    } catch (error: any) {
      console.error("Error fetching courses:", error);
      toast.error(
        error.response?.data?.message || "خطأ في تحميل بيانات الدورات"
      );
      setCourses([]);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number, name: string) => {
    if (window.confirm(`هل أنت متأكد من حذف الدورة ${name}؟`)) {
      try {
        const response = await coursesAPI.delete(id);
        if (response.data.success) {
          setCourses(courses.filter((c) => c.id !== id));
          toast.success("تم حذف الدورة بنجاح");
        } else {
          toast.error(response.data.message || "خطأ في حذف الدورة");
        }
      } catch (error: any) {
        console.error("Error deleting course:", error);
        toast.error(error.response?.data?.message || "خطأ في حذف الدورة");
      }
    }
  };

  const exportCourses = () => {
    if (courses.length === 0) {
      toast.error("لا يوجد بيانات للتصدير");
      return;
    }

    try {
      const exportData = courses.map((course) => ({
        "اسم الدورة": course.name,
        الوصف: course.description,
        المدرب: course.instructor?.fullName || "غير محدد",
        السعر: course.price,
        "الحد الأقصى للطلاب": course.maxStudents,
        "عدد الطلاب الحاليين": course.currentStudents,
        "تاريخ البداية": new Date(course.startDate).toLocaleDateString("ar-EG"),
        "تاريخ النهاية": new Date(course.endDate).toLocaleDateString("ar-EG"),
        المواعيد: course.schedule,
        الحالة: course.statusArabic,
        الفئة: course.courseCategory?.name || "غير محدد",
        الفرع: course.branch?.name || "غير محدد",
      }));

      const ws = XLSX.utils.json_to_sheet(exportData);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, "Courses");

      // Auto-size columns
      const max_width = exportData.reduce(
        (w, r) => Math.max(w, Object.keys(r).join("").length),
        10
      );
      ws["!cols"] = [{ wch: max_width }];

      XLSX.writeFile(
        wb,
        `قائمة_الدورات_${new Date().toLocaleDateString("ar-EG")}.xlsx`
      );
      toast.success("تم تصدير بيانات الدورات بنجاح");
    } catch (error) {
      console.error("Error exporting courses:", error);
      toast.error("حدث خطأ أثناء تصدير البيانات");
    }
  };

  if (loading) {
    return (
      <div className="p-6 max-w-7xl mx-auto rtl">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
          <div className="space-y-4">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="h-20 bg-gray-200 rounded"></div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-7xl mx-auto rtl">
      {/* Header */}
      <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            إدارة الدورات
          </h1>
          <p className="text-gray-600">
            إجمالي الدورات: {filteredCourses.length} دورة
          </p>
        </div>
        <div className="flex items-center space-x-4 rtl:space-x-reverse mt-4 lg:mt-0">
          <button
            onClick={exportCourses}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
            disabled={courses.length === 0}
          >
            <Download className="w-4 h-4" />
            <span>تصدير</span>
          </button>
          <button
            onClick={fetchCourses}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
          >
            <RotateCw className="w-4 h-4" />
            <span>تحديث</span>
          </button>
          <Link
            to="/courses/new"
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <Plus className="w-4 h-4" />
            <span>دورة جديدة</span>
          </Link>
        </div>
      </div>

      {/* Search and Filters */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div className="flex flex-col lg:flex-row gap-4">
          <div className="flex-1 relative">
            <Search className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="البحث عن دورة (الاسم، الوصف، المدرب...)"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pr-10 pl-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          >
            <option value="all">جميع الحالات</option>
            <option value="active">نشطة</option>
            <option value="completed">مكتملة</option>
            <option value="cancelled">ملغاة</option>
            <option value="upcoming">قادمة</option>
          </select>
        </div>
      </div>

      {/* Courses Grid */}
      <div className="grid gap-6">
        {filteredCourses.length === 0 ? (
          <div className="bg-white rounded-xl shadow-sm p-12 text-center">
            <GraduationCap className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              لا توجد نتائج
            </h3>
            <p className="text-gray-500">
              {searchTerm
                ? "لم يتم العثور على دورات مطابقة للبحث"
                : "لم يتم إضافة أي دورات بعد"}
            </p>
            {!searchTerm && (
              <Link
                to="/courses/new"
                className="inline-flex items-center space-x-2 rtl:space-x-reverse mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                <Plus className="w-4 h-4" />
                <span>إضافة أول دورة</span>
              </Link>
            )}
          </div>
        ) : (
          filteredCourses.map((course) => (
            <div
              key={course.id}
              className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center justify-between mb-4">
                    <Link
                      to={`/courses/${course.id}`}
                      className="text-xl font-semibold text-blue-700 hover:underline"
                    >
                      {course.name}
                    </Link>
                    <span
                      className={`px-3 py-1 rounded-full text-sm font-medium ${
                        course.status === "Active"
                          ? "bg-green-100 text-green-800"
                          : course.status === "Completed"
                          ? "bg-gray-100 text-gray-800"
                          : course.status === "Cancelled"
                          ? "bg-red-100 text-red-800"
                          : "bg-yellow-100 text-yellow-800"
                      }`}
                    >
                      {course.statusArabic}
                    </span>
                  </div>
                  <p className="text-gray-600 mb-4">{course.description}</p>
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
                    <div className="flex items-center text-gray-600">
                      <Users className="w-4 h-4 ml-2" />
                      <span>
                        {course.currentStudents}/{course.maxStudents} طالب
                      </span>
                    </div>
                    <div className="flex items-center text-gray-600">
                      <Calendar className="w-4 h-4 ml-2" />
                      <span>
                        {new Date(course.startDate).toLocaleDateString("ar-EG")}
                      </span>
                    </div>
                    <div className="flex items-center text-gray-600">
                      <Clock className="w-4 h-4 ml-2" />
                      <span>{course.schedule}</span>
                    </div>
                    <div className="flex items-center text-gray-600">
                      <GraduationCap className="w-4 h-4 ml-2" />
                      <span>{course.instructor?.fullName || "غير محدد"}</span>
                    </div>
                    <div className="flex items-center text-gray-600">
                      <DollarSign className="w-4 h-4 ml-2" />
                      <span>{course.price} جنيه</span>
                    </div>
                  </div>
                </div>
              </div>
              <div className="flex items-center justify-end space-x-2 rtl:space-x-reverse mt-4 pt-4 border-t">
                <Link
                  to={`/courses/${course.id}/edit`}
                  className="flex items-center space-x-1 rtl:space-x-reverse px-3 py-2 text-blue-600 hover:text-blue-700 transition-colors"
                >
                  <Edit className="w-4 h-4" />
                  <span>تعديل</span>
                </Link>
                <button
                  onClick={() => handleDelete(course.id, course.name)}
                  className="flex items-center space-x-1 rtl:space-x-reverse px-3 py-2 text-red-600 hover:text-red-700 transition-colors"
                >
                  <Trash2 className="w-4 h-4" />
                  <span>حذف</span>
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default CourseList;
