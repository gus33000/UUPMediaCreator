// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;

namespace Microsoft.Wim
{
    /// <summary>
    /// An application defined method to be called when messages are set from the Windows® imaging API.
    /// </summary>
    /// <param name="messageType">The <see cref="WimMessageType" /> of the message.</param>
    /// <param name="message">
    /// An object containing information about the message.  The object's type depends on the messageType
    /// parameter.
    /// </param>
    /// <param name="userData">A user-defined object passed when the callback was registered.</param>
    /// <returns>
    /// To indicate success and to enable other subscribers to process the message return
    /// <see cref="WimMessageResult.Success" />. To prevent other subscribers from receiving the message, return
    /// <see cref="WimMessageResult.Done" />. To cancel an image apply or image capture, return
    /// <see cref="WimMessageResult.Abort" /> when handling the <see cref="WimMessageProcess" /> message.
    /// </returns>
    public delegate WimMessageResult WimMessageCallback(WimMessageType messageType, object message, object userData);

    /// <summary>
    /// Specifies the result of a WimMessageCallback.
    /// </summary>
    public enum WimMessageResult : uint
    {
        /// <summary>
        /// Cancels an image apply or image capture.
        /// </summary>
        Abort = WimgApi.WIM_MSG_ABORT_IMAGE,

        /// <summary>
        /// Indicates success and prevents other subscribers from receiving the message.
        /// </summary>
        Done = WimgApi.WIM_MSG_DONE,

        /// <summary>
        /// Indicates the error can be ignored and the imaging operation continues.
        /// </summary>
        SkipError = WimgApi.WIM_MSG_SKIP_ERROR,

        /// <summary>
        /// Indicate success and to enable other subscribers to process the message
        /// </summary>
        Success = WimgApi.WIM_MSG_SUCCESS,
    }

    /// <summary>
    /// Specifies the type of message sent to the WIMMessageCallback.
    /// </summary>
    public enum WimMessageType : uint
    {
        /// <summary>
        /// Base message value.
        /// </summary>
        None = 0x8000 + 0x1476,

        /// <summary>
        /// Sent to the WIMMessageCallback function in debug builds with text messages containing status and error information.
        /// </summary>
        Text = 0x9477,

        /// <summary>
        /// Sent to a WIMMessageCallback function to indicate an update in the progress of an image application.
        /// </summary>
        /// <remarks>Progress estimates typically increase during the early stages of an image apply and later decrease, so the calling process must handle this as appropriate.</remarks>
        Progress = 0x9478,

        /// <summary>
        /// Sent to a WIMMessageCallback function to enable the caller to prevent a file or a directory from being captured or applied.
        /// </summary>
        Process = 0x9479,

        /// <summary>
        /// Sent to a WIMMessageCallback function to indicate that volume information is gathered during an image capture.
        /// </summary>
        Scanning = 0x947A,

        /// <summary>
        /// Sent to a WIMMessageCallback function to indicate the number of files to capture or to apply.
        /// </summary>
        SetRange = 0x947B,

        /// <summary>
        /// Sent to a WIMMessageCallback function to indicate the number of files that were captured or applied.
        /// </summary>
        SetPosition = 0x947C,

        /// <summary>
        /// Sent to the WIMMessageCallback function to indicate that a file was either captured or applied.
        /// </summary>
        StepIt = 0x947D,

        /// <summary>
        /// Sent to the WIMMessageCallback function to enable the caller to prevent a file resource from being compressed during a capture.
        /// </summary>
        Compress = 0x947E,

        /// <summary>
        /// Sent to a WIMMessageCallback function to alert the caller that an error occurred while capturing or applying an image.
        /// </summary>
        Error = 0x947F,

        /// <summary>
        /// Sent to a WIMMessageCallback function to enable the caller to align a file resource on a particular alignment boundary.
        /// </summary>
        Alignment = 0x9480,

        /// <summary>
        /// Sent to a WIMMessageCallback function when an I/O error occurs during a WIMApplyImage operation.
        /// </summary>
        Retry = 0x9481,

        /// <summary>
        /// Sent to a WIMMessageCallback function to enable the caller to change the size or the name of a piece of a split Windows® image (.wim) file.
        /// </summary>
        Split = 0x9482,

        /// <summary>
        /// Sent to a WIMMessageCallback function to provide the caller with information about the file being applied during a WIMApplyImage operation.
        /// </summary>
        FileInfo = 0x9483,

