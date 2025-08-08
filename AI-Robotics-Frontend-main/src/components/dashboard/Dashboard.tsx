import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import DashboardStats from "./DashboardStats";
import { reportsAPI } from "../../lib/api";
import { DashboardStats as Stats } from "../../types";
import { UserRole } from "../../types";
import CafeteriaDashboard from "../cafeteria/CafeteriaDashboard";
import {
  Calendar,
  Users,
  DollarSign,
  BarChart as TrendingUp,
  Clock,
  GraduationCap as BookOpen,
} from "lucide-react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
} from "recharts";
import { Link } from "react-router-dom";

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [stats, setStats] = useState<Stats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      const response = await reportsAPI.getDashboard();
      setStats(response.data.data);
    } catch (error) {
      console.error("Error fetching dashboard data:", error);
      // Mock data for demo
      setStats({
        totalStudents: 234,
        totalCourses: 15,
        activeCourses: 12,
        totalRevenue: 125000,
        monthlyRevenue: 24500,
        activeWorkspaceBookings: 8,
        pendingPayments: 3,
        upcomingClasses: 7,
      });
    } finally {
      setLoading(false);
    }
  };

  // Mock chart data
  const revenueData = [
    { name: "يناير", revenue: 18500, courses: 145, workspace: 4200 },
    { name: "فبراير", revenue: 22300, courses: 167, workspace: 5100 },
    { name: "مارس", revenue: 19800, courses: 134, workspace: 3800 },
    { name: "أبريل", revenue: 26100, courses: 189, workspace: 6300 },
    { name: "مايو", revenue: 23700, courses: 156, workspace: 5400 },
    { name: "يونيو", revenue: 24500, courses: 167, workspace: 5800 },
  ];

  const courseCategoryData = [
    { name: "الروبوتات", value: 35, color: "#3B82F6" },
    { name: "البرمجة", value: 30, color: "#10B981" },
    { name: "الذكاء الاصطناعي", value: 20, color: "#8B5CF6" },
    { name: "الميكاترونيكس", value: 15, color: "#F59E0B" },
  ];

  const studentProgressData = [
    { name: "الأسبوع 1", completed: 23, inProgress: 45, new: 12 },
    { name: "الأسبوع 2", completed: 28, inProgress: 42, new: 18 },
    { name: "الأسبوع 3", completed: 31, inProgress: 38, new: 15 },
    { name: "الأسبوع 4", completed: 35, inProgress: 41, new: 22 },
  ];

  const getWelcomeMessage = () => {
    const hour = new Date().getHours();
    if (hour < 12) return "صباح الخير";
    if (hour < 18) return "مساء الخير";
    return "مساء الخير";
  };

  return (
    <div className="p-6 max-w-7xl mx-auto rtl">
      {/* Welcome Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          {getWelcomeMessage()}، {user?.fullName}
        </h1>
        <p className="text-gray-600">
          مرحباً بك في لوحة التحكم - {user?.branch}
        </p>
        <div className="mt-4 text-sm text-gray-500">
          {new Date().toLocaleDateString("ar-EG", {
            weekday: "long",
            year: "numeric",
            month: "long",
            day: "numeric",
          })}
        </div>
        {user?.role === UserRole.Admin && (
          <button
            onClick={() => navigate("/dashboard/add-trainer")}
            className="mt-6 bg-primary-500 hover:bg-primary-600 text-white font-bold py-2 px-6 rounded-lg shadow transition-all duration-200"
          >
            إضافة مدرب جديد
          </button>
        )}
      </div>

      {/* Stats Cards */}
      {stats && <DashboardStats stats={stats} isLoading={loading} />}

      {/* Charts Section */}
      {/* الرسوم البيانية مخفية مؤقتًا حتى يتم تجهيز بيانات حقيقية لها من الـ backend */}
      {/*
      <div className="grid lg:grid-cols-2 gap-8 mb-8">
        ...
      </div>
      <div className="grid lg:grid-cols-1 gap-8 mb-8">
        ...
      </div>
      */}

      {/* Quick Actions */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6 mb-8">
        <div className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow cursor-pointer">
          <div className="flex items-center space-x-3 rtl:space-x-reverse mb-4">
            <div className="bg-blue-50 p-3 rounded-lg">
              <Users className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <h4 className="font-semibold text-gray-900">طالب جديد</h4>
              <p className="text-sm text-gray-600">إضافة طالب للنظام</p>
            </div>
          </div>
          <button
            onClick={() => navigate("/students/new")}
            className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-colors"
          >
            إضافة الآن
          </button>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow cursor-pointer">
          <div className="flex items-center space-x-3 rtl:space-x-reverse mb-4">
            <div className="bg-green-50 p-3 rounded-lg">
              <BookOpen className="w-6 h-6 text-green-600" />
            </div>
            <div>
              <h4 className="font-semibold text-gray-900">كورس جديد</h4>
              <p className="text-sm text-gray-600">إنشاء كورس تدريبي</p>
            </div>
          </div>
          <button
            onClick={() => navigate("/courses/new")}
            className="w-full bg-green-600 text-white py-2 rounded-lg hover:bg-green-700 transition-colors"
          >
            إنشاء الآن
          </button>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow cursor-pointer">
          <div className="flex items-center space-x-3 rtl:space-x-reverse mb-4">
            <div className="bg-purple-50 p-3 rounded-lg">
              <Calendar className="w-6 h-6 text-purple-600" />
            </div>
            <div>
              <h4 className="font-semibold text-gray-900">حجز مساحة</h4>
              <p className="text-sm text-gray-600">حجز مساحة عمل</p>
            </div>
          </div>
          <button
            onClick={() => alert("ميزة حجز المساحة قيد التطوير")}
            className="w-full bg-purple-600 text-white py-2 rounded-lg hover:bg-purple-700 transition-colors"
          >
            حجز الآن
          </button>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow cursor-pointer">
          <div className="flex items-center space-x-3 rtl:space-x-reverse mb-4">
            <div className="bg-orange-50 p-3 rounded-lg">
              <Clock className="w-6 h-6 text-orange-600" />
            </div>
            <div>
              <h4 className="font-semibold text-gray-900">تسجيل حضور</h4>
              <p className="text-sm text-gray-600">تسجيل حضور الطلاب</p>
            </div>
          </div>
          <button
            onClick={() => alert("ميزة تسجيل الحضور قيد التطوير")}
            className="w-full bg-orange-600 text-white py-2 rounded-lg hover:bg-orange-700 transition-colors"
          >
            تسجيل الآن
          </button>
        </div>
        <Link
          to="/expenses"
          className="bg-yellow-600 hover:bg-yellow-700 text-white font-bold rounded-xl px-6 py-6 flex flex-col items-center justify-center shadow transition-all duration-200"
        >
          <span className="text-2xl mb-2">💰</span>
          <span>صفحة المصروفات</span>
        </Link>
      </div>

      {/* Revenue Chart */}
      <div className="bg-white rounded-xl shadow-sm p-6 border">
        <h3 className="text-lg font-semibold text-gray-900 mb-6">
          اتجاه الإيرادات الشهرية
        </h3>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={revenueData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <Tooltip />
            <Line
              type="monotone"
              dataKey="revenue"
              stroke="#8884d8"
              strokeWidth={2}
            />
            <Line
              type="monotone"
              dataKey="courses"
              stroke="#82ca9d"
              strokeWidth={2}
            />
            <Line
              type="monotone"
              dataKey="workspace"
              stroke="#ffc658"
              strokeWidth={2}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>

      {/* Cafeteria Dashboard */}
      <CafeteriaDashboard />
    </div>
  );
};

export default Dashboard;
