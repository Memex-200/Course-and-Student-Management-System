import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { UserRole } from '../types';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: UserRole[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ 
  children, 
  requiredRoles 
}) => {
  const { isAuthenticated, user, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRoles && user?.role) {
    // Debug role checking
    console.log('Required roles:', requiredRoles);
    console.log('User role:', user.role, typeof user.role);
    console.log('Includes check:', requiredRoles.includes(user.role));
    
    if (!requiredRoles.includes(user.role)) {
      return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50 rtl">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-4">غير مصرح لك بالوصول</h1>
            <p className="text-gray-600">ليس لديك صلاحية للوصول لهذه الصفحة</p>
            <p className="text-sm text-gray-500 mt-2">الدور المطلوب: {requiredRoles.join(' أو ')}</p>
            <p className="text-sm text-gray-500">دورك الحالي: {user.role}</p>
          </div>
        </div>
      );
    }
  }

  return <>{children}</>;
};

export default ProtectedRoute;