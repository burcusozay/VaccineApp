import React, { useState } from "react";
import api from "../js/api-call";
import { useAuth } from "../js/AuthContext";

export default function Login() {
    const { login } = useAuth();
    const [username, setUserName] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState(null);

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const response = await api.post("/Account/Login", { username, password });
            if (response.data && response.data.accessToken) {
                login(response.data);
                setError(null);
            } else {
                setError("Login failed.");
            }
        } catch (err) {
            setError("Kullanıcı adı veya şifre hatalı.");
        }
    };

    return (
        <div className="login-page">
            <h1>Login</h1>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <input type="text" placeholder="Usename" value={username} onChange={e => setUserName(e.target.value)} />
                </div>
                <div className="form-group">
                    <input type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} />
                </div>
                {error && <div style={{ color: "red" }}>{error}</div>}
                <div>
                    <button type="submit">Login</button>
                </div>
            </form>
        </div>
    );
}
