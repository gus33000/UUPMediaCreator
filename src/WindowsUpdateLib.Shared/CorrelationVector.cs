using System;

//
// Source: https://raw.githubusercontent.com/ADeltaX/ProtoBuildBot/main/src/BuildChecker/Classes/Helpers/CorrelationVector.cs
// Released under the MIT License (as of 2020-11-04)
//

namespace WindowsUpdateLib
{
    internal class CorrelationVector
    {

        private string baseVector;
        private int currentVector;

        private readonly string base64CharSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private readonly int id0Length = 16;
        bool isInitialized = false;

        internal enum Settings
        {
            SYNCREFRESHINTERVAL,
            QUEUEDRAININTERVAL,
            SNAPSHOTSCHEDULEINTERVAL,
            MAXEVENTSIZEINBYTES,
            MAXEVENTSPERPOST,
            SAMPLERATE,
            MAXFILESSPACE,
            UPLOADENABLED,
            PERSISTENCE,
            LATENCY,
            HTTPTIMEOUTINTERVAL,
            THREADSTOUSEWITHEXECUTOR,
            MAXCORRELATIONVECTORLENGTH,
            MAXCRITICALCANADDATTEMPTS,
            MAXRETRYPERIOD,
            BASERETRYPERIOD,
            CONSTANTFORRETRYPERIOD,
            NORMALEVENTMEMORYQUEUESIZE,
            CLLSETTINGSURL,
            HOSTSETTINGSETAG,
            CLLSETTINGSETAG,
            VORTEXPRODURL
        }

        internal void Init()
        {
            baseVector = SeedCorrelationVector();
            currentVector = 1;
            isInitialized = true;
        }

        protected static int getCllSettingsAsInt(Settings setting)
        {
            int asInt = (int)setting;
            return asInt;
        }

        private bool CanExtend()
        {
            int vectorSize = (int)Math.Floor(Math.Log10(currentVector) + 1);

            if (baseVector.Length + 1 + vectorSize + 1 + 1 > getCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH))
            {
                return false;
            }

            return true;
        }

        private bool CanIncrement(int newVector)
        {
            if (newVector - 1 == int.MaxValue)
            {
                return false;
            }
            int vectorSize = (int)Math.Floor(Math.Log10(newVector) + 1);

            // Get the length of the existing string + length of the new extension + the length of the dot
            if (baseVector.Length + vectorSize + 1 > getCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH))
            {
                return false;
            }

            return true;
        }

        internal string Extend()
        {
            if (!isInitialized)
            {
                Init();
            }

            if (CanExtend())
            {
                baseVector = GetValue();
                currentVector = 1;
            }

            return GetValue();
        }

        internal string GetValue()
        {
            if (!isInitialized)
            {
                return null;
            }

            return baseVector + "." + currentVector;
        }

        internal string Increment()
        {
            if (!isInitialized)
            {
                Init();
            }

            int newVector = currentVector + 1;
            // Check if we can increment
            if (CanIncrement(newVector))
            {
                currentVector = newVector;
            }

            return GetValue();
        }

        bool IsValid(string vector)
        {
            if (vector.Length > getCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH))
            {
                return false;
            }

            string validationPattern = "^[" + base64CharSet + "]{16}(.[0-9]+)+$";
            if (vector != validationPattern)
            {
                return false;
            }

            return true;
        }

        private string SeedCorrelationVector()
        {
            string result = "";

            Random r = new Random();
            for (int i = 0; i < id0Length; i++)
                result += base64CharSet[(r.Next(base64CharSet.Length))];

            return result;
        }

        internal void SetValue(string vector)
        {
            if (IsValid(vector))
            {
                int lastDot = vector.LastIndexOf(".");
                baseVector = vector.Substring(0, lastDot);
                currentVector = int.Parse(vector.Substring(lastDot + 1));
                isInitialized = true;
            }
            else
            {
                throw new Exception("Cannot set invalid correlation vector value");
            }
        }
    }
}