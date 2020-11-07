using CompDB;
using MediaCreationLib.Planning.NET;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowsUpdateLib;

namespace UUPDownload
{
    public static class BuildTargets
    {
        public class EditionPlanningWithLanguage
        {
            public List<EditionTarget> EditionTargets;
            public string LanguageCode;
        }

        public static async Task<EditionPlanningWithLanguage> GetTargetedPlanAsync(this UpdateData update, string LanguageCode)
        {
            var compDBs = await update.GetCompDBsAsync();
            CompDBXmlClass.Package editionPackPkg = compDBs.GetEditionPackFromCompDBs();

            string editionPkg = await update.DownloadFileFromDigestAsync(editionPackPkg.Payload.PayloadItem.PayloadHash);
            return await update.GetTargetedPlanAsync(LanguageCode, editionPkg);
        }

        public static async Task<EditionPlanningWithLanguage> GetTargetedPlanAsync(this UpdateData update, string LanguageCode, string editionPkg)
        {
            var compDBs = await update.GetCompDBsAsync();
            if (string.IsNullOrEmpty(editionPkg))
            {
                return null;
            }

            List<EditionTarget> targets;
            _ = ConversionPlanBuilder.GetTargetedPlan(compDBs, editionPkg, LanguageCode, out targets, null);
            return new EditionPlanningWithLanguage() { EditionTargets = targets, LanguageCode = LanguageCode };
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
