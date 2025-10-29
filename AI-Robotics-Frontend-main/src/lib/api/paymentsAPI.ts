import axios from "./axios";

export interface PaymentRecord {
  id: number;
  studentName: string;
  courseName: string;
  amount: number;
  paymentMethod: string;
  paymentMethodArabic: string;
  paymentDate: string;
  processedBy: string;
  notes: string;
  registrationId: number;
  branchName: string;
}

export interface CourseRegistration {
  id: number;
  student: { id: number; fullName: string };
  course: { id: number; name: string };
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentStatus: string;
  paymentStatusArabic: string;
}

export interface TransactionRow {
  id: number;
  studentName: string;
  courseName: string;
  amount: number;
  paymentDate: string;
  branchName: string;
  transactionType: "income" | "expenses";
  paymentStatus: string;
  invoiceUrl: string;
  category: string;
  description: string;
  processedBy: string;
  paymentMethod: string;
  paymentType: string;
}

export interface FinancialSummary {
  totalIncome: number;
  totalExpenses: number;
  netBalance: number;
}

export interface PaymentData {
  amount: string;
  paymentMethod: string;
  notes: string;
}

class PaymentsAPI {
  // Get detailed payments
  async getDetailedPayments(params?: {
    startDate?: string;
    endDate?: string;
    courseId?: number;
    studentId?: number;
    branchId?: number;
  }): Promise<PaymentRecord[]> {
    try {
      console.log("Fetching detailed payments with params:", params);
      const response = await axios.get("/payments/detailed-payments", {
        params,
      });
      console.log("Detailed payments response:", response.data);

      if (response.data.success && response.data.data) {
        return response.data.data.map((payment: any) => ({
          id: payment.id,
          studentName: payment.studentName || payment.StudentName || "-",
          courseName: payment.courseName || payment.CourseName || "-",
          amount: payment.amount || payment.Amount || 0,
          paymentMethod:
            payment.paymentMethod || payment.PaymentMethod || "Cash",
          paymentMethodArabic:
            payment.paymentMethodArabic ||
            payment.PaymentMethodArabic ||
            "نقدي",
          paymentDate:
            payment.paymentDate ||
            payment.PaymentDate ||
            new Date().toISOString(),
          processedBy: payment.processedBy || payment.ProcessedBy || "-",
          notes: payment.notes || payment.Notes || "",
          registrationId: payment.registrationId || payment.RegistrationId || 0,
          branchName: payment.branchName || payment.BranchName || "-",
        }));
      }
      return [];
    } catch (error) {
      console.error("Error fetching detailed payments:", error);
      return [];
    }
  }

  // Get course registrations
  async getCourseRegistrations(params?: {
    status?: string;
    startDate?: string;
    endDate?: string;
  }): Promise<CourseRegistration[]> {
    try {
      console.log("Fetching course registrations with params:", params);
      const response = await axios.get("/payments/course-registrations", {
        params,
      });
      console.log("Course registrations response:", response.data);

      if (Array.isArray(response.data)) {
        return response.data.map((reg: any) => ({
          id: reg.id,
          student: {
            id: reg.student?.id || reg.StudentId || 0,
            fullName:
              reg.student?.fullName || reg.Student || reg.StudentName || "-",
          },
          course: {
            id: reg.course?.id || reg.CourseId || 0,
            name: reg.course?.name || reg.Course || reg.CourseName || "-",
          },
          totalAmount: reg.totalAmount || reg.TotalAmount || 0,
          paidAmount: reg.paidAmount || reg.PaidAmount || 0,
          remainingAmount: reg.remainingAmount || reg.RemainingAmount || 0,
          paymentStatus: reg.paymentStatus || reg.PaymentStatus || "Unpaid",
          paymentStatusArabic:
            reg.paymentStatusArabic || reg.PaymentStatusArabic || "غير مدفوع",
        }));
      }
      return [];
    } catch (error) {
      console.error("Error fetching course registrations:", error);
      return [];
    }
  }

