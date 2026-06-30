using Microsoft.Maui.ApplicationModel;

namespace StepsCounter;

public class ActivityRecognitionPermission :
    Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new[]
        {
            (Android.Manifest.Permission.ActivityRecognition, true)
        };
#endif
}