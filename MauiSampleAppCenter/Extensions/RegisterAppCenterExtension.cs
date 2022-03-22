using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using MauiSampleAppCenter.Helpers;

namespace MauiSampleAppCenter.Extensions;

public static partial class ConfigServicesExtensions
{
    public static MauiAppBuilder RegisterAppCenter(this MauiAppBuilder builder)
    {
        var appSecrets = string.Empty;

        if (Guid.TryParse(AppSettings.AppCenteriOSSecret, out Guid iOSSecret))
        {
            appSecrets += $"ios={iOSSecret};";
        }

        if (Guid.TryParse(AppSettings.AppCenterAndroidSecret, out Guid AndroidSecret))
        {
            appSecrets += $"android={AndroidSecret};";
        }

        AppCenter.Start(appSecrets, typeof(Crashes));

        return builder;
    }
}