        /// <summary>
        /// Sent to a WIMMessageCallback function to alert the caller that a non-critical error occurred while capturing or applying an image.
        /// </summary>
        Info = 0x9484,

        /// <summary>
        /// Sent to a WIMMessageCallback function to warn the caller that a non-critical error occurred while capturing or applying an image.
        /// </summary>
        Warning = 0x9485,

        /// <summary>
        /// Sent to a WIMMessageCallback function to warn the caller that the Object ID for a particular file could not be restored.
        /// </summary>
        WarningObjectId = 0x9487,

        /// <summary>
        /// Sent to a WIMMessageCallback function to tell the caller that a stale mount directory is being removed.
        /// </summary>
        StaleMountDirectory = 0x9488,

        /// <summary>
        /// Sent to a WIMMessageCallback function to tell the caller how many stale files were removed.
        /// </summary>
        StaleMountFile = 0x9489,

        /// <summary>
        /// Sent to a WIMMessageCallback function to indicate progress during an image-cleanup operation.
        /// </summary>
        MountCleanupProgress = 0x948A,

        /// <summary>
        /// Sent to a WIMMessageCallback function to indicate that a drive is being scanned during a cleanup operation.
        /// </summary>
        CleanupScanningDrive = 0x948B,

        /// <summary>
        /// Sent to a WIMMessageCallback function to indicate that an image has been mounted to multiple locations. Only one mount location can have changes written back to the .wim file.
        /// </summary>
        ImageAlreadyMounted = 0x948C,

        /// <summary>
        /// Sent to a WIMMessageCallback function to indicate that an image is being unmounted as part of the cleanup process.
        /// </summary>
        CleanupUnmountingImage = 0x948D,

        /// <summary>
        /// Sent to a WIMMessageCallback function to allow the caller to abort an imaging operation that is currently processing a file resource.
        /// </summary>
        /// <remarks>This message is provided to allow applications to abort imaging operations that would otherwise not be aborted until the next WIM_MSG_PROCESS message.</remarks>
        QueryAbort = 0x948E,
    }

    public static partial class WimgApi
    {
        /// <summary>
        /// An application-defined function used with the WIMRegisterMessageCallback or WIMUnregisterMessageCallback functions.
        /// </summary>
        /// <param name="messageId">Specifies the type of message.</param>
        /// <param name="wParam">
        /// Specifies the first additional message information. The contents of this parameter depend on the value of
        /// the dwMessageId parameter.
        /// </param>
        /// <param name="lParam">
        /// Specifies the second additional message information. The contents of this parameter depend on the value of
        /// the dwMessageId parameter.
        /// </param>
        /// <param name="userData">
        /// A handle that specifies the user-defined value passed to the WIMRegisterMessageCallback
        /// function.
        /// </param>
        /// <returns>
        /// To indicate success and to enable other subscribers to process the message return WIM_MSG_SUCCESS. To prevent
        /// other subscribers from receiving the message, return WIM_MSG_DONE. To cancel an image apply or image capture, return
        /// WIM_MSG_ABORT_IMAGE when handling the WIM_MSG_PROCESS message.
        /// </returns>
        /// <remarks>
        /// Call the WIMUnregisterMessageCallback function with the result index when the WIMMessageCallback function is no longer
        /// required.
        /// Do not use WIM_MSG_ABORT_IMAGE to cancel the process as a shortcut method of extracting a single file. Windows® Imaging
        /// API is multi-threaded and aborting a process will cancel all background threads, which may include the single file you
        /// want to extract. If you want to extract a single file, use the WIMExtractImagePath function.
        /// </remarks>
        internal delegate WimMessageResult WIMMessageCallback(WimMessageType messageId, IntPtr wParam, IntPtr lParam, IntPtr userData);
    }

    /// <summary>
    /// Represents a wrapper class for the native callback functionality.  This class exposes a native callback and then calls
    /// the managed callback with marshaled values.
    /// </summary>
    internal sealed class WimMessageCallbackWrapper
    {
        /// <summary>
        /// The user's callback method.
        /// </summary>
        private readonly WimMessageCallback _callback;

        /// <summary>
        /// The user's custom data to pass around
        /// </summary>
        private readonly object _userData;

