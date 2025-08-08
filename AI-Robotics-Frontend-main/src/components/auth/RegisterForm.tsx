import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { authAPI } from "../../lib/api";
import {
  Eye,
  EyeOff,
  UserPlus,
  Loader2,
  Brain,
  Bot,
  Zap,
  CircuitBoard,
  Sparkles,
  User,
  Mail,
  Phone,
  Building2,
} from "lucide-react";
import toast from "react-hot-toast";
import { RegisterData, UserRole, AuthResponse } from "../../types";

const RegisterForm: React.FC = () => {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900">
      <div className="bg-white rounded-xl shadow-lg p-8 max-w-md w-full text-center">
        <h2 className="text-2xl font-bold mb-4 text-gray-900">التسجيل مغلق</h2>
        <p className="text-gray-700">
          التسجيل متاح فقط عن طريق الإدارة. يرجى التواصل مع الشركة لإضافة حسابك.
        </p>
      </div>
    </div>
  );
};

export default RegisterForm;
