import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { authAPI } from "../../lib/api";
import {
  Eye,
  EyeOff,
  LogIn,
  Loader2,
  Brain,
  Bot,
  Zap,
  CircuitBoard,
  Sparkles,
} from "lucide-react";
import toast from "react-hot-toast";
import { UserRole, AuthResponse } from "../../types";

const LoginForm: React.FC = () => {
  const [formData, setFormData] = useState({
    username: "",
    password: "",
  });
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);

    try {
      const response = await authAPI.login(formData);

      // Debug logging
      console.log("Login response:", response.data);

      // Check if response.data has the expected structure
      if (!response.data || typeof response.data !== "object") {
        throw new Error("Invalid response format");
      }

      // Extract user and token from response data
      const userData = response.data.user;
      const token = response.data.token;

      console.log("User data:", userData);
      console.log("Token:", token);

      if (!userData || !token) {
        throw new Error("Missing user data or token");
      }

      // Transform userRole to role if needed
      const normalizedUserData = {
        ...userData,
        role:
          typeof userData.userRole === "string"
            ? parseInt(userData.userRole, 10)
            : typeof userData.userRole === "number"
            ? userData.userRole
            : typeof userData.role === "string"
            ? parseInt(userData.role, 10)
            : userData.role,
      };

      // Validate user data has required fields
      if (
        !normalizedUserData.id ||
        !normalizedUserData.username ||
        !normalizedUserData.role
      ) {
        throw new Error("Invalid user data structure");
      }

      // Debug the normalized role
      console.log(
        "Normalized role:",
        normalizedUserData.role,
        typeof normalizedUserData.role
      );

      login(normalizedUserData, token);
      toast.success(`مرحباً بك ${normalizedUserData.fullName}`);

      // Redirect based on user role
      switch (normalizedUserData.role) {
        case UserRole.Student:
          console.log("Redirecting to student page...");
          navigate("/student");
          break;
        case UserRole.Employee:
          console.log("Redirecting to employee dashboard...");
          navigate("/employee/dashboard");
          break;
        case UserRole.Admin:
          console.log("Redirecting to admin dashboard...");
          navigate("/dashboard");
          break;
        default:
          console.log("Unknown role, redirecting to dashboard...");
          navigate("/dashboard");
      }
    } catch (error: any) {
      console.error("Login error:", error);
      // Show backend error message if available
      const backendMsg = error.response?.data?.message;
      if (
        error.message === "Invalid response format" ||
        error.message === "Missing user data or token" ||
        error.message === "Invalid user data structure"
      ) {
        toast.error("خطأ في الاستجابة من الخادم");
      } else {
        toast.error(backendMsg || error.message || "خطأ في تسجيل الدخول");
      }
    } finally {
      setIsLoading(false);
    }
  };

  // Floating particles component
  const FloatingParticles = () => (
    <div className="absolute inset-0 overflow-hidden pointer-events-none">
      {[...Array(15)].map((_, i) => (
        <div
          key={i}
          className="particle"
          style={{
            left: `${Math.random() * 100}%`,
            top: `${Math.random() * 100}%`,
            animationDelay: `${Math.random() * 6}s`,
            animationDuration: `${4 + Math.random() * 4}s`,
          }}
        />
      ))}
    </div>
  );

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 rtl relative overflow-hidden">
      <FloatingParticles />

      {/* Animated background elements */}
      <div className="absolute inset-0 opacity-20">
        <div className="absolute top-20 left-20 w-32 h-32 bg-primary-500 rounded-full animate-float"></div>
        <div
          className="absolute top-40 right-32 w-24 h-24 bg-accent-500 rounded-full animate-float"
          style={{ animationDelay: "1s" }}
        ></div>
        <div
          className="absolute bottom-32 left-1/3 w-20 h-20 bg-electric-500 rounded-full animate-float"
          style={{ animationDelay: "2s" }}
        ></div>
        <div
          className="absolute bottom-20 right-20 w-16 h-16 bg-warning-500 rounded-full animate-float"
          style={{ animationDelay: "3s" }}
        ></div>
      </div>

      {/* Circuit pattern overlay */}
      <div className="absolute inset-0 opacity-10">
        <svg className="w-full h-full" viewBox="0 0 100 100">
          <defs>
            <pattern
              id="circuit-login"
              x="0"
              y="0"
              width="20"
              height="20"
              patternUnits="userSpaceOnUse"
            >
              <circle cx="10" cy="10" r="1" fill="#0ea5e9" />
              <path
                d="M10 0v20M0 10h20"
                stroke="#0ea5e9"
                strokeWidth="0.5"
                className="circuit-line"
              />
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#circuit-login)" />
        </svg>
      </div>

      <div className="max-w-md w-full mx-4 relative z-10">
        <div className="bg-gradient-to-br from-secondary-800/80 to-secondary-900/80 backdrop-blur-xl rounded-3xl shadow-2xl p-8 border border-primary-500/20 glow-blue">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="relative mb-6">
              <div className="w-20 h-20 bg-gradient-to-br from-primary-500 to-accent-500 rounded-2xl flex items-center justify-center mx-auto animate-pulse-glow glow-blue">
                <Brain className="w-10 h-10 text-white animate-float" />
              </div>
              <div className="absolute -top-2 -right-2 w-6 h-6 bg-electric-400 rounded-full animate-bounce-gentle"></div>
              <div
                className="absolute -bottom-2 -left-2 w-4 h-4 bg-accent-400 rounded-full animate-bounce-gentle"
                style={{ animationDelay: "0.5s" }}
              ></div>
            </div>

            <h2 className="text-3xl font-bold bg-gradient-to-r from-primary-300 to-accent-400 bg-clip-text text-transparent mb-2">
              تسجيل الدخول
            </h2>
            <p className="text-gray-400 mb-4">
              شركة الذكاء الاصطناعي والروبوتات
            </p>

            {/* Tech indicators */}
            <div className="flex items-center justify-center space-x-4 rtl:space-x-reverse">
              {[Bot, Zap, CircuitBoard].map((Icon, index) => (
                <div
                  key={index}
                  className="w-6 h-6 text-primary-400 animate-float"
                  style={{ animationDelay: `${index * 0.5}s` }}
                >
                  <Icon className="w-full h-full" />
                </div>
              ))}
            </div>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Username field */}
            <div className="relative">
              <label
                htmlFor="username"
                className="block text-sm font-medium text-gray-300 mb-2 flex items-center"
              >
                <Sparkles className="w-4 h-4 ml-2 text-primary-400 animate-wiggle" />
                اسم المستخدم
              </label>
              <div className="relative">
                <input
                  id="username"
                  name="username"
                  type="text"
                  required
                  value={formData.username}
                  onChange={handleChange}
                  className="w-full px-4 py-4 bg-secondary-700/50 border border-primary-500/30 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300 text-white placeholder-gray-400 backdrop-blur-sm"
                  placeholder="أدخل اسم المستخدم"
                  dir="ltr"
                />
                <div className="absolute inset-0 bg-gradient-to-r from-primary-500/10 to-accent-500/10 opacity-0 focus-within:opacity-100 transition-opacity rounded-xl pointer-events-none"></div>
              </div>
            </div>

            {/* Password field */}
            <div className="relative">
              <label
                htmlFor="password"
                className="block text-sm font-medium text-gray-300 mb-2 flex items-center"
              >
                <Sparkles className="w-4 h-4 ml-2 text-accent-400 animate-wiggle" />
                كلمة المرور
              </label>
              <div className="relative">
                <input
                  id="password"
                  name="password"
                  type={showPassword ? "text" : "password"}
                  required
                  value={formData.password}
                  onChange={handleChange}
                  className="w-full px-4 py-4 bg-secondary-700/50 border border-primary-500/30 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300 text-white placeholder-gray-400 pr-12 backdrop-blur-sm"
                  placeholder="أدخل كلمة المرور"
                  dir="ltr"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-primary-300 transition-colors p-1 rounded-lg hover:bg-primary-900/20"
                >
                  {showPassword ? (
                    <EyeOff className="w-5 h-5" />
                  ) : (
                    <Eye className="w-5 h-5" />
                  )}
                </button>
                <div className="absolute inset-0 bg-gradient-to-r from-primary-500/10 to-accent-500/10 opacity-0 focus-within:opacity-100 transition-opacity rounded-xl pointer-events-none"></div>
              </div>
            </div>

            {/* Submit button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-gradient-to-r from-primary-500 via-accent-500 to-electric-500 text-white py-4 px-4 rounded-xl hover:from-primary-400 hover:via-accent-400 hover:to-electric-400 focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 focus:ring-offset-secondary-900 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center space-x-2 rtl:space-x-reverse font-medium text-lg glow-blue hover:glow-purple transform hover:scale-105"
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-5 h-5 animate-spin" />
                  <span>جاري تسجيل الدخول</span>
                  <div className="loading-dots"></div>
                </>
              ) : (
                <>
                  <LogIn className="w-5 h-5" />
                  <span>تسجيل الدخول</span>
                  <Zap className="w-4 h-4 animate-bounce-gentle" />
                </>
              )}
            </button>
          </form>

          {/* Links */}
          <div className="mt-6 text-center">
            <p className="text-gray-400">
              ليس لديك حساب؟{" "}
              <Link
                to="/register"
                className="text-primary-400 hover:text-accent-400 font-medium transition-colors"
              >
                إنشاء حساب جديد
              </Link>
            </p>
          </div>

          <div className="mt-6 pt-6 border-t border-primary-500/20 text-center">
            <Link
              to="/"
              className="text-sm text-gray-400 hover:text-primary-300 transition-colors flex items-center justify-center space-x-2 rtl:space-x-reverse"
            >
              <Bot className="w-4 h-4 animate-robot-walk" />
              <span>العودة للموقع الرئيسي</span>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginForm;
