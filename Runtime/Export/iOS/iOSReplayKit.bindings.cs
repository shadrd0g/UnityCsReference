// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEngine.Bindings;
using uei = UnityEngine.Internal;

namespace UnityEngine.Apple.ReplayKit
{
    [NativeHeader("PlatformDependent/iPhonePlayer/IOSScriptBindings.h")]
    public static partial class ReplayKit
    {
        // please note that we call trampoline function directly
        // they return int (as we want to have c-compatible interface) while c# expects bool
        // and so the code generated looks like bool f() { return f_returning_int(); }
        // this is perfectly standard c++ (at least we believe so) and it saves us from writing stupid glue code

        // TODO: at the time of writing there was a bug with attributes on properties: they were not "propagated",
        // thats why we do NativeConditional on both get/set and not on property itself

        extern public static bool APIAvailable
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitAPIAvailable")]
            get;
        }

        extern public static bool broadcastingAPIAvailable
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitBroadcastingAPIAvailable")]
            get;
        }

        extern public static bool recordingAvailable
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitRecordingAvailable")]
            get;
        }

        extern public static bool isRecording
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitIsRecording")]
            get;
        }

        extern public static bool isBroadcasting
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitIsBroadcasting")]
            get;
        }

        extern public static bool isBroadcastingPaused
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitIsBroadcastingPaused")]
            get;
        }

        extern public static bool isPreviewControllerActive
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitIsPreviewControllerActive")]
            get;
        }

        extern public static bool cameraEnabled
        {
            [NativeConditional("PLATFORM_IOS")]
            [FreeFunction("UnityReplayKitIsCameraEnabled")]
            get;

            [NativeConditional("PLATFORM_IOS")]
            [FreeFunction("UnityReplayKitSetCameraEnabled")]
            set;
        }
        extern public static bool microphoneEnabled
        {
            [NativeConditional("PLATFORM_IOS")]
            [FreeFunction("UnityReplayKitIsMicrophoneEnabled")]
            get;

            [NativeConditional("PLATFORM_IOS")]
            [FreeFunction("UnityReplayKitSetMicrophoneEnabled")]
            set;
        }

        extern public static string broadcastURL
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitGetBroadcastURL")]
            get;
        }

        extern public static string lastError
        {
            [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
            [FreeFunction("UnityReplayKitLastError")]
            get;
        }

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("ReplayKitScripting::StartRecording")]
        extern private static bool StartRecordingImpl(bool enableMicrophone, bool enableCamera);

        public delegate void BroadcastStatusCallback(bool hasStarted, string errorMessage);

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("ReplayKitScripting::StartBroadcasting")]
        extern private static void StartBroadcastingImpl(BroadcastStatusCallback callback, bool enableMicrophone, bool enableCamera);

        // we cannot have default args in public api yet (because we still support js that doesnt have it)
        // public static bool StartRecording(bool enableMicrophone = false, bool enableCamera = false);
        // public static bool StartBroadcasting(BroadcastStatusCallback callback, bool enableMicrophone = false, bool enableCamera = false);
        public static bool StartRecording([uei.DefaultValue("false")] bool enableMicrophone, [uei.DefaultValue("false")] bool enableCamera)
        {
            return StartRecordingImpl(enableMicrophone, enableCamera);
        }

        public static bool StartRecording(bool enableMicrophone)
        {
            return StartRecording(enableMicrophone, false);
        }

        public static bool StartRecording()
        {
            return StartRecording(false, false);
        }

        public static void StartBroadcasting(BroadcastStatusCallback callback, [uei.DefaultValue("false")] bool enableMicrophone, [uei.DefaultValue("false")] bool enableCamera)
        {
            StartBroadcastingImpl(callback, enableMicrophone, enableCamera);
        }

        public static void StartBroadcasting(BroadcastStatusCallback callback, bool enableMicrophone)
        {
            StartBroadcasting(callback, enableMicrophone, false);
        }

        public static void StartBroadcasting(BroadcastStatusCallback callback)
        {
            StartBroadcasting(callback, false, false);
        }


        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("UnityReplayKitStopRecording")]
        extern public static bool StopRecording();

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("UnityReplayKitStopBroadcasting")]
        extern public static void StopBroadcasting();

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("UnityReplayKitPauseBroadcasting")]
        extern public static void PauseBroadcasting();

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("UnityReplayKitResumeBroadcasting")]
        extern public static void ResumeBroadcasting();

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("UnityReplayKitPreview")]
        extern public static bool Preview();

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("UnityReplayKitDiscard")]
        extern public static bool Discard();

        public static bool ShowCameraPreviewAt(float posX, float posY)
        {
            return ShowCameraPreviewAt(posX, posY, -1.0f, -1.0f);
        }

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("UnityReplayKitShowCameraPreviewAt")]
        extern public static bool ShowCameraPreviewAt(float posX, float posY, float width, float height);

        [NativeConditional("PLATFORM_APPLE_NONDESKTOP")]
        [FreeFunction("UnityReplayKitHideCameraPreview")]
        extern public static void HideCameraPreview();
    }
}
