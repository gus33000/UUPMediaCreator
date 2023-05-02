using UnifiedUpdatePlatform.Imaging;
using UnifiedUpdatePlatform.Common.Messaging;

namespace UnifiedUpdatePlatform.Media.Creator.Installer
{
    internal static class InstallerLogger
    {
        private static readonly UnifiedUpdatePlatform.Common.Messaging.Common.ProcessPhase Phase = UnifiedUpdatePlatform.Common.Messaging.Common.ProcessPhase.CreatingWindowsInstaller;

        internal static IImaging.ProgressCallback GetImagingCallback(this ProgressCallback progressCallback)
        {
            return (Operation, ProgressPercentage, IsIndeterminate) => progressCallback?.Invoke(Phase, IsIndeterminate, ProgressPercentage, Operation);
        }

        internal static void Log(this ProgressCallback progressCallback, string Operation)
        {
            progressCallback.Invoke(Phase, true, 0, Operation);
        }

        internal static void Log(this ProgressCallback progressCallback, string Operation, int Progress)
        {
            progressCallback.Invoke(Phase, false, Progress, Operation);
        }
    }
}
