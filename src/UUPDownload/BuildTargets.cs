using CompDB;
using MediaCreationLib.Planning.NET;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowsUpdateLib;

namespace UUPDownload
{
    public static class BuildTargets
    {
        public static async Task<List<EditionTarget>> GetTargetedPlanAsync(this UpdateData update, string LanguageCode)
        {
            var compDBs = await update.GetCompDBsAsync();
            CompDBXmlClass.Package? editionPackPkg = compDBs.GetEditionPackFromCompDBs();

            string editionPkg = await update.DownloadFileFromDigestAsync(editionPackPkg.Payload.PayloadItem.PayloadHash);

            List<EditionTarget> targets;
            _ = ConversionPlanBuilder.GetTargetedPlan(compDBs, editionPkg, LanguageCode, out targets, null);
            return targets;
        }

        public static void PrintAvailablePlan(this List<EditionTarget> targets)
        {
            foreach (var target in targets)
            {
                foreach (var str in ConversionPlanBuilder.PrintEditionTarget(target))
                    Logging.Log(str);
            }
        }
    }
}
