import React, { useEffect, useMemo, useState } from "react";
import * as XLSX from "xlsx";
import paymentsAPI, {
  TransactionRow,
  FinancialSummary,
} from "../../lib/api/paymentsAPI.ts";
import axios from "../../lib/api/axios";
import {
  Users,
  BookText,
  DollarSign,
  Calendar,
  MapPin,
  Filter,
  FileDown,
  Eye,
  TrendingUp,
  TrendingDown,
  Wallet,
  RefreshCw,
  Search,
} from "lucide-react";

const ExpensesPage: React.FC = () => {
  const [transactions, setTransactions] = useState<TransactionRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [branchFilter, setBranchFilter] = useState("all");
  const [typeFilter, setTypeFilter] = useState<"all" | "income" | "expense">(
    "all"
  );
  const [courseFilter, setCourseFilter] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [summary, setSummary] = useState({
    totalIncome: 0,
    totalExpenses: 0,
    netBalance: 0,
  });
  const [error, setError] = useState<string | null>(null);
  const [loadingInvoice, setLoadingInvoice] = useState<number | null>(null);

  useEffect(() => {
    fetchTransactions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dateFrom, dateTo]);

  // تحديث البيانات كل 30 ثانية
  useEffect(() => {
    const interval = setInterval(() => {
      fetchTransactions();
    }, 30000);

    return () => clearInterval(interval);
  }, []);

  const fetchTransactions = async () => {
    setLoading(true);
    setError(null);
    try {
      // Try to get transactions from the main endpoint
      let result = await paymentsAPI.getAllTransactions({
        startDate: dateFrom || undefined,
        endDate: dateTo || undefined,
      });

      // If no data, try alternative endpoint
      if (result.transactions.length === 0) {
        console.log(
          "No transactions from main endpoint, trying alternative..."
        );
        result = await paymentsAPI.getAlternativeTransactions();
      }

      setTransactions(result.transactions);
      setSummary(result.summary);

      console.log("Processed transactions:", result.transactions);
      console.log("Summary from API:", result.summary);
    } catch (error) {
      console.error("Error fetching transactions:", error);
      // Try alternative endpoint as fallback
      try {
        console.log("Trying alternative endpoint as fallback...");
        const result = await paymentsAPI.getAlternativeTransactions();
        setTransactions(result.transactions);
        setSummary(result.summary);
      } catch (altError) {
        console.error("Alternative endpoint also failed:", altError);
        setError("خطأ في تحميل البيانات - يرجى المحاولة مرة أخرى");
        setTransactions([]);
      }
    } finally {
      setLoading(false);
    }
  };

  const exportToExcel = () => {
    if (filtered.length === 0) return;

    const totalsRow = {
      "إجمالي الدخل": summary.totalIncome,
      "إجمالي المصروف": summary.totalExpenses,
      "صافي الرصيد": summary.netBalance,
    } as any;

    const rows = filtered.map((t) => ({
      "اسم الطالب": t.studentName || "-",
      "اسم الكورس": t.courseName || "-",
      المبلغ: t.amount,
      "نوع العملية": t.transactionType === "income" ? "دخل" : "مصروف",
      "طريقة الدفع": t.paymentMethod || "-",
      "نوع الدفع": t.paymentType || "-",
      "تاريخ الدفع": t.paymentDate
        ? new Date(t.paymentDate).toLocaleDateString("ar-EG")
        : "-",
      الفئة: t.category || "-",
      الوصف: t.description || "-",
      "حالة الدفع": t.paymentStatus === "paid" ? "مدفوع" : "غير مدفوع",
      الفرع: t.branchName || "-",
      "تمت المعالجة بواسطة": t.processedBy || "-",
    }));

    const ws = XLSX.utils.json_to_sheet([...rows, {}, totalsRow]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Transactions");
    XLSX.writeFile(
      wb,
      `دفتر_المعاملات_${new Date().toLocaleDateString("ar-EG")}.xlsx`
    );
  };

  const openInvoice = async (invoiceUrl: string, transactionId: number) => {
    setLoadingInvoice(transactionId);
    try {
      // Make the request with authentication
      const response = await axios.get(invoiceUrl, {
        responseType: "blob", // Important for PDF files
      });

      // Create a blob URL and open it
      const blob = new Blob([response.data], { type: "application/pdf" });
      const url = window.URL.createObjectURL(blob);
      window.open(url, "_blank");

      // Clean up the URL after a delay
      setTimeout(() => {
        window.URL.revokeObjectURL(url);
      }, 1000);
    } catch (error: any) {
      console.error("Error opening invoice:", error);
      let errorMessage = "خطأ في فتح الفاتورة. يرجى المحاولة مرة أخرى.";

      if (error.response?.status === 404) {
        errorMessage = "الفاتورة غير موجودة.";
      } else if (error.response?.status === 401) {
        errorMessage = "غير مصرح لك بعرض هذه الفاتورة.";
      } else if (error.response?.status === 500) {
        errorMessage = "خطأ في الخادم. يرجى المحاولة مرة أخرى.";
      }

      alert(errorMessage);
    } finally {
      setLoadingInvoice(null);
    }
  };

  // الفلاتر والحسابات
  const filtered = useMemo(() => {
    return transactions.filter((t) => {
      const matchesBranch =
        branchFilter === "all" || t.branchName === branchFilter;
      const matchesType =
        typeFilter === "all" ||
        (typeFilter === "income" && t.transactionType === "income") ||
        (typeFilter === "expense" && t.transactionType === "expenses");
      const matchesCourse =
        !courseFilter ||
        (t.courseName || "").toLowerCase().includes(courseFilter.toLowerCase());
      const matchesDateFrom =
        !dateFrom || new Date(t.paymentDate) >= new Date(dateFrom);
      const matchesDateTo =
        !dateTo || new Date(t.paymentDate) <= new Date(dateTo);
      return (
        matchesBranch &&
        matchesType &&
        matchesCourse &&
        matchesDateFrom &&
        matchesDateTo
      );
    });
  }, [transactions, branchFilter, typeFilter, courseFilter, dateFrom, dateTo]);

  return (
    <div className="p-6 max-w-7xl mx-auto rtl">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          دفتر المعاملات المالية
        </h1>
        <p className="text-gray-600">
          عرض وإدارة جميع المعاملات المالية في النظام
        </p>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
          {error}
        </div>
      )}

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <div className="text-sm text-gray-600 mb-1">إجمالي الدخل</div>
              <div className="text-3xl font-bold text-green-600">
                {summary.totalIncome.toLocaleString()} جنيه
              </div>
            </div>
            <div className="p-3 bg-green-100 rounded-full">
              <TrendingUp className="w-8 h-8 text-green-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <div className="text-sm text-gray-600 mb-1">إجمالي المصروف</div>
              <div className="text-3xl font-bold text-red-600">
                {summary.totalExpenses.toLocaleString()} جنيه
              </div>
            </div>
            <div className="p-3 bg-red-100 rounded-full">
              <TrendingDown className="w-8 h-8 text-red-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <div className="text-sm text-gray-600 mb-1">صافي الرصيد</div>
              <div
                className={`text-3xl font-bold ${
                  summary.netBalance >= 0 ? "text-green-600" : "text-red-600"
                }`}
              >
                {summary.netBalance.toLocaleString()} جنيه
              </div>
            </div>
            <div
              className={`p-3 rounded-full ${
                summary.netBalance >= 0 ? "bg-green-100" : "bg-red-100"
              }`}
            >
              <DollarSign
                className={`w-8 h-8 ${
                  summary.netBalance >= 0 ? "text-green-600" : "text-red-600"
                }`}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Filters and Actions */}
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200 mb-6">
        <div className="flex flex-col lg:flex-row gap-4 items-center justify-between">
          <div className="flex flex-col sm:flex-row gap-3 items-center w-full lg:w-auto">
            <div className="relative">
              <Search className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
              <input
                type="text"
                placeholder="البحث في الكورسات..."
                value={courseFilter}
                onChange={(e) => setCourseFilter(e.target.value)}
                className="pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent w-full sm:w-64"
              />
            </div>

            <select
              value={branchFilter}
              onChange={(e) => setBranchFilter(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="all">كل الفروع</option>
              <option value="أسيوط">أسيوط</option>
              <option value="أبوتيج">أبوتيج</option>
            </select>

            <select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value as any)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="all">الكل</option>
              <option value="income">دخل فقط</option>
              <option value="expense">مصروف فقط</option>
            </select>

            <input
              type="date"
              value={dateFrom}
              onChange={(e) => setDateFrom(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />

            <input
              type="date"
              value={dateTo}
              onChange={(e) => setDateTo(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          <div className="flex gap-2">
            <button
              onClick={fetchTransactions}
              disabled={loading}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              <RefreshCw
                className={`w-4 h-4 ${loading ? "animate-spin" : ""}`}
              />
              <span>{loading ? "جاري التحديث..." : "تحديث"}</span>
            </button>

            <button
              onClick={exportToExcel}
              className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
            >
              <FileDown className="w-4 h-4" />
              <span>تصدير إكسل</span>
            </button>
          </div>
        </div>
      </div>

      {/* Transactions Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <Users className="w-4 h-4 text-gray-400" />
                    اسم الطالب
                  </div>
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <BookText className="w-4 h-4 text-gray-400" />
                    الكورس
                  </div>
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <DollarSign className="w-4 h-4 text-gray-400" />
                    المبلغ
                  </div>
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  نوع العملية
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  طريقة الدفع
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  الفئة
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  حالة الدفع
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <Calendar className="w-4 h-4 text-gray-400" />
                    تاريخ الدفع
                  </div>
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  <div className="flex items-center gap-2">
                    <MapPin className="w-4 h-4 text-gray-400" />
                    الفرع
                  </div>
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  الفاتورة
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {loading ? (
                <tr>
                  <td
                    colSpan={10}
                    className="text-center py-12 text-lg text-gray-500"
                  >
                    <div className="flex items-center justify-center gap-2">
                      <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
                      جاري التحميل...
                    </div>
                  </td>
                </tr>
              ) : filtered.length === 0 ? (
                <tr>
                  <td
                    colSpan={10}
                    className="text-center py-16 text-xl text-gray-500"
                  >
                    <div className="flex flex-col items-center gap-4">
                      <Wallet className="w-16 h-16 text-gray-300" />
                      <span>لا توجد معاملات مالية بعد</span>
                      <span className="text-3xl">💰</span>
                    </div>
                  </td>
                </tr>
              ) : (
                filtered.map((row) => (
                  <tr
                    key={row.id}
                    className={`hover:bg-gray-50 transition-colors ${
                      row.transactionType === "income"
                        ? "border-r-4 border-green-500"
                        : "border-r-4 border-red-500"
                    }`}
                  >
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {row.studentName || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {row.courseName || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                      {row.amount.toLocaleString()} جنيه
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 py-1 text-xs font-medium rounded-full ${
                          row.transactionType === "income"
                            ? "bg-green-100 text-green-800"
                            : "bg-red-100 text-red-800"
                        }`}
                      >
                        {row.transactionType === "income" ? "دخل" : "مصروف"}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {row.paymentMethod || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {row.paymentType || row.category || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 py-1 text-xs font-medium rounded-full ${
                          row.paymentStatus === "paid"
                            ? "bg-green-100 text-green-800"
                            : "bg-yellow-100 text-yellow-800"
                        }`}
                      >
                        {row.paymentStatus === "paid" ? "مدفوع" : "غير مدفوع"}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {new Date(row.paymentDate).toLocaleDateString("ar-EG")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {row.branchName || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <button
                        onClick={() => openInvoice(row.invoiceUrl, row.id)}
                        disabled={loadingInvoice === row.id}
                        className="flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs font-medium hover:bg-blue-200 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        {loadingInvoice === row.id ? (
                          <div className="animate-spin rounded-full h-3 w-3 border-b-2 border-blue-700"></div>
                        ) : (
                          <Eye className="w-3 h-3" />
                        )}
                        {loadingInvoice === row.id
                          ? "جاري التحميل..."
                          : "عرض الفاتورة"}
                      </button>
                    </td>
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
