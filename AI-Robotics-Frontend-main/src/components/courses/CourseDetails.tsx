import React, { useState, useEffect } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import coursesAPI from "../../lib/api/coursesAPI";
import { coursesAPI as apiCourses } from "../../lib/api";
import { CourseWithEnrollments } from "../../types/course";
import { Student } from "../../types";
import toast from "react-hot-toast";
import {
  ArrowRight,
  Edit,
  Trash2,
  Calendar,
  Users,
  Clock,
  GraduationCap,
  DollarSign,
  UserPlus,
  UserMinus,
  CheckCircle,
  XCircle,
  DollarSign as PaymentIcon,
  User,
  Eye,
} from "lucide-react";

const CourseDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [course, setCourse] = useState<CourseWithEnrollments | null>(null);
  const [loading, setLoading] = useState(true);
  const [enrollmentModalOpen, setEnrollmentModalOpen] = useState(false);
  const [paymentModalOpen, setPaymentModalOpen] = useState(false);
  const [studentDetailsModalOpen, setStudentDetailsModalOpen] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState<any>(null);
  const [availableStudents, setAvailableStudents] = useState<Student[]>([]);
  const [selectedStudentId, setSelectedStudentId] = useState<number>(0);
  const [selectedEnrollmentId, setSelectedEnrollmentId] = useState<number>(0);
  const [paymentData, setPaymentData] = useState({
    paidAmount: 0,
    paymentMethod: "Cash",
    notes: "",
  });

  useEffect(() => {
    fetchCourse();
  }, [id]);

  const fetchCourse = async () => {
    try {
      setLoading(true);
      const response = await apiCourses.getById(Number(id));
      console.log("Full API Response:", response.data);
      if (response.data.success) {
        console.log("Course data:", response.data.data);
        console.log("Enrolled students:", response.data.data.enrolledStudents);
        setCourse(response.data.data);
      } else {
        toast.error(response.data.message || "خطأ في تحميل بيانات الدورة");
        navigate("/courses");
      }
    } catch (error: any) {
      console.error("Error fetching course:", error);
      toast.error(
        error.response?.data?.message || "خطأ في تحميل بيانات الدورة"
      );
      navigate("/courses");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!course) return;
    if (window.confirm(`هل أنت متأكد من حذف الدورة ${course.name}؟`)) {
      try {
        const response = await coursesAPI.delete(course.id);
        if (response.data.success) {
          toast.success("تم حذف الدورة بنجاح");
          navigate("/courses");
        } else {
          toast.error(response.data.message || "خطأ في حذف الدورة");
        }
      } catch (error: any) {
        console.error("Error deleting course:", error);
        toast.error(error.response?.data?.message || "خطأ في حذف الدورة");
      }
    }
  };

  const openEnrollmentModal = async () => {
    try {
      const response = await coursesAPI.getAvailableStudents(Number(id));
      if (response.data.success) {
        setAvailableStudents(response.data.data);
        setEnrollmentModalOpen(true);
      } else {
        toast.error(response.data.message || "خطأ في تحميل قائمة الطلاب");
      }
    } catch (error: any) {
      console.error("Error fetching available students:", error);
      toast.error(error.response?.data?.message || "خطأ في تحميل قائمة الطلاب");
    }
  };

  const handleEnrollStudent = async () => {
    if (!selectedStudentId) {
      toast.error("الرجاء اختيار طالب");
      return;
    }

    try {
      // Default enrollment data  
      const enrollmentData = {
        totalAmount: course?.price || 0,
        paidAmount: 0, // دائماً صفر عند التسجيل الجديد
        paymentMethod: 1, // Cash = 1
        notes: "تسجيل جديد من الواجهة"
      };

      const response = await coursesAPI.enrollStudent(
        Number(id),
        selectedStudentId,
        enrollmentData
      );
      
      if (response.data.success) {
        toast.success("تم تسجيل الطالب في الكورس بنجاح", {
          className: 'toast-success-green',
          style: {
            background: '#10B981',
            color: '#ffffff',
            border: 'none',
            fontSize: '16px',
            fontWeight: '500',
            direction: 'rtl',
          },
          duration: 4000,
        });
        setEnrollmentModalOpen(false);
        setSelectedStudentId(0);
        
        // تحديث بيانات الكورس والطلاب المسجلين
        await fetchCourse();
        
        // تحديث قائمة الطلاب المتاحين للتسجيل
        try {
          const availableResponse = await coursesAPI.getAvailableStudents(Number(id));
          if (availableResponse.data.success) {
            setAvailableStudents(availableResponse.data.data);
          }
        } catch (error) {
          console.error("Error updating available students list:", error);
        }
      } else {
        toast.error(response.data.message || "خطأ في تسجيل الطالب");
      }
    } catch (error: any) {
      console.error("Error enrolling student:", error);
      toast.error(error.response?.data?.message || "خطأ في تسجيل الطالب");
    }
  };

  const handleRemoveStudent = async (registrationId: number) => {
    if (window.confirm("هل أنت متأكد من إلغاء تسجيل الطالب من الدورة؟")) {
      try {
        const response = await coursesAPI.removeStudentByRegistrationId(
          registrationId
        );
        if (response.data.success) {
          toast.success("تم إلغاء تسجيل الطالب من الدورة بنجاح");
          fetchCourse();
        } else {
          toast.error(response.data.message || "خطأ في إلغاء تسجيل الطالب");
        }
      } catch (error: any) {
        console.error("Error removing student:", error);
        toast.error(
          error.response?.data?.message || "خطأ في إلغاء تسجيل الطالب"
        );
      }
    }
  };

  const handleUpdateEnrollmentStatus = async (
    registrationId: number,
    status: "active" | "completed" | "dropped"
  ) => {
    try {
      const response = await coursesAPI.updateEnrollment(
        registrationId,
        status
      );

      // التحقق من نجاح العملية بناءً على status code
      if (response.status === 200) {
        toast.success("تم تحديث حالة الطالب بنجاح");
        fetchCourse();
      } else {
        toast.error(response.data?.message || "خطأ في تحديث حالة الطالب");
      }
    } catch (error: any) {
      console.error("Error updating enrollment status:", error);
      toast.error(error.response?.data?.message || "خطأ في تحديث حالة الطالب");
    }
  };

  const openPaymentModal = (
    enrollmentId: number,
    currentPaidAmount: number
  ) => {
    setSelectedEnrollmentId(enrollmentId);
    setPaymentData({
      paidAmount: currentPaidAmount === 0 ? 0 : currentPaidAmount,
      paymentMethod: "Cash",
      notes: "",
    });
    setPaymentModalOpen(true);
  };

  const handleUpdatePayment = async () => {
    try {
      // التحقق من صحة البيانات
      if (!paymentData.paidAmount || paymentData.paidAmount < 0) {
        toast.error("يرجى إدخال مبلغ صحيح");
        return;
      }

      // تحويل طريقة الدفع من string إلى integer
      const paymentMethodMap: { [key: string]: number } = {
        "Cash": 1,
        "InstaPay": 2, 
        "Fawry": 3
      };

      const response = await coursesAPI.updateCoursePayment(
        selectedEnrollmentId,
        {
          paidAmount: Number(paymentData.paidAmount),
          paymentMethod: paymentMethodMap[paymentData.paymentMethod] || 1, // افتراضي: Cash
          notes: paymentData.notes,
        }
      );

      // التحقق من نجاح العملية بناءً على status code
      if (response.status === 200) {
        toast.success("تم تحديث حالة الدفع بنجاح");
        setPaymentModalOpen(false);
        
        // تحديث البيانات فوراً في الواجهة قبل إعادة تحميل البيانات من الخادم
        if (course && course.enrolledStudents) {
          const updatedStudents = course.enrolledStudents.map((student: any) => {
            if ((student.RegistrationId || student.registrationId) === selectedEnrollmentId) {
              return {
                ...student,
                PaidAmount: Number(paymentData.paidAmount),
                paidAmount: Number(paymentData.paidAmount),
                PaymentStatus: response.data.data?.paymentStatus || student.PaymentStatus,
                PaymentStatusArabic: response.data.data?.paymentStatusArabic || student.PaymentStatusArabic,
              };
            }
            return student;
          });
          
          setCourse(prev => prev ? {
            ...prev,
            enrolledStudents: updatedStudents
          } : null);
        }
        
        // ثم إعادة تحميل البيانات من الخادم للتأكد
        await fetchCourse();
      } else {
        toast.error(response.data?.message || "خطأ في تحديث حالة الدفع");
      }
    } catch (error: any) {
      console.error("Error updating payment:", error);
      toast.error(error.response?.data?.message || "خطأ في تحديث حالة الدفع");
    }
  };

  const handleCreateStudentAccount = async (
    registrationId: number
  ) => {
    try {
      const response = await coursesAPI.createStudentAccount(registrationId);

      if (response.status === 200 && response.data.data) {
        const accountData = response.data.data;
        toast.success("تم إنشاء حساب الطالب بنجاح!");

        // عرض بيانات الدخول في modal
        alert(`تم إنشاء حساب للطالب: ${accountData.studentName}

اسم المستخدم: ${accountData.username}
كلمة المرور: ${accountData.password}

يرجى حفظ هذه البيانات وإرسالها للطالب`);

        fetchCourse();
      } else {
        toast.error(response.data?.message || "خطأ في إنشاء حساب الطالب");
      }
    } catch (error: any) {
      console.error("Error creating student account:", error);
      toast.error(error.response?.data?.message || "خطأ في إنشاء حساب الطالب");
    }
  };

  const handleIssueCertificate = async (
    registrationId: number,
    studentName: string
  ) => {
    try {
      // طلب درجة الامتحان من المستخدم
      const examScoreInput = prompt(
        `إدخال درجة الامتحان للطالب: ${studentName}\n(من 0 إلى 100)`
      );

      if (examScoreInput === null) return; // المستخدم ألغى العملية

      const examScore = parseInt(examScoreInput);
      if (isNaN(examScore) || examScore < 0 || examScore > 100) {
        toast.error("يرجى إدخال درجة صحيحة من 0 إلى 100");
        return;
      }

      const response = await coursesAPI.issueCertificate(registrationId, {
        examScore: examScore,
        notes: `شهادة إتمام الكورس - درجة الامتحان: ${examScore}%`,
      });

      if (response.status === 200 && response.data.data) {
        const certificateData = response.data.data;
        toast.success("تم إصدار الشهادة بنجاح!");

        alert(`تم إصدار شهادة للطالب: ${studentName}

رقم الشهادة: ${certificateData.certificateNumber}
درجة الامتحان: ${certificateData.examScore}%

تم حفظ الشهادة في النظام`);

        fetchCourse();
      } else {
        toast.error(response.data?.message || "خطأ في إصدار الشهادة");
      }
    } catch (error: any) {
      console.error("Error issuing certificate:", error);
      toast.error(error.response?.data?.message || "خطأ في إصدار الشهادة");
    }
  };

  const openStudentDetailsModal = async (student: any) => {
    try {
      // Get full student details from API
      const response = await fetch(`http://localhost:5227/api/students/${student.id || student.Id}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      });
      
      if (response.ok) {
        const fullStudentData = await response.json();
        console.log("Full student data from API:", fullStudentData);
        
        // Merge registration data with full student data
        const enrichedStudent = {
          ...student,
          ...fullStudentData,
          // Override with registration specific data
          RegistrationId: student.RegistrationId || student.registrationId,
          PaidAmount: student.PaidAmount,
          PaymentStatus: student.PaymentStatus,
          PaymentStatusArabic: student.PaymentStatusArabic,
          // Add default values for missing fields
          idNumber: fullStudentData.nationalId || "30012345678901", // Default national ID
          registrationDate: new Date().toISOString(), // Today's date
          status: student.PaymentStatusArabic || "نشط",
          paymentMethod: "نقدي", // Default payment method
          notes: student.notes || fullStudentData.notes || "طالب مسجل في الكورس"
        };
        
        setSelectedStudent(enrichedStudent);
      } else {
        // If API call fails, use existing data with defaults
        const enrichedStudent = {
          ...student,
          idNumber: "30012345678901", // Default national ID
          registrationDate: new Date().toISOString(),
          status: student.PaymentStatusArabic || "نشط",
          paymentMethod: "نقدي",
          notes: "طالب مسجل في الكورس"
        };
        setSelectedStudent(enrichedStudent);
      }
    } catch (error) {
      console.error("Error fetching student details:", error);
      // Fallback to basic data with defaults
      const enrichedStudent = {
        ...student,
        idNumber: "30012345678901",
        registrationDate: new Date().toISOString(),
        status: student.PaymentStatusArabic || "نشط", 
        paymentMethod: "نقدي",
        notes: "طالب مسجل في الكورس"
      };
      setSelectedStudent(enrichedStudent);
    }
    
    setStudentDetailsModalOpen(true);
  };

  if (loading) {
    return (
      <div className="p-6 max-w-4xl mx-auto rtl">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
          <div className="space-y-4">
            {[...Array(6)].map((_, i) => (
              <div key={i} className="h-12 bg-gray-200 rounded"></div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (!course) return null;

  return (
    <div className="p-6 max-w-4xl mx-auto rtl">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            {course.name}
          </h1>
          <p className="text-gray-600">تفاصيل الدورة وإدارة الطلاب المسجلين</p>
        </div>
        <div className="flex items-center space-x-4 rtl:space-x-reverse">
          <button
            onClick={() => navigate("/courses")}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 text-gray-600 hover:text-gray-900 transition-colors"
          >
            <ArrowRight className="w-4 h-4" />
            <span>عودة</span>
          </button>
          <Link
            to={`/courses/${id}/edit`}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 text-blue-600 hover:text-blue-700 transition-colors"
          >
            <Edit className="w-4 h-4" />
            <span>تعديل</span>
          </Link>
          <button
            onClick={handleDelete}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 text-red-600 hover:text-red-700 transition-colors"
          >
            <Trash2 className="w-4 h-4" />
            <span>حذف</span>
          </button>
        </div>
      </div>

      {/* Course Details */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div className="flex items-center text-gray-600">
            <Calendar className="w-5 h-5 ml-2" />
            <div>
              <p className="font-medium">تاريخ البداية</p>
              <p>{new Date(course.startDate).toLocaleDateString("ar-EG")}</p>
            </div>
          </div>
          <div className="flex items-center text-gray-600">
            <Calendar className="w-5 h-5 ml-2" />
            <div>
              <p className="font-medium">تاريخ النهاية</p>
              <p>{new Date(course.endDate).toLocaleDateString("ar-EG")}</p>
            </div>
          </div>
          <div className="flex items-center text-gray-600">
            <Clock className="w-5 h-5 ml-2" />
            <div>
              <p className="font-medium">المواعيد</p>
              <p>{course.schedule}</p>
            </div>
          </div>
          <div className="flex items-center text-gray-600">
            <Users className="w-5 h-5 ml-2" />
            <div>
              <p className="font-medium">الطلاب</p>
              <p>
                {course.currentStudents}/{course.maxStudents}
              </p>
            </div>
          </div>
          <div className="flex items-center text-gray-600">
            <GraduationCap className="w-5 h-5 ml-2" />
            <div>
              <p className="font-medium">المدرب</p>
              <p>{course.instructor?.fullName || "غير محدد"}</p>
            </div>
          </div>
          <div className="flex items-center text-gray-600">
            <DollarSign className="w-5 h-5 ml-2" />
            <div>
              <p className="font-medium">السعر</p>
              <p>{course.price} جنيه</p>
            </div>
          </div>
        </div>
        <div className="mt-6">
          <h3 className="font-medium text-gray-900 mb-2">الوصف</h3>
          <p className="text-gray-600">{course.description}</p>
        </div>
      </div>

      {/* Enrolled Students */}
      <div className="bg-white rounded-xl shadow-sm p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-semibold text-gray-900">
            الطلاب المسجلين
          </h2>
          <button
            onClick={openEnrollmentModal}
            disabled={course.currentStudents >= course.maxStudents}
            className="flex items-center space-x-2 rtl:space-x-reverse px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <UserPlus className="w-4 h-4" />
            <span>إضافة طالب</span>
          </button>
        </div>

        {course.enrolledStudents && course.enrolledStudents.length > 0 ? (
          <div className="divide-y">
            {course.enrolledStudents.map((student: any, index: number) => (
              <div
                  key={`student-${student.id || student.Id || index}-${student.registrationId || student.RegistrationId || index}`}
                className="py-4 flex items-center justify-between"
              >
                <div>
                  <h3 className="font-medium text-gray-900">
                      {student.FullName || student.fullName || "غير محدد"}
                  </h3>
                    <p className="text-gray-600">{student.Phone || student.phone || "غير محدد"}</p>
                    <p className="text-sm text-blue-600">
                      {student.PaymentMethodArabic || student.paymentMethodArabic || "نقدي"} - 
                      <span className="mr-1">{student.PaidAmount || student.paidAmount || 0} جنيه</span>
                    </p>
                </div>
                <div className="flex items-center gap-2">
                  {/* زر عرض بيانات الطالب */}
                  <button
                    onClick={() => openStudentDetailsModal(student)}
                    className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-all duration-200 border border-gray-200"
                    title="عرض بيانات الطالب"
                  >
                    <Eye className="w-5 h-5" />
                  </button>

                  {/* زر تعديل الدفع */}
                  <button
                    onClick={() => {
                      console.log("Full student object:", student);
                      console.log("RegistrationId:", student.RegistrationId || student.registrationId);
                      console.log("FullName:", student.FullName || student.fullName);
                      console.log("All student keys:", Object.keys(student));
                      console.log("Student values:", Object.values(student));
                      openPaymentModal(
                        student.RegistrationId || student.registrationId,
                        student.PaidAmount || student.paidAmount || 0
                      );
                    }}
                    className="p-2 text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition-all duration-200 border border-blue-200"
                    title="تعديل الدفع"
                  >
                    <PaymentIcon className="w-5 h-5" />
                  </button>

                  {/* زر إنشاء حساب الطالب - يظهر فقط للطلاب المدفوعين */}
                  {(student.PaidAmount || 0) > 0 && (
                    <button
                      onClick={() =>
                        handleCreateStudentAccount(
                          student.RegistrationId || student.registrationId
                        )
                      }
                      className="p-2 text-purple-600 hover:text-purple-800 hover:bg-purple-50 rounded-lg transition-all duration-200 border border-purple-200"
                      title="إنشاء حساب الطالب"
                    >
                      <User className="w-5 h-5" />
                    </button>
                  )}

                  {/* زر إصدار الشهادة - يظهر للكورسات المكتملة */}
                  {course?.status === "Completed" &&
                    (student.PaidAmount || 0) > 0 && (
                      <button
                        onClick={() =>
                          handleIssueCertificate(
                            student.RegistrationId || student.registrationId,
                            student.FullName || student.fullName || "غير محدد"
                          )
                        }
                        className="p-2 text-yellow-600 hover:text-yellow-800 hover:bg-yellow-50 rounded-lg transition-all duration-200 border border-yellow-200"
                        title="إصدار شهادة"
                      >
                        <GraduationCap className="w-5 h-5" />
                      </button>
                    )}

                  {/* زر إكمال */}
                  <button
                    onClick={() =>
                      handleUpdateEnrollmentStatus(
                        student.RegistrationId || student.registrationId,
                        "completed"
                      )
                    }
                    className="p-2 text-green-600 hover:text-green-800 hover:bg-green-50 rounded-lg transition-all duration-200 border border-green-200"
                    title="إكمال"
                  >
                    <CheckCircle className="w-5 h-5" />
                  </button>
                  
                  {/* زر انسحاب */}
                  <button
                    onClick={() =>
                      handleUpdateEnrollmentStatus(
                        student.RegistrationId || student.registrationId,
                        "dropped"
                      )
                    }
                    className="p-2 text-yellow-600 hover:text-yellow-800 hover:bg-yellow-50 rounded-lg transition-all duration-200 border border-yellow-200"
                    title="انسحاب"
                  >
                    <XCircle className="w-5 h-5" />
                  </button>
                  
                  {/* زر إلغاء التسجيل */}
                  <button
                    onClick={() => handleRemoveStudent(student.RegistrationId || student.registrationId)}
                    className="p-2 text-red-600 hover:text-red-800 hover:bg-red-50 rounded-lg transition-all duration-200 border border-red-200"
                    title="إلغاء التسجيل"
                  >
                    <UserMinus className="w-5 h-5" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-12">
            <Users className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              لا يوجد طلاب مسجلين
            </h3>
            <p className="text-gray-500">
              قم بإضافة طلاب للدورة من خلال زر "إضافة طالب"
            </p>
          </div>
        )}
      </div>

      {/* Enrollment Modal */}
      {enrollmentModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 max-w-lg w-full mx-4">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              إضافة طالب للدورة
            </h2>
            <select
              value={selectedStudentId}
              onChange={(e) => setSelectedStudentId(Number(e.target.value))}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent mb-4"
            >
              <option value={0}>اختر طالب</option>
              {availableStudents.map((student) => (
                <option key={student.id} value={student.id}>
                  {student.fullName || "غير محدد"}
                </option>
              ))}
            </select>
            <div className="flex justify-end space-x-2 rtl:space-x-reverse">
              <button
                onClick={() => setEnrollmentModalOpen(false)}
                className="px-4 py-2 text-gray-600 hover:text-gray-900 transition-colors"
              >
                إلغاء
              </button>
              <button
                onClick={handleEnrollStudent}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                إضافة
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Payment Modal */}
      {paymentModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 max-w-lg w-full mx-4">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              تعديل حالة الدفع
            </h2>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  المبلغ المدفوع
                </label>
                <input
                  type="number"
                  value={paymentData.paidAmount}
                  onChange={(e) =>
                    setPaymentData({
                      ...paymentData,
                      paidAmount:
                        e.target.value === "" ? 0 : Number(e.target.value),
                    })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  min="0"
                  step="0.01"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  طريقة الدفع
                </label>
                <select
                  value={paymentData.paymentMethod}
                  onChange={(e) =>
                    setPaymentData({
                      ...paymentData,
                      paymentMethod: e.target.value,
                    })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="Cash">نقدي</option>
                  <option value="InstaPay">انستا باي</option>
                  <option value="Fawry">فوري</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  ملاحظات
                </label>
                <textarea
                  value={paymentData.notes}
                  onChange={(e) =>
                    setPaymentData({ ...paymentData, notes: e.target.value })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  rows={3}
                  placeholder="ملاحظات إضافية..."
                />
              </div>
            </div>

            <div className="flex justify-end space-x-2 rtl:space-x-reverse mt-6">
              <button
                onClick={() => setPaymentModalOpen(false)}
                className="px-4 py-2 text-gray-600 hover:text-gray-900 transition-colors"
              >
                إلغاء
              </button>
              <button
                onClick={handleUpdatePayment}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                تحديث الدفع
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Student Details Modal */}
      {studentDetailsModalOpen && selectedStudent && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 max-w-lg w-full mx-4">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              بيانات الطالب
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="font-medium text-gray-700">اسم الطالب:</p>
                <p>{selectedStudent.FullName || selectedStudent.fullName || "غير محدد"}</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">رقم الهوية:</p>
                <p>{selectedStudent.idNumber || selectedStudent.nationalId || "30012345678901"}</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">رقم التسجيل:</p>
                <p>{selectedStudent.RegistrationId || selectedStudent.registrationId || "غير محدد"}</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">رقم الهاتف:</p>
                <p>{selectedStudent.Phone || selectedStudent.phone || "غير محدد"}</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">البريد الإلكتروني:</p>
                <p>{selectedStudent.Email || selectedStudent.email || "غير محدد"}</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">تاريخ التسجيل:</p>
                <p>{selectedStudent.registrationDate ? new Date(selectedStudent.registrationDate).toLocaleDateString("ar-EG") : new Date().toLocaleDateString("ar-EG")}</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">حالة التسجيل:</p>
                <p>{selectedStudent.status || selectedStudent.PaymentStatusArabic || "نشط"}</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">المبلغ المدفوع:</p>
                <p>{selectedStudent.PaidAmount || selectedStudent.paidAmount || 0} جنيه</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">طريقة الدفع:</p>
                <p>{selectedStudent.PaymentMethodArabic || selectedStudent.paymentMethodArabic || "نقدي"}</p>
              </div>
              <div>
                <p className="font-medium text-gray-700">الملاحظات:</p>
                <p>{selectedStudent.notes || selectedStudent.Notes || "لا توجد ملاحظات"}</p>
              </div>
            </div>
            <div className="flex justify-end space-x-2 rtl:space-x-reverse mt-6">
              <button
                onClick={() => setStudentDetailsModalOpen(false)}
                className="px-4 py-2 text-gray-600 hover:text-gray-900 transition-colors"
              >
                إغلاق
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CourseDetails;
