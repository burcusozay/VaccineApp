import axios from "axios";

export const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:44395/api";
const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});



// Token otomatik eklenmesi
api.interceptors.request.use((config) => {
  // Sadece /login'e token eklenmez
  if (!config.url?.toLowerCase().includes("/login")) {
    const tokenString = sessionStorage.getItem("accessToken");
    if (tokenString) {
      const { accessToken } = JSON.parse(tokenString);
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
  }
  return config;
});

export default api;
