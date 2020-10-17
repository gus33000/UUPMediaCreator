// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

#if NET40
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Allows you to obtain the method or property name of the caller to the method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal class CallerMemberNameAttribute : Attribute
    {
    }
}
#endif