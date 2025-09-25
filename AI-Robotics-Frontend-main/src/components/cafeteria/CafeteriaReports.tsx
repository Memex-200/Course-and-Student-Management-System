import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
//import { useAuth } from "../../contexts/AuthContext";
import { cafeteriaAPI } from "../../lib/api";
import { CafeteriaStatistics } from "../../types";
import {
  ArrowRight,
  Download,
  Calendar,
  DollarSign,
  Users,
  Filter,
} from "lucide-react";
import {
  BarChart as RechartsBarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
  PieChart as RechartsPieChart,
  Pie,
  Cell,
  Legend,
} from "recharts";
import toast from "react-hot-toast";

const CafeteriaReports: React.FC = () => {
  //const { user } = useAuth();
  const [statistics, setStatistics] = useState<CafeteriaStatistics | null>(
    null
  );
  const [loading, setLoading] = useState(true);
  const [selectedPeriod, setSelectedPeriod] = useState("today");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [revenueData, setRevenueData] = useState<any[]>([]);
  const [categoryData, setCategoryData] = useState<any[]>([]);
  const [topItems, setTopItems] = useState<any[]>([]);
  const [salesTrend, setSalesTrend] = useState<any[]>([]);

  const periodOptions = [
    { value: "today", label: "اليوم" },
    { value: "week", label: "هذا الأسبوع" },
    { value: "month", label: "هذا الشهر" },
    { value: "quarter", label: "هذا الربع" },
    { value: "year", label: "هذا العام" },
    { value: "custom", label: "فترة مخصصة" },
  ];

  const COLORS = ["#8884d8", "#82ca9d", "#ffc658", "#ff7300", "#8dd1e1"];

  useEffect(() => {
    fetchReports();
    // generateMockData(); // ألغِ توليد البيانات الوهمية
  }, [selectedPeriod, startDate, endDate]);

  const fetchReports = async () => {
    try {
      setLoading(true);
      const params: any = {};
      if (selectedPeriod === "custom" && startDate && endDate) {
        params.startDate = startDate;
        params.endDate = endDate;
      }
      const response = await cafeteriaAPI.getStatistics(params);
      setStatistics(response.data);
      setTopItems(response.data.topSellingItems || []);
      setRevenueData(response.data.revenueData || []);
      setCategoryData(response.data.categoryData || []);
      setSalesTrend(response.data.salesTrend || []);
    } catch (error) {
      console.error("Error fetching reports:", error);
      toast.error("خطأ في تحميل التقارير");
    } finally {
      setLoading(false);
    }
  };

  const exportToExcel = async () => {
    try {
      const params: any = {};
      if (selectedPeriod === "custom" && startDate && endDate) {
        params.startDate = startDate;
        params.endDate = endDate;
      }

      const response = await cafeteriaAPI.exportToExcel(params);

      // Create download link
      const blob = new Blob([response.data], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `cafeteria-report-${
        new Date().toISOString().split("T")[0]
      }.xlsx`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      toast.success("تم تصدير التقرير بنجاح");
    } catch (error) {
      console.error("Error exporting to Excel:", error);
      toast.error("خطأ في تصدير التقرير");
    }
  };

  const exportToPDF = async () => {
    try {
      const params: any = {};
      if (selectedPeriod === "custom" && startDate && endDate) {
        params.startDate = startDate;
        params.endDate = endDate;
      }

      const response = await cafeteriaAPI.exportToPDF(params);

      // Create download link
      const blob = new Blob([response.data], { type: "application/pdf" });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `cafeteria-report-${
        new Date().toISOString().split("T")[0]
      }.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      toast.success("تم إنشاء ملف PDF بنجاح");
    } catch (error) {
      console.error("Error exporting to PDF:", error);
      toast.error("خطأ في إنشاء ملف PDF");
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600"></div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center gap-4">
          <Link
            to="/cafeteria"
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <ArrowRight className="w-5 h-5" />
          </Link>
          <div>
            <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
              <DollarSign className="w-8 h-8 text-purple-500" />
              تقارير الكافيتيريا
            </h1>
            <p className="text-gray-600 mt-2">
              إحصائيات شاملة للمبيعات والأرباح والأداء
            </p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={exportToExcel}
            className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
          >
            <Download className="w-4 h-4" />
            Excel
          </button>
          <button
            onClick={exportToPDF}
            className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
          >
            <Users className="w-4 h-4" />
            PDF
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl shadow-sm p-6 border mb-8">
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Filter className="w-5 h-5" />
          فلترة التقارير
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              الفترة الزمنية
            </label>
            <select
              value={selectedPeriod}
              onChange={(e) => setSelectedPeriod(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
            >
              {periodOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          {selectedPeriod === "custom" && (
            <>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  من تاريخ
                </label>
                <input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  إلى تاريخ
                </label>
                <input
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                />
              </div>
            </>
          )}
        </div>
      </div>

      {/* Summary Statistics */}
      {statistics && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div className="bg-white rounded-xl shadow-sm p-6 border">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">
                  إجمالي الإيرادات
                </p>
                <p className="text-2xl font-bold text-green-600 mt-1">
                  {statistics.totalRevenue.toLocaleString()} جنيه
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  +12% من الفترة السابقة
                </p>
              </div>
              <div className="p-3 rounded-lg bg-green-50">
                <DollarSign className="w-6 h-6 text-green-600" />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">
                  إجمالي الطلبات
                </p>
                <p className="text-2xl font-bold text-blue-600 mt-1">
                  {statistics.totalOrders}
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  +8% من الفترة السابقة
                </p>
              </div>
              <div className="p-3 rounded-lg bg-blue-50">
                <Users className="w-6 h-6 text-blue-600" />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">صافي الربح</p>
                <p className="text-2xl font-bold text-purple-600 mt-1">
                  {statistics.totalProfit.toLocaleString()} جنيه
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  هامش ربح{" "}
                  {(
                    (statistics.totalProfit / statistics.totalRevenue) *
                    100
                  ).toFixed(1)}
                  %
                </p>
              </div>
              <div className="p-3 rounded-lg bg-purple-50">
                <Calendar className="w-6 h-6 text-purple-600" />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">
                  متوسط قيمة الطلب
                </p>
                <p className="text-2xl font-bold text-orange-600 mt-1">
                  {statistics.averageOrderValue.toFixed(1)} جنيه
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  +5% من الفترة السابقة
                </p>
              </div>
              <div className="p-3 rounded-lg bg-orange-50">
                <Calendar className="w-6 h-6 text-orange-600" />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
        {/* Revenue Trend Chart */}
        <div className="bg-white rounded-xl shadow-sm p-6 border">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">
            اتجاه الإيرادات (آخر 7 أيام)
          </h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={revenueData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip
                formatter={(value, name) => [
                  `${value} ${
                    name === "revenue"
                      ? "جنيه"
                      : name === "orders"
                      ? "طلب"
                      : "جنيه"
                  }`,
                  name === "revenue"
                    ? "الإيرادات"
                    : name === "orders"
                    ? "الطلبات"
                    : "الربح",
                ]}
              />
              <Line
                type="monotone"
                dataKey="revenue"
                stroke="#8884d8"
                strokeWidth={2}
                name="revenue"
              />
              <Line
                type="monotone"
                dataKey="profit"
                stroke="#82ca9d"
                strokeWidth={2}
                name="profit"
              />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {/* Category Distribution */}
        <div className="bg-white rounded-xl shadow-sm p-6 border">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">
            توزيع المبيعات بالفئات
          </h3>
          <ResponsiveContainer width="100%" height={300}>
            <RechartsPieChart>
              <Pie
                data={categoryData}
                cx="50%"
                cy="50%"
                labelLine={false}
                label={({ name, percent }) =>
                  percent !== undefined && percent !== null
                    ? `${name} ${(percent * 100).toFixed(0)}%`
                    : name
                }
                outerRadius={80}
                fill="#8884d8"
                dataKey="value"
              >
                {categoryData.map((_, index) => (
                  <Cell
                    key={`cell-${index}`}
                    fill={COLORS[index % COLORS.length]}
                  />
                ))}
              </Pie>
              <Tooltip formatter={(value) => [`${value} جنيه`, "المبيعات"]} />
            </RechartsPieChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Orders by Time Chart */}
      <div className="bg-white rounded-xl shadow-sm p-6 border mb-8">
        <h3 className="text-lg font-semibold text-gray-900 mb-6">
          الطلبات والإيرادات اليومية
        </h3>
        <ResponsiveContainer width="100%" height={400}>
          <RechartsBarChart data={revenueData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" />
            <YAxis yAxisId="left" />
            <YAxis yAxisId="right" orientation="right" />
            <Tooltip
              formatter={(value, name) => {
                if (name === "الإيرادات") return [`${value} جنيه`, "الإيرادات"];
                if (name === "عدد الطلبات")
                  return [`${value} طلب`, "عدد الطلبات"];
                return [value, name];
              }}
            />
            <Legend />
            <Bar
              yAxisId="left"
              dataKey="revenue"
              fill="#8884d8"
              name="الإيرادات"
            />
            <Bar
              yAxisId="right"
              dataKey="orders"
              fill="#82ca9d"
              name="عدد الطلبات"
            />
          </RechartsBarChart>
        </ResponsiveContainer>
      </div>

      {/* Top Selling Items & Monthly Trend */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Top Selling Items */}
        <div className="bg-white rounded-xl shadow-sm p-6 border">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">
            أكثر المنتجات مبيعاً
          </h3>
          <div className="space-y-4">
            {topItems.map((item, index) => (
              <div
                key={index}
                className="flex items-center justify-between p-4 bg-gray-50 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 bg-purple-100 text-purple-600 rounded-full flex items-center justify-center text-sm font-semibold">
                    {index + 1}
                  </div>
                  <div>
                    <p className="font-medium text-gray-900">{item.name}</p>
                    <p className="text-sm text-gray-600">
                      {item.quantity} قطعة
                    </p>
                  </div>
                </div>
                <div className="text-right">
                  <p className="font-semibold text-gray-900">
                    {item.sales} جنيه
                  </p>
                  <p className="text-sm text-gray-600">إجمالي المبيعات</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Monthly Sales Trend */}
        <div className="bg-white rounded-xl shadow-sm p-6 border">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">
            اتجاه المبيعات الشهرية
          </h3>
          <ResponsiveContainer width="100%" height={300}>
            <RechartsBarChart data={salesTrend}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="week" />
              <YAxis />
              <Tooltip
                formatter={(value, name) => {
                  if (name === "المبيعات") return [`${value} جنيه`, "المبيعات"];
                  if (name === "عدد الطلبات")
                    return [`${value} طلب`, "عدد الطلبات"];
                  return [value, name];
                }}
              />
              <Legend />
              <Bar dataKey="sales" fill="#8884d8" name="المبيعات" />
              <Bar dataKey="orders" fill="#82ca9d" name="عدد الطلبات" />
            </RechartsBarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Detailed Statistics Table */}
      <div className="bg-white rounded-xl shadow-sm p-6 border mt-8">
        <h3 className="text-lg font-semibold text-gray-900 mb-6">
          إحصائيات تفصيلية
        </h3>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  المقياس
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  القيمة
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  المقارنة
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {statistics &&
                [
                  {
                    metric: "إجمالي الطلبات",
                    value: statistics.totalOrders,
                    comparison: "+8%",
                  },
                  {
                    metric: "الطلبات المكتملة",
                    value: statistics.completedOrders,
                    comparison: "+12%",
                  },
                  {
                    metric: "الطلبات الملغية",
                    value: statistics.cancelledOrders,
                    comparison: "-5%",
                  },
                  {
                    metric: "معدل الإلغاء",
                    value: `${(
                      (statistics.cancelledOrders / statistics.totalOrders) *
                      100
                    ).toFixed(1)}%`,
                    comparison: "-2%",
                  },
                  {
                    metric: "إجمالي الإيرادات",
                    value: `${statistics.totalRevenue.toLocaleString()} جنيه`,
                    comparison: "+15%",
                  },
                  {
                    metric: "إجمالي التكاليف",
                    value: `${statistics.totalCost.toLocaleString()} جنيه`,
                    comparison: "+8%",
                  },
                  {
                    metric: "صافي الربح",
                    value: `${statistics.totalProfit.toLocaleString()} جنيه`,
                    comparison: "+22%",
                  },
                  {
                    metric: "هامش الربح",
                    value: `${(
                      (statistics.totalProfit / statistics.totalRevenue) *
                      100
                    ).toFixed(1)}%`,
                    comparison: "+3%",
                  },
                ].map((row, index) => (
                  <tr key={index}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {row.metric}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {row.value}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <span
                        className={`${
                          row.comparison.startsWith("+")
                            ? "text-green-600"
                            : "text-red-600"
                        }`}
                      >
                        {row.comparison}
                      </span>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default CafeteriaReports;
