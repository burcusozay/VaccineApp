import React, { useState, useEffect, useCallback } from 'react';
import api from '../../Api/api-call';
import { BASE_URL } from '../../Api/api-call';
// ===================================================================================
// 1. YENİDEN KULLANILABİLİR BİLEŞEN: GenericTable
// ===================================================================================
/**
 * Herhangi bir veri dizisini ve sütun yapılandırmasını alarak dinamik bir HTML tablosu oluşturan jenerik bileşen.
 * @param {object[]} data - Görüntülenecek veri dizisi.
 * @param {object[]} columns - Sütunları tanımlayan yapılandırma dizisi. Her obje { Header: string, accessor: string } formatında olmalıdır.
 * @param {boolean} loading - Verinin yüklenip yüklenmediğini belirten durum.
 * @param {string|null} error - Varsa, veri çekme sırasında oluşan hata mesajı.
 */
export const GenericTable = ({ data, columns, loading, error, url }) => {
    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        fetchData(page);
    }, [page]);

    const fetchData = async (page) => {
        setLoading(true);
        try {
            const res = await api.post(`${BASE_URL}/${url}`);
            const json = await res.json();
            setData(json.data);
            setTotal(json.total);
        } catch (err) {
            setData([]);
            setTotal(0);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return <div className="loading">Veriler Yükleniyor...</div>;
    }

    if (error) {
        return <div className="error">Hata: {error}</div>;
    }

    if (!data || data.length === 0) {
        return <div className="loading">Gösterilecek veri bulunamadı.</div>;
    }

    return (
        <div className="table-container">
            <table>
                <thead>
                    <tr>
                        {columns.map((col) => (
                            <th key={col.accessor}>{col.Header}</th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {data.map((item, index) => (
                        <tr key={item.id || index}>
                            {columns.map((col) => (
                                <td key={col.accessor}>{item[col.accessor]}</td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};