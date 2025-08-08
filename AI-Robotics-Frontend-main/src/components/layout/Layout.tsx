import React from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Navbar from './Navbar';

const Layout: React.FC = () => {
  return (
    <div className="flex h-screen bg-gray-50 rtl">
      {/* Main content wrapper */}
      <div className="flex-1 flex flex-col mr-64"> {/* Add margin to account for fixed sidebar */}
        {/* Navbar with higher z-index */}
        <div className="sticky top-0 z-30">
          <Navbar />
        </div>
        
        {/* Main content area */}
        <main className="flex-1 overflow-auto relative z-10 p-6">
          <Outlet />
        </main>
      </div>

      {/* Fixed sidebar with high z-index */}
      <div className="fixed right-0 top-0 z-40 h-full">
        <Sidebar />
      </div>
    </div>
  );
};

export default Layout;