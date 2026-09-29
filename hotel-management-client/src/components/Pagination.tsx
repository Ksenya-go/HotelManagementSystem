interface PaginationProps {
  pageNumber: number;
  totalPages: number;
  ariaLabel: string;
  onChange: (page: number) => void;
}

export default function Pagination({ pageNumber, totalPages, ariaLabel, onChange }: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <nav className="pagination-nav" aria-label={ariaLabel}>
      <button
        type="button"
        className={`btn btn-sm btn-outline-teal ${pageNumber <= 1 ? "disabled" : ""}`}
        disabled={pageNumber <= 1}
        onClick={() => onChange(pageNumber - 1)}
      >
        Попередня
      </button>

      {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
        <button
          key={page}
          type="button"
          className={`btn btn-sm ${page === pageNumber ? "btn-teal" : "btn-outline-teal"}`}
          onClick={() => onChange(page)}
        >
          {page}
        </button>
      ))}

      <button
        type="button"
        className={`btn btn-sm btn-outline-teal ${pageNumber >= totalPages ? "disabled" : ""}`}
        disabled={pageNumber >= totalPages}
        onClick={() => onChange(pageNumber + 1)}
      >
        Наступна
      </button>
    </nav>
  );
}