  // Get all transactions
  async getAllTransactions(params?: {
    startDate?: string;
    endDate?: string;
  }): Promise<{ transactions: TransactionRow[]; summary: FinancialSummary }> {
    try {
      // Try the debug endpoint first to see what data is available
      console.log("Fetching transactions with params:", params);

      // Try multiple endpoints in order of preference
      const endpoints = [
        "/payments/debug-all-transactions",
        "/payments/clean-transactions",
        "/payments/all-transactions",
      ];

      let response = null;
      let lastError = null;

      for (const endpoint of endpoints) {
        try {
          console.log(`Trying endpoint: ${endpoint}`);
          response = await axios.get(endpoint, { params });
          console.log(`${endpoint} response:`, response.data);

          if (response.data && (response.data.success || response.data.data)) {
            break;
          }
        } catch (error) {
          console.log(`${endpoint} failed:`, error);
          lastError = error;
          continue;
        }
      }

      if (!response || !response.data) {
        throw lastError || new Error("All endpoints failed");
      }

      const data = response.data;
      console.log("Using data from:", response.config.url);
      console.log("Data structure:", data);

      // Process transactions based on the endpoint used
      let transactions: TransactionRow[] = [];
      let summary: FinancialSummary = {
        totalIncome: 0,
        totalExpenses: 0,
        netBalance: 0,
      };

      if (data.success && data.data) {
        // Clean transactions or all transactions endpoint
        transactions = (data.data || []).map((transaction: any) => ({
          id: transaction.id,
          studentName:
            transaction.studentName || transaction.StudentName || "-",
          courseName: transaction.courseName || transaction.CourseName || "-",
          amount: transaction.amount || transaction.Amount || 0,
          paymentDate:
            transaction.paymentDate ||
            transaction.PaymentDate ||
            new Date().toISOString(),
          branchName: transaction.branchName || transaction.BranchName || "-",
          transactionType: (transaction.transactionType || "income") as
            | "income"
            | "expenses",
          paymentStatus: transaction.paymentStatus || "paid",
          invoiceUrl:
            transaction.invoiceUrl || `/api/invoices/payment/${transaction.id}`,
          category: transaction.category || "التدريب",
          description:
            transaction.description || transaction.notes || "معاملة مالية",
          processedBy:
            transaction.processedBy || transaction.ProcessedBy || "-",
          paymentMethod:
            transaction.paymentMethod || transaction.PaymentMethod || "نقدي",
          paymentType:
            transaction.paymentType || transaction.PaymentType || "رسوم الكورس",
        }));

        summary = {
          totalIncome: data.totalIncome || 0,
          totalExpenses: data.totalExpenses || 0,
          netBalance: data.netBalance || 0,
        };
      } else if (Array.isArray(data)) {
        // Direct array response
        transactions = data.map((transaction: any) => ({
          id: transaction.id,
          studentName:
            transaction.studentName || transaction.StudentName || "-",
          courseName: transaction.courseName || transaction.CourseName || "-",
          amount: transaction.amount || transaction.Amount || 0,
          paymentDate:
            transaction.paymentDate ||
            transaction.PaymentDate ||
            new Date().toISOString(),
          branchName: transaction.branchName || transaction.BranchName || "-",
          transactionType: "income" as const,
          paymentStatus: "paid",
          invoiceUrl: `/api/invoices/payment/${transaction.id}`,
          category: "التدريب",
          description:
            transaction.description || transaction.notes || "معاملة مالية",
          processedBy:
            transaction.processedBy || transaction.ProcessedBy || "-",
          paymentMethod:
            transaction.paymentMethod || transaction.PaymentMethod || "نقدي",
          paymentType: "رسوم الكورس",
        }));

        summary = {
          totalIncome: transactions.reduce((sum, t) => sum + t.amount, 0),
          totalExpenses: 0,
          netBalance: transactions.reduce((sum, t) => sum + t.amount, 0),
        };
      }

      console.log("Processed transactions:", transactions.length);
      console.log("Summary:", summary);

      return { transactions, summary };
    } catch (error) {
      console.error("Error fetching transactions:", error);
      return {
        transactions: [],
        summary: { totalIncome: 0, totalExpenses: 0, netBalance: 0 },
      };
    }
  }

