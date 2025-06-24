
// ============================================================================
// Sunum Bileşenleri (GenericTable ve Pagination)
// ============================================================================
export const GenericTable = ({ data, columns }) => (
  <div className="table-responsive">
    <table className="generic-table">
      <thead>
        <tr>
          {columns.map(col => <th key={col.Header}>{col.Header}</th>)}
        </tr>
      </thead>
      <tbody>
        {data.map((item, index) => (
          <tr key={item.id || index}>
            {columns.map(col => (
              <td key={`${col.accessor}-${item.id || index}`}>
                {col.Cell ? col.Cell(item) : String(item[col.accessor] ?? '')}
              </td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  </div>
);
