export interface User {
  id: number;
  username: string;
  fullName: string;
  email: string;
  phone: string;
  address: string;
  role?: UserRole;
  userRole?: UserRole;
  branchId: number;
}

export enum UserRole {
  Admin = 1,
  Employee = 2,
  Student = 3
}

export interface AuthResponse {
  token?: string;
  user?: User;
  message?: string;
}

export interface RegisterData {
  username: string;
  password: string;
  confirmPassword?: string; // This is only for frontend validation
  fullName: string;
  email: string;
  phone: string;
  userRole: UserRole; // Changed to use enum
  branchId: number;
}

export interface Student {
  id: number;
  fullName: string;
  phone: string;
  email?: string;
  age: number;
  gender: 'Male' | 'Female';
  school?: string;
  grade?: string;
  address?: string;
  parentName: string;
  parentPhone: string;
  parentEmail?: string;
  emergencyContact?: string;
  emergencyPhone?: string;
  medicalConditions?: string;
  preferredTransportation?: string;
  branchId: number;
  branch?: {
    id: number;
    name: string;
  };
  registeredCourses?: number;
  isActive: boolean;
  createdAt: string;
}

export interface Course {
  id: number;
  name: string;
  description: string;
  price: number;
  maxStudents: number;
  currentStudents: number;
  startDate: string;
  endDate: string;
  schedule: string;
  status: 'Planned' | 'Active' | 'Completed' | 'Cancelled' | 'Suspended';
  statusArabic: string;
  courseCategoryId?: number | null;
  courseCategory?: CourseCategory;
  instructorId?: number | null;
  instructor?: Instructor;
  roomId?: number;
  labId?: number;
  branchId?: number | null;
  branch?: Branch;
}

export interface CourseCategory {
  id: number;
  name: string;
  description: string;
  minAge: number;
  maxAge: number;
}

export interface Instructor {
  id: number;
  fullName: string;
  email: string;
  phone: string;
  specialization: string;
}

export interface WorkspaceBooking {
  id: number;
  studentId?: number;
  student?: Student;
  customerName: string;
  customerPhone: string;
  startTime: string;
  endTime: string;
  hourlyRate: number;
  totalHours: number;
  totalAmount: number;
  paidAmount: number;
  paymentStatus: 'Pending' | 'PartiallyPaid' | 'FullyPaid' | 'Overdue' | 'Cancelled';
  paymentStatusArabic: string;
  paymentMethod: 'Cash' | 'InstaPay' | 'Fawry';
  paymentMethodArabic: string;
  purpose: string;
  notes?: string;
  isActive: boolean;
}

export interface SharedWorkspace {
  id: number;
  name: string;
  description: string;
  maxCapacity: number;
  currentOccupancy: number;
  availableCapacity: number;
  hourlyRatePerPerson: number;
  hasWifi: boolean;
  hasPrinter: boolean;
  hasProjector: boolean;
  hasWhiteboard: boolean;
  equipment: string;
  status: 'Available' | 'Occupied' | 'Full' | 'Maintenance';
  statusArabic: string;
  isActive: boolean;
}

export interface SharedWorkspaceBooking {
  id: number;
  sharedWorkspaceId: number;
  sharedWorkspace: SharedWorkspace;
  studentId?: number;
  student?: Student;
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  startTime: string;
  endTime: string;
  numberOfPeople: number;
  hourlyRate: number;
  totalHours: number;
  totalAmount: number;
  paidAmount: number;
  paymentStatus: 'Pending' | 'PartiallyPaid' | 'FullyPaid' | 'Overdue' | 'Cancelled';
  paymentStatusArabic: string;
  paymentMethod: 'Cash' | 'InstaPay' | 'Fawry';
  paymentMethodArabic: string;
  bookingType: 'Individual' | 'Group' | 'Study' | 'Meeting' | 'Project';
  bookingTypeArabic: string;
  status: 'Active' | 'Completed' | 'Cancelled';
  statusArabic: string;
  purpose: string;
  requiredEquipment?: string;
  checkInTime?: string;
  checkOutTime?: string;
}

export interface Payment {
  id: number;
  amount: number;
  paymentDate: string;
  paymentMethod: 'Cash' | 'InstaPay' | 'Fawry';
  paymentMethodArabic: string;
  notes?: string;
  type: string;
  referenceId: number;
}

