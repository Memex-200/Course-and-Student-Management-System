import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { cafeteriaAPI, studentsAPI } from "../../lib/api";
import { CafeteriaItem, Student, PaymentMethod } from "../../types";
import {
  ArrowRight,
  Plus,
  Search,
  Trash2,
  User,
  DollarSign,
  Eye,
  Clock,
  XCircle,
} from "lucide-react";
import toast from "react-hot-toast";

interface CartItem {
  item: CafeteriaItem;
  quantity: number;
  notes?: string;
  customization?: string;
}

const PointOfSale: React.FC = () => {
  const [items, setItems] = useState<CafeteriaItem[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCategory, setSelectedCategory] = useState<string>("all");
  const [students, setStudents] = useState<Student[]>([]);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [customerName, setCustomerName] = useState("");
  const [customerPhone, setCustomerPhone] = useState("");
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(PaymentMethod.Cash);
  const [paidAmount, setPaidAmount] = useState(0);
  const [discount, setDiscount] = useState(0);

  const categories = [
    { value: "all", label: "جميع الفئات" },
    { value: "1", label: "مشروبات" },
    { value: "2", label: "وجبات خفيفة" },
    { value: "3", label: "وجبات" },
    { value: "4", label: "حلويات" },
    { value: "5", label: "فواكه" },
    { value: "6", label: "أخرى" },
  ];

  useEffect(() => {
    fetchItems();
    fetchStudents();
  }, [selectedCategory]);

  const fetchItems = async () => {
    try {
      setLoading(true);
      const params = selectedCategory !== "all" 
        ? { category: selectedCategory, isAvailable: true } 
        : { isAvailable: true };
      const response = await cafeteriaAPI.getItems(params);
      setItems(response.data); // استخدم البيانات الحقيقية من السيرفر
    } catch (error) {
      console.error("Error fetching items:", error);
      toast.error("خطأ في تحميل المنتجات");
    } finally {
      setLoading(false);
    }
  };

  const fetchStudents = async () => {
    try {
      const response = await studentsAPI.getAll();
      setStudents(response.data.data || []);
    } catch (error) {
      console.error("Error fetching students:", error);
    }
  };

  const addToCart = (item: CafeteriaItem) => {
    if (item.stockQuantity <= 0) {
      toast.error("هذا المنتج غير متوفر في المخزون");
      return;
    }

    const existingItem = cart.find(cartItem => cartItem.item.id === item.id);
    
    if (existingItem) {
      if (existingItem.quantity >= item.stockQuantity) {
        toast.error("الكمية المطلوبة تتجاوز المخزون المتاح");
        return;
      }
      updateQuantity(item.id, existingItem.quantity + 1);
    } else {
      setCart([...cart, { item, quantity: 1 }]);
    }
  };

  const updateQuantity = (itemId: number, newQuantity: number) => {
    if (newQuantity <= 0) {
      removeFromCart(itemId);
      return;
    }

    const item = items.find(i => i.id === itemId);
    if (item && newQuantity > item.stockQuantity) {
      toast.error("الكمية المطلوبة تتجاوز المخزون المتاح");
      return;
    }

    setCart(cart.map(cartItem => 
      cartItem.item.id === itemId 
        ? { ...cartItem, quantity: newQuantity }
        : cartItem
    ));
  };

  const removeFromCart = (itemId: number) => {
    setCart(cart.filter(cartItem => cartItem.item.id !== itemId));
  };

  const getCartTotal = () => {
    return cart.reduce((total, cartItem) => total + (cartItem.item.price * cartItem.quantity), 0);
  };

  const getFinalTotal = () => {
    return getCartTotal() - discount;
  };

  const getChange = () => {
    return paidAmount - getFinalTotal();
  };

  const handleCreateOrder = async () => {
    if (cart.length === 0) {
      toast.error("يرجى إضافة منتجات إلى السلة");
      return;
    }

    if (!selectedStudent && (!customerName || !customerPhone)) {
      toast.error("يرجى اختيار طالب أو إدخال بيانات العميل");
      return;
    }

    if (paidAmount < getFinalTotal()) {
      toast.error("المبلغ المدفوع أقل من إجمالي الطلب");
      return;
    }

    try {
      const orderData = {
        studentId: selectedStudent?.id,
        customerName: selectedStudent?.fullName || customerName,
        customerPhone: selectedStudent?.phone || customerPhone,
        discountAmount: discount,
        paidAmount: paidAmount,
        paymentMethod: paymentMethod,
        notes: "طلب من نقطة البيع",
        items: cart.map(cartItem => ({
          cafeteriaItemId: cartItem.item.id,
          quantity: cartItem.quantity,
          notes: cartItem.notes || "",
          customization: cartItem.customization || "",
        })),
      };

      await cafeteriaAPI.createOrder(orderData);
      toast.success("تم إنشاء الطلب بنجاح");
      
      // Reset everything
      setCart([]);
      setSelectedStudent(null);
      setCustomerName("");
      setCustomerPhone("");
      setPaidAmount(0);
      setDiscount(0);
      fetchItems(); // Refresh stock
      
    } catch (error) {
      console.error("Error creating order:", error);
      toast.error("خطأ في إنشاء الطلب");
    }
  };

  const filteredItems = items.filter((item) =>
    item.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    item.description.toLowerCase().includes(searchTerm.toLowerCase())
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
            <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
              <DollarSign className="w-8 h-8 text-green-500" />
              نقطة البيع
            </h1>
            <p className="text-gray-600 mt-2">
              إنشاء طلبات جديدة وإدارة المبيعات
            </p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Products Section */}
        <div className="lg:col-span-2">
          {/* Filters */}
          <div className="flex flex-col sm:flex-row gap-4 mb-6">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                <input
                  type="text"
                  placeholder="البحث في المنتجات..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
                />
              </div>
            </div>
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
            >
              {categories.map((category) => (
                <option key={category.value} value={category.value}>
                  {category.label}
                </option>
              ))}
            </select>
          </div>

          {/* Products Grid */}
          {loading ? (
            <div className="flex justify-center items-center h-64">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-600"></div>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
              {filteredItems.map((item) => (
                <div
                  key={item.id}
                  className="bg-white rounded-lg shadow-sm p-4 border hover:shadow-md transition-shadow cursor-pointer"
                  onClick={() => addToCart(item)}
                >
                  <div className="text-center">
                    <h3 className="font-semibold text-gray-900 mb-2">
                      {item.name}
                    </h3>
                    <p className="text-sm text-gray-600 mb-3">
                      {item.description}
                    </p>
                    <div className="flex items-center justify-between">
                      <span className="text-lg font-bold text-green-600">
                        {item.price} جنيه
                      </span>
                      <span className="text-sm text-gray-500">
                        {item.stockQuantity} متاح
                      </span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Cart & Customer Section */}
        <div className="space-y-6">
          {/* Customer Selection */}
          <div className="bg-white rounded-lg shadow-sm p-6 border">
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <User className="w-5 h-5" />
              بيانات العميل
            </h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  اختيار طالب
                </label>
                <select
                  value={selectedStudent?.id || ""}
                  onChange={(e) => {
                    const student = students.find(s => s.id === Number(e.target.value));
                    setSelectedStudent(student || null);
                    if (student) {
                      setCustomerName("");
                      setCustomerPhone("");
                    }
                  }}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
                >
                  <option value="">اختر طالب أو أدخل بيانات يدوياً</option>
                  {students.map((student) => (
                    <option key={student.id} value={student.id}>
                      {student.fullName} - {student.phone}
                    </option>
                  ))}
                </select>
              </div>

              {!selectedStudent && (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      اسم العميل
                    </label>
                    <input
                      type="text"
                      value={customerName}
                      onChange={(e) => setCustomerName(e.target.value)}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      رقم الهاتف
                    </label>
                    <input
                      type="text"
                      value={customerPhone}
                      onChange={(e) => setCustomerPhone(e.target.value)}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
                    />
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Cart */}
          <div className="bg-white rounded-lg shadow-sm p-6 border">
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Eye className="w-5 h-5" />
              السلة ({cart.length})
            </h3>

            <div className="space-y-3 max-h-64 overflow-y-auto">
              {cart.length === 0 ? (
                <p className="text-gray-500 text-center py-4">السلة فارغة</p>
              ) : (
                cart.map((cartItem) => (
                  <div
                    key={cartItem.item.id}
                    className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                  >
                    <div className="flex-1">
                      <h4 className="font-medium text-sm">{cartItem.item.name}</h4>
                      <p className="text-xs text-gray-600">
                        {cartItem.item.price} جنيه × {cartItem.quantity}
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => updateQuantity(cartItem.item.id, cartItem.quantity - 1)}
                        className="p-1 hover:bg-gray-200 rounded"
                      >
                        <XCircle className="w-4 h-4" />
                      </button>
                      <span className="w-8 text-center text-sm">{cartItem.quantity}</span>
                      <button
                        onClick={() => updateQuantity(cartItem.item.id, cartItem.quantity + 1)}
                        className="p-1 hover:bg-gray-200 rounded"
                      >
                        <Plus className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => removeFromCart(cartItem.item.id)}
                        className="p-1 hover:bg-red-100 rounded text-red-600"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>

          {/* Payment */}
          <div className="bg-white rounded-lg shadow-sm p-6 border">
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Clock className="w-5 h-5" />
              الدفع
            </h3>

            <div className="space-y-4">
              <div className="flex justify-between">
                <span>المجموع الفرعي:</span>
                <span>{getCartTotal()} جنيه</span>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  الخصم
                </label>
                <input
                  type="number"
                  value={discount}
                  onChange={(e) => setDiscount(Number(e.target.value))}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
                />
              </div>

              <div className="flex justify-between text-lg font-semibold">
                <span>الإجمالي:</span>
                <span>{getFinalTotal()} جنيه</span>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  طريقة الدفع
                </label>
                <select
                  value={paymentMethod}
                  onChange={(e) => setPaymentMethod(Number(e.target.value) as PaymentMethod)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
                >
                  <option value={PaymentMethod.Cash}>نقدي</option>
                  <option value={PaymentMethod.InstaPay}>InstaPay</option>
                  <option value={PaymentMethod.Fawry}>فوري</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  المبلغ المدفوع
                </label>
                <input
                  type="number"
                  value={paidAmount}
                  onChange={(e) => setPaidAmount(Number(e.target.value))}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
                />
              </div>

              {paidAmount > getFinalTotal() && (
                <div className="flex justify-between text-green-600">
                  <span>الباقي:</span>
                  <span>{getChange()} جنيه</span>
                </div>
              )}

              <button
                onClick={handleCreateOrder}
                disabled={cart.length === 0}
                className="w-full bg-green-600 text-white py-3 rounded-lg hover:bg-green-700 transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              >
                <DollarSign className="w-5 h-5" />
                إتمام الطلب
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PointOfSale; 