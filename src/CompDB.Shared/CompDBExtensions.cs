using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompDB
{
    public static class CompDBExtensions
    {
        public static string GetCommonlyUsedIncorrectFileName(this CompDBXmlClass.Package pkg)
        {
            return pkg.Payload.PayloadItem.Path.Replace('\\', Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar).Last().Replace("~31bf3856ad364e35", "").Replace("~.", ".").Replace("~", "-").Replace("-.", ".");
        }

        public static CompDBXmlClass.CompDB GetNeutralCompDB(this IEnumerable<CompDBXmlClass.CompDB> compDBs)
        {
            foreach (var compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase) &&
                        compDB.Tags.Tag != null &&
                        compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        return compDB;
                    }
                }
                //
                // Older style compdbs have no tag elements, we need to find out if it's a neutral compdb using another way
                //
                else if (compDB.Features?.Feature?.FirstOrDefault(x =>
                    x.Type?.Contains("BaseNeutral", StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    return compDB;
                }
            }

            return null;
        }

        public static HashSet<CompDBXmlClass.CompDB> GetEditionCompDBsForLanguage(
            this IEnumerable<CompDBXmlClass.CompDB> compDBs,
            string LanguageCode)
        {
            HashSet<CompDBXmlClass.CompDB> filteredCompDBs = new HashSet<CompDBXmlClass.CompDB>();

            foreach (var compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                // TODO: Do not do contains
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                        compDB.Tags.Tag != null &&
                        compDB.Tags.Tag.Count == 3 &&
                        compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                        compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
                        compDB.Tags.Tag.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        filteredCompDBs.Add(compDB);
                    }
                }
                //
                // Older style compdbs have no tag elements, we need to find out if it's an edition compdb using another way
                //
                else if (compDB.Features?.Feature?.FirstOrDefault(x =>
                        x.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true &&
                        x.FeatureID?.Contains(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    filteredCompDBs.Add(compDB);
                }
            }

            return filteredCompDBs;
        }

        public static HashSet<CompDBXmlClass.CompDB> GetEditionCompDBs(this IEnumerable<CompDBXmlClass.CompDB> compDBs)
        {
            HashSet<CompDBXmlClass.CompDB> filteredCompDBs = new HashSet<CompDBXmlClass.CompDB>();

            foreach (var compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                // TODO: Do not do contains
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                        compDB.Tags.Tag != null &&
                        compDB.Tags.Tag.Count == 3 &&
                        compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                        compDB.Tags.Tag.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        filteredCompDBs.Add(compDB);
                    }
                }
                //
                // Older style compdbs have no tag elements, we need to find out if it's an edition compdb using another way
                //
                else if (compDB.Features?.Feature?.FirstOrDefault(x =>
                        x.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    filteredCompDBs.Add(compDB);
                }
            }

            return filteredCompDBs;
        }

        public static IEnumerable<string> GetAvailableLanguages(this IEnumerable<CompDBXmlClass.CompDB> compDBs)
        {
            return compDBs.GetEditionCompDBs().Select(x =>
            {
                if (x.Tags != null)
                {
                    if (x.Tags.Tag == null || x.Tags.Tag.Count <= 0)
                    {
                        return null;
                    }
                    return x.Tags.Tag.FirstOrDefault(y => y.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase)).Value;
                }
                else if (x.Features.Feature != null && x.Features.Feature.FirstOrDefault(y =>
                       y.Type != null && y.Type.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase)) != null)
                {
                    return x.Features.Feature.FirstOrDefault(y =>
                       y.Type != null && y.Type.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase)).FeatureID.Split('_')[1];
                }
                return null;
            }).Where(x => !string.IsNullOrEmpty(x)).Distinct();
        }

        public static CompDBXmlClass.Package GetEditionPackFromCompDBs(this IEnumerable<CompDBXmlClass.CompDB> compDBs)
        {
            HashSet<CompDBXmlClass.Package> pkgs = new HashSet<CompDBXmlClass.Package>();

            //
            // Get base editions that are available with all their files
            //
            HashSet<CompDBXmlClass.CompDB> filteredCompDBs = compDBs.GetEditionCompDBs();

            if (filteredCompDBs.Count() > 0)
            {
                foreach (var compDB in filteredCompDBs)
                {
                    foreach (CompDBXmlClass.Package feature in filteredCompDBs.First().Features.Feature[0].Packages.Package)
                    {
                        CompDBXmlClass.Package pkg = filteredCompDBs.First().Packages.Package.First(x => x.ID == feature.ID);

                        string file = pkg.Payload.PayloadItem.Path.Replace('\\', Path.DirectorySeparatorChar);

                        if (!file.EndsWith(".esd", StringComparison.InvariantCultureIgnoreCase) ||
                            !file.Contains("microsoft-windows-editionspecific", StringComparison.InvariantCultureIgnoreCase) ||
                            file.Contains("WOW64", StringComparison.InvariantCultureIgnoreCase) ||
                            file.Contains("arm64.arm", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // We do not care about this file
                            continue;
                        }

                        pkgs.Add(pkg);
                    }
                }
            }

            CompDBXmlClass.Package minpkg = null;
            if (pkgs.Count > 0)
            {
                foreach (var pkg in pkgs)
                {
                    if (minpkg == null)
                    {
                        minpkg = pkg;
                    }
                    else if (ulong.Parse(minpkg.Payload.PayloadItem.PayloadSize) > ulong.Parse(pkg.Payload.PayloadItem.PayloadSize))
                    {
                        minpkg = pkg;
                    }
                }
            }

            return minpkg;
        }
    }
}
