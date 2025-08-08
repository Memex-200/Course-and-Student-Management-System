import React, { useEffect, useState } from "react";
import { coursesAPI } from '../../lib/api';
import { attendanceAPI } from '../../lib/api';
import toast from 'react-hot-toast';

interface CourseOption {
  id: number;
  name: string;
  sessionsCount: number;
  isActive: boolean;
}

interface Student {
  id: number;
  fullName: string;
  phone: string;
}

interface AttendanceRecord {
  id: number;
  sessionDate: string;
  status: string; // 'Present' | 'Absent'
  student: { id: number; fullName: string; };
}

const AttendanceManagement: React.FC = () => {
  const [courses, setCourses] = useState<CourseOption[]>([]);
  const [selectedCourseId, setSelectedCourseId] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [students, setStudents] = useState<Student[]>([]);
  const [attendanceRecords, setAttendanceRecords] = useState<AttendanceRecord[]>([]);
  const [sessions, setSessions] = useState<string[]>([]); // session dates as ISO strings
  const [tableLoading, setTableLoading] = useState(false);
  const [tableError, setTableError] = useState<string | null>(null);
  const [attendanceState, setAttendanceState] = useState<{ [studentId: number]: { [session: string]: boolean } }>({});
  const [saving, setSaving] = useState(false);
  const [selectedCourse, setSelectedCourse] = useState<CourseOption | null>(null);

  useEffect(() => {
    fetchCourses();
  }, []);

  // عند تغيير الكورس المختار، احفظ بيانات الكورس المختار
  useEffect(() => {
    if (selectedCourseId) {
      const course = courses.find(c => c.id === selectedCourseId) || null;
      setSelectedCourse(course);
      fetchAttendanceTable(selectedCourseId, course);
    } else {
      setSelectedCourse(null);
      setStudents([]);
      setAttendanceRecords([]);
      setSessions([]);
      setAttendanceState({});
    }
  }, [selectedCourseId, courses]);

  const fetchCourses = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await coursesAPI.getAll();
      if (res.data.success) {
        setCourses(res.data.data.filter((c: any) => c.isActive));
      } else {
        setError(res.data.message || 'فشل في جلب الكورسات');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'حدث خطأ في جلب الكورسات');
    } finally {
      setLoading(false);
    }
  };

  // عدل fetchAttendanceTable ليأخذ بيانات الكورس
  const fetchAttendanceTable = async (courseId: number, course: CourseOption | null) => {
    setTableLoading(true);
    setTableError(null);
    try {
      const res = await attendanceAPI.getCourseAttendance(courseId);
      const data = res.data;
      if (data && data.registeredStudents) {
        // تطبيع بيانات الطلاب
        const students = (data.registeredStudents || []).map((s: any) => ({
          id: s.id ?? s.Id,
          fullName: s.fullName ?? s.FullName,
          phone: s.phone ?? s.Phone,
          registrationDate: s.registrationDate ?? s.RegistrationDate,
        }));
        setStudents(students);
        // استخراج تواريخ الجلسات الفريدة من attendanceRecords
        let uniqueSessions: string[] = [];
        if (data.attendanceRecords && data.attendanceRecords.length > 0) {
          uniqueSessions = Array.from(new Set(data.attendanceRecords.map((a: any) => a.sessionDate)));
          uniqueSessions.sort();
        } else if (course && course.sessionsCount > 0) {
          // إذا لم توجد سجلات حضور، أنشئ تواريخ افتراضية (جلسة 1 ...)
          uniqueSessions = Array.from({ length: course.sessionsCount }, (_, i) => `جلسة ${i + 1}`);
        }
        setSessions(uniqueSessions);
        setAttendanceRecords(data.attendanceRecords || []);
        // بناء attendanceState: لكل طالب ولكل جلسة، إذا وجد سجل حضور، استخدمه، وإلا اعتبره غائب
        const state: { [studentId: number]: { [session: string]: boolean } } = {};
        students.forEach((student: any) => {
          state[student.id] = {};
          uniqueSessions.forEach((session: string) => {
            if (data.attendanceRecords && data.attendanceRecords.length > 0) {
              const record = data.attendanceRecords.find((a: any) => a.studentId === student.id && a.sessionDate === session);
              state[student.id][session] = record ? record.status === 'Present' : false;
            } else {
              state[student.id][session] = false;
            }
          });
        });
        setAttendanceState(state);
        setTableError(null);
      } else {
        // حتى لو لم توجد بيانات حضور، أنشئ الأعمدة إذا كان sessionsCount > 0
        if (course && course.sessionsCount > 0) {
          const uniqueSessions = Array.from({ length: course.sessionsCount }, (_, i) => `جلسة ${i + 1}`);
          setSessions(uniqueSessions);
          setAttendanceState({});
          setTableError(null);
        } else {
          setTableError('لا توجد بيانات حضور متاحة لهذا الكورس');
        }
      }
    } catch (err: any) {
      setTableError(err.response?.data?.message || 'حدث خطأ في جلب بيانات الحضور');
    } finally {
      setTableLoading(false);
    }
  };

  const handleCheckboxChange = (studentId: number, session: string) => {
    setAttendanceState(prev => ({
      ...prev,
      [studentId]: {
        ...prev[studentId],
        [session]: !prev[studentId][session]
      }
    }));
  };

  const handleSave = async () => {
    if (!selectedCourseId || sessions.length === 0) return;
    setSaving(true);
    try {
      // احفظ فقط الجلسات التي هي تواريخ فعلية (وليس 'جلسة 1' ...)
      const realSessions = sessions.filter(s => /^\d{4}-\d{2}-\d{2}T/.test(s));
      if (realSessions.length === 0) {
        toast.error('لا توجد جلسات فعلية يمكن حفظها. يجب إنشاء جلسة حضور أولاً.');
        setSaving(false);
        return;
      }
      for (const session of realSessions) {
        // استخدم sessionDate الفعلي من attendanceRecords إذا وجد
        const record = attendanceRecords.find(r => r.sessionDate === session);
        const sessionDateToSend = record ? record.sessionDate : session;
        const studentAttendances = students.map(student => ({
          studentId: student.id,
          status: attendanceState[student.id]?.[session] ? 1 : 0, // 1 = Present, 0 = Absent
        }));
        await attendanceAPI.updateAttendanceSession(selectedCourseId, {
          sessionDate: sessionDateToSend,
          studentAttendances,
        });
      }
      toast.success('تم حفظ الحضور بنجاح');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'حدث خطأ أثناء حفظ الحضور');
    } finally {
      setSaving(false);
    }
  };

  // دالة إنشاء جلسة حضور جديدة بتاريخ اليوم
  const handleCreateSession = async () => {
    if (!selectedCourseId || students.length === 0) {
      toast.error('يجب اختيار كورس ووجود طلاب مسجلين أولاً');
      return;
    }
    const now = new Date();
    const sessionDate = now.toISOString();
    const studentAttendances = students.map(student => ({
      studentId: student.id,
      status: 0, // 0 = Absent
    }));
    try {
      await attendanceAPI.createAttendanceSession(selectedCourseId, {
        sessionDate,
        studentAttendances,
      });
      toast.success('تم إنشاء جلسة الحضور بنجاح');
      // أعد تحميل بيانات الحضور
      fetchAttendanceTable(selectedCourseId, selectedCourse);
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'حدث خطأ أثناء إنشاء جلسة الحضور');
    }
  };

  // حساب ملخص الحضور
  const totalStudents = students.length;
  const totalSessions = sessions.length;
  const totalCells = totalStudents * totalSessions;
  const totalPresent = Object.values(attendanceState).reduce((sum, studentSessions) => sum + Object.values(studentSessions).filter(Boolean).length, 0);
  const overallAttendanceRate = totalCells > 0 ? Math.round((totalPresent / totalCells) * 100) : 0;

  const getStudentAttendanceRate = (studentId: number) => {
    if (!attendanceState[studentId]) return 0;
    const presentCount = Object.values(attendanceState[studentId]).filter(Boolean).length;
    return totalSessions > 0 ? Math.round((presentCount / totalSessions) * 100) : 0;
  };

  return (
    <div className="p-8">
      <h1 className="text-3xl font-bold mb-6 text-primary-600">متابعة الحضور</h1>
      <p className="text-gray-600 mb-6">اختر الكورس لمتابعة حضور وغياب الطلاب.</p>

      {loading ? (
        <div className="text-center text-primary-500">جاري تحميل الكورسات...</div>
      ) : error ? (
        <div className="text-center text-red-500">{error}</div>
      ) : (
        <div className="mb-8">
          <label className="block mb-2 text-lg font-semibold text-gray-700">اختر الكورس:</label>
          <select
            className="w-full p-3 rounded-lg border border-primary-300 focus:outline-none focus:ring-2 focus:ring-primary-400"
            value={selectedCourseId ?? ''}
            onChange={e => setSelectedCourseId(Number(e.target.value) || null)}
          >
            <option value="">-- اختر كورس --</option>
            {courses.map(course => (
              <option key={course.id} value={course.id}>
                {course.name} (عدد الجلسات: {course.sessionsCount})
              </option>
            ))}
          </select>
        </div>
      )}

      {selectedCourseId && (
        <div className="mt-8">
          {/* زر إضافة جلسة حضور اليوم */}
          <div className="mb-4 flex justify-end">
            <button
              onClick={handleCreateSession}
              className="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-6 rounded-lg shadow transition-colors"
            >
              إضافة جلسة حضور اليوم
            </button>
          </div>
          {/* ملخص الحضور */}
          {!tableLoading && !tableError && students.length > 0 && sessions.length > 0 && (
            <div className="mb-6 flex flex-wrap gap-6 items-center bg-primary-50 border border-primary-200 rounded-lg p-4">
              <div className="text-primary-800 font-bold">عدد الطلاب: <span className="text-primary-600">{totalStudents}</span></div>
              <div className="text-primary-800 font-bold">عدد الجلسات: <span className="text-primary-600">{totalSessions}</span></div>
              <div className="text-primary-800 font-bold">إجمالي الحضور: <span className="text-green-600">{totalPresent}</span></div>
              <div className="text-primary-800 font-bold">نسبة الحضور الكلية: <span className="text-blue-600">{overallAttendanceRate}%</span></div>
            </div>
          )}
          {tableLoading ? (
            <div className="text-center text-primary-500">جاري تحميل جدول الحضور...</div>
          ) : tableError ? (
            <div className="text-center text-red-500">{tableError}</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full border border-primary-200 rounded-lg">
                <thead>
                  <tr>
                    <th className="p-2 border-b bg-primary-100 text-primary-800">الطالب</th>
                    {sessions.map((session, idx) => (
                      <th key={session} className="p-2 border-b bg-primary-100 text-primary-800">
                        {session.startsWith('جلسة') ? session : `الجلسة ${idx + 1}`}
                        {session.startsWith('جلسة') ? null : <div className="text-xs text-gray-500">{new Date(session).toLocaleDateString('ar-EG')}</div>}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {students.map(student => (
                    <tr key={student.id}>
                      <td className="p-2 border-b font-semibold text-primary-900">{student.fullName}
                        <span className="ml-2 text-xs text-blue-600 font-bold">{getStudentAttendanceRate(student.id)}%</span>
                      </td>
                      {sessions.map(session => (
                        <td key={session} className="p-2 border-b text-center">
                          <input
                            type="checkbox"
                            checked={attendanceState[student.id]?.[session] || false}
                            onChange={() => handleCheckboxChange(student.id, session)}
                            className="w-5 h-5 text-primary-600 rounded focus:ring-primary-500"
                          />
                        </td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          {!tableLoading && !tableError && students.length > 0 && sessions.length > 0 && (
            <div className="mt-6 flex justify-end">
              <button
                onClick={handleSave}
                disabled={saving}
                className={`bg-primary-600 hover:bg-primary-700 text-white font-bold py-3 px-8 rounded-xl transition-colors ${saving ? 'opacity-60 cursor-not-allowed' : ''}`}
              >
                {saving ? 'جاري الحفظ...' : 'حفظ الحضور'}
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default AttendanceManagement; 