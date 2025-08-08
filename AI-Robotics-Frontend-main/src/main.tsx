import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'

// إضافة CSS للـ toast notifications
const toastStyles = `
  .Toaster__toast--success {
    background-color: #10B981 !important;
    color: #FFFFFF !important;
  }
  .Toaster__toast--error {
    background-color: #EF4444 !important;
    color: #FFFFFF !important;
  }
  .Toaster__toast {
    font-size: 16px !important;
    font-weight: 500 !important;
    direction: rtl !important;
  }
`;

// إضافة الـ styles للصفحة
const styleElement = document.createElement('style');
styleElement.textContent = toastStyles;
document.head.appendChild(styleElement);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
