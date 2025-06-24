import axios from "axios";

export const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:44395/api";

// Helper fonksiyonları: Token verisini sessionStorage'dan güvenli bir şekilde alır.
function getTokenData() {
    const tokenDataString = sessionStorage.getItem("accessToken");
    if (!tokenDataString) return null;
    try {
        return JSON.parse(tokenDataString);
    } catch (e) {
        console.error("Token verisi parse edilemedi.", e);
        return null;
    }
}

function getAccessToken() {
    return getTokenData()?.accessToken;
}

function getRefreshToken() {
    return getTokenData()?.refreshToken;
}

// ===================================================================
// API İstemcileri (Public ve Private)
// ===================================================================
const publicApi = axios.create({
    baseURL: BASE_URL,
    headers: { "Content-Type": "application/json" },
});

const privateApi = axios.create({
    baseURL: BASE_URL,
    headers: { "Content-Type": "application/json" },
});

// ===================================================================
// Private API Interceptor'ları (Token Ekleme ve Yenileme Mantığı)
// ===================================================================

// 1. İstek Interceptor'ı: Her isteğe token ekler.
privateApi.interceptors.request.use(
    (config) => {
        const token = getAccessToken();
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// 2. Yanıt Interceptor'ı: 401 hatasını yakalar ve token yenilemeyi dener.
privateApi.interceptors.response.use(
    (response) => response, // Başarılı yanıtları doğrudan döndür
    async (error) => {
        const originalRequest = error.config;
        
        // Eğer hata 401 ise ve bu istek daha önce denenmediyse
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true; // İsteği 'denendi' olarak işaretle (sonsuz döngüyü önler)
            
            try {
                const rt = getRefreshToken();
                if (!rt) {
                    console.error("Refresh token bulunamadı. Oturum sonlandırılıyor.");
                    sessionStorage.removeItem("accessToken");
                    window.location.href = '/login';
                    return Promise.reject(error);
                }

                console.log("Access token süresi doldu. Yenisi isteniyor...");
                
                // Token yenileme isteği, token gerektirmediği için publicApi ile yapılır.
                const response = await publicApi.post('/Account/RefreshToken', { refreshToken: rt });
                const newTokens = response.data;

                // Yeni token'ları kaydet
                sessionStorage.setItem("accessToken", JSON.stringify(newTokens));
                
                console.log("Yeni token alındı. Orijinal istek tekrarlanıyor.");
                
                // Orijinal isteğin başlığını yeni token ile güncelle
                originalRequest.headers.Authorization = `Bearer ${newTokens.accessToken}`;
                
                // Orijinal isteği yeni token ile tekrar gönder
                return privateApi(originalRequest);

            } catch (_error) {
                // Refresh token da geçersizse veya başka bir hata oluşursa
                console.error("Refresh token ile yeni token alınamadı.", _error);
                sessionStorage.removeItem("accessToken");
                window.location.href = '/login';
                return Promise.reject(_error);
            }
        }
        
        // Diğer tüm hataları reddet
        return Promise.reject(error);
    }
);


// ===================================================================
// Genel Hata Yönetimi (Snackbar için)
// ===================================================================
let onErrorCallback = null;
export function setApiErrorCallback(cb) {
    onErrorCallback = cb;
}

const setupErrorInterceptor = (instance) => {
    instance.interceptors.response.use(
        (response) => response,
        (error) => {
            if (axios.isCancel(error) || error.config._retry) {
                return Promise.reject(error);
            }
            if (onErrorCallback) {
                let errorMessage = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";

                // EĞER HATA 400 VE FLUENTVALIDATION HATASI İSE:
                if (error.response?.status === 400 && error.response.data?.errors) {
                    const validationErrors = error.response.data.errors;
                    // Tüm hata mesajlarını birleştir
                    errorMessage = Object.values(validationErrors).flat().join(' ');
                } 
                // Diğer genel hatalar için
                else if (error.response?.data) {
                    if (typeof error.response.data === 'object' && error.response.data.title) {
                        errorMessage = error.response.data.title;
                    } else if (typeof error.response.data.message === 'string') {
                        errorMessage = error.response.data.message;
                    } else if (typeof error.response.data === 'string') {
                        errorMessage = error.response.data;
                    }
                } else if (error.message) {
                    errorMessage = error.message;
                }
                
                onErrorCallback(errorMessage);
            }
            return Promise.reject(error);
        }
    );
};

setupErrorInterceptor(publicApi);
setupErrorInterceptor(privateApi); // Bu, refresh sonrası hataları yakalar


// ===================================================================
// DIŞARI AKTARILAN API FONKSİYONLARI
// ===================================================================

export async function loginUser(credentials) {
    const response = await publicApi.post("/Account/Login", credentials);
    return response.data;
}

export async function postData(path, data = {}) {
    const finalPath = path.startsWith('/') ? path : `/${path}`;
    const response = await privateApi.post(finalPath, data);
    return response.data;
}

/**
 * GET isteği ile ID'ye göre tek bir kayıt getirir.
 * `methodName` parametresi artık opsiyoneldir.
 * @param {string} controllerName - API controller'ının adı.
 * @param {string|number} id - Getirilecek kaydın ID'si.
 * @param {string} [methodName] - Opsiyonel: Kullanılacak özel bir metot adı.
 */
export async function getDataById(controllerName, id, methodName) {
  // Eğer methodName verilmişse URL'e eklenir, verilmemişse standart /controller/id formatı kullanılır.
  const url = methodName 
    ? `/${controllerName}/${methodName}/${id}` 
    : `/${controllerName}/${id}`;
  
  const response = await privateApi.get(url);
  return response.data;
}

export async function getDataByParams(path, params = {}) {
    const finalPath = path.startsWith('/') ? path : `/${path}`;
    const url = `${finalPath}?${new URLSearchParams(params)}`;
    const response = await privateApi.get(url);
    return response.data;
}

/**
 * ID'ye göre bir kaynağı günceller. PUT metodunu kullanır.
 * URL formatı: /{controllerName}/{id}
 * @param {string} controllerName - API controller'ının adı.
 * @param {string|number} id - Güncellenecek kaydın ID'si.
 * @param {object} data - İsteğin gövdesinde gönderilecek güncel veri.
 */
export async function updateData(controllerName, id, data) {
    const url = `/${controllerName}/${id}`;
    const response = await privateApi.put(url, data);
    return response.data;
}

/**
 * ID'ye göre bir kaynağı geçici olarak siler (isDeleted = true).
 * POST metodunu kullanır ve body göndermez.
 * @param {string} controllerName - API controller'ının adı.
 * @param {string|number} id - Geçici olarak silinecek kaydın ID'si.
 */
export async function softDeleteData(controllerName, id) {
    const url = `/${controllerName}/SoftDelete/${id}`;
    // Bu işlem için body göndermeye gerek yok, ID yeterlidir.
    const response = await privateApi.post(url); 
    return response.data;
}

/**
 * YENİ: Filtre parametreleri ile Excel dosyası indirme isteği atar.
 * @param {string} controllerName - API controller'ının adı.
 * @param {object} params - Filtreleme için request body'si olarak gönderilecek veri.
 * @returns {Promise<{fileData: Blob, fileName: string}>} - Dosya verisi ve adını içeren nesne.
 */
export async function downloadExcel(controllerName, params = {}) {
    const url = `/${controllerName}/Excel`;
    
    // Yanıt tipini 'blob' olarak ayarlayarak dosyayı indirmeye hazırlıyoruz.
    const response = await privateApi.post(url, params, { responseType: 'blob' });

    // Dosya adını 'Content-Disposition' başlığından almaya çalışıyoruz.
    let fileName = 'export.xlsx'; // Varsayılan dosya adı
    const contentDisposition = response.headers['content-disposition'];
    if (contentDisposition) {
        const fileNameMatch = contentDisposition.match(/filename="(.+)"/);
        if (fileNameMatch.length > 1) {
            fileName = fileNameMatch[1];
        }
    }
    
    return {
        fileData: response.data,
        fileName: fileName
    };
}