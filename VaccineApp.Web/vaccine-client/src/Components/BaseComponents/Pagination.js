import React, { useState, useEffect, useCallback } from 'react';

// ===================================================================================
// 2. YENİDEN KULLANILABİLİR BİLEŞEN: Pagination
// ===================================================================================
/**
 * Sayfalama için UI kontrollerini (geri, ileri, sayfa bilgisi) oluşturan bileşen.
 * @param {number} currentPage - Mevcut aktif sayfa numarası.
 * @param {number} totalPages - Toplam sayfa sayısı.
 * @param {function} onPageChange - Sayfa değiştirildiğinde tetiklenen callback fonksiyonu.
 */
export const Pagination = ({ currentPage, totalPages, onPageChange }) => {
  if (totalPages <= 1) return null;

  return (
    <div className="pagination-controls">
      <button 
        onClick={() => onPageChange(currentPage - 1)} 
        disabled={currentPage === 1}
      >
        &laquo; Geri
      </button>
      <span>
        Sayfa <strong>{currentPage}</strong> / <strong>{totalPages}</strong>
      </span>
      <button 
        onClick={() => onPageChange(currentPage + 1)} 
        disabled={currentPage === totalPages}
      >
        İleri &raquo;
      </button>
    </div>
  );
};
