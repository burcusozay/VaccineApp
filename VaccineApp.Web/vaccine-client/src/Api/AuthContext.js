import React, { createContext, useState, useContext } from "react";
import { jwtDecode } from 'jwt-decode'; // jwt-decode import edildi

const AuthContext = createContext();

export function AuthProvider({ children }) {
  // DÜZELTME: State artık sadece token'ı değil, token ve çözümlenmiş kullanıcı verisini tutuyor.
  const [authData, setAuthData] = useState(() => {
    const tokenDataString = sessionStorage.getItem("accessToken");
    if (!tokenDataString) return null;

    try {
        const tokenData = JSON.parse(tokenDataString);
        // Sayfa yenilendiğinde de token'ı çözüp kullanıcı bilgisini al
        // Bu, JWT'nin içinde 'user' nesnesi olarak saklanmışsa çalışır.
        // Eğer tokenData'nın kendisi JWT ise, doğrudan onu çözmeliyiz.
        // Sakladığımız yapıya göre düzenliyoruz:
        if (tokenData.accessToken) {
            const decodedUser = jwtDecode(tokenData.accessToken);
            return { ...tokenData, user: decodedUser };
        }
        return null; // Geçersiz format
    } catch (e) {
        console.error("Geçersiz token. Oturum temizleniyor.", e);
        sessionStorage.removeItem("accessToken");
        return null;
    }
  });

  const login = (tokenData) => {
    try {
        // Gelen token'ı çözerek kullanıcı bilgilerini (rol vb.) al
        const decodedUser = jwtDecode(tokenData.accessToken);
        // Hem tokenları hem de çözümlenmiş kullanıcıyı state'e ve storage'a kaydet
        const dataToStore = { ...tokenData, user: decodedUser };

        sessionStorage.setItem("accessToken", JSON.stringify(dataToStore));
        setAuthData(dataToStore);
    } catch (error) {
        console.error("Login sırasında token çözümlenemedi:", error);
        // Hatalı token ile login yapılmasını engelle
        logout();
    }
  };

  const logout = () => {
    sessionStorage.removeItem("accessToken");
    setAuthData(null);
  };

  // Context'ten artık authData (token ve user bilgilerini içeren nesne) paylaşılıyor.
  return (
    <AuthContext.Provider value={{ authData, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
