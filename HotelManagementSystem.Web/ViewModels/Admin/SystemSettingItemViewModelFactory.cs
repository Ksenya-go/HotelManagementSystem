using HotelManagementSystem.Application.SystemSettings;
using Microsoft.Extensions.Localization;

namespace HotelManagementSystem.Web.ViewModels.Admin;

public sealed class SystemSettingItemViewModelFactory(
    IStringLocalizer<SharedResource> localizer)
{
    public SystemSettingItemViewModel Create(SystemSettingDto setting)
    {
        var displayName = GetDisplayName(setting);

        return new SystemSettingItemViewModel(
            setting.Id,
            setting.Key,
            displayName,
            setting.Value);
    }

    private string GetDisplayName(SystemSettingDto setting)
    {
        var resourceKey = setting.Key switch
        {
            "hotel.checkInTime" => "Setting_CheckInTime",
            "hotel.checkOutTime" => "Setting_CheckOutTime",
            "hotel.currency" => "Setting_Currency",
            "hotel.name" => "Setting_HotelName",
            _ => null
        };

        return resourceKey is null
            ? setting.Description
            : localizer[resourceKey].Value;
    }
}
