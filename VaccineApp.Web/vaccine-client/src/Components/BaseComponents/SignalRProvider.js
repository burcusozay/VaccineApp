import React, { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from '../../Api/AuthContext';
import { useSnackbar } from './SnakebarProvider'; 
import { BASE_URL } from '../../Api/api-client'; // api-client'tan alacağız

/**
 * SignalR bağlantısını yöneten ve bildirimleri snackbar ile gösteren Provider.
 * Bu bileşen, AuthProvider ve SnackbarProvider içinde kullanılmalıdır.
 */
export function SignalRProvider({ children }) {
    const { token } = useAuth(); // Kullanıcı oturum bilgisini al
    const showSnackbar = useSnackbar(); // Bildirim gösterme fonksiyonunu al
    const connectionRef = useRef(null); // Bağlantıyı component re-render'ları arasında koru

    useEffect(() => {
        // Eğer kullanıcı giriş yapmışsa (token varsa) bağlantıyı kur
        if (token && token.accessToken) {
            const hubUrl = `${BASE_URL}/notificationhub`;
            
            // Yeni bir bağlantı oluştur
            const newConnection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl, {
                    // Her istekte güncel access token'ı gönder
                    accessTokenFactory: () => token.accessToken
                })
                .withAutomaticReconnect()
                .build();
            
            connectionRef.current = newConnection;

            // Sunucudan "ReceiveErrorNotification" olayını dinle
            newConnection.on('ReceiveErrorNotification', (notification) => {
                console.log("SignalR Bildirimi Geldi:", notification);
                const message = notification.message || "Bilinmeyen bir sunucu bildirimi.";
                // Mevcut snackbar sistemini kullanarak bildirimi göster
                showSnackbar(message, 'warning'); // Hata yerine uyarı olarak gösterebiliriz
            });

            // Bağlantıyı başlat
            newConnection.start()
                .then(() => console.log('SignalR Bağlantısı Kuruldu.'))
                .catch(err => console.error('SignalR Bağlantı Hatası:', err));

        } else if (connectionRef.current) {
            // Eğer kullanıcı çıkış yapmışsa mevcut bağlantıyı durdur
            connectionRef.current.stop()
                .then(() => console.log('SignalR Bağlantısı Durduruldu.'));
            connectionRef.current = null;
        }

        // Cleanup: Component kaldırıldığında veya token değiştiğinde bağlantıyı durdur
        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop();
            }
        };
    }, [token, showSnackbar]); // Sadece token değiştiğinde çalışır

    return <>{children}</>;
}
