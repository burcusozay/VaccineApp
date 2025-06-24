import '../../style/datatable.css'; // Harici CSS dosyasını import et

// ============================================================================
// YENİ: Yardımcı Fonksiyon
// ============================================================================
/**
 * Bir string değerin tarih formatında olup olmadığını kontrol eder ve
 * eğer tarih ise, onu "dd.MM.yyyy HH:mm:ss" formatına çevirir.
 * @param {any} value - Biçimlendirilecek değer.
 * @returns {string} - Biçimlendirilmiş tarih veya orijinal değer.
 */
const formatCellContent = (value) => {
  const strValue = String(value ?? '');
  // Standart ISO 8601 formatını (YYYY-MM-DDTHH:mm:ss) kontrol eden regex
  if (/\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/.test(strValue)) {
    try {
      const date = new Date(strValue);
      // Geçersiz bir tarih nesnesi oluşup oluşmadığını kontrol et
      if (isNaN(date.getTime())) {
        return strValue;
      }

      const day = String(date.getDate()).padStart(2, '0');
      const month = String(date.getMonth() + 1).padStart(2, '0'); // Aylar 0-indekslidir
      const year = date.getFullYear();
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');
      const seconds = String(date.getSeconds()).padStart(2, '0');

      return `${day}.${month}.${year} ${hours}:${minutes}:${seconds}`;
    } catch (e) {
      return strValue; // Formatlama sırasında hata olursa orijinal değeri döndür
    }
  }
  return strValue;
};

// ============================================================================
// Sunum Bileşenleri (GenericTable ve GenericPagination)
// ============================================================================
export const GenericTable = ({ data, columns }) => (
    <div className="table-responsive">
      <table className="generic-table">
        <thead>
          <tr>{columns.map(col => <th key={col.Header}>{col.Header}</th>)}</tr>
        </thead>
        <tbody>
          {data.map((item, index) => (
            <tr key={item.id || index}>
              {columns.map(col => (
                <td key={`${col.accessor}-${item.id || index}`}>
                  {/* DÜZELTME: Hücre içeriğini format fonksiyonundan geçiriyoruz */}
                  {col.Cell ? col.Cell(item) : formatCellContent(item[col.accessor])}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
  