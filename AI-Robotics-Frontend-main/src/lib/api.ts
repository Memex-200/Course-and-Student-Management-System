import axios from "axios";
import { AuthResponse, RegisterData } from "../types";
import { API_CONFIG } from "../config/api";

const API_BASE_URL = API_CONFIG.BASE_URL;

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: false, // Disable cookies to avoid CORS credential restrictions
});

// Request interceptor to add auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
    // Only log in development
    if (process.env.NODE_ENV === "development") {
      console.log("Request config:", {
        url: config.url,
        method: config.method,
        headers: config.headers,
        data: config.data,
      });
    }
  }
  return config;
});

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => {
    // Check if the response has data property
    if (!response.data) {
      throw new Error("Invalid response format: missing data");
    }

    // For auth endpoints except update, validate the response structure
    if (
      response.config.url?.includes("/auth/") &&
      !response.config.url.includes("/auth/update")
    ) {
      const { data } = response;
      if (!data.user || !data.token) {
        console.error("Invalid auth response:", data);
        throw new Error("Invalid response format: missing user or token");
      }
    }

    return response;
  },
  (error) => {
    // Log error details
    console.error("API Error:", {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      headers: error.response?.headers,
      config: {
        url: error.config?.url,
        method: error.config?.method,
        headers: error.config?.headers,
        data: error.config?.data,
      },
    });

    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export const authAPI = {
  login: async (credentials: { username: string; password: string }) => {
    try {
      const response = await api.post<AuthResponse>("/auth/login", credentials);
      // Additional validation
      if (!response.data.user || !response.data.token) {
        throw new Error("Invalid response format: missing user or token");
      }
      return response;
    } catch (error) {
      console.error("Login API error:", error);
      throw error;
    }
  },

  register: async (userData: RegisterData) => {
    try {
      const response = await api.post<AuthResponse>("/auth/register", userData);
      // Additional validation
      if (!response.data.user || !response.data.token) {
        throw new Error("Invalid response format: missing user or token");
      }
      return response;
    } catch (error) {
      console.error("Register API error:", error);
      throw error;
    }
  },

  updateUser: async (userData: {
    fullName: string;
    email?: string;
    phone?: string;
    address?: string;
  }) => {
    try {
      const response = await api.put<AuthResponse>("/auth/update", userData);
      if (!response.data.user || !response.data.token) {
        throw new Error("Invalid response format: missing user or token");
      }

      // Update localStorage with new token and user data
      localStorage.setItem("token", response.data.token);
      localStorage.setItem("user", JSON.stringify(response.data.user));

      return response;
    } catch (error) {
      console.error("Update user API error:", error);
      throw error;
    }
  },

  changePassword: async (passwordData: {
    currentPassword: string;
    newPassword: string;
  }) => {
    try {
      const response = await api.post("/auth/change-password", passwordData);
      return response;
    } catch (error) {
      console.error("Change password API error:", error);
      throw error;
    }
  },
};

export const studentsAPI = {
  getAll: (params?: { branchId?: number; search?: string }) =>
    api.get("/students", { params }),

  getById: (id: number) => api.get(`/students/${id}`),

  create: (data: any) => api.post("/students", data),

  update: (id: number, data: any) => api.put(`/students/${id}`, data),

  delete: (id: number) =>
    api.delete(`/students/${id}`, {
      headers: {
        "Content-Type": "application/json",
      },
    }),

  registerForCourse: (id: number, courseData: any) =>
    api.post(`/students/${id}/register-course`, courseData),

  getBranches: () => api.get("/courses/test-data"),

  // Get student's own dashboard (enrolled courses)
  getMyDashboard: () => {
    const sid = JSON.parse(localStorage.getItem("user") || "{}").studentId;
    return api.get("/students/my-dashboard", {
      params: sid ? { studentId: sid } : undefined,
    });
  },
};

export const coursesAPI = {
  getAll: (params?: {
    categoryId?: number;
    status?: string;
    branchId?: number;
  }) => api.get("/courses", { params }),

  getById: (id: number) => api.get(`/courses/${id}`),

  create: (data: any) => api.post("/courses", data),

  update: (id: number, data: any) => api.put(`/courses/${id}`, data),

  delete: (id: number) => api.delete(`/courses/${id}`),

  getCategories: () => api.get("/courses/categories"),

  // Get available courses for student registration
  getAvailable: () => api.get("/courses/available"),
};

export const workspaceAPI = {
  getBookings: (params?: any) => api.get("/workspace/bookings", { params }),

  createBooking: (data: any) => api.post("/workspace/book", data),

  endBooking: (id: number) => api.put(`/workspace/bookings/${id}/end`),

  addPayment: (id: number, paymentData: any) =>
    api.post(`/workspace/bookings/${id}/payment`, paymentData),
};

export const sharedWorkspaceAPI = {
  getSpaces: () => api.get("/SharedWorkspace/spaces"),

  getBookings: (params?: any) =>
    api.get("/SharedWorkspace/bookings", { params }),

  createBooking: (data: any) => api.post("/SharedWorkspace/book", data),

  checkIn: (bookingId: number) =>
    api.post(`/SharedWorkspace/checkin/${bookingId}`),

  checkOut: (bookingId: number) =>
    api.post(`/SharedWorkspace/checkout/${bookingId}`),

  getOccupancy: () => api.get("/SharedWorkspace/occupancy"),

  processPayment: (bookingId: number, paymentData: any) =>
    api.post(`/SharedWorkspace/payment/${bookingId}`, paymentData),

  getStatistics: () => api.get("/SharedWorkspace/statistics"),
};

export const paymentsAPI = {
  getCoursePayments: () => api.get("/payments/course-registrations"),

  getWorkspacePayments: () => api.get("/payments/workspace-bookings"),

  getSharedWorkspacePayments: () =>
    api.get("/payments/shared-workspace-bookings"),

  getCafeteriaPayments: () => api.get("/payments/cafeteria-orders"),

  processCoursePayment: (id: number, paymentData: any) =>
    api.post(`/payments/course-registrations/${id}/payment`, paymentData),

  processWorkspacePayment: (id: number, paymentData: any) =>
    api.post(`/payments/workspace-bookings/${id}/payment`, paymentData),

  getStatistics: () => api.get("/payments/statistics"),
};

export const cafeteriaAPI = {
  // Items Management
  getItems: (params?: { category?: string; isAvailable?: boolean }) =>
    api.get("/cafeteria/items", { params }),

  createItem: (data: {
    name: string;
    description: string;
    category: number;
    price: number;
    cost: number;
    stockQuantity: number;
    minimumStock: number;
    isAvailable: boolean;
  }) => api.post("/cafeteria/items", data),

  updateItem: (id: number, data: any) =>
    api.put(`/cafeteria/items/${id}`, data),

  deleteItem: (id: number) => api.delete(`/cafeteria/items/${id}`),

  // Orders Management
  getOrders: (params?: {
    status?: string;
    startDate?: string;
    endDate?: string;
  }) => api.get("/cafeteria/orders", { params }),

  getOrder: (id: number) => api.get(`/cafeteria/orders/${id}`),

  createOrder: (data: {
    studentId?: number;
    employeeId?: number;
    customerName?: string;
    customerPhone?: string;
    taxAmount?: number;
    discountAmount?: number;
    paidAmount: number;
    paymentMethod: number;
    notes?: string;
    items: Array<{
      cafeteriaItemId: number;
      quantity: number;
      notes?: string;
      customization?: string;
    }>;
  }) => api.post("/cafeteria/orders", data),

  updateOrder: (
    id: number,
    data: {
      customerName?: string;
      customerPhone?: string;
      discountAmount?: number;
      paidAmount?: number;
      paymentMethod?: number;
      notes?: string;
    }
  ) => api.put(`/cafeteria/orders/${id}`, data),

  updateOrderStatus: (id: number, data: { status: number; notes?: string }) =>
    api.put(`/cafeteria/orders/${id}/status`, data),

  deleteOrder: (id: number) => api.delete(`/cafeteria/orders/${id}`),

  // Statistics
  getStatistics: (params?: { startDate?: string; endDate?: string }) =>
    api.get("/cafeteria/statistics", { params }),

  // Quick Stats
  getQuickStats: () => api.get("/cafeteria/quick-stats"),

  // Payment Management
  processPayment: (
    orderId: number,
    data: { amount: number; paymentMethod: number; notes?: string }
  ) => api.post(`/cafeteria/orders/${orderId}/payment`, data),

  // Export Reports
  exportToExcel: (params?: { startDate?: string; endDate?: string }) =>
    api.get("/cafeteria/export/excel", {
      params,
      responseType: "blob",
    }),

  exportToPDF: (params?: { startDate?: string; endDate?: string }) =>
    api.get("/cafeteria/export/pdf", {
      params,
      responseType: "blob",
    }),
};

export const equipmentAPI = {
  getAll: () => api.get("/equipment"),

  getById: (id: number) => api.get(`/equipment/${id}`),

  create: (data: any) => api.post("/equipment", data),

  update: (id: number, data: any) => api.put(`/equipment/${id}`, data),

  reserve: (id: number, reservationData: any) =>
    api.post(`/equipment/${id}/reserve`, reservationData),

  returnEquipment: (reservationId: number) =>
    api.put(`/equipment/reservations/${reservationId}/return`),
};

export const expensesAPI = {
  getAll: (params?: any) => api.get("/expenses", { params }),

  getById: (id: number) => api.get(`/expenses/${id}`),

  create: (data: any) => api.post("/expenses", data),

  update: (id: number, data: any) => api.put(`/expenses/${id}`, data),

  approve: (id: number) => api.put(`/expenses/${id}/approve`),

  reject: (id: number) => api.put(`/expenses/${id}/reject`),

  getStatistics: () => api.get("/expenses/statistics"),
};

export const reportsAPI = {
  getCoursePerformance: (params?: any) =>
    api.get("/reports/course-performance", { params }),

  getStudentAttendance: (params?: any) =>
    api.get("/reports/student-attendance", { params }),

  getFinancialSummary: (params?: any) =>
    api.get("/reports/financial-summary", { params }),

  getDashboard: () => api.get("/reports/dashboard"),
};

export const attendanceAPI = {
  getCourseAttendance: (courseId: number) =>
    api.get(`/attendance/courses/${courseId}`),

  getStudentAttendance: (studentId: number) =>
    api.get(`/attendance/student/${studentId}`),

  recordAttendance: (data: any) => api.post("/attendance", data),

  updateAttendance: (id: number, data: any) =>
    api.put(`/attendance/${id}`, data),

  updateAttendanceSession: (courseId: number, body: any) =>
    api.put(`/attendance/courses/${courseId}/session`, body),

  createAttendanceSession: (courseId: number, body: any) =>
    api.post(`/attendance/courses/${courseId}/session`, body),

  getCourseSessions: (courseId: number) =>
    api.get(`/attendance/sessions/${courseId}`),
};

export default api;
