using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;

namespace HotelManagementSystem.Web.Extensions;

public static class TempDataMessageExtensions
{
    public static void SetSuccessMessage(
        this ITempDataDictionary tempData,
        LocalizedString message)
    {
        tempData["Success"] = message.Value;
    }

    public static void SetErrorMessage(
        this ITempDataDictionary tempData,
        LocalizedString message)
    {
        tempData["Error"] = message.Value;
    }

    public static void SetErrorMessage(
        this ITempDataDictionary tempData,
        string message)
    {
        tempData["Error"] = message;
    }
}
