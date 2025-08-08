import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
//import { useAuth } from "../../contexts/AuthContext";
import { GraduationCap, Plus, Calendar, DollarSign, Filter } from "lucide-react";
import { cafeteriaAPI } from "../../lib/api";

interface RecentOrder {
  id: number;
  orderNumber: string;
  orderDate: string;
  status: string;
  statusArabic: string;
  totalAmount: number;
  createdByUser: {
    id: number;
    fullName: string;
  };
}

const CafeteriaManagement: React.FC = () => {
  //const { user } = useAuth();
  const [stats, setStats] = useState({
    totalItems: 0,
    lowStockItems: 0,
    todayOrders: 0,
    todayRevenue: 0,
    pendingOrders: 0,
  });
  const [recentOrders, setRecentOrders] = useState<RecentOrder[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchStats();
    fetchRecentOrders();
  }, []);

  const fetchStats = async () => {
    try {
      const response = await cafeteriaAPI.getQuickStats();
      setStats(response.data);
    } catch (error) {
      console.error("Error fetching cafeteria quick stats:", error);
    }
  };

  const fetchRecentOrders = async () => {
    try {
      setLoading(true);
      const response = await cafeteriaAPI.getOrders();
      setRecentOrders(response.data.slice(0, 5)); // Get only the first 5 orders
    } catch (error) {
      console.error("Error fetching recent orders:", error);
    } finally {
      setLoading(false);
    }
  };

  const formatTimeAgo = (dateString: string) => {
    const orderDate = new Date(dateString);
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - orderDate.getTime()) / (1000 * 60));
    
    if (diffInMinutes < 1) return "الآن";
    if (diffInMinutes < 60) return `منذ ${diffInMinutes} دقيقة`;
    
    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) return `منذ ${diffInHours} ساعة`;
    
    const diffInDays = Math.floor(diffInHours / 24);
    return `منذ ${diffInDays} يوم`;
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Pending":
        return "bg-yellow-100 text-yellow-800";
      case "Preparing":
        return "bg-blue-100 text-blue-800";
      case "Ready":
        return "bg-green-100 text-green-800";
      case "Delivered":
        return "bg-purple-100 text-purple-800";
      case "Cancelled":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const quickActions = [
    {
      title: "إدارة المنتجات",
      description: "إضافة وتعديل منتجات الكافيتيريا",
      icon: GraduationCap,
      path: "/cafeteria/products",
      color: "bg-blue-500",
    },
    {
      title: "نقطة البيع",
      description: "إنشاء طلبات جديدة للعملاء",
      icon: Plus,
      path: "/cafeteria/point-of-sale",
      color: "bg-green-500",
    },
    {
      title: "إدارة الطلبات",
      description: "عرض ومتابعة جميع الطلبات",
      icon: Calendar,
      path: "/cafeteria/orders",
      color: "bg-orange-500",
    },
    {
      title: "التقارير",
      description: "تقارير المبيعات والأرباح",
      icon: DollarSign,
      path: "/cafeteria/reports",
      color: "bg-purple-500",
    },
  ];

  const statsCards = [
    {
      title: "إجمالي المنتجات",
      value: stats.totalItems,
      icon: GraduationCap,
      color: "text-blue-600",
      bgColor: "bg-blue-50",
    },
    {
      title: "منتجات قاربت النفاد",
      value: stats.lowStockItems,
      icon: Filter,
      color: "text-red-600",
      bgColor: "bg-red-50",
    },
    {
      title: "طلبات اليوم",
      value: stats.todayOrders,
      icon: Plus,
      color: "text-green-600",
      bgColor: "bg-green-50",
    },
    {
      title: "إيرادات اليوم",
      value: `${stats.todayRevenue} جنيه`,
      icon: DollarSign,
      color: "text-purple-600",
      bgColor: "bg-purple-50",
    },
    {
      title: "طلبات في الانتظار",
      value: stats.pendingOrders,
      icon: Calendar,
      color: "text-orange-600",
      bgColor: "bg-orange-50",
    },
  ];

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
            <GraduationCap className="w-8 h-8 text-orange-500" />
            إدارة الكافيتيريا
          </h1>
          <p className="text-gray-600 mt-2">
            إدارة شاملة للمنتجات والطلبات والمبيعات
          </p>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6 mb-8">
        {statsCards.map((stat, index) => (
          <div
            key={index}
            className="bg-white rounded-xl shadow-sm p-6 border hover:shadow-md transition-shadow"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">{stat.title}</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">
                  {stat.value}
                </p>
              </div>
              <div className={`p-3 rounded-lg ${stat.bgColor}`}>
                <stat.icon className={`w-6 h-6 ${stat.color}`} />
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {quickActions.map((action, index) => (
          <Link
            key={index}
            to={action.path}
            className="block bg-white rounded-xl shadow-sm p-6 border hover:shadow-lg transition-all duration-300 hover:-translate-y-1"
          >
            <div className="flex items-center mb-4">
              <div className={`p-3 rounded-lg ${action.color} text-white`}>
                <action.icon className="w-6 h-6" />
              </div>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              {action.title}
            </h3>
            <p className="text-gray-600 text-sm">{action.description}</p>
          </Link>
        ))}
      </div>

      {/* Recent Activity */}
      <div className="mt-8 bg-white rounded-xl shadow-sm p-6 border">
        <h2 className="text-xl font-semibold text-gray-900 mb-6 flex items-center gap-2">
          <Calendar className="w-5 h-5 text-blue-600" />
          النشاط الأخير
        </h2>
        <div className="space-y-4">
          {loading ? (
            <div className="flex items-center justify-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <span className="mr-3 text-gray-600">جاري التحميل...</span>
            </div>
          ) : recentOrders.length > 0 ? (
            recentOrders.map((order) => (
              <div key={order.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div className="flex items-center gap-3">
                  <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                  <div>
                    <p className="text-sm font-medium text-gray-900">
                      طلب جديد #{order.orderNumber}
                    </p>
                    <p className="text-xs text-gray-500">
                      بواسطة {order.createdByUser?.fullName || "غير محدد"} • {formatTimeAgo(order.orderDate)}
                    </p>
                  </div>
                </div>
                <div className="text-left">
                  <span className={`inline-block px-2 py-1 text-xs rounded-full ${getStatusColor(order.status)}`}>
                    {order.statusArabic}
                  </span>
                  <p className="text-sm font-medium text-gray-900 mt-1">{order.totalAmount} جنيه</p>
                </div>
              </div>
            ))
          ) : (
            <div className="text-center py-8 text-gray-500">
              لا توجد طلبات حديثة
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default CafeteriaManagement; 