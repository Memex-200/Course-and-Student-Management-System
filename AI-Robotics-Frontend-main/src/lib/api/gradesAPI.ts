import axios from "./axios";

export interface Student {
  id: number;
  fullName: string;
}

export interface Course {
  id: number;
  name: string;
}

export interface Grade {
  id: number;
  studentId: number;
  studentName: string;
  courseId: number;
  courseName: string;
  grade: number;
  notes: string;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy?: string;
}

export interface GradeFormData {
  studentId: string;
  courseId: string;
  grade: string;
  notes: string;
}

class GradesAPI {
  // Get all grades
  async getGrades(params?: {
    studentId?: number;
    courseId?: number;
  }): Promise<Grade[]> {
    try {
      console.log("[gradesAPI] GET /studentgrades params:", params);
      const response = await axios.get("/studentgrades", { params });
      console.log("[gradesAPI] GET /studentgrades response:", response.data);
      if (response.data?.success) {
        return response.data.data || [];
      }
      console.warn("[gradesAPI] Unexpected response shape for getGrades");
      return [];
    } catch (error) {
      console.error(
        "[gradesAPI] Error fetching grades:",
        (error as any)?.response?.data || (error as any)?.message
      );
      return [];
    }
  }

  // Get alternative grades endpoint
  async getGradesAlternative(): Promise<Grade[]> {
    try {
      console.log("[gradesAPI] GET /studentgrades/all (alternative)");
      const response = await axios.get("/studentgrades/all");
      console.log(
        "[gradesAPI] GET /studentgrades/all response:",
        response.data
      );
      if (response.data?.success) {
        return response.data.data || [];
      }
      console.warn(
        "[gradesAPI] Unexpected response shape for getGradesAlternative"
      );
      return [];
    } catch (error) {
      console.error(
        "[gradesAPI] Error fetching grades from alternative endpoint:",
        (error as any)?.response?.data || (error as any)?.message
      );
      return [];
    }
  }

  // Get all students
  async getStudents(): Promise<Student[]> {
    try {
      console.log("[gradesAPI] GET /students");
      const response = await axios.get("/students");
      console.log("[gradesAPI] /students response:", response.data);
      if (response.data?.success) {
        return response.data.data || [];
      }
      console.warn("[gradesAPI] Unexpected response shape for getStudents");
      return [];
    } catch (error) {
      console.error(
        "[gradesAPI] Error fetching students:",
        (error as any)?.response?.data || (error as any)?.message
      );
      // Try alternative endpoint
      try {
        console.log("[gradesAPI] Fallback GET /students/all");
        const altResponse = await axios.get("/students/all");
        console.log("[gradesAPI] /students/all response:", altResponse.data);
        if (altResponse.data?.success) {
          return altResponse.data.data || [];
        }
        return [];
      } catch (altError) {
        console.error(
          "[gradesAPI] Error fetching students from alternative endpoint:",
          (altError as any)?.response?.data || (altError as any)?.message
        );
        return [];
      }
    }
  }

  // Get alternative students endpoint
  async getStudentsAlternative(): Promise<Student[]> {
    try {
      const response = await axios.get("/students/all");
      return response.data || [];
    } catch (error) {
      console.error(
        "Error fetching students from alternative endpoint:",
        error
      );
      return [];
    }
  }

  // Get all courses
  async getCourses(): Promise<Course[]> {
    try {
      console.log("[gradesAPI] GET /courses");
      const response = await axios.get("/courses");
      console.log("[gradesAPI] /courses response:", response.data);
      if (response.data?.success) {
        return response.data.data || [];
      }
      console.warn("[gradesAPI] Unexpected response shape for getCourses");
      return [];
    } catch (error) {
      console.error(
        "[gradesAPI] Error fetching courses:",
        (error as any)?.response?.data || (error as any)?.message
      );
      // Try alternative endpoint
      try {
        console.log("[gradesAPI] Fallback GET /courses/all");
        const altResponse = await axios.get("/courses/all");
        console.log("[gradesAPI] /courses/all response:", altResponse.data);
        if (altResponse.data?.success) {
          return altResponse.data.data || [];
        }
        return [];
      } catch (altError) {
        console.error(
          "[gradesAPI] Error fetching courses from alternative endpoint:",
          (altError as any)?.response?.data || (altError as any)?.message
        );
        return [];
      }
    }
  }

  // Get alternative courses endpoint
  async getCoursesAlternative(): Promise<Course[]> {
    try {
      const response = await axios.get("/courses/all");
      return response.data || [];
    } catch (error) {
      console.error("Error fetching courses from alternative endpoint:", error);
      return [];
    }
  }

