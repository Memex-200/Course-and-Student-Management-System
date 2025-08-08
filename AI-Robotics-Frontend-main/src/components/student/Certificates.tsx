import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { Award, Download, Calendar, Bot, Star } from 'lucide-react';

const Certificates: React.FC = () => {
  const { user } = useAuth();

  // Mock data (will be replaced with API calls)
  const certificates = [
    {
      id: 1,
      name: 'شهادة إتمام دورة Arduino',
      courseName: 'مقدمة في الروبوتات',
      instructor: 'د. أحمد محمد',
      date: '2024-02-15',
      grade: 'امتياز',
      skills: [
        'برمجة Arduino',
        'تصميم الدوائر الإلكترونية',
        'برمجة المستشعرات',
        'التحكم في المحركات'
      ]
    },
    {
      id: 2,
      name: 'شهادة Python للمبتدئين',
      courseName: 'البرمجة بلغة Python',
      instructor: 'م. سارة أحمد',
      date: '2024-01-20',
      grade: 'جيد جداً',
      skills: [
        'أساسيات Python',
        'البرمجة كائنية التوجه',
        'التعامل مع قواعد البيانات',
        'بناء التطبيقات'
      ]
    }
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-secondary-900 via-secondary-800 to-primary-900 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-white mb-2">الشهادات</h1>
          <p className="text-gray-400">شهادات إتمام الدورات التدريبية</p>
        </div>

        {/* Certificates Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {certificates.map(cert => (
            <div
              key={cert.id}
              className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-6 border border-primary-500/20"
            >
              {/* Certificate Header */}
              <div className="flex items-start justify-between mb-6">
                <div>
                  <h2 className="text-2xl font-bold text-white mb-2">{cert.name}</h2>
                  <p className="text-gray-300">{cert.courseName}</p>
                </div>
                <Award className="w-12 h-12 text-warning-400" />
              </div>

              {/* Certificate Details */}
              <div className="space-y-4 mb-6">
                <div className="flex items-center space-x-3 rtl:space-x-reverse">
                  <Bot className="w-5 h-5 text-primary-400" />
                  <div>
                    <p className="text-gray-400 text-sm">المدرب</p>
                    <p className="text-white">{cert.instructor}</p>
                  </div>
                </div>

                <div className="flex items-center space-x-3 rtl:space-x-reverse">
                  <Calendar className="w-5 h-5 text-primary-400" />
                  <div>
                    <p className="text-gray-400 text-sm">تاريخ الإصدار</p>
                    <p className="text-white">
                      {new Date(cert.date).toLocaleDateString('ar-EG')}
                    </p>
                  </div>
                </div>

                <div className="flex items-center space-x-3 rtl:space-x-reverse">
                  <Star className="w-5 h-5 text-warning-400" />
                  <div>
                    <p className="text-gray-400 text-sm">التقدير</p>
                    <p className="text-warning-400 font-semibold">{cert.grade}</p>
                  </div>
                </div>
              </div>

              {/* Skills */}
              <div className="mb-6">
                <h3 className="text-lg font-semibold text-white mb-3">المهارات المكتسبة</h3>
                <div className="flex flex-wrap gap-2">
                  {cert.skills.map((skill, index) => (
                    <span
                      key={index}
                      className="px-3 py-1 bg-primary-500/20 text-primary-300 rounded-full text-sm"
                    >
                      {skill}
                    </span>
                  ))}
                </div>
              </div>

              {/* Actions */}
              <div className="flex justify-between items-center">
                <button className="bg-warning-500 hover:bg-warning-600 text-white rounded-xl px-6 py-2 transition-colors flex items-center">
                  <Download className="w-4 h-4 ml-2" />
                  تحميل الشهادة
                </button>
                
                <button className="text-primary-400 hover:text-primary-300 transition-colors">
                  عرض التفاصيل
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* No Certificates */}
        {certificates.length === 0 && (
          <div className="bg-gradient-to-br from-secondary-800/50 to-secondary-900/50 backdrop-blur-xl rounded-2xl p-8 border border-primary-500/20 text-center">
            <Award className="w-16 h-16 text-primary-400 mx-auto mb-4" />
            <h3 className="text-xl font-semibold text-white mb-2">لا توجد شهادات بعد</h3>
            <p className="text-gray-400">أكمل الكورسات المسجل فيها للحصول على الشهادات</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default Certificates; 