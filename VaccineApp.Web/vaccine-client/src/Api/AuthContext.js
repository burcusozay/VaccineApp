import React, { createContext, useState, useContext } from "react";

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => {
    const t = sessionStorage.getItem("accessToken");
    return t ? JSON.parse(t) : null;
  });

  const login = (tok) => {
    sessionStorage.setItem("accessToken", JSON.stringify(tok));
    setToken(tok);
  };

  const logout = () => {
    sessionStorage.removeItem("accessToken");
    setToken(null);
  };

  return (
    <AuthContext.Provider value={{ token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
