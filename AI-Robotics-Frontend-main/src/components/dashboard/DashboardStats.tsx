import React from 'react';
import { Users, BookOpen, DollarSign, TrendingUp, Clock, AlertCircle } from 'lucide-react';
import { DashboardStats as Stats } from '../../types';

interface DashboardStatsProps {
  stats: Stats;
  isLoading?: boolean;
}

const DashboardStats: React.FC<DashboardStatsProps> = ({ stats, isLoading }) => {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        {[...Array(4)].map((_, index) => (
          <div key={index} className="bg-white rounded-xl shadow-sm p-6 animate-pulse">
            <div className="h-4 bg-gray-200 rounded w-1/2 mb-4"></div>
            <div className="h-8 bg-gray-200 rounded w-1/3 mb-2"></div>
            <div className="h-3 bg-gray-200 rounded w-3/4"></div>
          </div>
        ))}
      </div>
    );
  }

  const statCards = [
    {
      title: 'إجمالي الطلاب',
      value: stats.totalStudents,
      icon: Users,
      color: 'blue',
      bgColor: 'bg-blue-50',
      iconColor: 'text-blue-600',
      change: '+12% من الشهر الماضي',
    },
    {
      title: 'الكورسات النشطة',
      value: `${stats.activeCourses}/${stats.totalCourses}`,
      icon: BookOpen,
      color: 'green',
      bgColor: 'bg-green-50',
      iconColor: 'text-green-600',
      change: `${stats.totalCourses} كورس إجمالي`,
    },
    {
      title: 'الإيرادات الشهرية',
      value: `${stats.monthlyRevenue.toLocaleString()} ج.م`,
      icon: DollarSign,
      color: 'purple',
      bgColor: 'bg-purple-50',
      iconColor: 'text-purple-600',
      change: `إجمالي: ${stats.totalRevenue.toLocaleString()} ج.م`,
    },
    {
      title: 'الحجوزات النشطة',
      value: stats.activeWorkspaceBookings,
      icon: Clock,
      color: 'orange',
      bgColor: 'bg-orange-50',
      iconColor: 'text-orange-600',
      change: 'مساحات العمل',
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      {statCards.map((stat, index) => {
        const Icon = stat.icon;
        return (
          <div key={index} className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600 mb-1">{stat.title}</p>
                <p className="text-2xl font-bold text-gray-900 mb-1">{stat.value}</p>
                <p className="text-xs text-gray-500">{stat.change}</p>
              </div>
              <div className={`${stat.bgColor} p-3 rounded-lg`}>
                <Icon className={`w-6 h-6 ${stat.iconColor}`} />
              </div>
            </div>
          </div>
        );
      })}

      {/* Additional Stats for Admin */}
      {stats.pendingPayments > 0 && (
        <div className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow border-l-4 border-red-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">مدفوعات معلقة</p>
              <p className="text-2xl font-bold text-red-600 mb-1">{stats.pendingPayments}</p>
              <p className="text-xs text-red-500">تحتاج متابعة</p>
            </div>
            <div className="bg-red-50 p-3 rounded-lg">
              <AlertCircle className="w-6 h-6 text-red-600" />
            </div>
          </div>
        </div>
      )}

      {stats.upcomingClasses > 0 && (
        <div className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow border-l-4 border-blue-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">محاضرات قادمة</p>
              <p className="text-2xl font-bold text-blue-600 mb-1">{stats.upcomingClasses}</p>
              <p className="text-xs text-blue-500">هذا الأسبوع</p>
            </div>
            <div className="bg-blue-50 p-3 rounded-lg">
              <TrendingUp className="w-6 h-6 text-blue-600" />
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default DashboardStats;