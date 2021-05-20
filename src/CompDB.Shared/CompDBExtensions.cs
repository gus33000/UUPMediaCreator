/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
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
            foreach (CompDBXmlClass.CompDB compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase) &&
                        compDB.Tags.Tag?.Find(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true)
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
            HashSet<CompDBXmlClass.CompDB> filteredCompDBs = new();

            foreach (CompDBXmlClass.CompDB compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                // TODO: Do not do contains
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                        compDB.Tags.Tag?.Count == 3 &&
                        compDB.Tags.Tag.Find(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                        compDB.Tags.Tag.Find(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
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
            HashSet<CompDBXmlClass.CompDB> filteredCompDBs = new();

            foreach (CompDBXmlClass.CompDB compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                // TODO: Do not do contains
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                        compDB.Tags.Tag?.Count == 3 &&
                        compDB.Tags.Tag.Find(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
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
                    return x.Tags.Tag == null || x.Tags.Tag.Count == 0
                        ? null
                        : x.Tags.Tag.Find(y => y.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase)).Value;
                }
                else if (x.Features.Feature != null && Array.Find(x.Features.Feature, y =>
                       y.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    return Array.Find(x.Features.Feature, y =>
                       y.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true).FeatureID.Split('_')[1];
                }
                return null;
            }).Where(x => !string.IsNullOrEmpty(x)).Distinct();
        }

        public static CompDBXmlClass.Package GetEditionPackFromCompDBs(this IEnumerable<CompDBXmlClass.CompDB> compDBs)
        {
            HashSet<CompDBXmlClass.Package> pkgs = new();

            //
            // Get base editions that are available with all their files
            //
            HashSet<CompDBXmlClass.CompDB> filteredCompDBs = compDBs.GetEditionCompDBs();

            if (filteredCompDBs.Count > 0)
            {
                foreach (CompDBXmlClass.CompDB compDB in filteredCompDBs)
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
                foreach (CompDBXmlClass.Package pkg in pkgs)
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
