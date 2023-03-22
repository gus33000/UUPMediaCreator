using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Windows.Internal.Flighting
{
    [Guid("41845433-1668-4264-8a63-315eb82ab0d6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    [ComImport]
    internal interface ICommonTargetingAttributesFactory
    {
        [PreserveSig]
        int GetClientAttributesForApp([MarshalAs(UnmanagedType.HString)][In] string appId, [MarshalAs(UnmanagedType.HString)][In] string appVersion, out IClientAttributes ica);

        [PreserveSig]
        int GetClientAttributesFromList(IEnumerable<string> attributeList, out IClientAttributes ica);

        [PreserveSig]
        int GetClientAttributesForAppEx([MarshalAs(UnmanagedType.HString)][In] string appId, [MarshalAs(UnmanagedType.HString)][In] string appVersion, int clientAttributeFlags, out IClientAttributes ica);
    }
}