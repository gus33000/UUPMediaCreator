// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;

using DWORD = System.UInt32;
using LARGE_INTEGER = System.UInt64;

namespace Microsoft.Wim
{
    /// <summary>
    /// Specifies a method to call during process of a file copy.
    /// </summary>
    /// <param name="progress">A <see cref="CopyFileProgress"/> containing the status of the file copy.</param>
    /// <param name="userData">User specified data that was passed to the original copy method.</param>
    /// <returns>A <see cref="CopyFileProgressAction"/> value indicating if the file copy should be canceled.</returns>
    public delegate CopyFileProgressAction CopyFileProgressCallback(CopyFileProgress progress, object userData);

    /// <summary>
    /// Indicates the action to take during copy file progress.
    /// </summary>
    public enum CopyFileProgressAction : uint
    {
        /// <summary>
        /// Indicates that CopyFileEx should continue the copy operation.
        /// </summary>
        Continue = 0x00000000,

        /// <summary>
        /// Indicates that CopyFileEx should cancel the copy operation and delete the destination file.
        /// </summary>
        Cancel = 0x00000001,

        /// <summary>
        /// Indicates that CopyFileEx should stop the copy operation. It can be restarted at a later time.
        /// </summary>
        Stop = 0x00000002,

        /// <summary>
        /// Indicates that CopyFileEx should continue the copy operation, but stop invoking CopyProgressRoutine to report progress.
        /// </summary>
        Quiet = 0x00000003,
    }

    /// <summary>
    /// Indicates the reason CopyProgressRoutine was called.
    /// </summary>
    internal enum CopyFileProgressCallbackReason : uint
    {
        /// <summary>
        /// Another part of the data file was copied.
        /// </summary>
        ChunkFinished = 0x00000000,

        /// <summary>
        /// Another stream was created and is about to be copied. This is the callback reason given when the callback routine is first invoked.
        /// </summary>
        StreamSwitch = 0x00000001,
    }

    /// <summary>
    /// Represents the status of a file copy.
    /// </summary>
    public sealed class CopyFileProgress
    {
        /// <summary>
        /// The <see cref="CopyFileProgressCallback"/> method the user wants called when progress is made.
        /// </summary>
        private readonly CopyFileProgressCallback _progressCallback;

        /// <summary>
        /// Stores the user data and is passed to the user's callback.
        /// </summary>
        private readonly object _userData;

        /// <summary>
        /// The DateTime when the file copy began.
        /// </summary>
        private DateTime _timeStarted = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFileProgress"/> class.
        /// </summary>
        /// <param name="sourceFilePath">The full path to the source file being copied.</param>
        /// <param name="destinationFilePath">The full path to the destination file being copied.</param>
        /// <param name="copyProgressCallback">A <see cref="CopyFileProgressCallback"/> method to call when progress is made.</param>
        /// <param name="userData">An object containing data to be used by the method.</param>
        public CopyFileProgress(string sourceFilePath, string destinationFilePath, CopyFileProgressCallback copyProgressCallback, object userData)
        {
            // Save the input parameters (Validation should be done by CopyFileEx since this is an internal constructor)
            SourceFilePath = sourceFilePath;
            DestinationFilePath = destinationFilePath;

            // It is OK for this to be null
            _progressCallback = copyProgressCallback;

            // Save the user's object for later user
            _userData = userData;
        }

        /// <summary>
        /// Gets the path to the destination file being copied.
        /// </summary>
        public string DestinationFilePath
        {
            get;
        }

        /// <summary>
        /// Gets an estimated amount of time remaining until the copy operation will complete.
        /// </summary>
        public TimeSpan EstimatedTimeRemaining
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the percentage of the file that has been copied since the copy operation began.
        /// </summary>
        public decimal PercentComplete
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the path to the source file being copied.
        /// </summary>
        public string SourceFilePath
        {
            get;
        }

        /// <summary>
        /// Gets the total size of the file, in bytes.
        /// </summary>
        public long TotalFileSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total number of bytes transferred from the source file to the destination file since the copy operation began.
        /// </summary>
        public long TransferredBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// A callback method for the native CopyFileEx function.
        ///
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa363854(v=vs.85).aspx
        /// </summary>
        /// <param name="totalFileSize">The total size of the file, in bytes.</param>
        /// <param name="totalBytesTransferred">The total number of bytes transferred from the source file to the destination file since the copy operation began.</param>
        /// <param name="streamSize">The total size of the current file stream, in bytes.</param>
        /// <param name="streamBytesTransferred">The total number of bytes in the current stream that have been transferred from the source file to the destination file since the copy operation began.</param>
        /// <param name="streamNumber">A handle to the current stream. The first time CopyProgressRoutine is called, the stream number is 1.</param>
        /// <param name="callbackReason">The reason that CopyProgressRoutine was called.</param>
        /// <param name="sourceFile">A handle to the source file.</param>
        /// <param name="destinationFile">A handle to the destination file.</param>
        /// <param name="data">Argument passed to CopyProgressRoutine by CopyFileEx, MoveFileTransacted, or MoveFileWithProgress.</param>
        /// <returns>The CopyProgressRoutine function should return one of the following values.
        /// PROGRESS_CANCEL - Cancel the copy operation and delete the destination file.
        /// PROGRESS_CONTINUE - Continue the copy operation.
        /// PROGRESS_QUIET - Continue the copy operation, but stop invoking CopyProgressRoutine to report progress.
        /// PROGRESS_STOP - Stop the copy operation. It can be restarted at a later time.
        /// </returns>
        public CopyFileProgressAction CopyProgressHandler(LARGE_INTEGER totalFileSize, LARGE_INTEGER totalBytesTransferred, LARGE_INTEGER streamSize, LARGE_INTEGER streamBytesTransferred, DWORD streamNumber, DWORD callbackReason, IntPtr sourceFile, IntPtr destinationFile, IntPtr data)
        {
            // See if a user callback was specified
            if (_progressCallback == null)
            {
                // Tell CopyFileEx to stop updating progress
                return CopyFileProgressAction.Quiet;
            }

            // See if the copy just started
            if (_timeStarted == DateTime.MinValue)
            {
                // Save the time the copy started
                _timeStarted = DateTime.Now;

                // Set total file size
                TotalFileSize = (long)totalFileSize;
            }

            // See if copy progress was made and the file has any content
            if (callbackReason == (DWORD)CopyFileProgressCallbackReason.ChunkFinished)
            {
                // Default the percent complete to 100
                decimal percentComplete = 1.0m;

                // See if the file has any content and that it isn't completely copied
                if (totalFileSize > 0 && totalBytesTransferred < totalFileSize)
                {
                    // Calculate the percent complete rounded to the nearest tenth
                    percentComplete = Math.Round(totalBytesTransferred / (decimal)totalFileSize, 2);
                }

                // See if progress was made percent-wise
                if (percentComplete != PercentComplete)
                {
                    // Set transferred bytes
                    TransferredBytes = (long)totalBytesTransferred;

                    // Set percent complete
                    PercentComplete = percentComplete;

                    // Calculate the estimated time remaining in seconds
                    EstimatedTimeRemaining = TimeSpan.FromSeconds(((DateTime.Now - _timeStarted).TotalSeconds / totalBytesTransferred) * (totalFileSize - totalBytesTransferred));

                    // Execute the user's callback method
                    return _progressCallback(this, _userData);
                }
            }

            // Return PROGRESS_CONTINUE to allow progress to continue
            return CopyFileProgressAction.Continue;
        }
    }
}