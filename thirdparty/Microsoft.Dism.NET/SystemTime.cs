// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

using WORD = System.UInt16;

namespace Microsoft.Dism
{
    /// <summary>
    /// Specifies a date and time, using individual members for the month, day, year, weekday, hour, minute, second, and millisecond. The time is either in coordinated universal time (UTC) or local time, depending on the function that is being called.
    /// </summary>
    /// <remarks>It is not recommended that you add and subtract values from the SYSTEMTIME structure to obtain relative times. Instead, you should
    /// <list type="bullet">
    ///     <item><description>Convert the SYSTEMTIME structure to a FILETIME structure.</description></item>
    ///     <item><description>Copy the resulting FILETIME structure to a ULARGE_INTEGER structure.</description></item>
    ///     <item><description>Use normal 64-bit arithmetic on the ULARGE_INTEGER value.</description></item>
    /// </list>
    /// The system can periodically refresh the time by synchronizing with a time source. Because the system time can be adjusted either forward or backward, do not compare system time readings to determine elapsed time. Instead, use one of the methods described in Windows Time.</remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms724950(v=vs.85).aspx" />
    /// <![CDATA[typedef struct _SYSTEMTIME {
    /// WORD wYear;
    /// WORD wMonth;
    /// WORD wDayOfWeek;
    /// WORD wDay;
    /// WORD wHour;
    /// WORD wMinute;
    /// WORD wSecond;
    /// WORD wMilliseconds;
    /// } SYSTEMTIME, *PSYSTEMTIME;]]>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct SystemTime
    {
        /// <summary>
        /// The year. The valid values for this member are 1601 through 30827.
        /// </summary>
        public WORD Year;

        /// <summary>
        /// The month. January = 1 and December = 12
        /// </summary>
        public WORD Month;

        /// <summary>
        /// The day of the week. Sunday = 0 and Saturday = 6
        /// </summary>
        public WORD DayOfWeek;

        /// <summary>
        /// The day of the month. The valid values for this member are 1 through 31.
        /// </summary>
        public WORD Day;

        /// <summary>
        /// The hour. The valid values for this member are 0 through 23.
        /// </summary>
        public WORD Hour;

        /// <summary>
        /// The minute. The valid values for this member are 0 through 59.
        /// </summary>
        public WORD Minute;

        /// <summary>
        /// The second. The valid values for this member are 0 through 59.
        /// </summary>
        public WORD Second;

        /// <summary>
        /// The millisecond. The valid values for this member are 0 through 999.
        /// </summary>
        public WORD Milliseconds;

        /// <summary>
        /// Converts a <see cref="System.DateTime" /> to a <see cref="SystemTime" />.
        /// </summary>
        /// <param name="dateTime">The time to convert.</param>
        public static implicit operator SystemTime(DateTime dateTime)
        {
            DateTime utc = dateTime.ToUniversalTime();

            return new SystemTime
            {
                Year = (WORD)utc.Year,
                Month = (WORD)utc.Month,
                Day = (WORD)utc.Day,
                DayOfWeek = (WORD)utc.DayOfWeek,
                Hour = (WORD)utc.Hour,
                Minute = (WORD)utc.Minute,
                Second = (WORD)utc.Second,
                Milliseconds = (WORD)utc.Millisecond,
            };
        }

        /// <summary>
        /// Converts a <see cref="SystemTime" /> to a <see cref="System.DateTime" />
        /// </summary>
        /// <param name="systemTime">The time to convert.</param>
        public static implicit operator DateTime(SystemTime systemTime)
        {
            return systemTime.Year == 0
                ? DateTime.MinValue
                : new DateTime(systemTime.Year, systemTime.Month, systemTime.Day, systemTime.Hour, systemTime.Minute, systemTime.Second, DateTimeKind.Utc);
        }
    }
}