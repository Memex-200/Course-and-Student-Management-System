// API Configuration
// Default BASE_URL to "/api" when VITE_API_URL is not provided so Vite proxy works in dev
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_URL || "/api",
  TIMEOUT: 10000,
};

// Environment check
export const isDevelopment = import.meta.env.DEV;
export const isProduction = import.meta.env.PROD;
