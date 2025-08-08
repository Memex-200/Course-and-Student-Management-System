import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, UserRole } from '../types';

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (user: User, token: string) => void;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
  setUser: (user: User) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const savedToken = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');
    
    if (savedToken && savedUser) {
      try {
        const parsedUser = JSON.parse(savedUser);
        // تطبيع الدور: إذا كان نص (Admin/Employee/Student) حوله إلى رقم
        let roleValue = parsedUser.role;
        if (typeof roleValue === 'string' && isNaN(Number(roleValue))) {
          if (roleValue === 'Admin') roleValue = 1;
          else if (roleValue === 'Employee') roleValue = 2;
          else if (roleValue === 'Student') roleValue = 3;
        } else {
          roleValue = parseInt(roleValue, 10);
        }
        const normalizedUser = {
          ...parsedUser,
          role: roleValue
        };
        // Debug stored user data
        console.log('Stored user data:', parsedUser);
        console.log('Normalized stored user:', normalizedUser);
        setToken(savedToken);
        setUser(normalizedUser);
      } catch (error) {
        console.error('Error parsing saved user data:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    }
    
    setIsLoading(false);
  }, []);

  const login = (userData: User, authToken: string) => {
    // Parse JWT token to get role
    const tokenParts = authToken.split('.');
    let tokenRole: number | undefined;
    
    try {
        const tokenPayload = JSON.parse(atob(tokenParts[1]));
        console.log('Token payload:', tokenPayload);
        
        // Try to get role from different possible claim types
        const roleValue = tokenPayload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 
                         tokenPayload.role;
        
        if (roleValue) {
            if (typeof roleValue === 'string' && isNaN(Number(roleValue))) {
              if (roleValue === 'Admin') tokenRole = 1;
              else if (roleValue === 'Employee') tokenRole = 2;
              else if (roleValue === 'Student') tokenRole = 3;
            } else {
              tokenRole = parseInt(roleValue, 10);
            }
            console.log('Role from token:', tokenRole);
        }
    } catch (error) {
        console.error('Error parsing token:', error);
    }

    // Ensure role is a number, prioritizing token role over user data
    const normalizedUser = {
        ...userData,
        role: tokenRole ?? (
            typeof userData.userRole === 'number' ? userData.userRole :
            typeof userData.userRole === 'string' ? parseInt(userData.userRole, 10) :
            typeof userData.role === 'number' ? userData.role :
            typeof userData.role === 'string' ? parseInt(userData.role, 10) :
            UserRole.Student // Default to Student if no valid role found
        )
    };
    
    // Debug role normalization
    console.log('Original user data:', userData);
    console.log('Token role:', tokenRole);
    console.log('Normalized user:', normalizedUser);
    console.log('Role type:', typeof normalizedUser.role);
    
    setUser(normalizedUser);
    setToken(authToken);
    localStorage.setItem('token', authToken);
    localStorage.setItem('user', JSON.stringify(normalizedUser));
};

  const logout = () => {
    setUser(null);
    setToken(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  const isAuthenticated = !!user && !!token;

  const value: AuthContextType = {
    user,
    token,
    login,
    logout,
    isAuthenticated,
    isLoading,
    setUser,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};