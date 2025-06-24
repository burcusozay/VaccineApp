import React, { useState, useEffect } from 'react';
import '../../style/modal.css';
import { DynamicInput } from './DynamicInput'; // Bileşen adını dışa aktarılanla eşleştir

export default function EditModal({ item, isOpen, onClose, onSave, isLoading }) {
  const [formData, setFormData] = useState({});

  // `item` prop'u değiştiğinde, form verisini güncelle.
  // Bu, modal'ın farklı kayıtlar için yeniden açıldığında doğru veriyi göstermesini sağlar.
  useEffect(() => {
    if (item) {
      setFormData(item);
    }
  }, [item]);

  if (!isOpen) {
    return null;
  }

  // Input'lardan gelen değişiklikleri state'e yansıt.
  const handleInputChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSave = (e) => {
    e.preventDefault(); // Form'un sayfayı yeniden yüklemesini engelle
    onSave(formData);
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()}> {/* İçeriğe tıklamanın modal'ı kapatmasını engelle */}
        {isLoading ? (
          <h2>Yükleniyor...</h2>
        ) : (
          <>
            <h2>Kayıt Düzenle (ID: {item?.id})</h2>
            {/* handleSave'i form'un onSubmit olayına bağlamak, Enter tuşuyla da kaydetmeyi sağlar */}
            <form className="modal-form" onSubmit={handleSave}>
              {Object.entries(formData).map(([key, value]) => {
                // ID alanını düzenlenemez yap
                if (key.toLowerCase() === 'id') {
                  return (
                    <div className="form-field" key={key}>
                      <label>{key}</label>
                      <input type="text" value={value} disabled />
                    </div>
                  );
                }
                return (
                  <div className="form-field" key={key}>
                    <label>{key}</label>
                    <DynamicInput field={key} value={value} onChange={handleInputChange} />
                  </div>
                );
              })}
            </form>
            <div className="modal-actions">
              <button type="button" className="action-button cancel" onClick={onClose}>İptal</button>
              <button type="button" className="action-button save" onClick={handleSave}>Kaydet</button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
