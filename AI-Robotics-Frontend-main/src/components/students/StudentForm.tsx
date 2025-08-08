import React, { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { studentsAPI } from "../../lib/api";
import { useAuth } from "../../contexts/AuthContext";
import {
  ArrowRight,
  Save,
  User,
  Phone,
  Mail,
  MapPin,
  Users,
  AlertCircle,
} from "lucide-react";
import toast from "react-hot-toast";

interface StudentFormData {
  fullName: string;
  phone: string;
  email?: string;
  age: number;
  gender: "Male" | "Female";
  address?: string;
  parentName: string;
  parentPhone: string;
  parentEmail?: string;
  preferredTransportation?: string;
  isActive?: boolean;
}

const StudentForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { user } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    reset,
  } = useForm<StudentFormData>();

  const age = watch("age");
  const isEditMode = !!id;
  const [branches, setBranches] = useState<{ id: number; name: string }[]>([]);
  const [selectedBranch, setSelectedBranch] = useState<number | null>(null);

  useEffect(() => {
    if (isEditMode) {
      setIsLoading(true);
      studentsAPI
        .getById(parseInt(id))
        .then((response) => {
          const studentData = response.data;
          reset({
            fullName: studentData.fullName,
            phone: studentData.phone,
            email: studentData.email,
            age: studentData.age,
            gender: studentData.gender,
            address: studentData.address,
            parentName: studentData.parentName,
            parentPhone: studentData.parentPhone,
            parentEmail: studentData.parentEmail,
            preferredTransportation: studentData.preferredTransportation,
            isActive: studentData.isActive,
          });
        })
        .catch((error) => {
          console.error("Error fetching student:", error);
          toast.error("خطأ في تحميل بيانات الطالب");
          navigate("/students");
        })
        .finally(() => {
          setIsLoading(false);
        });
    }
  }, [id, reset, navigate, isEditMode]);

  useEffect(() => {
    studentsAPI.getBranches().then((res) => {
      if (res.data && res.data.branches) {
        setBranches(res.data.branches);
        setSelectedBranch(res.data.branches[0]?.id || null);
      }
    });
  }, []);

  const onSubmit = async (data: StudentFormData) => {
    setIsSubmitting(true);
    try {
      const formData = {
        ...data,
        branchId: selectedBranch,
        // Set default values for optional fields
        email: data.email || "",
        address: data.address || "",
        parentEmail: data.parentEmail || "",
        preferredTransportation: data.preferredTransportation || "",
        school: "",
        grade: "",
        notes: "",
      };

      if (isEditMode) {
        await studentsAPI.update(parseInt(id), formData);
        toast.success("تم تحديث بيانات الطالب بنجاح");
      } else {
        await studentsAPI.create(formData);
        toast.success("تم إضافة الطالب بنجاح");
      }
      navigate("/students");
    } catch (error: any) {
      console.error("Error saving student:", error);
      toast.error(
        error.response?.data?.message ||
          `خطأ في ${isEditMode ? "تحديث" : "إضافة"} الطالب`
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const getAgeCategory = (age: number) => {
    if (age >= 4 && age <= 6) return "المرحلة التأسيسية (4-6 سنوات)";
    if (age >= 7 && age <= 12) return "المرحلة الاستكشافية (7-12 سنة)";
    if (age >= 13 && age <= 17) return "المرحلة المتقدمة (13-17 سنة)";
    if (age >= 18) return "تخصصات البالغين (18+ سنة)";
    return "";
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-4xl mx-auto rtl">
      {/* Header */}
      <div className="flex items-center space-x-4 rtl:space-x-reverse mb-8">
        <button
          onClick={() => navigate("/students")}
          className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
        >
          <ArrowRight className="w-5 h-5" />
        </button>
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {isEditMode ? "تعديل بيانات الطالب" : "إضافة طالب جديد"}
          </h1>
          <p className="text-gray-600">
            {isEditMode
              ? "تعديل بيانات الطالب في الشركة"
              : "إضافة طالب جديد للشركة"}
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
        {/* Branch Selection */}
        <div className="bg-white rounded-xl shadow-sm p-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            الفرع <span className="text-red-500">*</span>
          </label>
          <select
            value={selectedBranch ?? ""}
            onChange={(e) => setSelectedBranch(Number(e.target.value))}
            className="w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent mb-4"
            required
          >
            <option value="">اختر الفرع</option>
            {branches.map((branch) => (
              <option key={branch.id} value={branch.id}>
                {branch.name}
              </option>
            ))}
          </select>
        </div>

        {/* Personal Information */}
        <div className="bg-white rounded-xl shadow-sm p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center">
            <User className="w-5 h-5 ml-2 text-blue-600" />
            المعلومات الشخصية
          </h2>

          <div className="grid md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                الاسم الكامل <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                {...register("fullName", {
                  required: "الاسم الكامل مطلوب",
                  maxLength: {
                    value: 100,
                    message: "الاسم الكامل يجب ألا يتجاوز 100 حرف",
                  },
                })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.fullName ? "border-red-500" : "border-gray-300"
                }`}
                placeholder="أدخل الاسم الكامل"
              />
              {errors.fullName && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.fullName.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                العمر <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                min="4"
                max="100"
                {...register("age", {
                  required: "العمر مطلوب",
                  min: {
                    value: 4,
                    message: "العمر يجب أن يكون 4 سنوات على الأقل",
                  },
                  max: { value: 100, message: "العمر غير صحيح" },
                })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.age ? "border-red-500" : "border-gray-300"
                }`}
                placeholder="أدخل العمر"
              />
              {errors.age && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.age.message}
                </p>
              )}
              {age && age >= 4 && (
                <p className="mt-2 text-sm text-blue-600 bg-blue-50 p-2 rounded">
                  {getAgeCategory(age)}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                الجنس <span className="text-red-500">*</span>
              </label>
              <select
                {...register("gender", { required: "الجنس مطلوب" })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.gender ? "border-red-500" : "border-gray-300"
                }`}
              >
                <option value="">اختر الجنس</option>
                <option value="Male">ذكر</option>
                <option value="Female">أنثى</option>
              </select>
              {errors.gender && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.gender.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                رقم الهاتف <span className="text-red-500">*</span>
              </label>
              <input
                type="tel"
                {...register("phone", {
                  required: "رقم الهاتف مطلوب",
                  pattern: {
                    value: /^01[0-2,5]{1}[0-9]{8}$/,
                    message: "رقم هاتف غير صحيح (01xxxxxxxxx)",
                  },
                  maxLength: {
                    value: 20,
                    message: "رقم الهاتف يجب ألا يتجاوز 20 رقم",
                  },
                })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.phone ? "border-red-500" : "border-gray-300"
                }`}
                placeholder="أدخل رقم الهاتف"
                dir="ltr"
              />
              {errors.phone && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.phone.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                البريد الإلكتروني
              </label>
              <input
                type="email"
                {...register("email", {
                  maxLength: {
                    value: 100,
                    message: "البريد الإلكتروني يجب ألا يتجاوز 100 حرف",
                  },
                  pattern: {
                    value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                    message: "البريد الإلكتروني غير صحيح",
                  },
                })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.email ? "border-red-500" : "border-gray-300"
                }`}
                placeholder="أدخل البريد الإلكتروني (اختياري)"
                dir="ltr"
              />
              {errors.email && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.email.message}
                </p>
              )}
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                العنوان
              </label>
              <textarea
                {...register("address", {
                  maxLength: {
                    value: 200,
                    message: "العنوان يجب ألا يتجاوز 200 حرف",
                  },
                })}
                rows={3}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.address ? "border-red-500" : "border-gray-300"
                }`}
                placeholder="أدخل العنوان بالتفصيل"
              />
              {errors.address && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.address.message}
                </p>
              )}
            </div>

            {isEditMode && (
              <div>
                <label className="flex items-center space-x-2 rtl:space-x-reverse">
                  <input
                    type="checkbox"
                    {...register("isActive")}
                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <span className="text-sm font-medium text-gray-700">نشط</span>
                </label>
              </div>
            )}
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
              <label className="block text-sm font-medium text-gray-700 mb-2">
                اسم ولي الأمر <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                {...register("parentName", {
                  required: "اسم ولي الأمر مطلوب",
                  maxLength: {
                    value: 100,
                    message: "اسم ولي الأمر يجب ألا يتجاوز 100 حرف",
                  },
                })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.parentName ? "border-red-500" : "border-gray-300"
                }`}
                placeholder="أدخل اسم ولي الأمر"
              />
              {errors.parentName && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.parentName.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                هاتف ولي الأمر <span className="text-red-500">*</span>
              </label>
              <input
                type="tel"
                {...register("parentPhone", {
                  required: "هاتف ولي الأمر مطلوب",
                  pattern: {
                    value: /^01[0-2,5]{1}[0-9]{8}$/,
                    message: "رقم هاتف غير صحيح (01xxxxxxxxx)",
                  },
                  maxLength: {
                    value: 20,
                    message: "رقم الهاتف يجب ألا يتجاوز 20 رقم",
                  },
                })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.parentPhone ? "border-red-500" : "border-gray-300"
                }`}
                placeholder="أدخل هاتف ولي الأمر"
                dir="ltr"
              />
              {errors.parentPhone && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.parentPhone.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                بريد ولي الأمر الإلكتروني{" "}
                <span className="text-red-500">*</span>
              </label>
              <input
                type="email"
                {...register("parentEmail", {
                  maxLength: {
                    value: 100,
                    message: "البريد الإلكتروني يجب ألا يتجاوز 100 حرف",
                  },
                  pattern: {
                    value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                    message: "البريد الإلكتروني غير صحيح",
                  },
                })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.parentEmail ? "border-red-500" : "border-gray-300"
                }`}
                placeholder="أدخل بريد ولي الأمر (اختياري)"
                dir="ltr"
              />
              {errors.parentEmail && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.parentEmail.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                وسيلة النقل المفضلة
              </label>
              <select
                {...register("preferredTransportation", {
                  maxLength: {
                    value: 100,
                    message: "وسيلة النقل يجب ألا تتجاوز 100 حرف",
                  },
                })}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                  errors.preferredTransportation
                    ? "border-red-500"
                    : "border-gray-300"
                }`}
              >
                <option value="">اختر وسيلة النقل</option>
                <option value="والدين">والدين</option>
                <option value="أتوبيس الشركة">أتوبيس الشركة</option>
                <option value="مواصلات عامة">مواصلات عامة</option>
                <option value="أخرى">أخرى</option>
              </select>
              {errors.preferredTransportation && (
                <p className="mt-1 text-sm text-red-600 flex items-center">
                  <AlertCircle className="w-4 h-4 ml-1" />
                  {errors.preferredTransportation.message}
                </p>
              )}
            </div>
          </div>
        </div>

        {/* Submit Buttons */}
        <div className="flex items-center justify-end space-x-4 rtl:space-x-reverse">
          <button
            type="button"
            onClick={() => navigate("/students")}
            className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
          >
            إلغاء
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="flex items-center space-x-2 rtl:space-x-reverse px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <Save className="w-4 h-4" />
            <span>
              {isSubmitting
                ? "جاري الحفظ..."
                : isEditMode
                ? "حفظ التعديلات"
                : "حفظ البيانات"}
            </span>
          </button>
        </div>
      </form>
    </div>
  );
};

export default StudentForm;
