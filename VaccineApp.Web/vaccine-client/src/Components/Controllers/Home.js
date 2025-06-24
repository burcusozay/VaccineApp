import React, { useState, useMemo, useCallback, useEffect } from 'react';
import ServerSideDataTable from '../BaseComponents/ServerSideDataTable';
// import EditModal from '../BaseComponents/EditModal';
import AddEditModal from '../BaseComponents/AddEditModal'; // EditModal -> AddEditModal olarak değiştirildi
import { getDataById, postData, updateData, softDeleteData, downloadExcel } from '../../Api/api-client';
import { useSnackbar } from '../BaseComponents/SnakebarProvider';
import { useAuth } from '../../Api/AuthContext'; // 'useAuth' hook'u import edildi
import '../../style/index.css';

export default function Home() {

  const tableConfigs = useMemo(() => [
    {
      name: 'Temperatures', controller: 'FreezerTemperature', action: 'FreezerTemperatureList', tableName: 'tblFreezerTemperatures',
      dummyAddData: { id: 0, temperature: 0, freezerId: 1, isDeleted: false, isActive: true, createdDate: new Date().toISOString() }
    },
    {
      name: 'Kullanıcılar', controller: 'User', action: '', tableName: 'tblUsers',
      dummyAddData: { id: 0, userName: '', name: '', surname: '', email: '', phone: '', normalizeName: '', isDeleted: false, isActive: true, createdDate: new Date().toISOString() }
    },
    {
      name: 'Dolaplar', controller: 'Freezer', action: 'FreezerList', tableName: 'tblVaccines',
      dummyAddData: { id: 0, name: '', orderNo: 0, isDeleted: false, isActive: true, createdDate: new Date().toISOString() }
    },
  ], []);

  const { logout } = useAuth(); // AuthContext'ten logout fonksiyonu alındı
  const [activeTableConfig, setActiveTableConfig] = useState(tableConfigs[0]);
  const [parameters, setParams] = useState({ pageSize: 5 });

  // Modal için state'ler
  const [modalMode, setModalMode] = useState(null); // 'add' veya 'edit'
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [isLoadingItem, setIsLoadingItem] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0); // Tabloyu yenilemek için
  const showSnackbar = useSnackbar();

  // "Yeni Ekle" butonuna tıklandığında çalışacak fonksiyon
  const handleAdd = useCallback(() => {
    if (!activeTableConfig) return;
    setModalMode('add');
    // Yeni kayıt için dummy datayı ayarla
    setEditingItem(activeTableConfig.dummyAddData);
    setIsModalOpen(true);
  }, [activeTableConfig]);

  // "Düzenle" butonuna tıklandığında çalışacak fonksiyon
  const handleEdit = useCallback(async (row) => {
    if (!activeTableConfig) return;
    setModalMode('edit');
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

  // Modal kapatma fonksiyonu
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingItem(null);
    setModalMode(null);
  };
 
const handleSave = async (itemToSave) => {
    if (!activeTableConfig) return;
    try {
      if (modalMode === 'edit') {
        // Düzenleme
        await updateData(activeTableConfig.controller, itemToSave.id, itemToSave);
        showSnackbar("Kayıt başarıyla güncellendi.", "success");
      } else if (modalMode === 'add') {
        // Ekleme
        await postData(`/${activeTableConfig.controller}/Add`, itemToSave);
        showSnackbar("Kayıt başarıyla eklendi.", "success");
      }
      handleCloseModal();
      setRefreshKey(prev => prev + 1);
    } catch (error) {
      showSnackbar(`Kayıt ${modalMode === 'edit' ? 'güncellenirken' : 'eklenirken'} bir hata oluştu.`, "error");
    }
  };

  /**
   * YENİ: Excel'e Aktar butonuna tıklandığında çalışacak fonksiyon.
   */
  const handleExportExcel = useCallback(async () => {
    if (!activeTableConfig) return;
    
    showSnackbar("Excel dosyası hazırlanıyor...", "info");
    try {
        // Sayfalama dışındaki tüm filtre parametrelerini göndererek dosyayı indir
        const exportParams = { ...parameters };
        delete exportParams.page;
        delete exportParams.pageSize;

        const { fileData, fileName } = await downloadExcel(activeTableConfig.controller, exportParams);
        
        // Tarayıcıda indirme işlemini tetikle
        const url = window.URL.createObjectURL(new Blob([fileData]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.parentNode.removeChild(link);
        window.URL.revokeObjectURL(url);

    } catch (error) {
        showSnackbar("Excel dosyası indirilirken bir hata oluştu.", "error");
    }
  }, [activeTableConfig, parameters, showSnackbar]);

  // // Sayfa ilk yüklendiğinde varsayılan tabloyu ayarla
  // useEffect(() => {
  //   if (!activeTableConfig) {
  //     setActiveTableConfig(tableConfigs[0]);
  //   }
  // }, [tableConfigs]);

  return (
    <div className="home-container">
      {/* Logout Butonu */}
      <button className="logout-button" onClick={logout}>
        Çıkış Yap
      </button>

      <h1>Yönetim Paneli</h1>
      <p>Görüntülemek istediğiniz veri türünü seçin:</p>
      <div>
        {tableConfigs.map(config => (
          <button
            key={config.tableName}
            className={`tab-button ${activeTableConfig?.tableName === config.tableName ? 'active' : ''}`}
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
            onAdd={handleAdd}
            onEdit={handleEdit}
            onDelete={handleDelete}
            onSoftDelete={handleSoftDelete}
            onExportExcel={handleExportExcel}
          />
        </div>
      )}

      {/* <EditModal
        isOpen={isModalOpen}
        item={editingItem}
        isLoading={isLoadingItem}
        onClose={handleCloseModal}
        onSave={handleSave}
      /> */}

      <AddEditModal
        isOpen={isModalOpen}
        title={modalMode === 'edit' ? 'Kayıt Düzenle' : 'Yeni Kayıt Ekle'}
        item={editingItem}
        isLoading={isLoadingItem}
        onClose={handleCloseModal}
        onSave={handleSave}
      />
    </div>
  );
}
