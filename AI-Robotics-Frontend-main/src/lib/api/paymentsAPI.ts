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
      const response = await axios.get("/payments/detailed-payments", {
        params,
      });
      if (response.data.success) {
        return response.data.data || [];
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
      const response = await axios.get("/payments/course-registrations", {
        params,
      });
      return response.data || [];
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
      // Try the clean transactions endpoint first
      const response = await axios.get("/payments/clean-transactions", {
        params,
      });
      console.log("API Response:", response.data); // Debug log

      if (response.data.success) {
        const transactions = (response.data.data || []).map(
          (transaction: any) => ({
            id: transaction.id,
            studentName: transaction.studentName || "-",
            courseName: transaction.courseName || "-",
            amount: transaction.amount || 0,
            paymentDate: transaction.paymentDate,
            branchName: transaction.branchName || "-",
            transactionType: "income" as const,
            paymentStatus: "paid",
            invoiceUrl: `/api/invoices/payment/${transaction.id}`,
            category: "التدريب",
            description: transaction.notes || "معاملة مالية",
            processedBy: transaction.processedBy || "-",
            paymentMethod: transaction.paymentMethod || "نقدي",
            paymentType: "رسوم الكورس",
          })
        );

        const summary = {
          totalIncome: response.data.totalIncome || 0,
          totalExpenses: 0, // No expenses in clean transactions
          netBalance: response.data.netBalance || 0,
        };

        console.log("Processed transactions:", transactions); // Debug log
        console.log("Summary:", summary); // Debug log

        return { transactions, summary };
      }
      return {
        transactions: [],
        summary: { totalIncome: 0, totalExpenses: 0, netBalance: 0 },
      };
    } catch (error) {
      console.error("Error fetching clean transactions:", error);
      // Fallback to old endpoint if clean fails
      try {
        const response = await axios.get("/payments/all-transactions", {
          params,
        });
        console.log("Fallback API Response:", response.data);

        if (response.data.success) {
          const transactions = (response.data.data || []).map(
            (transaction: any) => ({
              id: transaction.id,
              studentName: transaction.studentName || "-",
              courseName: transaction.courseName || "-",
              amount: transaction.amount || 0,
              paymentDate: transaction.paymentDate,
              branchName: transaction.branchName || "-",
              transactionType: transaction.transactionType as
                | "income"
                | "expenses",
              paymentStatus: "paid",
              invoiceUrl:
                transaction.transactionType === "income"
                  ? `/api/invoices/payment/${transaction.id}`
                  : `/api/invoices/expense/${transaction.id}`,
              category:
                transaction.transactionType === "income" ? "التدريب" : "أخرى",
              description: transaction.notes || "معاملة مالية",
              processedBy: transaction.processedBy || "-",
              paymentMethod: transaction.paymentMethod || "نقدي",
              paymentType:
                transaction.transactionType === "income"
                  ? "رسوم الكورس"
                  : "مصروفات",
            })
          );

          const summary = {
            totalIncome: response.data.totalIncome || 0,
            totalExpenses: response.data.totalExpenses || 0,
            netBalance: response.data.netBalance || 0,
          };

          return { transactions, summary };
        }
      } catch (fallbackError) {
        console.error("Fallback also failed:", fallbackError);
      }

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
      const response = await axios.post(
        `/payments/course-registrations/${registrationId}/payments`,
        {
          amount: parseFloat(paymentData.amount),
          paymentMethod: paymentData.paymentMethod,
          notes: paymentData.notes,
        }
      );

      return {
        success: true,
        message: response.data.message || "تم معالجة الدفعة بنجاح",
      };
    } catch (error: any) {
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
