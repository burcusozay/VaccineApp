import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { getDataByParams } from '../../Api/api-client';
import '../../style/datatable.css'; // Harici CSS dosyasını import et
import { GenericTable } from './GenericTable';
import { GenericPagination } from './GenericPagination';
// ============================================================================
// Ana Mantık Bileşeni: ServerSideDataTable (GÜNCELLENDİ)
// ============================================================================
/**
 * Sunucu taraflı sayfalama yapan ve sütunları dinamik olarak oluşturan veri tablosu.
 * @param {string} url - Verinin çekileceği tam API yolu.
 * @param {string} tableName - Bileşenin key'i için kullanılacak benzersiz isim.
 * @param {object} [params={}] - API'ye gönderilecek ek parametreler (örn: { pageSize: 20, filter: 'abc' }).
 * @param {object|null} [actionColumn=null] - Opsiyonel olarak eklenecek işlem sütunu tanımı.
 * @param {function} [onEdit] - Düzenle butonu tıklandığında çağrılacak fonksiyon. Satır verisini parametre olarak alır.
 * @param {function} [onDelete] - Sil butonu tıklandığında çağrılacak fonksiyon. Satır verisini parametre olarak alır.
 */
function ServerSideDataTable({ controller, action = '', title, tableName, params = {}, onAdd, onEdit, onDelete, onSoftDelete }) {
  const [data, setData] = useState([]);
  const [columns, setColumns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  
  const paramsString = JSON.stringify(params);
  const apiUrl = action ? `/${controller}/${action}` : `/${controller}`;

  // DÜZELTME: İşlem sütununu, onEdit ve onDelete fonksiyonlarına bağlı olarak useMemo ile oluşturuyoruz.
  // Bu, fonksiyonlar değişmediği sürece nesnenin yeniden oluşmasını engeller ve stabildir.
  const actionColumn = useMemo(() => {
    if (!onEdit && !onDelete) {
      return null;
    }
    return {
      Header: 'İşlemler',
      accessor: 'actions', // Benzersiz bir accessor
      Cell: (item) => ( // Bu bir fonksiyondur ve stringify edilmemelidir.
        <div style={{ display: 'flex', gap: '5px' }}>
          {onEdit && (
            <button className="action-button edit" onClick={() => onEdit(item)}>
              Düzenle
            </button>
          )}
           {onSoftDelete && (
            <button className="action-button archive" onClick={() => onSoftDelete(item)}>
              Arşivle
            </button>
          )}
          {onDelete && (
            <button className="action-button delete" onClick={() => onDelete(item)}>
              Sil
            </button>
          )}
        </div>
      )
    };
  }, [onEdit, onDelete, onSoftDelete]);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);

    const baseParams = JSON.parse(paramsString);
    const finalParams = {
        page: currentPage,
        pageSize: 10,
        ...baseParams,
    };
    
    Object.keys(finalParams).forEach(key => {
        if (finalParams[key] === null || finalParams[key] === undefined) {
            delete finalParams[key];
        }
    });

    try {
      const result = await getDataByParams(apiUrl, finalParams);
      const items = result.Items || result.items || [];
      const receivedTotalPages = result.TotalPages || result.totalPages || 1;
      
      setData(items);
      setTotalPages(receivedTotalPages);
      
      if (items.length > 0 && columns.length === 0) {
        const firstItem = items[0];
        const generatedColumns = Object.keys(firstItem).map(key => ({
          Header: key,
          accessor: key,
        }));

        // DÜZELTME: String'e çevrilmemiş, fonksiyon içeren orijinal actionColumn nesnesini ekliyoruz.
        if (actionColumn) {
          generatedColumns.push(actionColumn);
        }
        setColumns(generatedColumns);
      }
    } catch (e) {
      const errorMessage = e?.response?.data?.title || e.message || "Veriler alınırken bir hata oluştu.";
      setError(errorMessage);
      setData([]);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  // DÜZELTME: Bağımlılık dizisi güncellendi. Artık `actionColumnString` yerine `actionColumn` nesnesini kullanıyoruz.
  // Bu nesne `useMemo` ile kararlı hale getirildiği için sorun yaratmaz.
  }, [apiUrl, currentPage, paramsString, actionColumn, columns.length]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);
  
  useEffect(() => {
    setCurrentPage(1);
    setColumns([]);
    setData([]);
  }, [apiUrl, paramsString]);

  const renderContent = () => {
    if (loading) return <div className="table-message">Yükleniyor...</div>;
    if (error && data.length === 0) return <div className="table-message">Hata: {error}</div>;
    if (!loading && data.length === 0) return <div className="table-message">Gösterilecek veri bulunamadı.</div>;
    if (data.length > 0 && columns.length === 0) return <div className="table-message">Sütunlar oluşturuluyor...</div>;
    return <GenericTable data={data} columns={columns} />;
  };

  return (
    <div className="data-table-container" key={tableName}>
       <div className="table-header">
        <h2>{title}</h2>
        {/* YENİ: onAdd prop'u varsa butonu render et */}
        {onAdd && (
          <button className="add-new-button" onClick={onAdd}>
            + Yeni Ekle
          </button>
        )}
      </div>
      {renderContent()}
      <GenericPagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
    </div>
  );
}

export default ServerSideDataTable;