        /// <summary>
        /// Initializes a new instance of the <see cref="WimMessageCallbackWrapper"/> class.
        /// Initializes a new instance of the WimMessageCallbackWrapper class
        /// </summary>
        /// <param name="callback">
        /// A <see cref="WimMessageCallback" /> delegate to call when a message is received from the
        /// Windows® Imaging API.
        /// </param>
        /// <param name="userData">An object containing data to be used by the method.</param>
        public WimMessageCallbackWrapper(WimMessageCallback callback, object userData)
        {
            // Store the values
            _callback = callback;
            _userData = userData;

            // Store a reference to the native callback to avoid garbage collection
            NativeCallback = WimMessageCallback;
        }

        /// <summary>
        /// Gets the native callback delegate to be executed.
        /// </summary>
        public WimgApi.WIMMessageCallback NativeCallback { get; }

        /// <summary>
        /// A callback method for messages.
        /// </summary>
        /// <param name="messageId">Specifies the sent message.</param>
        /// <param name="wParam">
        /// A pointer to the first set of information.  Specifies additional message information. The contents of this parameter depend on the value of
        /// the MessageId parameter.
        /// </param>
        /// <param name="lParam">
        /// A pointer to the second set of information.  Specifies additional message information. The contents of this parameter depend on the value of
        /// the MessageId parameter.
        /// </param>
        /// <param name="userData">
        /// A handle that specifies the user-defined value passed to the WIMRegisterMessageCallback
        /// function.  This is currently not used.
        /// </param>
        /// <returns>A <see cref="WimMessageResult"/> object with the result.</returns>
        private WimMessageResult WimMessageCallback(WimMessageType messageId, IntPtr wParam, IntPtr lParam, IntPtr userData)
        {
            // Create a default message object as null
            object message = null;

            // Create a message object depending on the message type
            switch (messageId)
            {
                case WimMessageType.Alignment:
                    message = new WimMessageAlignment(wParam, lParam);
                    break;

                case WimMessageType.CleanupScanningDrive:
                    message = new WimMessageCleanupScanningDrive(wParam, lParam);
                    break;

                case WimMessageType.CleanupUnmountingImage:
                    message = new WimMessageCleanupUnmountingImage(wParam, lParam);
                    break;

                case WimMessageType.Compress:
                    message = new WimMessageCompress(wParam, lParam);
                    break;

                case WimMessageType.Error:
                    message = new WimMessageError(wParam, lParam);
                    break;

                case WimMessageType.FileInfo:
                    message = new WimMessageFileInfo(wParam, lParam);
                    break;

                case WimMessageType.ImageAlreadyMounted:
                    message = new WimMessageImageAlreadyMounted(wParam, lParam);
                    break;

                case WimMessageType.Info:
                    message = new WimMessageInformation(wParam, lParam);
                    break;

                case WimMessageType.MountCleanupProgress:
                    message = new WimMessageMountCleanupProgress(wParam, lParam);
                    break;

                case WimMessageType.Process:
                    message = new WimMessageProcess(wParam, lParam);
                    break;

                case WimMessageType.Progress:
                    message = new WimMessageProgress(wParam, lParam);
                    break;

                case WimMessageType.QueryAbort:
                    break;

                case WimMessageType.Retry:
                    message = new WimMessageRetry(wParam, lParam);
                    break;

                case WimMessageType.Scanning:
                    message = new WimMessageScanning(wParam, lParam);
                    break;

                case WimMessageType.SetPosition:
                    message = new WimMessageSetPosition(wParam, lParam);
                    break;

                case WimMessageType.SetRange:
                    message = new WimMessageSetRange(wParam, lParam);
                    break;

                case WimMessageType.Split:
                    message = new WimMessageSplit(wParam, lParam);
                    break;

                case WimMessageType.StaleMountDirectory:
                    message = new WimMessageStaleMountDirectory(wParam, lParam);
                    break;

                case WimMessageType.StaleMountFile:
                    message = new WimMessageStaleMountFile(wParam, lParam);
                    break;

                case WimMessageType.StepIt:
                    break;

                case WimMessageType.Text:
                    message = new WimMessageText(wParam, lParam);
                    break;

                case WimMessageType.Warning:
                    message = new WimMessageWarning(wParam, lParam);
                    break;

                case WimMessageType.WarningObjectId:
                    message = new WimMessageWarningObjectId(wParam, lParam);
                    break;

                default:
                    // Some messages are sent that aren't documented, so they are discarded and not sent to the user at this time
                    // When the messages are documented, they can be added to this wrapper
                    return WimMessageResult.Done;
            }

            // Call the users callback, pass the message type, message, and user data.  Return the users result value.
            return _callback(messageId, message, _userData);
        }
    }
}