import React, { useEffect, useState } from "react";
import axios from "../../lib/api/axios";
import { toast } from "react-hot-toast";
import {
  DollarSign,
  Users,
  BookText,
  Calendar,
  CreditCard,
  Edit,
  Trash2,
  Plus,
  Search,
  Filter,
  FileDown,
} from "lucide-react";
import * as XLSX from "xlsx";

interface PaymentRecord {
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
}

interface CourseRegistration {
  id: number;
  student: { id: number; fullName: string };
  course: { id: number; name: string };
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentStatus: string;
  paymentStatusArabic: string;
}

const PaymentManagement: React.FC = () => {
  const [payments, setPayments] = useState<PaymentRecord[]>([]);
  const [unpaidRegistrations, setUnpaidRegistrations] = useState<
    CourseRegistration[]
  >([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedTab, setSelectedTab] = useState<"payments" | "unpaid">(
    "payments"
  );

  // Payment Modal State
  const [paymentModalOpen, setPaymentModalOpen] = useState(false);
  const [selectedRegistration, setSelectedRegistration] =
    useState<CourseRegistration | null>(null);
  const [paymentData, setPaymentData] = useState({
    amount: "",
    paymentMethod: "Cash",
    notes: "",
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      // Fetch payment records
      const paymentsResponse = await axios.get("/payments/detailed-payments");
      if (paymentsResponse.data.success) {
        setPayments(paymentsResponse.data.data || []);
      }

      // Fetch unpaid registrations
      const registrationsResponse = await axios.get(
        "/payments/course-registrations"
      );
      if (registrationsResponse.data) {
        const unpaid = registrationsResponse.data.filter(
          (reg: any) => reg.paymentStatus !== "FullyPaid"
        );
        setUnpaidRegistrations(unpaid);
      }
    } catch (error) {
      console.error("Error fetching data:", error);
      toast.error("خطأ في تحميل البيانات");
    } finally {
      setLoading(false);
    }
  };

  const handleProcessPayment = async () => {
    if (!selectedRegistration || !paymentData.amount) {
      toast.error("يرجى إدخال جميع البيانات المطلوبة");
      return;
    }

    try {
      const response = await axios.post(
        `/payments/course-registrations/${selectedRegistration.id}/payment`,
        {
          amount: parseFloat(paymentData.amount),
          paymentMethod: paymentData.paymentMethod,
          notes: paymentData.notes,
        }
      );

      if (response.data.message) {
        toast.success(response.data.message);
        setPaymentModalOpen(false);
        setPaymentData({ amount: "", paymentMethod: "Cash", notes: "" });
        fetchData();
      }
    } catch (error: any) {
      console.error("Error processing payment:", error);
      toast.error(error.response?.data?.message || "خطأ في معالجة الدفعة");
    }
  };

  const exportToExcel = () => {
    const dataToExport =
      selectedTab === "payments"
        ? payments.map((payment) => ({
            "اسم الطالب": payment.studentName,
            "اسم الكورس": payment.courseName,
            المبلغ: payment.amount,
            "طريقة الدفع": payment.paymentMethodArabic,
            "تاريخ الدفع": new Date(payment.paymentDate).toLocaleDateString(
              "ar-EG"
            ),
            "معالج بواسطة": payment.processedBy,
            ملاحظات: payment.notes || "-",
          }))
        : unpaidRegistrations.map((reg) => ({
            "اسم الطالب": reg.student.fullName,
            "اسم الكورس": reg.course.name,
            "المبلغ الإجمالي": reg.totalAmount,
            "المبلغ المدفوع": reg.paidAmount,
            "المبلغ المتبقي": reg.remainingAmount,
            "حالة الدفع": reg.paymentStatusArabic,
          }));

    const ws = XLSX.utils.json_to_sheet(dataToExport);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(
      wb,
      ws,
      selectedTab === "payments" ? "المدفوعات" : "المدفوعات المعلقة"
    );
    XLSX.writeFile(
      wb,
      `${selectedTab === "payments" ? "payments" : "unpaid_registrations"}_${
        new Date().toISOString().split("T")[0]
      }.xlsx`
    );
  };

  const filteredPayments = payments.filter(
    (payment) =>
      payment.studentName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      payment.courseName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const filteredUnpaid = unpaidRegistrations.filter(
    (reg) =>
      reg.student.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      reg.course.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="p-6 bg-gradient-to-br from-blue-50 to-indigo-100 min-h-screen">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="bg-white rounded-xl shadow-lg p-6 mb-6">
          <div className="flex justify-between items-center mb-4">
            <h1 className="text-3xl font-bold text-gray-800 flex items-center gap-3">
              <DollarSign className="w-8 h-8 text-green-500" />
              إدارة المدفوعات
            </h1>
            <button
              onClick={exportToExcel}
              className="bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded-lg flex items-center gap-2 transition-colors"
            >
              <FileDown className="w-4 h-4" />
              تصدير Excel
            </button>
          </div>

          {/* Tabs */}
          <div className="flex space-x-4 mb-4">
            <button
              onClick={() => setSelectedTab("payments")}
              className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                selectedTab === "payments"
                  ? "bg-blue-500 text-white"
                  : "bg-gray-200 text-gray-700 hover:bg-gray-300"
              }`}
            >
              سجل المدفوعات
            </button>
            <button
              onClick={() => setSelectedTab("unpaid")}
              className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                selectedTab === "unpaid"
                  ? "bg-red-500 text-white"
                  : "bg-gray-200 text-gray-700 hover:bg-gray-300"
              }`}
            >
              المدفوعات المعلقة
            </button>
          </div>

          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="البحث بالطالب أو الكورس..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
        </div>

        {/* Content */}
        {loading ? (
          <div className="bg-white rounded-xl shadow-lg p-8 text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4"></div>
            <p className="text-gray-600">جاري التحميل...</p>
          </div>
        ) : (
          <div className="bg-white rounded-xl shadow-lg overflow-hidden">
            {selectedTab === "payments" ? (
              <PaymentsTable payments={filteredPayments} />
            ) : (
              <UnpaidTable
                registrations={filteredUnpaid}
                onProcessPayment={(reg) => {
                  setSelectedRegistration(reg);
                  setPaymentModalOpen(true);
                }}
              />
            )}
          </div>
        )}
      </div>

      {/* Payment Modal */}
      {paymentModalOpen && selectedRegistration && (
        <PaymentModal
          registration={selectedRegistration}
          paymentData={paymentData}
          setPaymentData={setPaymentData}
          onSubmit={handleProcessPayment}
          onClose={() => {
            setPaymentModalOpen(false);
            setPaymentData({ amount: "", paymentMethod: "Cash", notes: "" });
          }}
        />
      )}
    </div>
  );
};