export interface Equipment {
  id: number;
  name: string;
  serialNumber: string;
  model: string;
  manufacturer: string;
  purchaseDate: string;
  purchasePrice: number;
  warrantyExpiry?: string;
  condition: 'Excellent' | 'Good' | 'Fair' | 'Poor';
  conditionArabic: string;
  status: 'Available' | 'InUse' | 'Maintenance' | 'Damaged' | 'Retired';
  statusArabic: string;
  roomId?: number;
}

export interface Expense {
  id: number;
  title: string;
  description: string;
  category: number;
  categoryName: string;
  amount: number;
  expenseDate: string;
  priority: number;
  vendor?: string;
  paymentMethod: number;
  paymentMethodName: string;
  receiptNumber?: string;
  notes?: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  statusArabic: string;
  isRecurring: boolean;
  branchId: number;
}

export interface Employee {
  id: number;
  userId: number;
  user?: User;
  fullName: string;
  position: string;
  department: string;
  hireDate: string;
  isActive: boolean;
}

export enum PaymentStatus {
  Pending = 1,
  Unpaid = 2,
  PartiallyPaid = 3,
  FullyPaid = 4,
  Cancelled = 5
}

export enum PaymentMethod {
  Cash = 1,
  InstaPay = 2,
  Fawry = 3
}

// Cafeteria Types
export enum CafeteriaItemCategory {
  Beverages = 1,
  Snacks = 2,
  Meals = 3,
  Desserts = 4,
  Fruits = 5,
  Other = 6
}

export enum CafeteriaOrderStatus {
  Pending = 1,
  Preparing = 2,
  Ready = 3,
  Delivered = 4,
  Cancelled = 5
}

export interface CafeteriaItem {
  id: number;
  name: string;
  description: string;
  category: CafeteriaItemCategory;
  categoryArabic: string;
  price: number;
  cost: number;
  unit: string;
  stockQuantity: number;
  minimumStock: number;
  isAvailable: boolean;
  isActive: boolean;
  isLowStock: boolean;
  profitMargin: number;
  branchId: number;
  createdAt: string;
  notes?: string;
}

export interface CafeteriaOrderItem {
  id: number;
  cafeteriaItemId: number;
  cafeteriaItem?: CafeteriaItem;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  notes?: string;
  customization?: string;
}

export interface CafeteriaOrder {
  id: number;
  orderNumber: string;
  studentId?: number;
  employeeId?: number;
  student?: Student;
  employee?: Employee;
  customerName: string;
  customerPhone: string;
  orderDate: string;
  status: CafeteriaOrderStatus;
  statusArabic: string;
  subTotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentStatus: PaymentStatus;
  paymentStatusArabic: string;
  paymentMethod: PaymentMethod;
  paymentMethodArabic: string;
  branchId: number;
  createdByUserId: number;
  createdByUser?: User;
  preparedByUserId?: number;
  preparedByUser?: User;
  preparedAt?: string;
  deliveredByUserId?: number;
  deliveredByUser?: User;
  deliveredAt?: string;
  notes?: string;
  cancellationReason?: string;
  items?: CafeteriaOrderItem[];
  itemsCount: number;
  customer?: {
    name: string;
    phone: string;
    type?: string;
  };
}

export interface CafeteriaStatistics {
  totalOrders: number;
  pendingOrders: number;
  completedOrders: number;
  cancelledOrders: number;
  totalRevenue: number;
  totalPaid: number;
  totalCost: number;
  totalProfit: number;
  averageOrderValue: number;
  topSellingItems: Array<{
    item: string;
    quantity: number;
    revenue: number;
  }>;
  revenueData: Array<any>;
  categoryData: Array<any>;
  salesTrend: Array<any>;
}

export interface Branch {
  id: number;
  name: string;
  address: string;
  phone: string;
  hasWorkspace: boolean;
  hasSharedWorkspace: boolean;
  hasRooms: boolean;
}

export interface DashboardStats {
  totalStudents: number;
  totalCourses: number;
  activeCourses: number;
  totalRevenue: number;
  monthlyRevenue: number;
  activeWorkspaceBookings: number;
  pendingPayments: number;
  upcomingClasses: number;
}