  // Create new grade
  async createGrade(gradeData: {
    studentId: number;
    courseId: number;
    grade: number;
    notes: string;
  }): Promise<{ success: boolean; message: string }> {
    try {
      console.log("[gradesAPI] POST /studentgrades payload:", gradeData);
      const response = await axios.post("/studentgrades", gradeData);
      console.log("[gradesAPI] POST /studentgrades response:", response.data);

      return {
        success: !!response.data?.success,
        message: response.data?.message || "تم إضافة الدرجة بنجاح",
      };
    } catch (error: any) {
      console.error(
        "[gradesAPI] Error creating grade:",
        error.response?.data || error.message
      );
      console.error("[gradesAPI] Error status:", error.response?.status);

      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.error ||
        "حدث خطأ في حفظ الدرجة";

      return {
        success: false,
        message: errorMessage,
      };
    }
  }

  // Update grade
  async updateGrade(
    gradeId: number,
    gradeData: {
      grade: number;
      notes: string;
    }
  ): Promise<{ success: boolean; message: string }> {
    try {
      console.log(
        `[gradesAPI] PUT /studentgrades/${gradeId} payload:`,
        gradeData
      );
      const response = await axios.put(`/studentgrades/${gradeId}`, gradeData);
      console.log(
        `[gradesAPI] PUT /studentgrades/${gradeId} response:`,
        response.data
      );
      return {
        success: !!response.data?.success,
        message: response.data?.message || "تم تحديث الدرجة بنجاح",
      };
    } catch (error: any) {
      console.error(
        `[gradesAPI] Error updating grade ${gradeId}:`,
        error.response?.data || error.message
      );
      return {
        success: false,
        message: error.response?.data?.message || "حدث خطأ في تحديث الدرجة",
      };
    }
  }

  // Delete grade
  async deleteGrade(
    gradeId: number
  ): Promise<{ success: boolean; message: string }> {
    try {
      console.log(`[gradesAPI] DELETE /studentgrades/${gradeId}`);
      const response = await axios.delete(`/studentgrades/${gradeId}`);
      console.log(
        `[gradesAPI] DELETE /studentgrades/${gradeId} response:`,
        response.data
      );
      return {
        success: !!response.data?.success,
        message: response.data?.message || "تم حذف الدرجة بنجاح",
      };
    } catch (error: any) {
      console.error(
        `[gradesAPI] Error deleting grade ${gradeId}:`,
        error.response?.data || error.message
      );
      return {
        success: false,
        message: error.response?.data?.message || "حدث خطأ في حذف الدرجة",
      };
    }
  }

  // Get student grades
  async getStudentGrades(studentId: number): Promise<Grade[]> {
    try {
      console.log(`[gradesAPI] GET /studentgrades/student/${studentId}`);
      const response = await axios.get(`/studentgrades/student/${studentId}`);
      console.log(
        `[gradesAPI] GET /studentgrades/student/${studentId} response:`,
        response.data
      );
      if (response.data?.success) {
        return response.data.data || [];
      }
      console.warn(
        "[gradesAPI] Unexpected response shape for getStudentGrades"
      );
      return [];
    } catch (error) {
      console.error(
        `[gradesAPI] Error fetching student grades for ${studentId}:`,
        (error as any)?.response?.data || (error as any)?.message
      );
      return [];
    }
  }

  // Get course grades
  async getCourseGrades(courseId: number): Promise<Grade[]> {
    try {
      console.log(`[gradesAPI] GET /studentgrades?courseId=${courseId}`);
      const response = await axios.get("/studentgrades", {
        params: { courseId },
      });
      console.log(
        `[gradesAPI] GET /studentgrades by course response:`,
        response.data
      );
      if (response.data?.success) {
        return response.data.data || [];
      }
      console.warn("[gradesAPI] Unexpected response shape for getCourseGrades");
      return [];
    } catch (error) {
      console.error(
        `[gradesAPI] Error fetching course grades for ${courseId}:`,
        (error as any)?.response?.data || (error as any)?.message
      );
      return [];
    }
  }

  // Validate grade
  validateGrade(grade: number): { isValid: boolean; message?: string } {
    if (isNaN(grade) || grade < 0 || grade > 100) {
      return {
        isValid: false,
        message: "الدرجة يجب أن تكون بين 0 و 100",
      };
    }
    return { isValid: true };
  }

  // Get grade color and text
  getGradeInfo(grade: number): { color: string; text: string } {
    if (grade >= 90)
      return { color: "text-green-600 bg-green-100", text: "ممتاز" };
    if (grade >= 80)
      return { color: "text-blue-600 bg-blue-100", text: "جيد جداً" };
    if (grade >= 70)
      return { color: "text-yellow-600 bg-yellow-100", text: "جيد" };
    if (grade >= 60)
      return { color: "text-orange-600 bg-orange-100", text: "مقبول" };
    return { color: "text-red-600 bg-red-100", text: "ضعيف" };
  }
}

export const gradesAPI = new GradesAPI();
export default gradesAPI;
