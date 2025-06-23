import React, { useEffect } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Home from "./Components/Controllers/Home";
import Login from "./Components/Controllers/Login";
import { AuthProvider, useAuth } from "./Api/AuthContext";

// SnackbarProvider ve setApiErrorCallback'i ekle
import { SnackbarProvider, useSnackbar } from "./Components/BaseComponents/SnakebarProvider";
import { setApiErrorCallback } from "./Api/api-client";
import { SignalRProvider } from "./Components/BaseComponents/SignalRProvider"; // Yeni provider'ı import et

function PrivateRoute({ children }) {
  const { token } = useAuth();
  return token ? children : <Navigate to="/login" />;
}

function AppRoutes() {
  const { token } = useAuth();
  return (
    <Routes>
      <Route path="/login"
        element={token ? <Navigate to="/home" /> : <Login />}
      />
      <Route path="/home"
        element={<PrivateRoute><Home /></PrivateRoute>}
      />
      <Route path="*" element={<Navigate to={token ? "/home" : "/login"} />} />
    </Routes>
  );
}

// Bu bridge ile api-client'ın error callback'ine snakebar fonksiyonunu bağlıyoruz
function ApiInterceptorBridge() {
  const showSnackbar = useSnackbar();
  useEffect(() => {
    setApiErrorCallback(showSnackbar);
  }, [showSnackbar]);
  return null;
}

function App() {
  return (
    <div className="wrapper">
      <SnackbarProvider>
        <BrowserRouter>
          <AuthProvider>
            <SignalRProvider>
              <ApiInterceptorBridge />
              <AppRoutes />
            </SignalRProvider>
          </AuthProvider>
        </BrowserRouter>
      </SnackbarProvider>
    </div>
  );
}

export default App;
