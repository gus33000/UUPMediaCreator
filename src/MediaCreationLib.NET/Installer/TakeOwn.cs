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
using PrivilegeClass;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace MediaCreationLib.Installer
{
    public class TakeOwn
    {
        public static void TakeOwnDirectory(string path)
        {
            DirectoryInfo directoryInfo = new(path);
            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

            directorySecurity.SetOwner(WindowsIdentity.GetCurrent().User);

            Privilege p = new(Privilege.TakeOwnership);
            bool ownershipTaken = false;
            try
            {
                p.Enable();

                new DirectoryInfo(path).SetAccessControl(directorySecurity);
                ownershipTaken = true;
            }
            catch (PrivilegeClass.PrivilegeNotHeldException e)
            {
                Console.WriteLine("Failed to assign privileges. " + e.ToString());
            }
            finally
            {
                p.Revert();
            }

            if (ownershipTaken)
            {
                AdjustPermissionsForDirectory(path);

                System.Collections.Generic.IEnumerable<string> subFiles = Directory.EnumerateFiles(path);
                foreach (string subFile in subFiles)
                {
                    TakeOwnFile(subFile);
                }

                System.Collections.Generic.IEnumerable<string> subDirectories = Directory.EnumerateDirectories(path);
                foreach (string subDir in subDirectories)
                {
                    TakeOwnDirectory(subDir);
                }
            }
        }

        private static void AdjustPermissionsForDirectory(string path)
        {
            DirectoryInfo directoryInfo = new(path);
            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

            directorySecurity.SetAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.FullControl, AccessControlType.Allow));

            Privilege p = new(Privilege.TakeOwnership);
            try
            {
                p.Enable();

                new DirectoryInfo(path).SetAccessControl(directorySecurity);
            }
            catch (PrivilegeClass.PrivilegeNotHeldException e)
            {
                Console.WriteLine("Failed to assign privileges. " + e.ToString());
            }
            finally
            {
                p.Revert();
            }
        }

        public static void TakeOwnFile(string path)
        {
            FileInfo fileInfo = new(path);
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            fileSecurity.SetOwner(WindowsIdentity.GetCurrent().User);

            Privilege p = new(Privilege.TakeOwnership);
            bool ownershipTaken = false;
            try
            {
                p.Enable();

                new FileInfo(path).SetAccessControl(fileSecurity);
                ownershipTaken = true;
            }
            catch (PrivilegeClass.PrivilegeNotHeldException e)
            {
                Console.WriteLine("Failed to assign privileges. " + e.ToString());
            }
            finally
            {
                p.Revert();
            }

            if (ownershipTaken)
            {
                AdjustPermissionsForFile(path);
            }
        }

        private static void AdjustPermissionsForFile(string path)
        {
            FileInfo fileInfo = new(path);
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            fileSecurity.SetAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.FullControl, AccessControlType.Allow));

            Privilege p = new(Privilege.TakeOwnership);
            try
            {
                p.Enable();

                new FileInfo(path).SetAccessControl(fileSecurity);
            }
            catch (PrivilegeClass.PrivilegeNotHeldException e)
            {
                Console.WriteLine("Failed to assign privileges. " + e.ToString());
            }
            finally
            {
                p.Revert();
            }
        }
    }
}