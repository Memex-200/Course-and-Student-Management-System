import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { studentsAPI } from "../../lib/api";
import { Student } from "../../types";
import { useAuth } from "../../contexts/AuthContext";
import * as XLSX from "xlsx";
import {
  Search,
  Plus,
  Edit,
  Trash2,
  Phone,
  Mail,
  User,
  Calendar,
  Filter,
  Download,
  RotateCw,
  Eye,
} from "lucide-react";
import toast from "react-hot-toast";

const StudentList: React.FC = () => {
  const { user } = useAuth();
  const [students, setStudents] = useState<Student[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [filteredStudents, setFilteredStudents] = useState<Student[]>([]);
  const [selectedBranch, setSelectedBranch] = useState<string>("all");
  const branchOptions = [
    { value: "all", label: "كل الفروع" },
    { value: "أسيوط", label: "أسيوط" },
    { value: "أبوتيج", label: "أبوتيج" },
  ];

  useEffect(() => {
    fetchStudents();
  }, []);

  useEffect(() => {
    let filtered = students.filter(
      (student) =>
        student.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        student.phone.includes(searchTerm) ||
        (student.email?.toLowerCase().includes(searchTerm.toLowerCase()) ??
          false) ||
        student.parentName.toLowerCase().includes(searchTerm.toLowerCase())
    );
    if (selectedBranch !== "all") {
      filtered = filtered.filter((student) => {
        const branchName = (student.branch?.name || "")
          .replace("فرع", "")
          .trim()
          .toLowerCase();
        return branchName === selectedBranch.toLowerCase();
      });
    }
    setFilteredStudents(filtered);
  }, [students, searchTerm, selectedBranch]);

  const fetchStudents = async () => {
    try {
      setLoading(true);
      const params = user?.branchId ? { branchId: user.branchId } : {};
      const response = await studentsAPI.getAll(params);

      if (response.data.success) {
        setStudents(response.data.data || []);
        if (response.data.data.length === 0) {
          toast("لا يوجد طلاب مسجلين حالياً", { icon: "ℹ️" });
        }
      } else {
        toast.error(response.data.message || "خطأ في تحميل بيانات الطلاب");
        setStudents([]);
      }
    } catch (error: any) {
      console.error("Error fetching students:", error);
      toast.error(
        error.response?.data?.message || "خطأ في تحميل بيانات الطلاب"
      );
      setStudents([]);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number, name: string) => {
    if (window.confirm(`هل أنت متأكد من حذف الطالب ${name}؟`)) {
      try {
        const response = await studentsAPI.delete(id);
        if (response.data.success) {
          setStudents(students.filter((s) => s.id !== id));
          toast.success("تم حذف الطالب بنجاح");
        } else {
          toast.error(response.data.message || "خطأ في حذف الطالب");
        }
      } catch (error: any) {
        console.error("Error deleting student:", error);
        toast.error(error.response?.data?.message || "خطأ في حذف الطالب");
      }
    }
  };

  const exportStudents = () => {
    if (students.length === 0) {
      toast.error("لا يوجد بيانات للتصدير");
      return;
    }

    try {
      // Prepare data for export
      const exportData = students.map((student) => ({
        "الاسم الكامل": student.fullName,
        "رقم الهاتف": student.phone,
        "البريد الإلكتروني": student.email || "",
        العمر: student.age,
        الجنس: student.gender,
        المدرسة: student.school || "",
        الصف: student.grade || "",
        العنوان: student.address || "",
        "اسم ولي الأمر": student.parentName,
        "رقم هاتف ولي الأمر": student.parentPhone,
        "بريد ولي الأمر": student.parentEmail || "",
        "جهة الاتصال في الطوارئ": student.emergencyContact || "",
        "رقم هاتف الطوارئ": student.emergencyPhone || "",
        "الحالة الطبية": student.medicalConditions || "",
        "وسيلة النقل المفضلة": student.preferredTransportation || "",
        الفرع: student.branch?.name || "",
        "عدد الدورات المسجلة": student.registeredCourses || 0,
        "تاريخ التسجيل": new Date(student.createdAt).toLocaleDateString(
          "ar-EG"
        ),
        الحالة: student.isActive ? "نشط" : "غير نشط",
      }));

      // Create workbook and worksheet
      const ws = XLSX.utils.json_to_sheet(exportData);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, "Students");

      // Auto-size columns
      const max_width = exportData.reduce(
        (w, r) => Math.max(w, Object.keys(r).join("").length),
        10
      );
      ws["!cols"] = [{ wch: max_width }];

      // Save file
      XLSX.writeFile(
        wb,
        `قائمة_الطلاب_${new Date().toLocaleDateString("ar-EG")}.xlsx`
      );
      toast.success("تم تصدير بيانات الطلاب بنجاح");
    } catch (error) {
      console.error("Error exporting students:", error);
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
            إدارة الطلاب
          </h1>
          <p className="text-gray-600">
            إجمالي الطلاب: {filteredStudents.length} طالب
          </p>
        </div>
        <div className="flex items-center space-x-4 rtl:space-x-reverse mt-4 lg:mt-0">
          <button
            onClick={exportStudents}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
            disabled={students.length === 0}
          >
            <Download className="w-4 h-4" />
            <span>تصدير</span>
          </button>
          <button
            onClick={fetchStudents}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
          >
            <RotateCw className="w-4 h-4" />
            <span>تحديث</span>
          </button>
          <Link
            to="/students/new"
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <Plus className="w-4 h-4" />
            <span>طالب جديد</span>
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
              placeholder="البحث عن طالب (الاسم، الهاتف، البريد الإلكتروني...)"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pr-10 pl-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <select
            value={selectedBranch}
            onChange={(e) => setSelectedBranch(e.target.value)}
            className="px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          >
            {branchOptions.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
          <button className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <Filter className="w-4 h-4" />
            <span>فلترة</span>
          </button>
        </div>
      </div>

      {/* Students Grid */}
      <div className="grid gap-6">
        {filteredStudents.length === 0 ? (
          <div className="bg-white rounded-xl shadow-sm p-12 text-center">
            <User className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              لا توجد نتائج
            </h3>
            <p className="text-gray-500">
              {searchTerm
                ? "لم يتم العثور على طلاب مطابقين للبحث"
                : "لم يتم إضافة أي طلاب بعد"}
            </p>
            {!searchTerm && (
              <Link
                to="/students/new"
                className="inline-flex items-center space-x-2 rtl:space-x-reverse mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                <Plus className="w-4 h-4" />
                <span>إضافة أول طالب</span>
              </Link>
            )}
          </div>
        ) : (
          filteredStudents.map((student) => (
            <div
              key={student.id}
              className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow"
            >
              <div className="flex items-start justify-between">
                <div className="flex items-start space-x-4 rtl:space-x-reverse">
                  <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
                    <User className="w-6 h-6 text-blue-600" />
                  </div>
                  <div>
                    <h3 className="text-lg font-medium text-gray-900">
                      {student.fullName}
                    </h3>
                    <div className="mt-1 space-y-1">
                      <p className="text-sm text-gray-500 flex items-center">
                        <Phone className="w-4 h-4 ml-1" />
                        {student.phone}
                      </p>
                      {student.email && (
                        <p className="text-sm text-gray-500 flex items-center">
                          <Mail className="w-4 h-4 ml-1" />
                          {student.email}
                        </p>
                      )}
                      <p className="text-sm text-gray-500 flex items-center">
                        <Calendar className="w-4 h-4 ml-1" />
                        {new Date(student.createdAt).toLocaleDateString(
                          "ar-EG"
                        )}
                      </p>
                    </div>
                  </div>
                </div>
                <div className="flex items-center space-x-2 rtl:space-x-reverse">
                  <Link
                    to={`/students/${student.id}/view`}
                    className="p-2 text-gray-500 hover:text-gray-700 transition-colors"
                  >
                    <Eye className="w-5 h-5" />
                  </Link>
                  <Link
                    to={`/students/${student.id}/edit`}
                    className="p-2 text-gray-500 hover:text-gray-700 transition-colors"
                  >
                    <Edit className="w-5 h-5" />
                  </Link>
                  <button
                    onClick={() => handleDelete(student.id, student.fullName)}
                    className="p-2 text-red-500 hover:text-red-700 transition-colors"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>
                </div>
              </div>
              <div className="mt-4 flex items-center justify-between text-sm">
                <div className="flex items-center space-x-4 rtl:space-x-reverse">
                  <span className="text-gray-500">
                    الفرع: {student.branch?.name}
                  </span>
                  <span className="text-gray-500">
                    الكورسات: {student.registeredCourses}
                  </span>
                </div>
                <div>
                  {student.isActive ? (
                    <span className="px-2 py-1 bg-green-100 text-green-800 rounded-full text-xs">
                      نشط
                    </span>
                  ) : (
                    <span className="px-2 py-1 bg-red-100 text-red-800 rounded-full text-xs">
                      غير نشط
                    </span>
                  )}
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default StudentList;
