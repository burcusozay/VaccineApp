import React, { useState, useMemo } from 'react';
// DÜZELTME: ServerSideDataTable'ı yeni prop'larla kullanacağız.
import ServerSideDataTable from '../BaseComponents/ServerSideDataTable';

const buttonStyles = {
  padding: '10px 15px', margin: '0 5px 20px 0', cursor: 'pointer',
  border: '1px solid #ccc', borderRadius: '5px', backgroundColor: '#f0f0f0',
};

const activeButtonStyles = {
  ...buttonStyles,
  backgroundColor: '#007bff', color: 'white', borderColor: '#007bff',
};

export default function Home() {
  
  // ÖNERİLEN YAPI: URL'i manuel olarak yazmak yerine, 
  // controller ve action'ı ayrı ayrı tanımlamak daha az hataya yol açar.
  const tableConfigs = useMemo(() => [
    { name: 'Temperatures', controller: 'FreezerTemperature', action: 'FreezerTemperatures', tableName: 'tblFreezerTemperatures' },
    { name: 'Kullanıcılar', controller: 'User', action: '', tableName: 'tblUsers' }, // action boşsa, sadece controller kullanılır
    { name: 'Aşılar', controller: 'Vaccine', action: '', tableName: 'tblVaccines' },
  ], []);

  const [activeTableConfig, setActiveTableConfig] = useState(tableConfigs[0]);

  const [parameters, setParams] = useState({
    pageSize: 5,
    minValue: 0,
    maxValue: 10,
    startDate: null,
    endDate: null
  });

  return (
    <div style={{ padding: '20px' }}>
      <h1>Yönetim Paneli</h1>
      <p>Görüntülemek istediğiniz veri türünü seçin:</p>

      <div>
        {tableConfigs.map(config => (
          <button
            key={config.tableName}
            style={activeTableConfig.tableName === config.tableName ? activeButtonStyles : buttonStyles}
            onClick={() => setActiveTableConfig(config)}
          >
            {config.name}
          </button>
        ))}
      </div>

      <div>
        <h2>{activeTableConfig.name}</h2>
        <ServerSideDataTable
          key={activeTableConfig.tableName}
          tableName={activeTableConfig.tableName}
          // DÜZELTME: Artık 'controller' ve 'action' prop'larını kullanıyoruz.
          controller={activeTableConfig.controller}
          action={activeTableConfig.action}
          params={parameters}
        />
      </div>
    </div>
  );
}
