import React, { useState, useEffect } from 'react';
import { DynamicInput } from './DynamicInput';
import '../../style/modal.css';

export default function AddEditModal({ title, item, isOpen, onClose, onSave, isLoading }) {
  const [formData, setFormData] = useState({});

  useEffect(() => {
    // Modal her açıldığında veya düzenlenecek öğe değiştiğinde
    // formu bu öğenin verisiyle dolduruyoruz.
    setFormData(item || {});
  }, [item]);

  if (!isOpen) {
    return null;
  }

  const handleInputChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSave = (e) => {
    e.preventDefault();
    onSave(formData);
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        {isLoading ? (
          <h2>Yükleniyor...</h2>
        ) : (
          <>
            <h2>{title}</h2>
            <form className="modal-form" onSubmit={handleSave}>
              {Object.entries(formData).map(([key, value]) => {
                // 'id' alanı 'Yeni Kayıt' modunda gösterilmez.
                if (key.toLowerCase() === 'id' && !value) {
                  return null;
                }
                return (
                  <div className="form-field" key={key}>
                    <label>{key}</label>
                    <DynamicInput
                      field={key}
                      value={value}
                      onChange={handleInputChange}
                      // id alanı düzenleme modunda değiştirilemez.
                      disabled={key.toLowerCase() === 'id'}
                    />
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
