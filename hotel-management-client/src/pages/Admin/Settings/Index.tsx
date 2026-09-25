import { useEffect, useState } from "react";
import { settingsApi } from "@/api/settingsApi";
import type { SystemSettingDto } from "@/types/settings";

const settingNames: Record<string, string> = {
  "hotel.checkInTime": "Час заселення",
  "hotel.checkOutTime": "Час виселення",
  "hotel.currency": "Валюта",
  "hotel.name": "Назва готелю",
};

const MAX_LENGTH = 500;

interface Message {
  type: "success" | "danger";
  text: string;
}

function SettingCard({
  setting,
  onResult,
}: {
  setting: SystemSettingDto;
  onResult: (message: Message) => void;
}) {
  const [value, setValue] = useState(setting.value);
  const [saving, setSaving] = useState(false);
  const [fieldError, setFieldError] = useState<string | null>(null);

  const displayName = settingNames[setting.key] ?? setting.description;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!value.trim()) {
      setFieldError("Вкажіть значення налаштування.");
      return;
    }
    if (value.length > MAX_LENGTH) {
      setFieldError(`Значення не може перевищувати ${MAX_LENGTH} символів.`);
      return;
    }

    setFieldError(null);
    setSaving(true);
    try {
      await settingsApi.update(setting.id, value.trim());
      onResult({ type: "success", text: "Налаштування оновлено." });
    } catch (err: any) {
      onResult({
        type: "danger",
        text:
          err?.response?.status === 404
            ? "Налаштування не знайдено."
            : "Не вдалося оновити налаштування.",
      });
    } finally {
      setSaving(false);
    }
  };

  return (
    <form className="setting-card" onSubmit={handleSubmit} noValidate>
      <div className="setting-card-content">
        <div className="setting-copy">
          <strong>{displayName}</strong>
        </div>

        <div className="setting-control">
          <input
            className="form-control setting-input"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            aria-label={displayName}
            required
          />
          <button type="submit" className="btn btn-teal" disabled={saving}>
            {saving ? "Збереження..." : "Зберегти"}
          </button>
        </div>

        {fieldError && <span className="field-error">{fieldError}</span>}
      </div>
    </form>
  );
}

export default function AdminSettingsIndex() {
  const [settings, setSettings] = useState<SystemSettingDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState<Message | null>(null);

  useEffect(() => {
    settingsApi
      .list()
      .then(setSettings)
      .catch(() =>
        setMessage({ type: "danger", text: "Не вдалося завантажити налаштування." })
      )
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Системні налаштування</h1>
          <p className="page-subtitle">
            Керуйте параметрами конфігурації готельної платформи.
          </p>
        </div>
      </div>

      {message && (
        <div className={`alert app-alert alert-${message.type}`}>{message.text}</div>
      )}

      <section className="content-card settings-card">
        <div className="settings-header">
          <div>
            <h2>Конфігурація</h2>
          </div>
        </div>

        {loading ? (
          <p>Завантаження...</p>
        ) : (
          <div className="settings-grid">
            {settings.map((setting) => (
              <SettingCard key={setting.id} setting={setting} onResult={setMessage} />
            ))}
          </div>
        )}
      </section>
    </div>
  );
}