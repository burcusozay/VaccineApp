// src/js/api-client.js
import axios from "axios";

export const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:44395/api";

// AccessToken’i localStorage’dan çekiyoruz. Dilersen props/context ile de verebilirsin.
function getAccessToken() {
    // return localStorage.getItem("accessToken");
    const tokenDataString = sessionStorage.getItem("accessToken");
    if (!tokenDataString) return null;

    try {
        const tokenData = JSON.parse(tokenDataString);
        return tokenData?.accessToken; // Token objesinin içindeki accessToken'ı döndür
    } catch (e) {
        console.error("Access token parse edilemedi.", e);
        return null;
    }
}

// Axios örneği oluştur
const api = axios.create({
    baseURL: BASE_URL,
    headers: {
        "Content-Type": "application/json",
    },
});

// Request interceptor (token ekleme)
api.interceptors.request.use((config) => {
    const token = getAccessToken();
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Response interceptor (hata yönetimi, snakebar tetiklenmesi üstten)
let onErrorCallback = null;
export function setApiErrorCallback(cb) {
    onErrorCallback = cb;
}

api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (onErrorCallback) {
            const message =
                error.response?.data ||
                error.response?.data?.message ||
                error.message ||
                "Bilinmeyen bir hata oluştu";
            onErrorCallback(message);
        }
        return Promise.reject(error);
    }
);

// **Generic POST** metodu
export async function postGeneric(controllerName, methodName, data = {}) {
    // Örn: /User/Login gibi endpointler için
    const url = `/${controllerName}/${methodName}`;
    const response = await api.post(url, data);
    return response.data;
}
