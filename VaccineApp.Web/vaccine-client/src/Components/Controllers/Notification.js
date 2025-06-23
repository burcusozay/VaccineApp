import React, { useEffect, useState } from 'react';
import { BASE_URL } from "../../Api/api-call"; // API adresinizin do�ru oldu�undan emin olun
import * as signalR from '@microsoft/signalr';
import { ToastContainer, toast } from 'react-toastify'; // Bildirim k�t�phanesi
import 'react-toastify/dist/ReactToastify.css';

export default function NotificationComponent() {
    const [notifications, setNotifications] = useState([]);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(BASE_URL + '/notificationhub') // Hub endpoint'i
            .withAutomaticReconnect()
            .build();

        // Sunucudan gelen "ReceiveErrorNotification" olay�n� dinle.
        // Bu isim NotificationWorker'daki SendAsync ile ayn� olmal�.
        connection.on('ReceiveErrorNotification', (notification) => {
            console.log("Yeni hata bildirimi al�nd�:", notification);

            // Gelen bildirimi listenin ba��na ekle
            setNotifications((prevNotifications) => [notification, ...prevNotifications]);

            // Kullan�c�ya toast bildirimi g�ster
            toast.error(`Hata olu�tu! (Mesaj ID: ${notification.messageId}) - ${notification.error}`);
        });

        const startConnection = async () => {
            try {
                await connection.start();
                console.log('SignalR ba�lant�s� ba�ar�l�.');
            } catch (err) {
                console.error('SignalR ba�lant� hatas�:', err);
                toast.error('Bildirim sunucusuna ba�lan�lamad�.');
            }
        };

        startConnection();

        // Component unmount oldu�unda ba�lant�y� durdur
        return () => {
            if (connection.state === signalR.HubConnectionState.Connected) {
                connection.stop();
            }
        };
    }, []); // Bo� dependency array, sadece bir kez �al��mas�n� sa�lar

    return (
        <div className="notification-container">
            {/* Toast bildirimlerinin g�sterilece�i alan */}
            <ToastContainer
                position="top-right"
                autoClose={10000}
                hideProgressBar={false}
                newestOnTop={false}
                closeOnClick
                rtl={false}
                pauseOnFocusLoss
                draggable
                pauseOnHover
            />
            <h2>Hata Bildirimleri</h2>
            {notifications.length === 0 ? (
                <p>Hen�z bir hata bildirimi yok.</p>
            ) : (
                <ul className="notification-list">
                    {notifications.map((n, i) => (
                        <li key={`${n.messageId}-${i}`}>
                            <div className="notification-header">
                                <strong>Mesaj ID:</strong> {n.messageId}
                            </div>
                            <div className="notification-body">
                                <strong>Hata:</strong> {n.error}
                            </div>
                            <div className="notification-footer">
                                <small>Zaman: {new Date(n.createdAt).toLocaleString()}</small>
                            </div>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

