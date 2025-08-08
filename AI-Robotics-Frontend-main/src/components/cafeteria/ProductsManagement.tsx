import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { cafeteriaAPI } from "../../lib/api";
import { CafeteriaItem } from "../../types";
import {
  ArrowRight,
  Plus,
  Search,
  Edit,
  Trash2,
  XCircle,
} from "lucide-react";
import toast from "react-hot-toast";

const ProductsManagement: React.FC = () => {
  const [items, setItems] = useState<CafeteriaItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCategory, setSelectedCategory] = useState<string>("all");
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingItem, setEditingItem] = useState<CafeteriaItem | null>(null);

  const categories = [
    { value: "all", label: "جميع الفئات" },
    { value: "1", label: "مشروبات" },
    { value: "2", label: "وجبات خفيفة" },
    { value: "3", label: "وجبات" },
    { value: "4", label: "حلويات" },
    { value: "5", label: "فواكه" },
    { value: "6", label: "أخرى" },
  ];

  const [formData, setFormData] = useState({
    name: "",
    description: "",
    category: 1,
    price: 0,
    cost: 0,
    stockQuantity: 0,
    minimumStock: 0,
    isAvailable: true,
  });

  useEffect(() => {
    fetchItems();
  }, [selectedCategory]);

  const fetchItems = async () => {
    try {
      setLoading(true);
      const params = selectedCategory !== "all" ? { category: selectedCategory } : {};
      const response = await cafeteriaAPI.getItems(params);
      setItems(response.data); // استخدم البيانات الحقيقية من السيرفر
    } catch (error) {
      console.error("Error fetching items:", error);
      toast.error("خطأ في تحميل المنتجات");
    } finally {
      setLoading(false);
    }
  };

  const handleAddItem = async () => {
    try {
      await cafeteriaAPI.createItem(formData);
      toast.success("تم إضافة المنتج بنجاح");
      setShowAddModal(false);
      resetForm();
      fetchItems();
    } catch (error) {
      console.error("Error adding item:", error);
      toast.error("خطأ في إضافة المنتج");
    }
  };

  const handleUpdateItem = async () => {
    if (!editingItem) return;
    
    try {
      await cafeteriaAPI.updateItem(editingItem.id, formData);
      toast.success("تم تحديث المنتج بنجاح");
      setEditingItem(null);
      resetForm();
      fetchItems();
    } catch (error) {
      console.error("Error updating item:", error);
      toast.error("خطأ في تحديث المنتج");
    }
  };

  const handleDeleteItem = async (id: number) => {
    if (window.confirm("هل أنت متأكد من حذف هذا المنتج؟")) {
      try {
        await cafeteriaAPI.deleteItem(id);
        toast.success("تم حذف المنتج بنجاح");
        fetchItems();
      } catch (error) {
        console.error("Error deleting item:", error);
        toast.error("خطأ في حذف المنتج");
      }
    }
  };

  const resetForm = () => {
    setFormData({
      name: "",
      description: "",
      category: 1,
      price: 0,
      cost: 0,
      stockQuantity: 0,
      minimumStock: 0,
      isAvailable: true,
    });
  };

  const openEditModal = (item: CafeteriaItem) => {
    setEditingItem(item);
    setFormData({
      name: item.name,
      description: item.description,
      category: item.category,
      price: item.price,
      cost: item.cost,
      stockQuantity: item.stockQuantity,
      minimumStock: item.minimumStock,
      isAvailable: item.isAvailable,
    });
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
            <h1 className="text-3xl font-bold text-gray-900">إدارة المنتجات</h1>
            <p className="text-gray-600 mt-2">
              إضافة وتعديل وحذف منتجات الكافيتيريا
            </p>
          </div>
        </div>
        <button
          onClick={() => setShowAddModal(true)}
          className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
        >
          <Plus className="w-5 h-5" />
          إضافة منتج جديد
        </button>
      </div>

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
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
        </div>
        <select
          value={selectedCategory}
          onChange={(e) => setSelectedCategory(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
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
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredItems.map((item) => (
            <div
              key={item.id}
              className="bg-white rounded-xl shadow-sm p-6 border hover:shadow-md transition-shadow"
            >
              {/* Item Header */}
              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <h3 className="text-lg font-semibold text-gray-900">
                    {item.name}
                  </h3>
                  <p className="text-sm text-gray-600 mt-1">
                    {item.categoryArabic}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  {item.isLowStock && (
                    <XCircle className="w-5 h-5 text-red-500" />
                  )}
                  <span
                    className={`inline-block px-2 py-1 text-xs rounded-full ${
                      item.isAvailable
                        ? "bg-green-100 text-green-800"
                        : "bg-red-100 text-red-800"
                    }`}
                  >
                    {item.isAvailable ? "متاح" : "غير متاح"}
                  </span>
                </div>
              </div>

              {/* Item Details */}
              <div className="space-y-2 mb-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">السعر:</span>
                  <span className="font-medium">{item.price} جنيه</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">المخزون:</span>
                  <span
                    className={`font-medium ${
                      item.isLowStock ? "text-red-600" : "text-gray-900"
                    }`}
                  >
                    {item.stockQuantity} {item.unit}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">هامش الربح:</span>
                  <span className="font-medium text-green-600">
                    {item.profitMargin} جنيه
                  </span>
                </div>
              </div>

              {/* Actions */}
              <div className="flex items-center gap-2">
                <button
                  onClick={() => openEditModal(item)}
                  className="flex-1 flex items-center justify-center gap-2 px-3 py-2 text-blue-600 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors"
                >
                  <Edit className="w-4 h-4" />
                  تعديل
                </button>
                <button
                  onClick={() => handleDeleteItem(item.id)}
                  className="flex-1 flex items-center justify-center gap-2 px-3 py-2 text-red-600 bg-red-50 rounded-lg hover:bg-red-100 transition-colors"
                >
                  <Trash2 className="w-4 h-4" />
                  حذف
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add/Edit Modal */}
      {(showAddModal || editingItem) && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 max-w-md w-full mx-4">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              {editingItem ? "تعديل المنتج" : "إضافة منتج جديد"}
            </h2>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  اسم المنتج
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) =>
                    setFormData({ ...formData, name: e.target.value })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  الوصف
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) =>
                    setFormData({ ...formData, description: e.target.value })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  rows={3}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    السعر
                  </label>
                  <input
                    type="number"
                    value={formData.price}
                    onChange={(e) =>
                      setFormData({ ...formData, price: Number(e.target.value) })
                    }
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    التكلفة
                  </label>
                  <input
                    type="number"
                    value={formData.cost}
                    onChange={(e) =>
                      setFormData({ ...formData, cost: Number(e.target.value) })
                    }
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    الكمية الحالية
                  </label>
                  <input
                    type="number"
                    value={formData.stockQuantity}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        stockQuantity: Number(e.target.value),
                      })
                    }
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    الحد الأدنى للمخزون
                  </label>
                  <input
                    type="number"
                    value={formData.minimumStock}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        minimumStock: Number(e.target.value),
                      })
                    }
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  الفئة
                </label>
                <select
                  value={formData.category}
                  onChange={(e) =>
                    setFormData({ ...formData, category: Number(e.target.value) })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  {categories.slice(1).map((category) => (
                    <option key={category.value} value={category.value}>
                      {category.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  checked={formData.isAvailable}
                  onChange={(e) =>
                    setFormData({ ...formData, isAvailable: e.target.checked })
                  }
                  className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                />
                <label className="mr-2 text-sm font-medium text-gray-700">
                  متاح للبيع
                </label>
              </div>
            </div>

            <div className="flex justify-end gap-4 mt-6">
              <button
                onClick={() => {
                  setShowAddModal(false);
                  setEditingItem(null);
                  resetForm();
                }}
                className="px-4 py-2 text-gray-600 hover:text-gray-900 transition-colors"
              >
                إلغاء
              </button>
              <button
                onClick={editingItem ? handleUpdateItem : handleAddItem}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                {editingItem ? "تحديث" : "إضافة"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProductsManagement; 