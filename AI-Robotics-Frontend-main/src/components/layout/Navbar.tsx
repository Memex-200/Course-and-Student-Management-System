import React, { useState } from "react";
import { LogOut, User, Bell, Settings, Zap, Bot, Brain } from "lucide-react";
import { useAuth } from "../../contexts/AuthContext";

const Navbar: React.FC = () => {
  const { user, logout } = useAuth();
  const [notifications] = useState(3); // Mock notification count

  return (
    <nav className="bg-gradient-to-r from-secondary-900/95 to-secondary-800/95 backdrop-blur-lg shadow-xl border-b border-primary-500/20 relative">
      {/* Animated background elements */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute top-2 left-20 w-8 h-8 bg-primary-500 rounded-full animate-float"></div>
        <div
          className="absolute top-4 right-32 w-6 h-6 bg-accent-500 rounded-full animate-float"
          style={{ animationDelay: "1s" }}
        ></div>
        <div
          className="absolute bottom-2 left-1/2 w-4 h-4 bg-electric-500 rounded-full animate-float"
          style={{ animationDelay: "2s" }}
        ></div>
      </div>

      <div className="relative px-6 py-4">
        <div className="flex items-center justify-between relative">
          {/* Left side - Title and branch */}
          <div className="flex items-center space-x-4 rtl:space-x-reverse">
            <div className="flex items-center space-x-3 rtl:space-x-reverse">
              <div className="relative">
                <div className="w-10 h-10 bg-gradient-to-br from-primary-500 to-accent-500 rounded-xl flex items-center justify-center animate-pulse-glow">
                  <Brain className="w-6 h-6 text-white animate-float" />
                </div>
                <div className="absolute -top-1 -right-1 w-3 h-3 bg-electric-400 rounded-full animate-bounce-gentle"></div>
              </div>
              <div>
                <h1 className="text-xl font-bold bg-gradient-to-r from-primary-300 to-accent-400 bg-clip-text text-transparent">
                  شركة الذكاء الاصطناعي والروبوتات
                </h1>
                <div className="flex items-center space-x-2 rtl:space-x-reverse">
                  <span className="text-sm text-primary-300 bg-gradient-to-r from-primary-900/50 to-accent-900/50 px-3 py-1 rounded-full border border-primary-500/30 backdrop-blur-sm">
                    {user?.branch}
                  </span>
                  <Bot className="w-4 h-4 text-accent-400 animate-robot-walk" />
                </div>
              </div>
            </div>
          </div>

          {/* Right side - User actions */}
          <div className="flex items-center space-x-4 rtl:space-x-reverse">
            {/* Notifications */}
            <button className="relative p-3 text-gray-400 hover:text-primary-300 hover:bg-primary-900/20 rounded-xl transition-all duration-300 group">
              <Bell className="w-5 h-5 group-hover:animate-wiggle" />
              {notifications > 0 && (
                <span className="absolute -top-1 -right-1 w-5 h-5 bg-gradient-to-r from-error-500 to-error-600 text-white text-xs rounded-full flex items-center justify-center animate-bounce-gentle">
                  {notifications}
                </span>
              )}
              <div className="absolute inset-0 bg-gradient-to-r from-primary-500/10 to-accent-500/10 opacity-0 group-hover:opacity-100 transition-opacity rounded-xl"></div>
            </button>

            {/* Settings */}
            <button className="relative p-3 text-gray-400 hover:text-accent-300 hover:bg-accent-900/20 rounded-xl transition-all duration-300 group">
              <Settings className="w-5 h-5 group-hover:animate-rotate-slow" />
              <div className="absolute inset-0 bg-gradient-to-r from-accent-500/10 to-electric-500/10 opacity-0 group-hover:opacity-100 transition-opacity rounded-xl"></div>
            </button>

            {/* AI Assistant indicator */}
            <div className="flex items-center space-x-2 rtl:space-x-reverse px-3 py-2 bg-gradient-to-r from-electric-900/30 to-electric-800/30 rounded-xl border border-electric-500/20">
              <Zap className="w-4 h-4 text-electric-400 animate-bounce-gentle" />
              <span className="text-xs text-electric-300 tech-font">
                AI مساعد
              </span>
            </div>

            {/* User profile */}
            <div className="flex items-center space-x-3 rtl:space-x-reverse border-r border-primary-500/20 pr-4 rtl:pr-0 rtl:pl-4 rtl:border-l rtl:border-r-0">
              <div className="text-right rtl:text-left">
                <p className="text-sm font-medium text-white">
                  {user?.fullName}
                </p>
                <div className="flex items-center space-x-2 rtl:space-x-reverse">
                  <p className="text-xs text-gray-400">{user?.userRole}</p>
                  <div className="w-2 h-2 bg-electric-400 rounded-full animate-pulse-glow"></div>
                </div>
              </div>
              <div className="relative">
                <div className="w-10 h-10 bg-gradient-to-br from-primary-500 to-accent-500 rounded-xl flex items-center justify-center glow-blue">
                  <User className="w-5 h-5 text-white" />
                </div>
                <div className="absolute -bottom-1 -right-1 w-4 h-4 bg-electric-400 rounded-full border-2 border-secondary-900 animate-bounce-gentle"></div>
              </div>
            </div>

            {/* Logout */}
            <button
              onClick={logout}
              className="group flex items-center space-x-2 rtl:space-x-reverse text-gray-400 hover:text-error-400 transition-all duration-300 px-3 py-2 rounded-xl hover:bg-error-900/20"
            >
              <LogOut className="w-5 h-5 group-hover:animate-slide-left" />
              <span className="text-sm font-medium">تسجيل الخروج</span>
              <div className="absolute inset-0 bg-gradient-to-r from-error-500/10 to-error-600/10 opacity-0 group-hover:opacity-100 transition-opacity rounded-xl"></div>
            </button>
          </div>
        </div>
      </div>

      {/* Animated border */}
      <div className="absolute bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-primary-500 to-transparent animate-pulse"></div>
    </nav>
  );
};

export default Navbar;