  // Get alternative transaction data (fallback)
  async getAlternativeTransactions(): Promise<{
    transactions: TransactionRow[];
    summary: FinancialSummary;
  }> {
    try {
      // Get payments data
      const paymentsResponse = await axios.get("/payments/all-transactions");
      const payments = paymentsResponse.data?.data || [];

      // Get expenses data if available
      let expenses: any[] = [];
      try {
        const expensesResponse = await axios.get("/expenses");
        expenses = expensesResponse.data || [];
      } catch (expensesError) {
        console.log("Expenses endpoint not available");
      }

      // Combine payments and expenses
      const transactions: TransactionRow[] = [
        ...payments.map((payment: any) => ({
          id: payment.id,
          studentName: payment.studentName || payment.student?.fullName || "-",
          courseName: payment.courseName || payment.course?.name || "-",
          amount: payment.amount || 0,
          paymentDate:
            payment.paymentDate ||
            payment.createdAt ||
            new Date().toISOString(),
          branchName: payment.branchName || "-",
          transactionType: "income" as const,
          paymentStatus: "paid",
          invoiceUrl: `/api/invoices/payment/${payment.id}`,
          category: "التدريب",
          description: payment.notes || "دفعة تدريب",
          processedBy: payment.processedBy || payment.createdBy || "-",
        })),
        ...expenses.map((expense: any) => ({
          id: expense.id,
          studentName: "-",
          courseName: "-",
          amount: expense.amount || 0,
          paymentDate:
            expense.expenseDate ||
            expense.createdAt ||
            new Date().toISOString(),
          branchName: expense.branchName || "-",
          transactionType: "expense" as const,
          paymentStatus: "paid",
          invoiceUrl: `/api/invoices/expense/${expense.id}`,
          category: expense.category || "أخرى",
          description: expense.description || "مصروف",
          processedBy: expense.processedBy || expense.createdBy || "-",
        })),
      ];

      // Calculate summary
      const totalIncome = payments.reduce(
        (sum: number, p: any) => sum + (p.amount || 0),
        0
      );
      const totalExpenses = expenses.reduce(
        (sum: number, e: any) => sum + (e.amount || 0),
        0
      );

      const summary: FinancialSummary = {
        totalIncome,
        totalExpenses,
        netBalance: totalIncome - totalExpenses,
      };

      return { transactions, summary };
    } catch (error) {
      console.error("Error fetching alternative transactions:", error);
      return {
        transactions: [],
        summary: { totalIncome: 0, totalExpenses: 0, netBalance: 0 },
      };
    }
  }

  // Process payment
  async processPayment(
    registrationId: number,
    paymentData: PaymentData
  ): Promise<{ success: boolean; message: string }> {
    try {
      console.log(
        "Processing payment for registration:",
        registrationId,
        paymentData
      );
      const response = await axios.post(
        `/payments/course-registrations/${registrationId}/payment`,
        {
          amount: parseFloat(paymentData.amount),
          paymentMethod: paymentData.paymentMethod,
          notes: paymentData.notes,
        }
      );

      console.log("Payment processing response:", response.data);
      return {
        success: true,
        message: response.data.message || "تم معالجة الدفعة بنجاح",
      };
    } catch (error: any) {
      console.error("Payment processing error:", error);
      return {
        success: false,
        message: error.response?.data?.message || "خطأ في معالجة الدفعة",
      };
    }
  }

  // Get payment statistics
  async getPaymentStatistics(params?: {
    startDate?: string;
    endDate?: string;
  }): Promise<any> {
    try {
      const response = await axios.get("/payments/statistics", { params });
      return response.data;
    } catch (error) {
      console.error("Error fetching payment statistics:", error);
      return null;
    }
  }
}

export const paymentsAPI = new PaymentsAPI();
export default paymentsAPI;
