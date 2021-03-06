﻿/*
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
using System.Collections.Generic;
using System.IO;

namespace MediaCreationLib
{
    public static class Constants
    {
        internal static Dictionary<string, string> FriendlyEditionNames = new()
        {
            { "Starter", "Windows 10 Starter" },
            { "StarterN", "Windows 10 Starter N" },
            { "Core", "Windows 10 Home" },
            { "CoreCountrySpecific", "Windows 10 Home China" },
            { "CoreN", "Windows 10 Home N" },
            { "CoreSingleLanguage", "Windows 10 Home Single Language" },
            { "Education", "Windows 10 Education" },
            { "EducationN", "Windows 10 Education N" },
            { "Enterprise", "Windows 10 Enterprise" },
            { "EnterpriseEval", "Windows 10 Enterprise Evaluation" },
            { "EnterpriseG", "Windows 10 Enterprise G" },
            { "EnterpriseGN", "Windows 10 Enterprise G N" },
            { "EnterpriseN", "Windows 10 Enterprise N" },
            { "EnterpriseNEval", "Windows 10 Enterprise N Evaluation" },
            { "IoTEnterprise", "Windows 10 IoT Enterprise" },
            { "PPIPro", "Windows 10 Team" },
            { "Professional", "Windows 10 Pro" },
            { "ProfessionalCountrySpecific", "Windows 10 Pro China Only" },
            { "ProfessionalEducation", "Windows 10 Pro Education" },
            { "ProfessionalEducationN", "Windows 10 Pro Education N" },
            { "ProfessionalN", "Windows 10 Pro N" },
            { "ProfessionalSingleLanguage", "Windows 10 Pro Single Language" },
            { "ProfessionalWorkstation", "Windows 10 Pro for Workstations" },
            { "ProfessionalWorkstationN", "Windows 10 Pro N for Workstations" },
            { "ServerRdsh", "Windows 10 Enterprise multi-session" },
            { "EnterpriseS", "Windows 10 Enterprise LTSC" },
            { "EnterpriseSEval", "Windows 10 Enterprise LTSC Evaluation" },
            { "EnterpriseSN", "Windows 10 Enterprise N LTSC" },
            { "EnterpriseSNEval", "Windows 10 Enterprise N LTSC Evaluation" },
            { "IoTEnterpriseS", "Windows 10 IoT Enterprise LTSC" },
            { "Cloud", "Windows 10 S" },
            { "CloudN", "Windows 10 S N" },
            { "CloudE", "Windows 10 Lean" },
            { "CloudEN", "Windows 10 Lean N" },
            { "CloudEdition", "Windows 10 Cloud" },
        };

        internal static string[] SetupFilesToBackportStartingWith20231 = new string[]
        {
            $"sources{Path.DirectorySeparatorChar}imagelib.dll"
        };

        internal static string[] SetupFilesToBackport = new string[]
        {
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}appraiser.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}arunres.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}cmisetup.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}compatctrl.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}dism.exe.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}dismapi.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}dismcore.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}dismprov.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}folderprovider.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}imagingprovider.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}input.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}logprovider.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}MediaSetupUIMgr.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}nlsbres.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}pnpibs.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}reagent.adml",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}reagent.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}rollback.exe.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}setup.exe.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}setup_help_upgrade_or_custom.rtf",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}setupcompat.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}SetupCore.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}setupplatform.exe.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}SetupPrep.exe.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}smiengine.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}spwizres.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}upgloader.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}uxlibres.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}vhdprovider.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}vofflps.rtf",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}vofflps_server.rtf",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}w32uires.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}wdsclient.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}wdsimage.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}wimprovider.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}WinDlp.dll.mui",
            $"sources{Path.DirectorySeparatorChar}??-??{Path.DirectorySeparatorChar}winsetup.dll.mui",
            $"sources{Path.DirectorySeparatorChar}inf{Path.DirectorySeparatorChar}setup.cfg",
            $"sources{Path.DirectorySeparatorChar}alert.gif",
            $"sources{Path.DirectorySeparatorChar}appcompat.xsl",
            $"sources{Path.DirectorySeparatorChar}appcompat_bidi.xsl",
            $"sources{Path.DirectorySeparatorChar}appcompat_detailed_bidi_txt.xsl",
            $"sources{Path.DirectorySeparatorChar}appcompat_detailed_txt.xsl",
            $"sources{Path.DirectorySeparatorChar}appraiser.dll",
            $"sources{Path.DirectorySeparatorChar}ARUNIMG.dll",
            $"sources{Path.DirectorySeparatorChar}arunres.dll",
            $"sources{Path.DirectorySeparatorChar}autorun.dll",
            $"sources{Path.DirectorySeparatorChar}background.bmp",
            $"sources{Path.DirectorySeparatorChar}cmisetup.dll",
            $"sources{Path.DirectorySeparatorChar}compatctrl.dll",
            $"sources{Path.DirectorySeparatorChar}cryptosetup.dll",
            $"sources{Path.DirectorySeparatorChar}diager.dll",
            $"sources{Path.DirectorySeparatorChar}diagnostic.dll",
            $"sources{Path.DirectorySeparatorChar}diagtrack.dll",
            $"sources{Path.DirectorySeparatorChar}diagtrackrunner.exe",
            $"sources{Path.DirectorySeparatorChar}dism.exe",
            $"sources{Path.DirectorySeparatorChar}dismapi.dll",
            $"sources{Path.DirectorySeparatorChar}dismcore.dll",
            $"sources{Path.DirectorySeparatorChar}dismcoreps.dll",
            $"sources{Path.DirectorySeparatorChar}dismprov.dll",
            $"sources{Path.DirectorySeparatorChar}folderprovider.dll",
            $"sources{Path.DirectorySeparatorChar}hwcompat.dll",
            $"sources{Path.DirectorySeparatorChar}hwcompat.txt",
            $"sources{Path.DirectorySeparatorChar}hwexclude.txt",
            $"sources{Path.DirectorySeparatorChar}idwbinfo.txt",
            $"sources{Path.DirectorySeparatorChar}imagingprovider.dll",
            $"sources{Path.DirectorySeparatorChar}input.dll",
            $"sources{Path.DirectorySeparatorChar}lang.ini",
            $"sources{Path.DirectorySeparatorChar}locale.nls",
            $"sources{Path.DirectorySeparatorChar}logprovider.dll",
            $"sources{Path.DirectorySeparatorChar}MediaSetupUIMgr.dll",
            $"sources{Path.DirectorySeparatorChar}nlsbres.dll",
            $"sources{Path.DirectorySeparatorChar}ntdsupg.dll",
            $"sources{Path.DirectorySeparatorChar}offline.xml",
            $"sources{Path.DirectorySeparatorChar}pnpibs.dll",
            $"sources{Path.DirectorySeparatorChar}reagent.admx",
            $"sources{Path.DirectorySeparatorChar}reagent.dll",
            $"sources{Path.DirectorySeparatorChar}reagent.xml",
            $"sources{Path.DirectorySeparatorChar}rollback.exe",
            $"sources{Path.DirectorySeparatorChar}schema.dat",
            $"sources{Path.DirectorySeparatorChar}segoeui.ttf",
            $"sources{Path.DirectorySeparatorChar}setup.exe",
            $"sources{Path.DirectorySeparatorChar}setupcompat.dll",
            $"sources{Path.DirectorySeparatorChar}SetupCore.dll",
            $"sources{Path.DirectorySeparatorChar}SetupHost.exe",
            $"sources{Path.DirectorySeparatorChar}SetupMgr.dll",
            $"sources{Path.DirectorySeparatorChar}SetupPlatform.cfg",
            $"sources{Path.DirectorySeparatorChar}SetupPlatform.dll",
            $"sources{Path.DirectorySeparatorChar}SetupPlatform.exe",
            $"sources{Path.DirectorySeparatorChar}SetupPrep.exe",
            $"sources{Path.DirectorySeparatorChar}SmiEngine.dll",
            $"sources{Path.DirectorySeparatorChar}spflvrnt.dll",
            $"sources{Path.DirectorySeparatorChar}spprgrss.dll",
            $"sources{Path.DirectorySeparatorChar}spwizeng.dll",
            $"sources{Path.DirectorySeparatorChar}spwizimg.dll",
            $"sources{Path.DirectorySeparatorChar}spwizres.dll",
            $"sources{Path.DirectorySeparatorChar}sqmapi.dll",
            $"sources{Path.DirectorySeparatorChar}testplugin.dll",
            $"sources{Path.DirectorySeparatorChar}unattend.dll",
            $"sources{Path.DirectorySeparatorChar}unbcl.dll",
            $"sources{Path.DirectorySeparatorChar}upgloader.dll",
            $"sources{Path.DirectorySeparatorChar}upgrade_frmwrk.xml",
            $"sources{Path.DirectorySeparatorChar}uxlib.dll",
            $"sources{Path.DirectorySeparatorChar}uxlibres.dll",
            $"sources{Path.DirectorySeparatorChar}vhdprovider.dll",
            $"sources{Path.DirectorySeparatorChar}w32uiimg.dll",
            $"sources{Path.DirectorySeparatorChar}w32uires.dll",
            $"sources{Path.DirectorySeparatorChar}warning.gif",
            $"sources{Path.DirectorySeparatorChar}wdsclient.dll",
            $"sources{Path.DirectorySeparatorChar}wdsclientapi.dll",
            $"sources{Path.DirectorySeparatorChar}wdscore.dll",
            $"sources{Path.DirectorySeparatorChar}wdscsl.dll",
            $"sources{Path.DirectorySeparatorChar}wdsimage.dll",
            $"sources{Path.DirectorySeparatorChar}wdstptc.dll",
            $"sources{Path.DirectorySeparatorChar}wdsutil.dll",
            $"sources{Path.DirectorySeparatorChar}wimprovider.dll",
            $"sources{Path.DirectorySeparatorChar}wimgapi.dll",
            $"sources{Path.DirectorySeparatorChar}win32ui.dll",
            $"sources{Path.DirectorySeparatorChar}WinDlp.dll",
            $"sources{Path.DirectorySeparatorChar}winsetup.dll",
            $"sources{Path.DirectorySeparatorChar}wpx.dll",
            $"sources{Path.DirectorySeparatorChar}xmllite.dll",
            "setup.exe"
        };

        internal static byte[] winpejpg = new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46,
            0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x60,
            0x00, 0x60, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
            0x00, 0x02, 0x01, 0x01, 0x02, 0x01, 0x01, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x03,
            0x05, 0x03, 0x03, 0x03, 0x03, 0x03, 0x06, 0x04,
            0x04, 0x03, 0x05, 0x07, 0x06, 0x07, 0x07, 0x07,
            0x06, 0x07, 0x07, 0x08, 0x09, 0x0B, 0x09, 0x08,
            0x08, 0x0A, 0x08, 0x07, 0x07, 0x0A, 0x0D, 0x0A,
            0x0A, 0x0B, 0x0C, 0x0C, 0x0C, 0x0C, 0x07, 0x09,
            0x0E, 0x0F, 0x0D, 0x0C, 0x0E, 0x0B, 0x0C, 0x0C,
            0x0C, 0xFF, 0xDB, 0x00, 0x43, 0x01, 0x02, 0x02,
            0x02, 0x03, 0x03, 0x03, 0x06, 0x03, 0x03, 0x06,
            0x0C, 0x08, 0x07, 0x08, 0x0C, 0x0C, 0x0C, 0x0C,
            0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
            0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
            0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
            0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
            0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
            0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0xFF, 0xC0,
            0x00, 0x11, 0x08, 0x00, 0x20, 0x00, 0x20, 0x03,
            0x01, 0x22, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11,
            0x01, 0xFF, 0xC4, 0x00, 0x1F, 0x00, 0x00, 0x01,
            0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
            0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
            0x0A, 0x0B, 0xFF, 0xC4, 0x00, 0xB5, 0x10, 0x00,
            0x02, 0x01, 0x03, 0x03, 0x02, 0x04, 0x03, 0x05,
            0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7D, 0x01,
            0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21,
            0x31, 0x41, 0x06, 0x13, 0x51, 0x61, 0x07, 0x22,
            0x71, 0x14, 0x32, 0x81, 0x91, 0xA1, 0x08, 0x23,
            0x42, 0xB1, 0xC1, 0x15, 0x52, 0xD1, 0xF0, 0x24,
            0x33, 0x62, 0x72, 0x82, 0x09, 0x0A, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x25, 0x26, 0x27, 0x28, 0x29,
            0x2A, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A,
            0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A,
            0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A,
            0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A,
            0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A,
            0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A,
            0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99,
            0x9A, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8,
            0xA9, 0xAA, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7,
            0xB8, 0xB9, 0xBA, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6,
            0xC7, 0xC8, 0xC9, 0xCA, 0xD2, 0xD3, 0xD4, 0xD5,
            0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xE1, 0xE2, 0xE3,
            0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xF1,
            0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9,
            0xFA, 0xFF, 0xC4, 0x00, 0x1F, 0x01, 0x00, 0x03,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
            0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
            0x0A, 0x0B, 0xFF, 0xC4, 0x00, 0xB5, 0x11, 0x00,
            0x02, 0x01, 0x02, 0x04, 0x04, 0x03, 0x04, 0x07,
            0x05, 0x04, 0x04, 0x00, 0x01, 0x02, 0x77, 0x00,
            0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21, 0x31,
            0x06, 0x12, 0x41, 0x51, 0x07, 0x61, 0x71, 0x13,
            0x22, 0x32, 0x81, 0x08, 0x14, 0x42, 0x91, 0xA1,
            0xB1, 0xC1, 0x09, 0x23, 0x33, 0x52, 0xF0, 0x15,
            0x62, 0x72, 0xD1, 0x0A, 0x16, 0x24, 0x34, 0xE1,
            0x25, 0xF1, 0x17, 0x18, 0x19, 0x1A, 0x26, 0x27,
            0x28, 0x29, 0x2A, 0x35, 0x36, 0x37, 0x38, 0x39,
            0x3A, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
            0x4A, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
            0x5A, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
            0x6A, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79,
            0x7A, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88,
            0x89, 0x8A, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97,
            0x98, 0x99, 0x9A, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6,
            0xA7, 0xA8, 0xA9, 0xAA, 0xB2, 0xB3, 0xB4, 0xB5,
            0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xC2, 0xC3, 0xC4,
            0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xD2, 0xD3,
            0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xE2,
            0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA,
            0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9,
            0xFA, 0xFF, 0xDA, 0x00, 0x0C, 0x03, 0x01, 0x00,
            0x02, 0x11, 0x03, 0x11, 0x00, 0x3F, 0x00, 0xF9,
            0xBE, 0x8A, 0x28, 0xAF, 0xEB, 0x03, 0xF8, 0xAC,
            0x28, 0xA2, 0x8A, 0x00, 0x28, 0xA2, 0x8A, 0x00,
            0x28, 0xA2, 0x8A, 0x00, 0xFF, 0xD9
        };

        internal static string SOFTWARE_Hive_Location = Path.Combine("Windows", "System32", "config", "SOFTWARE");

        internal static string SYSTEM_Hive_Location = Path.Combine("Windows", "System32", "config", "SYSTEM");
    }
}