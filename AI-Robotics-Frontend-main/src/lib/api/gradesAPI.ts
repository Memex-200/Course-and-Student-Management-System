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
      const response = await axios.get("/studentgrades", { params });
      if (response.data.success) {
        return response.data.data || [];
      }
      return [];
    } catch (error) {
      console.error("Error fetching grades:", error);
      // Try alternative endpoint
      try {
        const altResponse = await axios.get("/studentgrades/all");
        return altResponse.data || [];
      } catch (altError) {
        console.error(
          "Error fetching grades from alternative endpoint:",
          altError
        );
        return [];
      }
    }
  }

  // Get alternative grades endpoint
  async getGradesAlternative(): Promise<Grade[]> {
    try {
      const response = await axios.get("/studentgrades/all");
      return response.data || [];
    } catch (error) {
      console.error("Error fetching grades from alternative endpoint:", error);
      return [];
    }
  }

  // Get all students
  async getStudents(): Promise<Student[]> {
    try {
      const response = await axios.get("/students");
      if (response.data.success) {
        return response.data.data || [];
      }
      return [];
    } catch (error) {
      console.error("Error fetching students:", error);
      // Try alternative endpoint
      try {
        const altResponse = await axios.get("/students/all");
        return altResponse.data || [];
      } catch (altError) {
        console.error(
          "Error fetching students from alternative endpoint:",
          altError
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
      const response = await axios.get("/courses");
      if (response.data.success) {
        return response.data.data || [];
      }
      return [];
    } catch (error) {
      console.error("Error fetching courses:", error);
      // Try alternative endpoint
      try {
        const altResponse = await axios.get("/courses/all");
        return altResponse.data || [];
      } catch (altError) {
        console.error(
          "Error fetching courses from alternative endpoint:",
          altError
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
      const response = await axios.post("/studentgrades", gradeData);
      return {
        success: true,
        message: response.data.message || "تم إضافة الدرجة بنجاح",
      };
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || "حدث خطأ في حفظ الدرجة",
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
      const response = await axios.put(`/studentgrades/${gradeId}`, gradeData);
      return {
        success: true,
        message: response.data.message || "تم تحديث الدرجة بنجاح",
      };
    } catch (error: any) {
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
      const response = await axios.delete(`/studentgrades/${gradeId}`);
      return {
        success: true,
        message: response.data.message || "تم حذف الدرجة بنجاح",
      };
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || "حدث خطأ في حذف الدرجة",
      };
    }
  }

  // Get student grades
  async getStudentGrades(studentId: number): Promise<Grade[]> {
    try {
      const response = await axios.get(`/studentgrades/student/${studentId}`);
      return response.data || [];
    } catch (error) {
      console.error("Error fetching student grades:", error);
      return [];
    }
  }

  // Get course grades
  async getCourseGrades(courseId: number): Promise<Grade[]> {
    try {
      const response = await axios.get("/studentgrades", {
        params: { courseId },
      });
      if (response.data.success) {
        return response.data.data || [];
      }
      return [];
    } catch (error) {
      console.error("Error fetching course grades:", error);
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
