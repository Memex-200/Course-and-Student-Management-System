import React, { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { studentsAPI } from "../../lib/api/index";
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
  GraduationCap,
} from "lucide-react";
import toast from "react-hot-toast";

interface StudentFormData {
  fullName: string;
  phone: string;
  email?: string;
  age: number;
  gender: "Male" | "Female";
  parentName: string;
  parentPhone: string;
  school: string;
  level: "Level1" | "Level2" | "Level3";
  isActive?: boolean;
}

const StudentForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { user } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [currentStep, setCurrentStep] = useState(1);
  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    reset,
    // setValue intentionally unused for now; reserved for future autofill
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
            parentName: studentData.parentName,
            parentPhone: studentData.parentPhone,
            school: studentData.school,
            level: studentData.level,
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

  const getAgeGroup = (age: number) => {
    if (age >= 4 && age <= 6) return "الفئة أ (4-6 سنوات)";
    if (age >= 7 && age <= 10) return "الفئة ب (7-10 سنوات)";
    if (age >= 11 && age <= 12) return "الفئة ج (11-12 سنة)";
    if (age >= 13 && age <= 17) return "الفئة د (13-17 سنة)";
    return "";
  };

  const getAgeGroupColor = (age: number) => {
    if (age >= 4 && age <= 6)
      return "bg-blue-100 text-blue-800 border-blue-200";
    if (age >= 7 && age <= 10)
      return "bg-green-100 text-green-800 border-green-200";
    if (age >= 11 && age <= 12)
      return "bg-yellow-100 text-yellow-800 border-yellow-200";
    if (age >= 13 && age <= 17)
      return "bg-purple-100 text-purple-800 border-purple-200";
    return "bg-gray-100 text-gray-800 border-gray-200";
  };

  const onSubmit = async (data: StudentFormData) => {
    setIsSubmitting(true);
    try {
      if (!selectedBranch) {
        toast.error("يرجى اختيار الفرع");
        return;
      }
      const formData = {
        // Map frontend field names to backend field names
        FullName: data.fullName,
        Phone: data.phone,
        Email: data.email || "",
        Age: data.age,
        Level: data.level === "Level1" ? 1 : data.level === "Level2" ? 2 : 3,
        Gender: data.gender,
        School: data.school || "",
        ParentName: data.parentName,
        ParentPhone: data.parentPhone,
        EmergencyContact: "",
        EmergencyPhone: "",
        MedicalConditions: "",
        BranchId: selectedBranch,
      };

      // Debug logging
      console.log("Form data being sent:", formData);
      console.log("Raw form data:", data);

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
      console.error("Error response:", error.response?.data);
      console.error("Error status:", error.response?.status);
      console.error("Error message:", error.response?.data?.message);
      toast.error(
        error.response?.data?.message ||
          `خطأ في ${isEditMode ? "تحديث" : "إضافة"} الطالب`
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const nextStep = () => {
    if (currentStep === 1 && age && age >= 4 && age <= 17) {
      setCurrentStep(2);
    }
  };

  const prevStep = () => {
    if (currentStep === 2) {
      setCurrentStep(1);
    }
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

      {/* Progress Steps */}
      {!isEditMode && (
        <div className="mb-8">
          <div className="flex items-center justify-center space-x-4 rtl:space-x-reverse">
            <div
              className={`flex items-center space-x-2 rtl:space-x-reverse ${
                currentStep >= 1 ? "text-blue-600" : "text-gray-400"
              }`}
            >
              <div
                className={`w-8 h-8 rounded-full flex items-center justify-center ${
                  currentStep >= 1 ? "bg-blue-600 text-white" : "bg-gray-200"
                }`}
              >
                1
              </div>
              <span className="font-medium">تحديد العمر والفئة</span>
            </div>
            <div
              className={`w-16 h-1 ${
                currentStep >= 2 ? "bg-blue-600" : "bg-gray-200"
              }`}
            ></div>
            <div
              className={`flex items-center space-x-2 rtl:space-x-reverse ${
                currentStep >= 2 ? "text-blue-600" : "text-gray-400"
              }`}
            >
              <div
                className={`w-8 h-8 rounded-full flex items-center justify-center ${
                  currentStep >= 2 ? "bg-blue-600 text-white" : "bg-gray-200"
                }`}
              >
                2
              </div>
              <span className="font-medium">البيانات الشخصية</span>
            </div>
          </div>
        </div>
      )}

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

        {/* Step 1: Age and Level Selection */}
        {!isEditMode && currentStep === 1 && (
          <div className="bg-white rounded-xl shadow-sm p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center">
              <User className="w-5 h-5 ml-2 text-blue-600" />
              الخطوة الأولى: تحديد العمر والمستوى
            </h2>

            <div className="grid md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  العمر <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  min="4"
                  max="17"
                  {...register("age", {
                    required: "العمر مطلوب",
                    min: {
                      value: 4,
                      message: "العمر يجب أن يكون 4 سنوات على الأقل",
                    },
                    max: { value: 17, message: "العمر يجب ألا يتجاوز 17 سنة" },
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
                {age && age >= 4 && age <= 17 && (
                  <div
                    className={`mt-2 p-3 rounded-lg border ${getAgeGroupColor(
                      age
                    )}`}
                  >
                    <p className="font-medium">{getAgeGroup(age)}</p>
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  المستوى <span className="text-red-500">*</span>
                </label>
                <select
                  {...register("level", { required: "المستوى مطلوب" })}
                  className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                    errors.level ? "border-red-500" : "border-gray-300"
                  }`}
                >
                  <option value="">اختر المستوى</option>
                  <option value="Level1">المستوى الأول</option>
                  <option value="Level2">المستوى الثاني</option>
                  <option value="Level3">المستوى الثالث</option>
                </select>
                {errors.level && (
                  <p className="mt-1 text-sm text-red-600 flex items-center">
                    <AlertCircle className="w-4 h-4 ml-1" />
                    {errors.level.message}
                  </p>
                )}
              </div>
            </div>

            <div className="mt-6 flex justify-end">
              <button
                type="button"
                onClick={nextStep}
                disabled={!age || age < 4 || age > 17}
                className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                التالي
              </button>
            </div>
          </div>
        )}

        {/* Step 2: Personal Information */}
        {(isEditMode || currentStep === 2) && (
          <>
            {/* Personal Information */}
            <div className="bg-white rounded-xl shadow-sm p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center">
                <User className="w-5 h-5 ml-2 text-blue-600" />
                {isEditMode
                  ? "المعلومات الشخصية"
                  : "الخطوة الثانية: المعلومات الشخصية"}
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

                {!isEditMode && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      العمر <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="number"
                      min="4"
                      max="17"
                      {...register("age", {
                        required: "العمر مطلوب",
                        min: {
                          value: 4,
                          message: "العمر يجب أن يكون 4 سنوات على الأقل",
                        },
                        max: {
                          value: 17,
                          message: "العمر يجب ألا يتجاوز 17 سنة",
                        },
                      })}
                      className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                        errors.age ? "border-red-500" : "border-gray-300"
                      }`}
                      placeholder="أدخل العمر"
                      readOnly
                    />
                    {age && age >= 4 && age <= 17 && (
                      <div
                        className={`mt-2 p-3 rounded-lg border ${getAgeGroupColor(
                          age
                        )}`}
                      >
                        <p className="font-medium">{getAgeGroup(age)}</p>
                      </div>
                    )}
                  </div>
                )}

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

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    المدرسة
                  </label>
                  <input
                    type="text"
                    {...register("school", {
                      maxLength: {
                        value: 100,
                        message: "اسم المدرسة يجب ألا يتجاوز 100 حرف",
                      },
                    })}
                    className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                      errors.school ? "border-red-500" : "border-gray-300"
                    }`}
                    placeholder="أدخل اسم المدرسة"
                  />
                  {errors.school && (
                    <p className="mt-1 text-sm text-red-600 flex items-center">
                      <AlertCircle className="w-4 h-4 ml-1" />
                      {errors.school.message}
                    </p>
                  )}
                </div>

                {!isEditMode && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      المستوى <span className="text-red-500">*</span>
                    </label>
                    <select
                      {...register("level", { required: "المستوى مطلوب" })}
                      className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                        errors.level ? "border-red-500" : "border-gray-300"
                      }`}
                    >
                      <option value="">اختر المستوى</option>
                      <option value="Level1">المستوى الأول</option>
                      <option value="Level2">المستوى الثاني</option>
                      <option value="Level3">المستوى الثالث</option>
                    </select>
                    {errors.level && (
                      <p className="mt-1 text-sm text-red-600 flex items-center">
                        <AlertCircle className="w-4 h-4 ml-1" />
                        {errors.level.message}
                      </p>
                    )}
                  </div>
                )}

                {isEditMode && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      المستوى <span className="text-red-500">*</span>
                    </label>
                    <select
                      {...register("level", { required: "المستوى مطلوب" })}
                      className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                        errors.level ? "border-red-500" : "border-gray-300"
                      }`}
                    >
                      <option value="">اختر المستوى</option>
                      <option value="Level1">المستوى الأول</option>
                      <option value="Level2">المستوى الثاني</option>
                      <option value="Level3">المستوى الثالث</option>
                    </select>
                    {errors.level && (
                      <p className="mt-1 text-sm text-red-600 flex items-center">
                        <AlertCircle className="w-4 h-4 ml-1" />
                        {errors.level.message}
                      </p>
                    )}
                  </div>
                )}

                {isEditMode && (
                  <div>
                    <label className="flex items-center space-x-2 rtl:space-x-reverse">
                      <input
                        type="checkbox"
                        {...register("isActive")}
                        className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                      />
                      <span className="text-sm font-medium text-gray-700">
                        نشط
                      </span>
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
              </div>
            </div>

            {/* Navigation for Step 2 */}
            {!isEditMode && (
              <div className="flex items-center justify-between">
                <button
                  type="button"
                  onClick={prevStep}
                  className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  السابق
                </button>
              </div>
            )}
          </>
        )}

        {/* Submit Buttons */}
        {(isEditMode || currentStep === 2) && (
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
        )}
      </form>
    </div>
  );
};

export default StudentForm;
