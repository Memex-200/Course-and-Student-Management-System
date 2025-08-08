import React, { useState } from "react";
import axios from "../../lib/api/axios";
import toast from "react-hot-toast";

const AddTrainerForm: React.FC = () => {
  const [form, setForm] = useState({
    fullName: "",
    username: "",
    password: "",
    branchId: "",
    email: "",
    phone: "",
    notes: "",
  });
  const [loading, setLoading] = useState(false);

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
    >
  ) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.fullName || !form.username || !form.password || !form.branchId) {
      toast.error("الاسم واسم المستخدم وكلمة المرور والفرع مطلوبة");
      return;
    }
    setLoading(true);
    try {
      const res = await axios.post("/employees", {
        fullName: form.fullName,
        username: form.username,
        password: form.password,
        branchId: Number(form.branchId),
        email: form.email,
        phone: form.phone,
        notes: form.notes,
      });
      if (res.data.success) {
        toast.success("تم إضافة المدرب بنجاح");
        setForm({
          fullName: "",
          username: "",
          password: "",
          branchId: "",
          email: "",
          phone: "",
          notes: "",
        });
      } else {
        toast.error(res.data.message || "حدث خطأ أثناء الإضافة");
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || "حدث خطأ أثناء الإضافة");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="bg-white rounded-xl shadow-sm p-6 max-w-lg mx-auto rtl"
    >
      <h2 className="text-2xl font-bold mb-6 text-center">إضافة مدرب جديد</h2>
      <div className="mb-4">
        <label className="block mb-1">
          الاسم الكامل<span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          name="fullName"
          value={form.fullName}
          onChange={handleChange}
          className="w-full px-4 py-2 border rounded-lg"
          required
        />
      </div>
      <div className="mb-4">
        <label className="block mb-1">
          اسم المستخدم<span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          name="username"
          value={form.username}
          onChange={handleChange}
          className="w-full px-4 py-2 border rounded-lg"
          required
        />
      </div>
      <div className="mb-4">
        <label className="block mb-1">
          كلمة المرور<span className="text-red-500">*</span>
        </label>
        <input
          type="password"
          name="password"
          value={form.password}
          onChange={handleChange}
          className="w-full px-4 py-2 border rounded-lg"
          required
        />
      </div>
      <div className="mb-4">
        <label className="block mb-1">
          الفرع<span className="text-red-500">*</span>
        </label>
        <select
          name="branchId"
          value={form.branchId}
          onChange={handleChange}
          className="w-full px-4 py-2 border rounded-lg"
          required
        >
          <option value="">اختر الفرع</option>
          <option value={1}>أسيوط</option>
          <option value={2}>أبوتيج</option>
        </select>
      </div>
      <div className="mb-4">
        <label className="block mb-1">البريد الإلكتروني</label>
        <input
          type="email"
          name="email"
          value={form.email}
          onChange={handleChange}
          className="w-full px-4 py-2 border rounded-lg"
        />
      </div>
      <div className="mb-4">
        <label className="block mb-1">رقم الهاتف</label>
        <input
          type="text"
          name="phone"
          value={form.phone}
          onChange={handleChange}
          className="w-full px-4 py-2 border rounded-lg"
        />
      </div>
      <div className="mb-4">
        <label className="block mb-1">ملاحظات</label>
        <textarea
          name="notes"
          value={form.notes}
          onChange={handleChange}
          className="w-full px-4 py-2 border rounded-lg"
          rows={2}
        />
      </div>
      <button
        type="submit"
        className="w-full bg-primary-500 text-white py-2 rounded-lg font-bold"
        disabled={loading}
      >
        {loading ? "جارٍ الإضافة..." : "إضافة المدرب"}
      </button>
    </form>
  );
};

export default AddTrainerForm;
