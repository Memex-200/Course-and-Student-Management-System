import React, { useEffect, useState } from "react";
import axios from "../../lib/api/axios";
import * as XLSX from "xlsx";
import {
  Users,
  BookText,
  DollarSign,
  Calendar,
  MapPin,
  Filter,
  FileDown,
} from "lucide-react";

interface ExpenseRow {
  id: number;
  studentName: string;
  courseName: string;
  amount: number;
  paymentStatus: string;
  paymentDate: string;
  branch: string;
  type: string; // وارد أو صادر
  category?: string;
  description?: string;
}

const ExpensesPage: React.FC = () => {
  const [expenses, setExpenses] = useState<ExpenseRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [branchFilter, setBranchFilter] = useState("all");
  const [statusFilter, setStatusFilter] = useState("all");

  useEffect(() => {
    fetchExpenses();
  }, []);

  const fetchExpenses = async () => {
    setLoading(true);
    try {
      const response = await axios.get("/expenses");
      const data = response.data.data || [];
      // تحويل البيانات إلى الشكل المطلوب
      const rows: ExpenseRow[] = data.map((exp: any) => ({
        id: exp.id,
        studentName: exp.studentName || "-",
        courseName: exp.courseName || "-",
        amount: exp.amount,
        paymentStatus: exp.statusArabic || exp.status,
        paymentDate: exp.expenseDate
          ? new Date(exp.expenseDate).toLocaleDateString("ar-EG")
          : "-",
        branch: exp.branchName || "-",
        type: exp.amount >= 0 ? "وارد" : "صادر", // لو المبلغ موجب اعتبره وارد
        category: exp.categoryArabic || exp.category || "-",
        description: exp.description || "-",
      }));
      setExpenses(rows);
    } catch (error) {
      setExpenses([]);
    } finally {
      setLoading(false);
    }
  };

  const exportToExcel = () => {
    if (expenses.length === 0) return;
    const ws = XLSX.utils.json_to_sheet(expenses);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Expenses");
    XLSX.writeFile(
      wb,
      `تقرير_المصروفات_${new Date().toLocaleDateString("ar-EG")}.xlsx`
    );
  };

  // فلترة حسب الفرع وحالة الدفع
  const filteredExpenses = expenses.filter(
    (row) =>
      (branchFilter === "all" || row.branch === branchFilter) &&
      (statusFilter === "all" || row.paymentStatus === statusFilter)
  );

  return (
    <div className="p-6 max-w-4xl mx-auto rtl">
      <div className="bg-gradient-to-br from-secondary-900 to-secondary-800 rounded-3xl p-8 shadow-2xl border border-primary-500/20">
        <div className="flex items-center mb-6 gap-3">
          <DollarSign className="w-8 h-8 text-yellow-500" />
          <h1 className="text-3xl font-bold text-white">صفحة المصروفات</h1>
        </div>

        <div className="flex flex-col md:flex-row gap-4 mb-6 items-center justify-between">
          <div className="flex gap-2 w-full md:w-auto">
            <select
              value={branchFilter}
              onChange={(e) => setBranchFilter(e.target.value)}
              className="px-4 py-2 border border-primary-500/30 rounded-lg bg-secondary-800/50 text-white focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            >
              <option value="all">كل الفروع</option>
              <option value="أسيوط">أسيوط</option>
              <option value="أبوتيج">أبوتيج</option>
            </select>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="px-4 py-2 border border-primary-500/30 rounded-lg bg-secondary-800/50 text-white focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            >
              <option value="all">كل الحالات</option>
              <option value="مدفوع">مدفوع</option>
              <option value="غير مدفوع">غير مدفوع</option>
              <option value="جزئي">جزئي</option>
            </select>
          </div>
          <button
            onClick={exportToExcel}
            className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-green-600 to-green-700 text-white rounded-xl hover:from-green-700 hover:to-green-800 transition-all duration-300 shadow-lg hover:shadow-xl"
          >
            <FileDown className="w-5 h-5" />
            <span>تصدير إكسل</span>
          </button>
        </div>

        <div className="overflow-x-auto bg-secondary-800/30 backdrop-blur-sm rounded-2xl border border-primary-500/20">
          <table className="min-w-full">
            <thead className="bg-secondary-700/50">
              <tr>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <Users className="w-4 h-4 text-blue-400" />
                    اسم الطالب
                  </div>
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <BookText className="w-4 h-4 text-green-400" />
                    الكورس
                  </div>
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <DollarSign className="w-4 h-4 text-yellow-400" />
                    المبلغ
                  </div>
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  نوع العملية
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  الفئة
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  الوصف
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  حالة الدفع
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <Calendar className="w-4 h-4 text-purple-400" />
                    تاريخ الدفع
                  </div>
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-primary-300 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <MapPin className="w-4 h-4 text-pink-400" />
                    الفرع
                  </div>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-primary-500/20">
              {loading ? (
                <tr>
                  <td
                    colSpan={9}
                    className="text-center py-12 text-lg text-primary-300"
                  >
                    <div className="flex items-center justify-center gap-2">
                      <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary-400"></div>
                      جاري التحميل...
                    </div>
                  </td>
                </tr>
              ) : filteredExpenses.length === 0 ? (
                <tr>
                  <td
                    colSpan={9}
                    className="text-center py-16 text-xl text-primary-300 font-bold"
                  >
                    <div className="flex flex-col items-center gap-4">
                      <DollarSign className="w-16 h-16 text-primary-400/50" />
                      <span>لا توجد بيانات مصروفات بعد</span>
                      <span className="text-3xl">🪙</span>
                    </div>
                  </td>
                </tr>
              ) : (
                filteredExpenses.map((row) => (
                  <tr
                    key={row.id}
                    className={`hover:bg-secondary-700/30 transition-all duration-200 ${
                      row.type === "وارد"
                        ? "bg-green-500/10 border-r-4 border-green-500"
                        : "bg-red-500/10 border-r-4 border-red-500"
                    }`}
                  >
                    <td className="px-6 py-4 font-medium text-white">
                      {row.studentName}
                    </td>
                    <td className="px-6 py-4 text-primary-200">
                      {row.courseName}
                    </td>
                    <td className="px-6 py-4 font-bold text-yellow-400">
                      {row.amount} جنيه
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`px-3 py-1 rounded-full text-xs font-bold ${
                          row.type === "وارد"
                            ? "bg-green-500/20 text-green-400 border border-green-500/30"
                            : "bg-red-500/20 text-red-400 border border-red-500/30"
                        }`}
                      >
                        {row.type}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-primary-200">
                      {row.category}
                    </td>
                    <td className="px-6 py-4 text-primary-200">
                      {row.description}
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`px-3 py-1 rounded-full text-xs font-bold ${
                          row.paymentStatus === "مدفوع"
                            ? "bg-green-500/20 text-green-400 border border-green-500/30"
                            : row.paymentStatus === "غير مدفوع"
                            ? "bg-red-500/20 text-red-400 border border-red-500/30"
                            : row.paymentStatus === "جزئي"
                            ? "bg-yellow-500/20 text-yellow-400 border border-yellow-500/30"
                            : "bg-gray-500/20 text-gray-400 border border-gray-500/30"
                        }`}
                      >
                        {row.paymentStatus}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-primary-200">
                      {row.paymentDate}
                    </td>
                    <td className="px-6 py-4 text-primary-200">{row.branch}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default ExpensesPage;