// Payments Table Component
const PaymentsTable: React.FC<{ payments: PaymentRecord[] }> = ({
  payments,
}) => (
  <div className="overflow-x-auto">
    <table className="w-full">
      <thead className="bg-gray-50">
        <tr>
          <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
            الطالب
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
            الكورس
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
            المبلغ
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
            طريقة الدفع
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
            التاريخ
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
            معالج بواسطة
          </th>
        </tr>
      </thead>
      <tbody className="bg-white divide-y divide-gray-200">
        {payments.length === 0 ? (
          <tr>
            <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
              لا توجد مدفوعات
            </td>
          </tr>
        ) : (
          payments.map((payment) => (
            <tr key={payment.id} className="hover:bg-gray-50">
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                {payment.studentName}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {payment.courseName}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-green-600 font-bold">
                {payment.amount} جنيه
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {payment.paymentMethodArabic}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {new Date(payment.paymentDate).toLocaleDateString("ar-EG")}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {payment.processedBy}
              </td>
            </tr>
          ))
        )}
      </tbody>
    </table>
  </div>
);

// Unpaid Table Component
const UnpaidTable: React.FC<{
  registrations: CourseRegistration[];
  onProcessPayment: (reg: CourseRegistration) => void;
}> = ({ registrations, onProcessPayment }) => (
  <div className="overflow-x-auto">
    <table className="w-full">
      <thead className="bg-red-50">
        <tr>
          <th className="px-6 py-3 text-right text-xs font-medium text-red-500 uppercase tracking-wider">
            الطالب
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-red-500 uppercase tracking-wider">
            الكورس
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-red-500 uppercase tracking-wider">
            المبلغ الإجمالي
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-red-500 uppercase tracking-wider">
            المدفوع
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-red-500 uppercase tracking-wider">
            المتبقي
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-red-500 uppercase tracking-wider">
            الحالة
          </th>
          <th className="px-6 py-3 text-right text-xs font-medium text-red-500 uppercase tracking-wider">
            إجراءات
          </th>
        </tr>
      </thead>
      <tbody className="bg-white divide-y divide-gray-200">
        {registrations.length === 0 ? (
          <tr>
            <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
              لا توجد مدفوعات معلقة
            </td>
          </tr>
        ) : (
          registrations.map((reg) => (
            <tr key={reg.id} className="hover:bg-gray-50">
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                {reg.student.fullName}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {reg.course.name}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {reg.totalAmount} جنيه
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-green-600">
                {reg.paidAmount} جنيه
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-red-600 font-bold">
                {reg.remainingAmount} جنيه
              </td>
              <td className="px-6 py-4 whitespace-nowrap">
                <span
                  className={`px-2 py-1 text-xs rounded-full ${
                    reg.paymentStatus === "Unpaid"
                      ? "bg-red-100 text-red-800"
                      : "bg-yellow-100 text-yellow-800"
                  }`}
                >
                  {reg.paymentStatusArabic}
                </span>
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button
                  onClick={() => onProcessPayment(reg)}
                  className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded text-xs flex items-center gap-1"
                >
                  <DollarSign className="w-3 h-3" />
                  دفع
                </button>
              </td>
            </tr>
          ))
        )}
      </tbody>
    </table>
  </div>
);

