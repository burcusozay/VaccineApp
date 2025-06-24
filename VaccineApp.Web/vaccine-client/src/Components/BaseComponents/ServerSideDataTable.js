import React, { useState, useEffect, useCallback } from 'react';
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
 */
function ServerSideDataTable({ controller, action = '', tableName, params = {}, actionColumn = null }) {
  const [data, setData] = useState([]);
  const [columns, setColumns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  
  // Nesne tipindeki prop'ların her render'da yeniden oluşmasını ve
  // useCallback'i gereksiz yere tetiklemesini önlemek için string'e çeviriyoruz.
  const paramsString = JSON.stringify(params);
  const apiUrl = action ? `/${controller}/${action}` : `/${controller}`;
  const actionColumnString = JSON.stringify(actionColumn);

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
      
      // ÖNEMLİ DÜZELTME: Sütunları sadece gerçekten boşken oluştur.
      // Bu, fetchData'nın tekrar çağrılsa bile setColumns'u tetiklemesini engeller.
      if (items.length > 0 && columns.length === 0) {
        const firstItem = items[0];
        const generatedColumns = Object.keys(firstItem).map(key => ({
          Header: key,
          accessor: key,
        }));

        const currentActionColumn = JSON.parse(actionColumnString);
        if (currentActionColumn) {
          generatedColumns.push({ ...currentActionColumn, accessor: 'actions' });
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
  // DÜZELTME: useCallback'in bağımlılık dizisi, gereksiz yeniden oluşturmaları önleyecek şekilde
  // sadece kararlı (primitive veya stringify edilmiş) değerlere bağlıdır.
  // `columns.length` buradan kaldırılmıştır, çünkü bu sonsuz döngünün asıl nedenidir.
  }, [apiUrl, currentPage, paramsString, actionColumnString]);

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
      {renderContent()}
      <GenericPagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
    </div>
  );
}

export default ServerSideDataTable;
