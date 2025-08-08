import React, { useState, useEffect } from "react";
import { useAuth } from "../../contexts/AuthContext";
import { User, Mail, Phone } from "lucide-react";
import toast from "react-hot-toast";
import { authAPI } from "../../lib/api";

const StudentAccount: React.FC = () => {
  const { user, setUser, login } = useAuth();
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    fullName: user?.fullName || "",
    email: user?.email || "",
    phone: user?.phone || "",
    address: user?.address || "",
    parentName: "",
    parentPhone: "",
    parentEmail: "",
    emergencyContact: "",
    emergencyPhone: "",
    joinDate: "2024-01-01",
    totalCourses: 3,
    completedCourses: 1,
    currentCourses: 2,
    certificates: 1,
  });

  // Reset form data when user changes
  useEffect(() => {
    if (user) {
      setFormData((prev) => ({
        ...prev,
        fullName: user.fullName || "",
        email: user.email || "",
        phone: user.phone || "",
        address: user.address || "",
      }));
    }
  }, [user]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isEditing) return; // Don't submit if not in edit mode

    try {
      const response = await authAPI.updateUser({
        fullName: formData.fullName,
        email: formData.email,
        phone: formData.phone,
        address: formData.address,
      });

      // Update the user context with the new data
      // Update user data and token using login function to ensure proper normalization
      if (response.data.token && response.data.user) {
        // Use the direct token and user data to update the context properly
        localStorage.setItem("token", response.data.token);
        localStorage.setItem("user", JSON.stringify(response.data.user));
        setUser(response.data.user);
      }

      toast.success("تم تحديث البيانات بنجاح");
      setIsEditing(false);
    } catch (error: any) {
      toast.error(
        error.response?.data?.message || "حدث خطأ أثناء تحديث البيانات"
      );
    }
  };

  return (
    <main className="flex-1 min-h-screen bg-secondary-950">
      <div className="flex flex-col min-h-screen">
        <div className="flex-1 p-6">
          <div className="max-w-7xl mx-auto">
            <div className="bg-gradient-to-b from-secondary-900/80 to-primary-900/80 rounded-2xl shadow-xl backdrop-blur-xl overflow-hidden border border-secondary-800/20">
              {/* Header with Edit/Save Buttons */}
              <div className="bg-gradient-to-r from-primary-600/90 to-primary-800/90 px-6 py-8 relative overflow-hidden backdrop-blur-xl border-b border-primary-700/20">
                <div className="absolute inset-0 opacity-10">
                  <div className="absolute top-0 left-0 w-40 h-40 bg-white rounded-full blur-3xl transform -translate-x-1/2 -translate-y-1/2"></div>
                  <div className="absolute bottom-0 right-0 w-32 h-32 bg-white rounded-full blur-2xl"></div>
                </div>

                <div className="relative flex items-center justify-between">
                  <div className="flex items-center space-x-4 rtl:space-x-reverse">
                    <div className="w-20 h-20 bg-gradient-to-br from-primary-500/80 to-accent-500/80 rounded-2xl shadow-lg flex items-center justify-center backdrop-blur-xl border border-primary-400/10">
                      <User className="w-10 h-10 text-white" />
                    </div>
                    <div>
                      <h1 className="text-2xl font-bold text-white">
                        {user?.fullName}
                      </h1>
                      <p className="text-primary-100">طالب في الشركة</p>
                    </div>
                  </div>

                  <div className="flex items-center gap-2">
                    {isEditing ? (
                      <>
                        <button
                          onClick={handleSubmit}
                          className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-primary-500 hover:bg-primary-600 text-white rounded-xl transition-all duration-200 border border-primary-400/10"
                        >
                          <span>حفظ</span>
                        </button>
                        <button
                          onClick={() => {
                            setIsEditing(false);
                            // Reset form data to original values
                            setFormData((prev) => ({
                              ...prev,
                              fullName: user?.fullName || "",
                              email: user?.email || "",
                              phone: user?.phone || "",
                              address: user?.address || "",
                            }));
                          }}
                          className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-white/5 hover:bg-white/10 text-white rounded-xl transition-all duration-200 border border-white/5"
                        >
                          <span>إلغاء</span>
                        </button>
                      </>
                    ) : (
                      <button
                        onClick={() => setIsEditing(true)}
                        className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-white/5 hover:bg-white/10 text-white rounded-xl transition-all duration-200 border border-white/5"
                      >
                        <User className="w-5 h-5" />
                        <span>تعديل</span>
                      </button>
                    )}
                  </div>
                </div>
              </div>

              {/* Content */}
              <div className="p-6 bg-secondary-900/10">
                <form className="space-y-6">
                  {/* Personal Information */}
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="bg-secondary-800/20 backdrop-blur-xl p-6 rounded-xl border border-secondary-700/20">
                      <h2 className="text-xl font-semibold text-white mb-4">
                        المعلومات الشخصية
                      </h2>
                      <div className="space-y-4">
                        <div>
                          <label className="block text-sm font-medium text-gray-300 mb-1">
                            الاسم الكامل
                          </label>
                          <div className="relative">
                            <User className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                              type="text"
                              name="fullName"
                              value={formData.fullName}
                              onChange={handleChange}
                              disabled={!isEditing}
                              className="w-full pr-10 py-2 bg-secondary-700/20 border border-secondary-600/20 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-white placeholder-gray-400 disabled:bg-secondary-800/20"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-300 mb-1">
                            البريد الإلكتروني
                          </label>
                          <div className="relative">
                            <Mail className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                              type="email"
                              name="email"
                              value={formData.email}
                              onChange={handleChange}
                              disabled={!isEditing}
                              className="w-full pr-10 py-2 bg-secondary-700/20 border border-secondary-600/20 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-white placeholder-gray-400 disabled:bg-secondary-800/20"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-300 mb-1">
                            رقم الهاتف
                          </label>
                          <div className="relative">
                            <Phone className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                              type="tel"
                              name="phone"
                              value={formData.phone}
                              onChange={handleChange}
                              disabled={!isEditing}
                              className="w-full pr-10 py-2 bg-secondary-700/20 border border-secondary-600/20 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-white placeholder-gray-400 disabled:bg-secondary-800/20"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-300 mb-1">
                            العنوان
                          </label>
                          <div className="relative">
                            <User className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                              type="text"
                              name="address"
                              value={formData.address}
                              onChange={handleChange}
                              disabled={!isEditing}
                              className="w-full pr-10 py-2 bg-secondary-700/20 border border-secondary-600/20 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-white placeholder-gray-400 disabled:bg-secondary-800/20"
                            />
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Parent Information */}
                    <div className="bg-secondary-800/20 backdrop-blur-xl p-6 rounded-xl border border-secondary-700/20">
                      <h2 className="text-xl font-semibold text-white mb-4">
                        معلومات ولي الأمر
                      </h2>
                      <div className="space-y-4">
                        <div>
                          <label className="block text-sm font-medium text-gray-300 mb-1">
                            اسم ولي الأمر
                          </label>
                          <div className="relative">
                            <User className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                              type="text"
                              name="parentName"
                              value={formData.parentName}
                              onChange={handleChange}
                              disabled={!isEditing}
                              className="w-full pr-10 py-2 bg-secondary-700/20 border border-secondary-600/20 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-white placeholder-gray-400 disabled:bg-secondary-800/20"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-300 mb-1">
                            رقم هاتف ولي الأمر
                          </label>
                          <div className="relative">
                            <Phone className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                              type="tel"
                              name="parentPhone"
                              value={formData.parentPhone}
                              onChange={handleChange}
                              disabled={!isEditing}
                              className="w-full pr-10 py-2 bg-secondary-700/20 border border-secondary-600/20 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-white placeholder-gray-400 disabled:bg-secondary-800/20"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-300 mb-1">
                            البريد الإلكتروني لولي الأمر
                          </label>
                          <div className="relative">
                            <Mail className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                              type="email"
                              name="parentEmail"
                              value={formData.parentEmail}
                              onChange={handleChange}
                              disabled={!isEditing}
                              className="w-full pr-10 py-2 bg-secondary-700/20 border border-secondary-600/20 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-white placeholder-gray-400 disabled:bg-secondary-800/20"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-300 mb-1">
                            رقم الطوارئ
                          </label>
                          <div className="relative">
                            <Phone className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                              type="tel"
                              name="emergencyPhone"
                              value={formData.emergencyPhone}
                              onChange={handleChange}
                              disabled={!isEditing}
                              className="w-full pr-10 py-2 bg-secondary-700/20 border border-secondary-600/20 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-white placeholder-gray-400 disabled:bg-secondary-800/20"
                            />
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Academic Information */}
                    <div className="md:col-span-2 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                      <div className="bg-secondary-800/20 backdrop-blur-xl p-6 rounded-xl border border-secondary-700/20">
                        <div className="flex items-center justify-between mb-4">
                          <User className="w-8 h-8 text-primary-400" />
                          <span className="text-3xl font-bold text-white">
                            {formData.totalCourses}
                          </span>
                        </div>
                        <h3 className="text-lg font-medium text-gray-300">
                          إجمالي الكورسات
                        </h3>
                      </div>

                      <div className="bg-secondary-800/20 backdrop-blur-xl p-6 rounded-xl border border-secondary-700/20">
                        <div className="flex items-center justify-between mb-4">
                          <User className="w-8 h-8 text-primary-400" />
                          <span className="text-3xl font-bold text-white">
                            {formData.completedCourses}
                          </span>
                        </div>
                        <h3 className="text-lg font-medium text-gray-300">
                          الكورسات المكتملة
                        </h3>
                      </div>

                      <div className="bg-secondary-800/20 backdrop-blur-xl p-6 rounded-xl border border-secondary-700/20">
                        <div className="flex items-center justify-between mb-4">
                          <User className="w-8 h-8 text-primary-400" />
                          <span className="text-3xl font-bold text-white">
                            {formData.currentCourses}
                          </span>
                        </div>
                        <h3 className="text-lg font-medium text-gray-300">
                          الكورسات الحالية
                        </h3>
                      </div>

                      <div className="bg-secondary-800/20 backdrop-blur-xl p-6 rounded-xl border border-secondary-700/20">
                        <div className="flex items-center justify-between mb-4">
                          <User className="w-8 h-8 text-primary-400" />
                          <span className="text-3xl font-bold text-white">
                            {formData.certificates}
                          </span>
                        </div>
                        <h3 className="text-lg font-medium text-gray-300">
                          الشهادات
                        </h3>
                      </div>
                    </div>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  );
};

export default StudentAccount;
