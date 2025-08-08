import React from "react";
import { useAuth } from "../../contexts/AuthContext";
import {
  Users,
  BookText,
  Calendar,
  Coffee,
  DollarSign,
  Activity,
  Clock,
  CheckSquare,
} from "lucide-react";

const EmployeeDashboard: React.FC = () => {
  const { user } = useAuth();

  const stats = [
    {
      title: "الطلاب النشطين",
      value: "120",
      icon: Users,
      change: "+12%",
      color: "text-primary-500",
    },
    {
      title: "الدورات الحالية",
      value: "15",
      icon: BookText,
      change: "+3",
      color: "text-accent-500",
    },
    {
      title: "الحجوزات اليوم",
      value: "8",
      icon: Calendar,
      change: "+2",
      color: "text-electric-500",
    },
    {
      title: "طلبات الكافيتريا",
      value: "25",
      icon: Coffee,
      change: "+5",
      color: "text-warning-500",
    },
  ];

  const quickActions = [
    {
      title: "تسجيل حضور طالب",
      icon: CheckSquare,
      color: "bg-primary-500/10 text-primary-500",
      onClick: () => console.log("تسجيل حضور"),
    },
    {
      title: "إضافة حجز جديد",
      icon: Calendar,
      color: "bg-accent-500/10 text-accent-500",
      onClick: () => console.log("إضافة حجز"),
    },
    {
      title: "طلب كافيتريا",
      icon: Coffee,
      color: "bg-electric-500/10 text-electric-500",
      onClick: () => console.log("طلب كافيتريا"),
    },
    {
      title: "تسجيل مصروفات",
      icon: DollarSign,
      color: "bg-warning-500/10 text-warning-500",
      onClick: () => console.log("تسجيل مصروفات"),
    },
  ];

  return (
    <main className="flex-1 p-6 bg-secondary-950 min-h-screen">
      <div className="max-w-7xl mx-auto">
        {/* Welcome Section */}
        <div className="bg-gradient-to-r from-primary-500/10 to-accent-500/10 rounded-2xl p-6 mb-6 backdrop-blur-xl border border-primary-500/20">
          <h1 className="text-2xl font-bold text-white mb-2">
            مرحباً بك، {user?.fullName}
          </h1>
          <p className="text-gray-400">
            لوحة تحكم الموظفين - شركة الذكاء الاصطناعي والروبوتات
          </p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
          {stats.map((stat, index) => (
            <div
              key={index}
              className="bg-secondary-900/50 rounded-2xl p-6 backdrop-blur-xl border border-secondary-800/50"
            >
              <div className="flex items-center justify-between mb-4">
                <stat.icon className={`w-8 h-8 ${stat.color}`} />
                <span className="text-green-500 text-sm">{stat.change}</span>
              </div>
              <h3 className="text-gray-400 text-sm mb-2">{stat.title}</h3>
              <p className="text-2xl font-bold text-white">{stat.value}</p>
            </div>
          ))}
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {quickActions.map((action, index) => (
            <button
              key={index}
              onClick={action.onClick}
              className="bg-secondary-900/50 rounded-2xl p-6 backdrop-blur-xl border border-secondary-800/50 hover:bg-secondary-800/50 transition-all duration-200"
            >
              <div
                className={`w-12 h-12 rounded-xl ${action.color} flex items-center justify-center mb-4`}
              >
                <action.icon className="w-6 h-6" />
              </div>
              <h3 className="text-white text-lg font-semibold">
                {action.title}
              </h3>
            </button>
          ))}
        </div>
      </div>
    </main>
  );
};

export default EmployeeDashboard;
