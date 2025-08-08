import React from "react";
import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import {
  LayoutDashboard,
  Users,
  GraduationCap,
  Settings,
  LogOut,
  Award,
  Bot,
  Brain,
  Wallet,
  Coffee,
  User,
  DollarSign,
} from "lucide-react";
import { UserRole } from "../../types";

const Sidebar: React.FC = () => {
  const location = useLocation();
  const { user, logout } = useAuth();

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  const adminLinks = [
    { path: "/dashboard", icon: LayoutDashboard, label: "لوحة التحكم" },
    { path: "/students", icon: Users, label: "الطلاب" },
    { path: "/courses", icon: GraduationCap, label: "الكورسات" },
    { path: "/attendance", icon: User, label: "الحضور" },
    { path: "/expenses", icon: Wallet, label: "المصروفات" },
    { path: "/payments", icon: DollarSign, label: "إدارة المدفوعات" },
    { path: "/cafeteria", icon: Coffee, label: "الكافيتريا" },
    { path: "/settings", icon: Settings, label: "الإعدادات" },
  ];

  const studentLinks = [
    { path: "/student", icon: LayoutDashboard, label: "الرئيسية" },
    { path: "/student/account", icon: User, label: "حسابي" },
    { path: "/student/courses", icon: Bot, label: "كورساتي" },
    {
      path: "/student/available-courses",
      icon: Brain,
      label: "الكورسات المتاحة",
    },
    { path: "/student/certificates", icon: Award, label: "الشهادات" },
  ];

  const links = user?.role === UserRole.Student ? studentLinks : adminLinks;

  return (
    <aside className="bg-gradient-to-b from-secondary-900 to-primary-900 text-white w-64 h-screen overflow-y-auto">
      <div className="flex flex-col h-full">
        {/* Logo */}
        <div className="mb-8 pt-4 px-4">
          <Link to="/" className="flex items-center justify-center">
            <Bot className="w-8 h-8 text-primary-500" />
            <span className="text-xl font-bold mr-2">شركة الروبوتات</span>
          </Link>
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-2">
          <ul className="space-y-2">
            {links.map((link) => (
              <li key={link.path}>
                <Link
                  to={link.path}
                  className={`flex items-center space-x-3 rtl:space-x-reverse px-4 py-2.5 rounded-xl transition-all duration-200 ${
                    isActive(link.path)
                      ? "bg-primary-500 text-white shadow-lg shadow-primary-500/20"
                      : "text-gray-400 hover:bg-primary-500/10 hover:text-white"
                  }`}
                >
                  <link.icon className="w-5 h-5" />
                  <span>{link.label}</span>
                </Link>
              </li>
            ))}
          </ul>
        </nav>

        {/* Logout */}
        <div className="p-4">
          <button
            onClick={logout}
            className="flex items-center space-x-3 rtl:space-x-reverse px-4 py-2.5 text-gray-400 hover:bg-primary-500/10 hover:text-white rounded-xl transition-all duration-200 w-full"
          >
            <LogOut className="w-5 h-5" />
            <span>تسجيل الخروج</span>
          </button>
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;
