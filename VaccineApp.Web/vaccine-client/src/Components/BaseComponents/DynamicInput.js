import React, { useState, useEffect } from 'react';
import '../../style/modal.css';

/**
 * YENİ: Bir Date nesnesini veya ISO string'ini, datetime-local input'unun
 * value prop'u için gereken 'YYYY-MM-DDTHH:mm' formatına, kullanıcının
 * yerel saat dilimini dikkate alarak çeviren yardımcı fonksiyon.
 * @param {Date|string} date - Formatlanacak tarih.
 * @returns {string} - 'YYYY-MM-DDTHH:mm' formatında string.
 */
function toLocalInputString(date) {
  const d = date instanceof Date ? date : new Date(date);
  if (isNaN(d.getTime())) {
    return ''; // Geçersiz tarihler için boş string döndür.
  }
  // Tarayıcının saat dilimi farkını alıp, UTC tarihinden çıkararak yerel tarihi buluyoruz.
  const timezoneOffset = d.getTimezoneOffset() * 60000; // milisaniye cinsinden
  const localDate = new Date(d.getTime() - timezoneOffset);
  // Sonucu ISO formatına çevirip saniye ve sonrası kısmı atıyoruz.
  return localDate.toISOString().slice(0, 16);
}


/**
 * Tarih-zaman input'u için özel, kontrollü bir bileşen.
 * Bu bileşen, kendi lokal state'ini yöneterek formatlama ve state senkronizasyon
 * sorunlarını çözer.
 */
const DateTimeInput = ({ initialValue, onChange }) => {
  // Input'un göstermesi gereken değeri (YYYY-MM-DDTHH:mm) tutan lokal state
  const [localValue, setLocalValue] = useState('');

  // Dışarıdan gelen `initialValue` (ISO string) değiştiğinde,
  // lokal state'i input'un anlayacağı formata çevirerek güncelle.
  useEffect(() => {
    // DÜZELTME: Tarihi, kullanıcının yerel saat dilimine göre doğru formata çeviriyoruz.
    if (initialValue) {
      setLocalValue(toLocalInputString(initialValue));
    } else {
      setLocalValue('');
    }
  }, [initialValue]);

  // Kullanıcı input'u değiştirdiğinde...
  const handleChange = (e) => {
    const newLocalValue = e.target.value;
    // 1. Görsel olarak input'un değerini hemen güncelle.
    setLocalValue(newLocalValue);
    
    // 2. Parent component'e (EditModal) değişikliği tam ISO formatında bildir.
    if (newLocalValue) {
      // Input'tan gelen lokal zamanı bir Date nesnesine çevirip
      // ardından standart UTC formatı olan ISO string'e dönüştür.
      const isoDate = new Date(newLocalValue).toISOString();
      onChange(isoDate);
    } else {
      onChange(null); // Eğer kullanıcı input'u boşalttıysa null gönder.
    }
  };

  return (
    <input
      type="datetime-local"
      value={localValue}
      onChange={handleChange}
    />
  );
};


export const DynamicInput = ({ field, value, onChange }) => {
  const inputType = typeof value;

  switch (inputType) {
    case 'boolean':
      return (
        <div className="switch-field">
          <label className="switch">
            <input
              type="checkbox"
              checked={value}
              onChange={(e) => onChange(field, e.target.checked)}
            />
            <span className="slider round"></span>
          </label>
        </div>
      );
    case 'number':
      return (
        <input
          type="number"
          value={value}
          onChange={(e) => onChange(field, parseFloat(e.target.value) || 0)}
        />
      );
    case 'string':
      // Tarih formatını (YYYY-MM-DDTHH:mm) kontrol et
      if (/\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(value)) {
        return (
          <DateTimeInput
            initialValue={value}
            onChange={(newValue) => onChange(field, newValue)}
          />
        );
      }
      // Tarih olmayan normal metin alanları için
      return (
        <input
          type="text"
          value={value}
          onChange={(e) => onChange(field, e.target.value)}
        />
      );
    default:
      // Diğer tipler (örn: object) veya null/undefined için
      return <input type="text" value={String(value ?? '')} disabled />;
  }
};
