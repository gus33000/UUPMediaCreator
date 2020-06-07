// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Dism.Properties;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents an exception in the DismApi.
    /// </summary>
    [Serializable]
    public class DismException : Win32Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismException" /> class.
        /// </summary>
        public DismException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public DismException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified. </param>
        public DismException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismException" /> class.
        /// </summary>
        /// <param name="error">The error code for the error.</param>
        public DismException(int error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismException" /> class with a specified error that is the cause of this exception.
        /// </summary>
        /// <param name="errorCode">The HRESULT, a coded numerical value that is assigned to a specific exception.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        internal DismException(int errorCode, string message)
            : base(errorCode, message)
        {
            HResult = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismException" /> class.
        /// </summary>
        /// <param name="serializationInfo">The <see cref="SerializationInfo" />.</param>
        /// <param name="streamingContext">The <see cref="StreamingContext" />.</param>
        protected DismException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a <see cref="DismException" /> or <see cref="Exception" /> for the specified error code.
        /// </summary>
        /// <param name="errorCode">The error code to get an exception for.</param>
        /// <returns>A <see cref="DismException" /> or <see cref="Exception" /> that represents the error code.</returns>
        internal static Exception GetDismExceptionForHResult(int errorCode)
        {
            // Look for known error codes
            switch ((uint)errorCode)
            {
                case DismApi.ERROR_REQUEST_ABORTED:
                case 0x80070000 ^ DismApi.ERROR_REQUEST_ABORTED:
                case DismApi.ERROR_CANCELLED:
                case 0x80070000 ^ DismApi.ERROR_CANCELLED:
                    return new OperationCanceledException();

                case DismApi.ERROR_SUCCESS_REBOOT_REQUIRED:
                    return new DismRebootRequiredException(errorCode);

                case DismApi.DISMAPI_E_DISMAPI_NOT_INITIALIZED:
                    // User has not called DismApi.Initialize()
                    return new DismNotInitializedException(errorCode);

                case DismApi.DISMAPI_E_OPEN_SESSION_HANDLES:
                    // User has not called CloseSession() on open sessions
                    return new DismOpenSessionsException(errorCode);

                case DismApi.CBS_E_NOT_APPLICABLE:
                    return new DismPackageNotApplicableException(errorCode);
            }

            // Attempt to get an error message from the DismApi
            string lastError = DismApi.GetLastErrorMessage();

            // See if the result is not null
            if (!string.IsNullOrEmpty(lastError))
            {
                // Return a DismException object
                return new DismException(errorCode, lastError.Trim());
            }

            // Return an Exception for the HResult
            return Marshal.GetExceptionForHR(errorCode);
        }
    }

    /// <summary>
    /// The exception that is thrown when an attempt to use the DismApi occurs without first calling <see cref="DismApi.Initialize(DismLogLevel)" />.
    /// </summary>
    [Serializable]
    public sealed class DismNotInitializedException : DismException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismNotInitializedException" /> class.
        /// </summary>
        public DismNotInitializedException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismNotInitializedException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public DismNotInitializedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismNotInitializedException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified. </param>
        public DismNotInitializedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismNotInitializedException" /> class.
        /// </summary>
        /// <param name="errorCode">The error code to associate with the exception.</param>
        internal DismNotInitializedException(int errorCode)
            : base(errorCode, Resources.DismExceptionMessageNotInitialized)
        {
        }

        private DismNotInitializedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when an attempt to shutdown the Dism API occurs while there are open sessions.
    /// </summary>
    [Serializable]
    public sealed class DismOpenSessionsException : DismException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismOpenSessionsException" /> class.
        /// </summary>
        public DismOpenSessionsException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismOpenSessionsException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public DismOpenSessionsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismOpenSessionsException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified. </param>
        public DismOpenSessionsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismOpenSessionsException" /> class.
        /// </summary>
        /// <param name="errorCode">The error code to associate with the exception.</param>
        internal DismOpenSessionsException(int errorCode)
            : base(errorCode, Resources.DismExceptionMessageOpenSessions)
        {
        }

        private DismOpenSessionsException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when a package is not applicable to a particular session.
    /// </summary>
    [Serializable]
    public sealed class DismPackageNotApplicableException : DismException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismPackageNotApplicableException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code to associate with the exception.</param>
        public DismPackageNotApplicableException(int errorCode)
        : base(errorCode, Resources.DismExceptionMessagePackageNotApplicable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismPackageNotApplicableException"/> class.
        /// </summary>
        public DismPackageNotApplicableException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismPackageNotApplicableException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public DismPackageNotApplicableException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismPackageNotApplicableException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified. </param>
        public DismPackageNotApplicableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when the previous operations requires a reboot.
    /// </summary>
    [Serializable]
    public sealed class DismRebootRequiredException : DismException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismRebootRequiredException" /> class.
        /// </summary>
        public DismRebootRequiredException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismRebootRequiredException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public DismRebootRequiredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismRebootRequiredException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified. </param>
        public DismRebootRequiredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DismRebootRequiredException" /> class.
        /// </summary>
        /// <param name="errorCode">The error code to associate with the exception.</param>
        internal DismRebootRequiredException(int errorCode)
            : base(errorCode, Resources.DismExceptionMessageRebootRequired)
        {
        }

        private DismRebootRequiredException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}