export { default as coursesAPI } from './coursesAPI';

// إعادة تصدير APIs من الملف الرئيسي
export { 
  authAPI, 
  studentsAPI, 
  coursesAPI as apiCourses, 
  reportsAPI, 
  expensesAPI, 
  workspaceAPI, 
  paymentsAPI,
  cafeteriaAPI
} from '../api'; 