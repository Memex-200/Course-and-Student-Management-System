import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { cafeteriaAPI } from "../../lib/api";
import { CafeteriaOrder, CafeteriaOrderStatus } from "../../types";
import {
  ArrowRight,
  Search,
  Eye,
  Clock,
  CheckCircle,
  XCircle,
  Calendar,
  User,
  DollarSign,
  Edit,
  Trash2,
} from "lucide-react";
import toast from "react-hot-toast";

const OrdersManagement: React.FC = () => {
  const [orders, setOrders] = useState<CafeteriaOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedStatus, setSelectedStatus] = useState<string>("all");
  const [selectedOrder, setSelectedOrder] = useState<CafeteriaOrder | null>(null);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editingOrder, setEditingOrder] = useState<CafeteriaOrder | null>(null);
  const [editFormData, setEditFormData] = useState({
    customerName: "",
    customerPhone: "",
    discountAmount: 0,
    paidAmount: 0,
    paymentMethod: 1,
    notes: ""
  });

  const statusOptions = [
    { value: "all", label: "جميع الحالات" },
    { value: "1", label: "في الانتظار" },
    { value: "2", label: "قيد التحضير" },
    { value: "3", label: "جاهز" },
    { value: "4", label: "تم التسليم" },
    { value: "5", label: "ملغي" },
  ];

  const getStatusIcon = (status: CafeteriaOrderStatus) => {
    switch (status) {
      case CafeteriaOrderStatus.Pending:
        return <Clock className="w-4 h-4" />;
      case CafeteriaOrderStatus.Preparing:
        return <Clock className="w-4 h-4" />;
      case CafeteriaOrderStatus.Ready:
        return <CheckCircle className="w-4 h-4" />;
      case CafeteriaOrderStatus.Delivered:
        return <CheckCircle className="w-4 h-4" />;
      case CafeteriaOrderStatus.Cancelled:
        return <XCircle className="w-4 h-4" />;
      default:
        return <Clock className="w-4 h-4" />;
    }
  };

  const getStatusColor = (status: CafeteriaOrderStatus) => {
    switch (status) {
      case CafeteriaOrderStatus.Pending:
        return "bg-yellow-100 text-yellow-800";
      case CafeteriaOrderStatus.Preparing:
        return "bg-blue-100 text-blue-800";
      case CafeteriaOrderStatus.Ready:
        return "bg-purple-100 text-purple-800";
      case CafeteriaOrderStatus.Delivered:
        return "bg-green-100 text-green-800";
      case CafeteriaOrderStatus.Cancelled:
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const getStatusArabic = (status: CafeteriaOrderStatus) => {
    switch (status) {
      case CafeteriaOrderStatus.Pending:
        return "في الانتظار";
      case CafeteriaOrderStatus.Preparing:
        return "قيد التحضير";
      case CafeteriaOrderStatus.Ready:
        return "جاهز";
      case CafeteriaOrderStatus.Delivered:
        return "تم التسليم";
      case CafeteriaOrderStatus.Cancelled:
        return "ملغي";
      default:
        return "غير محدد";
    }
  };

  useEffect(() => {
    fetchOrders();
  }, [selectedStatus]);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const params = selectedStatus !== "all" ? { status: selectedStatus } : {};
      const response = await cafeteriaAPI.getOrders(params);
      setOrders(response.data); // استخدم البيانات الحقيقية من السيرفر
    } catch (error) {
      console.error("Error fetching orders:", error);
      toast.error("خطأ في تحميل الطلبات");
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async (orderId: number, newStatus: CafeteriaOrderStatus) => {
    try {
      await cafeteriaAPI.updateOrderStatus(orderId, { 
        status: newStatus,
        notes: `تم تحديث الحالة إلى ${getStatusArabic(newStatus)}`
      });
      toast.success("تم تحديث حالة الطلب بنجاح");
      fetchOrders();
    } catch (error) {
      console.error("Error updating order status:", error);
      toast.error("خطأ في تحديث حالة الطلب");
    }
  };

  const openOrderDetails = async (order: CafeteriaOrder) => {
    try {
      const response = await cafeteriaAPI.getOrder(order.id);
      setSelectedOrder(response.data);
      setShowDetailsModal(true);
    } catch (error) {
      // Fallback to basic order data
      setSelectedOrder(order);
      setShowDetailsModal(true);
    }
  };

  const openEditOrder = async (order: CafeteriaOrder) => {
    try {
      const response = await cafeteriaAPI.getOrder(order.id);
      const orderData = response.data;
      setEditingOrder(orderData);
      
      // Initialize form data
      setEditFormData({
        customerName: orderData.customer?.name || orderData.customerName || "",
        customerPhone: orderData.customer?.phone || orderData.customerPhone || "",
        discountAmount: orderData.discountAmount || 0,
        paidAmount: orderData.paidAmount || 0,
        paymentMethod: orderData.paymentMethod || 1,
        notes: orderData.notes || ""
      });
      
      setShowEditModal(true);
    } catch (error) {
      // Fallback to basic order data
      setEditingOrder(order);
      setEditFormData({
        customerName: order.customerName || "",
        customerPhone: order.customerPhone || "",
        discountAmount: order.discountAmount || 0,
        paidAmount: order.paidAmount || 0,
        paymentMethod: order.paymentMethod || 1,
        notes: order.notes || ""
      });
      setShowEditModal(true);
    }
  };

  const handleUpdateOrder = async () => {
    if (!editingOrder) return;

    try {
      
      const updateData = {
        customerName: editFormData.customerName,
        customerPhone: editFormData.customerPhone,
        discountAmount: editFormData.discountAmount,
        paidAmount: editFormData.paidAmount,
        paymentMethod: editFormData.paymentMethod,
        notes: editFormData.notes
      };

      await cafeteriaAPI.updateOrder(editingOrder.id, updateData);
      toast.success("تم تحديث الطلب بنجاح");
      setShowEditModal(false);
      fetchOrders();
    } catch (error) {
      console.error("Error updating order:", error);
      toast.error("خطأ في تحديث الطلب");
    }
  };

  const handleDeleteOrder = async (orderId: number) => {
    if (!window.confirm("هل أنت متأكد من حذف هذا الطلب؟")) {
      return;
    }

    try {
      await cafeteriaAPI.deleteOrder(orderId);
      toast.success("تم حذف الطلب بنجاح");
      fetchOrders();
    } catch (error) {
      console.error("Error deleting order:", error);
      toast.error("خطأ في حذف الطلب");
    }
  };

  const filteredOrders = orders.filter((order) =>
    order.orderNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    order.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    order.customerPhone.includes(searchTerm)
  );

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center gap-4">
          <Link
            to="/cafeteria"
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <ArrowRight className="w-5 h-5" />
          </Link>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">إدارة الطلبات</h1>
            <p className="text-gray-600 mt-2">
              عرض ومتابعة وتحديث حالة طلبات الكافيتيريا
            </p>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4 mb-6">
        <div className="flex-1">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="البحث في الطلبات..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-orange-500 focus:border-transparent"
            />
          </div>
        </div>
        <select
          value={selectedStatus}
          onChange={(e) => setSelectedStatus(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-orange-500 focus:border-transparent"
        >
          {statusOptions.map((status) => (
            <option key={status.value} value={status.value}>
              {status.label}
            </option>
          ))}
        </select>
      </div>

      {/* Orders List */}
      {loading ? (
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-orange-600"></div>
        </div>
      ) : (
        <div className="bg-white rounded-xl shadow-sm border overflow-hidden">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    رقم الطلب
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    العميل
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    التاريخ
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    الحالة
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    الإجمالي
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    الإجراءات
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredOrders.map((order) => (
                  <tr key={order.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {order.orderNumber}
                      </div>
                      <div className="text-sm text-gray-500">
                        {order.itemsCount} منتج
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <User className="w-4 h-4 text-gray-400 ml-2" />
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {order.customerName}
                          </div>
                          <div className="text-sm text-gray-500">
                            {order.customerPhone}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <Calendar className="w-4 h-4 text-gray-400 ml-2" />
                        <div className="text-sm text-gray-900">
                          {new Date(order.orderDate).toLocaleDateString('ar-EG')}
                          <br />
                          <span className="text-xs text-gray-500">
                            {new Date(order.orderDate).toLocaleTimeString('ar-EG')}
                          </span>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`inline-flex items-center gap-1 px-2 py-1 text-xs rounded-full ${getStatusColor(order.status)}`}
                      >
                        {getStatusIcon(order.status)}
                        {getStatusArabic(order.status)}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <DollarSign className="w-4 h-4 text-gray-400 ml-2" />
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {order.totalAmount} جنيه
                          </div>
                          <div className="text-xs text-gray-500">
                            {order.paymentMethodArabic}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => openOrderDetails(order)}
                          className="text-blue-600 hover:text-blue-900 p-1 hover:bg-blue-50 rounded"
                          title="عرض التفاصيل"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                        
                        <button
                          onClick={() => openEditOrder(order)}
                          className="text-green-600 hover:text-green-900 p-1 hover:bg-green-50 rounded"
                          title="تعديل الطلب"
                        >
                          <Edit className="w-4 h-4" />
                        </button>
                        
                        <button
                          onClick={() => handleDeleteOrder(order.id)}
                          className="text-red-600 hover:text-red-900 p-1 hover:bg-red-50 rounded"
                          title="حذف الطلب"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                        
                        {order.status === CafeteriaOrderStatus.Pending && (
                          <button
                            onClick={() => handleUpdateStatus(order.id, CafeteriaOrderStatus.Preparing)}
                            className="text-blue-600 hover:text-blue-900 text-xs px-2 py-1 hover:bg-blue-50 rounded"
                          >
                            بدء التحضير
                          </button>
                        )}
                        
                        {order.status === CafeteriaOrderStatus.Preparing && (
                          <button
                            onClick={() => handleUpdateStatus(order.id, CafeteriaOrderStatus.Ready)}
                            className="text-purple-600 hover:text-purple-900 text-xs px-2 py-1 hover:bg-purple-50 rounded"
                          >
                            جاهز
                          </button>
                        )}
                        
                        {order.status === CafeteriaOrderStatus.Ready && (
                          <button
                            onClick={() => handleUpdateStatus(order.id, CafeteriaOrderStatus.Delivered)}
                            className="text-green-600 hover:text-green-900 text-xs px-2 py-1 hover:bg-green-50 rounded"
                          >
                            تم التسليم
                          </button>
                        )}

                        {(order.status === CafeteriaOrderStatus.Pending || order.status === CafeteriaOrderStatus.Preparing) && (
                          <button
                            onClick={() => handleUpdateStatus(order.id, CafeteriaOrderStatus.Cancelled)}
                            className="text-red-600 hover:text-red-900 text-xs px-2 py-1 hover:bg-red-50 rounded"
                          >
                            إلغاء
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {filteredOrders.length === 0 && (
            <div className="text-center py-8">
              <p className="text-gray-500">لا توجد طلبات متاحة</p>
            </div>
          )}
        </div>
      )}

      {/* Order Details Modal */}
      {showDetailsModal && selectedOrder && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-semibold text-gray-900">
                تفاصيل الطلب {selectedOrder.orderNumber}
              </h2>
              <button
                onClick={() => setShowDetailsModal(false)}
                className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
              >
                ×
              </button>
            </div>

            <div className="space-y-6">
              {/* Customer Info */}
              <div className="bg-gray-50 rounded-lg p-4">
                <h3 className="font-medium text-gray-900 mb-3">معلومات العميل</h3>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-gray-600">الاسم</p>
                    <p className="font-medium">{selectedOrder.customer?.name || selectedOrder.customerName}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">الهاتف</p>
                    <p className="font-medium">{selectedOrder.customer?.phone || selectedOrder.customerPhone}</p>
                  </div>
                </div>
              </div>

              {/* Order Info */}
              <div className="bg-gray-50 rounded-lg p-4">
                <h3 className="font-medium text-gray-900 mb-3">معلومات الطلب</h3>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-gray-600">تاريخ الطلب</p>
                    <p className="font-medium">
                      {new Date(selectedOrder.orderDate).toLocaleString('ar-EG')}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">الحالة</p>
                    <span
                      className={`inline-flex items-center gap-1 px-2 py-1 text-xs rounded-full ${getStatusColor(selectedOrder.status)}`}
                    >
                      {getStatusIcon(selectedOrder.status)}
                      {getStatusArabic(selectedOrder.status)}
                    </span>
                  </div>
                </div>
              </div>

              {/* Payment Info */}
              <div className="bg-gray-50 rounded-lg p-4">
                <h3 className="font-medium text-gray-900 mb-3">معلومات الدفع</h3>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="text-gray-600">المجموع الفرعي:</span>
                    <span>{selectedOrder.subTotal} جنيه</span>
                  </div>
                  {selectedOrder.discountAmount > 0 && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">الخصم:</span>
                      <span className="text-red-600">-{selectedOrder.discountAmount} جنيه</span>
                    </div>
                  )}
                  <div className="flex justify-between font-semibold">
                    <span>الإجمالي:</span>
                    <span>{selectedOrder.totalAmount} جنيه</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">طريقة الدفع:</span>
                    <span>{selectedOrder.paymentMethodArabic}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">حالة الدفع:</span>
                    <span className="text-green-600">{selectedOrder.paymentStatusArabic}</span>
                  </div>
                </div>
              </div>

              {/* Notes */}
              {selectedOrder.notes && (
                <div className="bg-gray-50 rounded-lg p-4">
                  <h3 className="font-medium text-gray-900 mb-3">ملاحظات</h3>
                  <p className="text-gray-700">{selectedOrder.notes}</p>
                </div>
              )}
            </div>

            <div className="flex justify-end mt-6">
              <button
                onClick={() => setShowDetailsModal(false)}
                className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
              >
                إغلاق
              </button>
            </div>
                      </div>
          </div>
        )}

        {/* Edit Order Modal */}
        {showEditModal && editingOrder && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-xl p-6 max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-xl font-semibold text-gray-900">
                  تعديل الطلب {editingOrder.orderNumber}
                </h2>
                <button
                  onClick={() => setShowEditModal(false)}
                  className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                >
                  ×
                </button>
              </div>

              <div className="space-y-6">
                {/* Customer Info */}
                <div className="bg-gray-50 rounded-lg p-4">
                  <h3 className="font-medium text-gray-900 mb-3">معلومات العميل</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        الاسم
                      </label>
                      <input
                        type="text"
                        value={editFormData.customerName}
                        onChange={(e) => setEditFormData({...editFormData, customerName: e.target.value})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-orange-500 focus:border-transparent"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        الهاتف
                      </label>
                      <input
                        type="text"
                        value={editFormData.customerPhone}
                        onChange={(e) => setEditFormData({...editFormData, customerPhone: e.target.value})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-orange-500 focus:border-transparent"
                      />
                    </div>
                  </div>
                </div>

                {/* Payment Info */}
                <div className="bg-gray-50 rounded-lg p-4">
                  <h3 className="font-medium text-gray-900 mb-3">معلومات الدفع</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        الخصم
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        value={editFormData.discountAmount}
                        onChange={(e) => setEditFormData({...editFormData, discountAmount: Number(e.target.value)})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-orange-500 focus:border-transparent"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        المبلغ المدفوع
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        value={editFormData.paidAmount}
                        onChange={(e) => setEditFormData({...editFormData, paidAmount: Number(e.target.value)})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-orange-500 focus:border-transparent"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        طريقة الدفع
                      </label>
                      <select
                        value={editFormData.paymentMethod}
                        onChange={(e) => setEditFormData({...editFormData, paymentMethod: Number(e.target.value)})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-orange-500 focus:border-transparent"
                      >
                        <option value={1}>نقداً</option>
                        <option value={2}>إنستا باي</option>
                        <option value={3}>فوري</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        الإجمالي الجديد
                      </label>
                      <div className="w-full px-3 py-2 bg-gray-100 rounded-lg text-gray-700">
                        {editingOrder.subTotal + editingOrder.taxAmount - editFormData.discountAmount} جنيه
                      </div>
                    </div>
                  </div>
                </div>

                {/* Notes */}
                <div className="bg-gray-50 rounded-lg p-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    ملاحظات
                  </label>
                  <textarea
                    value={editFormData.notes}
                    onChange={(e) => setEditFormData({...editFormData, notes: e.target.value})}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-orange-500 focus:border-transparent"
                  />
                </div>
              </div>

              <div className="flex justify-end gap-3 mt-6">
                <button
                  onClick={() => setShowEditModal(false)}
                  className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
                >
                  إلغاء
                </button>
                <button
                  onClick={handleUpdateOrder}
                  className="px-4 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700 transition-colors"
                >
                  حفظ التغييرات
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    );
  };

export default OrdersManagement; 