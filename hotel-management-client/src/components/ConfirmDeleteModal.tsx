import { useEffect } from "react";

interface ConfirmDeleteModalProps {
  title: string;
  rows: { label: string; value: string | number }[];
  warning: string;
  confirmLabel: string;
  busy?: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

export default function ConfirmDeleteModal({
  title,
  rows,
  warning,
  confirmLabel,
  busy = false,
  onConfirm,
  onClose,
}: ConfirmDeleteModalProps) {
  useEffect(() => {
    document.body.classList.add("modal-open");
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    window.addEventListener("keydown", onKeyDown);
    return () => {
      document.body.classList.remove("modal-open");
      window.removeEventListener("keydown", onKeyDown);
    };
  }, [onClose]);

  return (
    <>
      <div
        className="modal fade show d-block"
        tabIndex={-1}
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-delete-title"
        onClick={onClose}
      >
        <div
          className="modal-dialog modal-dialog-centered reservation-delete-modal"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="modal-content">
            <div className="modal-header">
              <div>
                <h2 className="modal-title" id="confirm-delete-title">
                  {title}
                </h2>
              </div>
              <button type="button" className="btn-close" aria-label="Закрити" onClick={onClose} />
            </div>

            <div className="modal-body">
              <div className="delete-reservation-info">
                {rows.map((row) => (
                  <div className="delete-info-item" key={row.label}>
                    <span>{row.label}</span>
                    <strong>{row.value}</strong>
                  </div>
                ))}
              </div>

              <div className="delete-warning">
                <span className="delete-warning-icon">!</span>
                <span>{warning}</span>
              </div>
            </div>

            <div className="modal-footer">
              <button type="button" className="btn btn-cancel" onClick={onClose}>
                Скасувати
              </button>
              <button type="button" className="btn btn-danger" disabled={busy} onClick={onConfirm}>
                {confirmLabel}
              </button>
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop fade show" />
    </>
  );
}