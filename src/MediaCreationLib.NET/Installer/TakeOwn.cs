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
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

            directorySecurity.SetOwner(WindowsIdentity.GetCurrent().User);

            Privilege p = new Privilege(Privilege.TakeOwnership);
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

                var subFiles = Directory.EnumerateFiles(path);
                foreach (var subFile in subFiles)
                {
                    TakeOwnFile(subFile);
                }

                var subDirectories = Directory.EnumerateDirectories(path);
                foreach (var subDir in subDirectories)
                {
                    TakeOwnDirectory(subDir);
                }
            }
        }

        private static void AdjustPermissionsForDirectory(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

            directorySecurity.SetAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.FullControl, AccessControlType.Allow));

            Privilege p = new Privilege(Privilege.TakeOwnership);
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
            FileInfo fileInfo = new FileInfo(path);
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            fileSecurity.SetOwner(WindowsIdentity.GetCurrent().User);

            Privilege p = new Privilege(Privilege.TakeOwnership);
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
            FileInfo fileInfo = new FileInfo(path);
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            fileSecurity.SetAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.FullControl, AccessControlType.Allow));

            Privilege p = new Privilege(Privilege.TakeOwnership);
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
