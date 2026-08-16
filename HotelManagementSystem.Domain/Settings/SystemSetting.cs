namespace HotelManagementSystem.Domain.Entities;

public sealed class SystemSetting
{
    private SystemSetting()
    {
    }

    public SystemSetting(
        string key,
        string value,
        string description)
    {
        Key = key;
        Value = value;
        Description = description;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; private set; }

    public void UpdateValue(string value)
    {
        Value = value?.Trim() ?? string.Empty;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}