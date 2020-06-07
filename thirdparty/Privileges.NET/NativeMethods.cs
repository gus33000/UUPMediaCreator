using System;
using System.Runtime.InteropServices;

namespace PrivilegeClass
{
	[Flags]
	internal enum TokenAccessLevels 
	{
		AssignPrimary       = 0x00000001,
		Duplicate           = 0x00000002,
		Impersonate         = 0x00000004,
		Query               = 0x00000008,
		QuerySource         = 0x00000010,
		AdjustPrivileges    = 0x00000020,
		AdjustGroups        = 0x00000040,
		AdjustDefault       = 0x00000080,
		AdjustSessionId     = 0x00000100,

		Read                = 0x00020000 | Query,

		Write               = 0x00020000 | AdjustPrivileges | AdjustGroups | AdjustDefault,

		AllAccess           = 0x000F0000       |
			AssignPrimary    |
			Duplicate        |
			Impersonate      |
			Query            |
			QuerySource      |
			AdjustPrivileges |
			AdjustGroups     |
			AdjustDefault    |
			AdjustSessionId,

		MaximumAllowed      = 0x02000000
	}

	internal enum SecurityImpersonationLevel
	{
		Anonymous = 0,
		Identification = 1,
		Impersonation = 2,
		Delegation = 3,
	}

	internal enum TokenType
	{
		Primary = 1,
		Impersonation = 2,
	}

	internal sealed class NativeMethods
	{
		internal const uint SE_PRIVILEGE_DISABLED           = 0x00000000;
		internal const uint SE_PRIVILEGE_ENABLED            = 0x00000002;

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct LUID 
		{
			internal uint LowPart;
			internal uint HighPart;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct LUID_AND_ATTRIBUTES 
		{
			internal LUID Luid;
			internal uint Attributes;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct TOKEN_PRIVILEGE 
		{
			internal uint                PrivilegeCount;
			internal LUID_AND_ATTRIBUTES Privilege;
		}

		internal const string ADVAPI32 = "advapi32.dll";
		internal const string KERNEL32 = "kernel32.dll";

		internal const int ERROR_SUCCESS = 0x0;
		internal const int ERROR_ACCESS_DENIED  = 0x5;
		internal const int ERROR_NOT_ENOUGH_MEMORY = 0x8;
		internal const int ERROR_NO_TOKEN = 0x3f0;
		internal const int ERROR_NOT_ALL_ASSIGNED = 0x514;
		internal const int ERROR_NO_SUCH_PRIVILEGE = 0x521;
		internal const int ERROR_CANT_OPEN_ANONYMOUS = 0x543;

		[DllImport(
			 KERNEL32,
			 SetLastError=true)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport(
			 ADVAPI32,
			 CharSet=CharSet.Unicode,
			 SetLastError=true)]
		internal static extern bool AdjustTokenPrivileges (
			[In]     IntPtr                TokenHandle,
			[In]     bool                  DisableAllPrivileges,
			[In]     ref TOKEN_PRIVILEGE   NewState,
			[In]     uint                  BufferLength,
			[In,Out] ref TOKEN_PRIVILEGE   PreviousState,
			[In,Out] ref uint              ReturnLength);

		[DllImport(
			 ADVAPI32,
			 CharSet=CharSet.Auto,
			 SetLastError=true)]
		internal static extern
			bool RevertToSelf();

		[DllImport(
			 ADVAPI32,
			 EntryPoint="LookupPrivilegeValueW",
			 CharSet=CharSet.Auto,
			 SetLastError=true)]
		internal static extern
			bool LookupPrivilegeValue (
			[In]     string             lpSystemName,
			[In]     string             lpName,
			[In,Out] ref LUID           Luid);

		[DllImport(
			 KERNEL32,
			 CharSet=CharSet.Auto,
			 SetLastError=true)]
		internal static extern 
			IntPtr GetCurrentProcess();

		[DllImport(
			 KERNEL32,
			 CharSet=CharSet.Auto,
			 SetLastError=true)]
		internal static extern 
			IntPtr GetCurrentThread();

		[DllImport(
			 ADVAPI32,
			 CharSet=CharSet.Unicode,
			 SetLastError=true)]
		internal static extern 
			bool OpenProcessToken (
			[In]     IntPtr              ProcessToken,
			[In]     TokenAccessLevels   DesiredAccess,
			[In,Out] ref IntPtr	         TokenHandle);

		[DllImport
			 (ADVAPI32,
			 CharSet=CharSet.Unicode,
			 SetLastError=true)]
		internal static extern
			bool OpenThreadToken(
			[In]     IntPtr              ThreadToken,
			[In]     TokenAccessLevels   DesiredAccess,
			[In]     bool                OpenAsSelf,
			[In,Out] ref IntPtr          TokenHandle);

		[DllImport
			 (ADVAPI32,
			 CharSet=CharSet.Unicode,
			 SetLastError=true)]
		internal static extern
			bool DuplicateTokenEx(
			[In]    IntPtr              ExistingToken,
			[In]    TokenAccessLevels   DesiredAccess,
			[In]    IntPtr              TokenAttributes,
			[In]    SecurityImpersonationLevel  ImpersonationLevel,
			[In]    TokenType           TokenType,
			[In,Out] ref IntPtr         NewToken);

		[DllImport
			 (ADVAPI32,
			 CharSet=CharSet.Unicode,
			 SetLastError=true)]
		internal static extern
			bool SetThreadToken(
			[In]    IntPtr              Thread,
			[In]    IntPtr              Token);
		
		static NativeMethods()
		{
		}
	}
}
