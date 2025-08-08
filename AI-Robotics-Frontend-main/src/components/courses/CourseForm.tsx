import React, { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import coursesAPI from "../../lib/api/coursesAPI";
import { CourseFormData, CourseWithEnrollments } from "../../types/course";
import { useAuth } from "../../contexts/AuthContext";
import toast from "react-hot-toast";
import { Save, ArrowRight } from "lucide-react";

const DAYS_OF_WEEK = [
  { value: "Sunday", label: "الأحد" },
  { value: "Monday", label: "الاثنين" },
  { value: "Tuesday", label: "الثلاثاء" },
  { value: "Wednesday", label: "الأربعاء" },
  { value: "Thursday", label: "الخميس" },
  { value: "Friday", label: "الجمعة" },
  { value: "Saturday", label: "السبت" },
] as const;

const CourseForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [categories, setCategories] = useState<
    Array<{ id: number; name: string }>
  >([]);
  const [branches, setBranches] = useState<Array<{ id: number; name: string }>>(
    []
  );
  const [instructors, setInstructors] = useState<
    Array<{ id: number; fullName: string }>
  >([]);
  const [rooms, setRooms] = useState<Array<{ id: number; name: string }>>([]);
  const [labs, setLabs] = useState<Array<{ id: number; name: string }>>([]);

  const [formData, setFormData] = useState<CourseFormData>({
    name: "",
    description: "",
    startDate: "",
    endDate: "",
    schedule: "",
    sessionsCount: 0,
    maxStudents: 0,
    price: 0,
    instructorId: null,
    courseCategoryId: null,
    branchId: null,
    content: "",
    prerequisites: "",
    scheduleDetails: {
      days: [],
      startTime: "",
      endTime: "",
    },
  });

  useEffect(() => {
    loadFormData();
    if (isEditing) {
      fetchCourse();
    }
  }, [id]);

  const loadFormData = async () => {
    try {
      const response = await coursesAPI.getTestData();
      if (response.data) {
        setCategories(response.data.categories);
        setBranches(response.data.branches);
        setInstructors(response.data.employees);
        setRooms(response.data.rooms);
        setLabs(response.data.labs);
      }
    } catch (error: any) {
      console.error("Error loading form data:", error);
      toast.error("خطأ في تحميل البيانات");
    }
  };

  const fetchCourse = async () => {
    try {
      setLoading(true);
      const response = await coursesAPI.getById(Number(id));
      if (response.data.success) {
        const course = response.data.data;
        setFormData({
          name: course.name,
          description: course.description,
          startDate: course.startDate.split("T")[0],
          endDate: course.endDate.split("T")[0],
          schedule: course.schedule,
          sessionsCount: course.sessionsCount,
          maxStudents: course.maxStudents,
          price: course.price,
          instructorId: course.instructorId,
          courseCategoryId: course.courseCategoryId,
          branchId: course.branchId,
          roomId: course.roomId,
          labId: course.labId,
          content: course.content || "",
          prerequisites: course.prerequisites || "",
          scheduleDetails: course.scheduleDetails || {
            days: [],
            startTime: "",
            endTime: "",
          },
        });
      } else {
        toast.error(response.data.message || "خطأ في تحميل بيانات الدورة");
        navigate("/courses");
      }
    } catch (error: any) {
      console.error("Error fetching course:", error);
      toast.error(
        error.response?.data?.message || "خطأ في تحميل بيانات الدورة"
      );
      navigate("/courses");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);

      // Convert days array to comma-separated string of Arabic day names
      const courseDays = formData.scheduleDetails.days
        .map((day) => DAYS_OF_WEEK.find((d) => d.value === day)?.label)
        .join("،");

      const dataToSubmit = {
        name: formData.name,
        description: formData.description,
        courseCategoryId: formData.courseCategoryId
          ? Number(formData.courseCategoryId)
          : null,
        price: Number(formData.price),
        sessionsCount: Number(formData.sessionsCount),
        maxStudents: Number(formData.maxStudents),
        startDate: new Date(formData.startDate).toISOString(),
        endDate: new Date(formData.endDate).toISOString(),
        branchId: formData.branchId ? Number(formData.branchId) : null,
        instructorId: formData.instructorId
          ? Number(formData.instructorId)
          : null,
        roomId: formData.roomId ? Number(formData.roomId) : null,
        labId: formData.labId ? Number(formData.labId) : null,
        content: formData.content,
        prerequisites: formData.prerequisites,
        courseDays: courseDays,
        startTime: formData.scheduleDetails.startTime,
        endTime: formData.scheduleDetails.endTime,
        notes: "",
      };

      console.log("Submitting course data:", dataToSubmit);

      const response = isEditing
        ? await coursesAPI.update(Number(id), dataToSubmit)
        : await coursesAPI.create(dataToSubmit);

      if (response.data.success) {
        toast.success(
          isEditing ? "تم تحديث الدورة بنجاح" : "تم إنشاء الدورة بنجاح"
        );
        navigate("/courses");
      } else {
        toast.error(response.data.message || "خطأ في حفظ الدورة");
      }
    } catch (error: any) {
      console.error("Error saving course:", error);
      const errorMessage =
        error.response?.data?.error ||
        error.response?.data?.message ||
        "خطأ في حفظ الدورة";
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: ["maxStudents", "price", "sessionsCount"].includes(name)
        ? Number(value)
        : value,
    }));
  };

  const handleScheduleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    if (name === "days") {
      const selectedOptions = Array.from(
        (e.target as HTMLSelectElement).selectedOptions,
        (option) => option.value as (typeof DAYS_OF_WEEK)[number]["value"]
      );
      setFormData((prev) => ({
        ...prev,
        scheduleDetails: {
          ...prev.scheduleDetails,
          days: selectedOptions,
        },
      }));
    } else {
      setFormData((prev) => ({
        ...prev,
        scheduleDetails: {
          ...prev.scheduleDetails,
          [name]: value,
        },
      }));
    }
  };

  if (loading) {
    return (
      <div className="p-6 max-w-4xl mx-auto rtl">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
          <div className="space-y-4">
            {[...Array(6)].map((_, i) => (
              <div key={i} className="h-12 bg-gray-200 rounded"></div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-4xl mx-auto rtl">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            {isEditing ? "تعديل دورة" : "إضافة دورة جديدة"}
          </h1>
          <p className="text-gray-600">
            قم بملء البيانات التالية لإنشاء دورة جديدة
          </p>
        </div>
        <button
          onClick={() => navigate("/courses")}
          className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 text-gray-600 hover:text-gray-900 transition-colors"
        >
          <ArrowRight className="w-4 h-4" />
          <span>عودة</span>
        </button>
      </div>

      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-xl shadow-sm p-6"
      >
        {/* Basic Information */}
        <div className="mb-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            معلومات أساسية
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                اسم الدورة
              </label>
              <input
                type="text"
                name="name"
                value={formData.name}
                onChange={handleChange}
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                السعر
              </label>
              <input
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                name="price"
                value={formData.price}
                onChange={handleChange}
                required
                min="0"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent appearance-none"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                عدد الحصص
              </label>
              <input
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                name="sessionsCount"
                value={formData.sessionsCount}
                onChange={handleChange}
                required
                min="1"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent appearance-none"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                الحد الأقصى للطلاب
              </label>
              <input
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                name="maxStudents"
                value={formData.maxStudents}
                onChange={handleChange}
                required
                min="1"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent appearance-none"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                الفرع
              </label>
              <select
                name="branchId"
                value={formData.branchId || ""}
                onChange={handleChange}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">اختر الفرع</option>
                <option value={1}>أسيوط</option>
                <option value={2}>أبوتيج</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                المدرب
              </label>
              <select
                name="instructorId"
                value={formData.instructorId || ""}
                onChange={handleChange}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">اختر المدرب</option>
                {instructors.length === 0 ? (
                  <option disabled>لا يوجد مدربين متاحين</option>
                ) : (
                  instructors.map((inst) => (
                    <option key={inst.id} value={inst.id}>
                      {inst.fullName}
                    </option>
                  ))
                )}
              </select>
            </div>
          </div>
        </div>

        {/* Schedule */}
        <div className="mb-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            مواعيد الدورة
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                أيام الدورة
              </label>
              <select
                name="days"
                multiple
                value={formData.scheduleDetails.days}
                onChange={handleScheduleChange}
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                size={4}
              >
                {DAYS_OF_WEEK.map((day) => (
                  <option key={day.value} value={day.value}>
                    {day.label}
                  </option>
                ))}
              </select>
              <p className="text-sm text-gray-500 mt-1">
                اضغط Ctrl للاختيار المتعدد
              </p>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  وقت البداية
                </label>
                <input
                  type="time"
                  name="startTime"
                  value={formData.scheduleDetails.startTime}
                  onChange={handleScheduleChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  وقت النهاية
                </label>
                <input
                  type="time"
                  name="endTime"
                  value={formData.scheduleDetails.endTime}
                  onChange={handleScheduleChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                تاريخ البداية
              </label>
              <input
                type="date"
                name="startDate"
                value={formData.startDate}
                onChange={handleChange}
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                تاريخ النهاية
              </label>
              <input
                type="date"
                name="endDate"
                value={formData.endDate}
                onChange={handleChange}
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>
        </div>

        {/* Course Content */}
        <div className="mb-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            محتوى الدورة
          </h2>
          <div className="space-y-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                وصف الدورة
              </label>
              <textarea
                name="description"
                value={formData.description}
                onChange={handleChange}
                required
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="وصف مختصر للدورة..."
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                محتوى الدورة
              </label>
              <textarea
                name="content"
                value={formData.content}
                onChange={handleChange}
                required
                rows={6}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="قم بتفصيل محتوى الدورة والمواضيع التي سيتم تغطيتها..."
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                المتطلبات الأساسية
              </label>
              <textarea
                name="prerequisites"
                value={formData.prerequisites}
                onChange={handleChange}
                required
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="المتطلبات الأساسية للانضمام للدورة..."
              />
            </div>
          </div>
        </div>

        <div className="mt-6 flex justify-end">
          <button
            type="submit"
            disabled={loading}
            className="flex items-center space-x-2 rtl:space-x-reverse px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
          >
            <Save className="w-4 h-4" />
            <span>{isEditing ? "حفظ التغييرات" : "إنشاء الدورة"}</span>
          </button>
        </div>
      </form>
    </div>
  );
};

export default CourseForm;
