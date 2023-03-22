using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Windows.Internal.Flighting
{
    [Guid("0723a53d-52e6-453b-9361-7826398f0111")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    [ComImport]
    internal interface IClientAttributes
    {
        [PreserveSig]
        int ToJsonString([MarshalAs(UnmanagedType.HString)] out string jsonString);

        [PreserveSig]
        int ToUriQueryString([MarshalAs(UnmanagedType.HString)] out string uriQueryString);

        IReadOnlyDictionary<string, int> AttributeErrors { get; }
    }
}
