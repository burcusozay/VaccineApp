export const GenericPagination = ({ currentPage, totalPages, onPageChange }) => {
  if (totalPages <= 1) return null;
  return (
    <div className="pagination-controls">
      <button onClick={() => onPageChange(currentPage - 1)} disabled={currentPage === 1}>&laquo; Önceki</button>
      <span className="pagination-info">Sayfa <strong>{currentPage}</strong> / <strong>{totalPages}</strong></span>
      <button onClick={() => onPageChange(currentPage + 1)} disabled={currentPage === totalPages}>Sonraki &raquo;</button>
    </div>
  );
};
