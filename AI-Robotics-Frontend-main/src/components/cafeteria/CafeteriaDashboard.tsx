import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
//import { cafeteriaAPI } from "../../lib/api";
import { DollarSign, Users, Filter as AlertTriangle, Calendar as TrendingUp } from "lucide-react";

const CafeteriaDashboard: React.FC = () => {
  const [stats, setStats] = useState({
    todayRevenue: 0,
    todayOrders: 0,
    lowStockItems: 0,
    pendingOrders: 0,
  });

  useEffect(() => {
    fetchQuickStats();
  }, []);

  const fetchQuickStats = async () => {
    try {
      // Mock data for demo
      setStats({
        todayRevenue: 1250,
        todayOrders: 18,
        lowStockItems: 3,
        pendingOrders: 2,
      });
    } catch (error) {
      console.error("Error fetching cafeteria stats:", error);
    }
  };

  const quickStats = [
    {
      title: "إيرادات اليوم",
      value: `${stats.todayRevenue} جنيه`,
      icon: DollarSign,
      color: "text-green-600",
      bgColor: "bg-green-50",
      change: "+12%",
    },
    {
      title: "طلبات اليوم",
      value: stats.todayOrders,
      icon: Users,
      color: "text-blue-600",
      bgColor: "bg-blue-50",
      change: "+8%",
    },
    {
      title: "مخزون منخفض",
      value: stats.lowStockItems,
      icon: AlertTriangle,
      color: "text-red-600",
      bgColor: "bg-red-50",
      change: "",
    },
    {
      title: "طلبات معلقة",
      value: stats.pendingOrders,
      icon: TrendingUp,
      color: "text-orange-600",
      bgColor: "bg-orange-50",
      change: "",
    },
  ];

  return (
    <div className="bg-white rounded-xl shadow-sm p-6 border">
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-lg font-semibold text-gray-900">
          ملخص الكافيتيريا
        </h3>
        <Link
          to="/cafeteria"
          className="text-blue-600 hover:text-blue-800 text-sm font-medium"
        >
          عرض التفاصيل
        </Link>
      </div>

      <div className="grid grid-cols-2 gap-4">
        {quickStats.map((stat, index) => (
          <div key={index} className="flex items-center gap-3">
            <div className={`p-2 rounded-lg ${stat.bgColor}`}>
              <stat.icon className={`w-4 h-4 ${stat.color}`} />
            </div>
            <div className="flex-1">
              <p className="text-xs text-gray-600">{stat.title}</p>
              <p className="text-sm font-semibold text-gray-900">
                {stat.value}
              </p>
              {stat.change && (
                <p className="text-xs text-green-600">{stat.change}</p>
              )}
            </div>
          </div>
        ))}
      </div>

      <div className="mt-4 pt-4 border-t border-gray-200">
        <div className="flex gap-2">
          <Link
            to="/cafeteria/point-of-sale"
            className="flex-1 bg-green-600 text-white text-xs py-2 px-3 rounded-lg hover:bg-green-700 transition-colors text-center"
          >
            نقطة البيع
          </Link>
          <Link
            to="/cafeteria/orders"
            className="flex-1 bg-orange-600 text-white text-xs py-2 px-3 rounded-lg hover:bg-orange-700 transition-colors text-center"
          >
            الطلبات
          </Link>
        </div>
      </div>
    </div>
  );
};

export default CafeteriaDashboard; 