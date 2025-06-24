import React, { useState, useMemo, useCallback, useEffect } from 'react';
import ServerSideDataTable from '../BaseComponents/ServerSideDataTable';
import EditModal from '../BaseComponents/EditModal';
import { getDataById, postData, updateData, softDeleteData } from '../../Api/api-client';
import { useSnackbar } from '../BaseComponents/SnakebarProvider';
import '../../style/index.css';

export default function Home() {
  const [activeTableConfig, setActiveTableConfig] = useState(null);
  const [parameters, setParams] = useState({ pageSize: 5 });
  
  // Modal için state'ler
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isLoadingItem, setIsLoadingItem] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0); // Tabloyu yenilemek için
  const showSnackbar = useSnackbar();

   // "Düzenle" butonuna tıklandığında çalışacak fonksiyon
  const handleEdit = useCallback(async (row) => {
    if (!activeTableConfig) return;
    setIsModalOpen(true);
    setIsLoadingItem(true);
    try {
      const fullItem = await getDataById(activeTableConfig.controller, row.id);
      setEditingItem(fullItem);
    } catch (error) {
      showSnackbar("Kayıt detayı alınamadı.", "error");
      handleCloseModal();
    } finally {
      setIsLoadingItem(false);
    }
  }, [activeTableConfig, showSnackbar]);

  // "Sil" butonuna tıklandığında çalışacak fonksiyon
  const handleDelete = useCallback(async (row) => {
    if (!activeTableConfig) return;
    if (window.confirm(`ID: ${row.id} olan kaydı silmek istediğinizden emin misiniz?`)) {
        try {
            await postData(`/${activeTableConfig.controller}/Delete/${row.id}`);
            showSnackbar("Kayıt başarıyla silindi.", "success");
            setRefreshKey(prev => prev + 1);
        } catch (error) {
            showSnackbar("Kayıt silinirken bir hata oluştu.", "error");
        }
    }
  }, [activeTableConfig, showSnackbar]);
  
  /**
   * YENİ: Geçici silme (arşivleme) fonksiyonu.
   */
  const handleSoftDelete = useCallback(async (row) => {
    if (!activeTableConfig) return;
    if (window.confirm(`ID: ${row.id} olan kaydı arşivlemek istediğinizden emin misiniz?`)) {
        try {
            // Yeni oluşturduğumuz API fonksiyonunu çağırıyoruz.
            await softDeleteData(activeTableConfig.controller, row.id);
            showSnackbar("Kayıt başarıyla arşivlendi.", "success");
            setRefreshKey(prev => prev + 1); // Tabloyu yenile
        } catch (error) {
            showSnackbar("Kayıt arşivlenirken bir hata oluştu.", "error");
        }
    }
  }, [activeTableConfig, showSnackbar]);


  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingItem(null);
  };

  // GÜNCELLEME: handleSave fonksiyonu artık yeni `updateData` fonksiyonunu kullanıyor.
  // Bu, REST API best practice'lerine en uygun yöntemdir.
  const handleSave = async (updatedItem) => {
    if (!activeTableConfig) return;
    try {
        // En doğru yöntem: PUT /api/Controller/{id} isteği atılır.
        await updateData(activeTableConfig.controller, updatedItem.id, updatedItem);
        showSnackbar("Kayıt başarıyla güncellendi.", "success");
        handleCloseModal();
        setRefreshKey(prev => prev + 1);
    } catch (error) {
        showSnackbar("Kayıt güncellenirken bir hata oluştu.", "error");
    }
  };

  const tableConfigs = useMemo(() => [
    { name: 'Temperatures', controller: 'FreezerTemperature', action: 'FreezerTemperatures', tableName: 'tblFreezerTemperatures' },
    { name: 'Kullanıcılar', controller: 'User', action: '', tableName: 'tblUsers' },
    { name: 'Aşılar', controller: 'Vaccine', action: '', tableName: 'tblVaccines' },
  ], []);

  // Sayfa ilk yüklendiğinde varsayılan tabloyu ayarla
  useEffect(() => {
      if (!activeTableConfig) {
          setActiveTableConfig(tableConfigs[0]);
      }
  }, [activeTableConfig, tableConfigs]);

  return (
    <div style={{ padding: '20px' }}>
      <h1>Yönetim Paneli</h1>
      <p>Görüntülemek istediğiniz veri türünü seçin:</p>
      <div>
        {tableConfigs.map(config => (
          <button
            key={config.tableName}
            className={activeTableConfig?.tableName === config.tableName ? 'tab-button active' : 'tab-button'}
            onClick={() => setActiveTableConfig(config)}
          >
            {config.name}
          </button>
        ))}
      </div>

      {activeTableConfig && (
        <div>
          <h2>{activeTableConfig.name}</h2>
          <ServerSideDataTable
            // Tabloyu yeniden yüklemek için refreshKey'i key prop'una ekliyoruz.
            key={`${activeTableConfig.tableName}-${refreshKey}`}
            tableName={activeTableConfig.tableName}
            controller={activeTableConfig.controller}
            action={activeTableConfig.action}
            params={parameters}
            onEdit={handleEdit}
            onDelete={handleDelete}
            onSoftDelete={handleSoftDelete}
          />
        </div>
      )}

      <EditModal
        isOpen={isModalOpen}
        item={editingItem}
        isLoading={isLoadingItem}
        onClose={handleCloseModal}
        onSave={handleSave}
      />
    </div>
  );
}