// Payment Modal Component
const PaymentModal: React.FC<{
  registration: CourseRegistration;
  paymentData: { amount: string; paymentMethod: string; notes: string };
  setPaymentData: React.Dispatch<
    React.SetStateAction<{
      amount: string;
      paymentMethod: string;
      notes: string;
    }>
  >;
  onSubmit: () => void;
  onClose: () => void;
}> = ({ registration, paymentData, setPaymentData, onSubmit, onClose }) => (
  <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div className="bg-white rounded-xl p-6 w-full max-w-md mx-4">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-xl font-bold text-gray-800">معالجة دفعة</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          ✕
        </button>
      </div>

      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            الطالب
          </label>
          <input
            type="text"
            value={registration.student.fullName}
            disabled
            className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-100"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            الكورس
          </label>
          <input
            type="text"
            value={registration.course.name}
            disabled
            className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-100"
          />
        </div>

        <div className="grid grid-cols-3 gap-2 text-sm">
          <div>
            <span className="text-gray-600">الإجمالي:</span>
            <div className="font-bold text-gray-800">
              {registration.totalAmount} جنيه
            </div>
          </div>
          <div>
            <span className="text-gray-600">المدفوع:</span>
            <div className="font-bold text-green-600">
              {registration.paidAmount} جنيه
            </div>
          </div>
          <div>
            <span className="text-gray-600">المتبقي:</span>
            <div className="font-bold text-red-600">
              {registration.remainingAmount} جنيه
            </div>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            مبلغ الدفعة *
          </label>
          <input
            type="number"
            value={paymentData.amount}
            onChange={(e) =>
              setPaymentData({ ...paymentData, amount: e.target.value })
            }
            max={registration.remainingAmount}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="أدخل مبلغ الدفعة"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            طريقة الدفع
          </label>
          <select
            value={paymentData.paymentMethod}
            onChange={(e) =>
              setPaymentData({ ...paymentData, paymentMethod: e.target.value })
            }
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          >
            <option value="Cash">نقدي</option>
            <option value="InstaPay">انستا باي</option>
            <option value="Fawry">فوري</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            ملاحظات
          </label>
          <textarea
            value={paymentData.notes}
            onChange={(e) =>
              setPaymentData({ ...paymentData, notes: e.target.value })
            }
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            rows={3}
            placeholder="ملاحظات إضافية..."
          />
        </div>
      </div>

      <div className="flex gap-3 mt-6">
        <button
          onClick={onSubmit}
          className="flex-1 bg-blue-500 hover:bg-blue-600 text-white py-2 px-4 rounded-lg font-medium transition-colors"
        >
          تأكيد الدفع
        </button>
        <button
          onClick={onClose}
          className="flex-1 bg-gray-300 hover:bg-gray-400 text-gray-700 py-2 px-4 rounded-lg font-medium transition-colors"
        >
          إلغاء
        </button>
      </div>
    </div>
  </div>
);

export default PaymentManagement;
