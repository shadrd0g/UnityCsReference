// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEngine;
using UnityEngine.Bindings;
using System;
using System.Runtime.CompilerServices;

namespace UnityEditor
{
    public sealed partial class PlayerSettings
    {
        [NativeHeader("Runtime/Misc/PlayerSettings.h")]
        [StaticAccessor("GetPlayerSettings().GetEditorOnlyForUpdate()", StaticAccessorType.Dot)]
        public sealed partial class PS5
        {
            [NativeProperty("ps5Passcode", false, TargetType.Function)] extern public static string passcode { get; set; }
            [NativeProperty("monoEnv", false, TargetType.Function)] extern public static string monoEnv { get; set; }
            [NativeProperty(TargetType = TargetType.Field)] extern public static bool playerPrefsSupport { get; set; }
            [NativeProperty(TargetType = TargetType.Field)] extern public static bool restrictedAudioUsageRights { get; set; }
            [NativeProperty("ps5ParamFilePath", false, TargetType.Function)] extern public static string paramFilePath { get; set; }
            [NativeProperty("ps5VideoOutPixelFormat", false, TargetType.Field)] extern public static int videoOutPixelFormat { get; set; }
            [NativeProperty("ps5VideoOutInitialWidth", false, TargetType.Field)] extern public static int videoOutInitialWidth { get; set; }
            [NativeProperty("ps5UseResolutionFallback", false, TargetType.Field)] extern public static bool useResolutionFallback { get; set; }
            [NativeProperty("ps5VideoOutOutputMode", false, TargetType.Field)] extern public static int videoOutOutputMode { get; set; }

            public static string SdkOverride
            {
                get
                {
                    return SdkOverrideInternal;
                }
                set
                {
                    string originalSDK = System.Environment.GetEnvironmentVariable("SCE_PROSPERO_SDK_DIR_ORIGINAL");
                    string newSDK;

                    if (String.IsNullOrEmpty(originalSDK))
                    {
                        // If the SCE_ORBIS_SDK_DIR_ORIGINAL has not been set then use the raw value, this could happen during initialization.
                        newSDK = value;
                    }
                    else
                    {
                        // Check for an empty SDK string and use the original SDK if so.
                        newSDK = String.IsNullOrEmpty(value) ? originalSDK : value;
                    }

                    System.Console.WriteLine($" SdkOverride newSDK:{newSDK.ToString()}  SdkOverrideInternal:{SdkOverrideInternal.ToString()}");
                    if (!String.IsNullOrEmpty(newSDK))  // we now always apply the value so that OnProjectWasLoaded() always reinitialises the shader compiler with the correct sdk
                    {
                        System.Environment.SetEnvironmentVariable("SCE_PROSPERO_SDK_DIR", newSDK);
                        SdkOverrideInternal = newSDK;
                    }
                }
            }

            [NativeProperty("ps5BackgroundImagePath", false, TargetType.Function)] extern public static string BackgroundImagePath { get; set; }
            [NativeProperty("ps5Pic2Path", false, TargetType.Function)] extern public static string Pic2Path { get; set; }
            [NativeProperty("ps5StartupImagePath", false, TargetType.Function)] extern public static string StartupImagePath { get; set; }
            [NativeProperty("ps5StartupImagesFolder", false, TargetType.Function)] extern public static string startupImagesFolder { get; set; }
            [NativeProperty("ps5IconImagesFolder", false, TargetType.Function)] extern public static string iconImagesFolder { get; set; }
            [NativeProperty("ps5SaveDataImagePath", false, TargetType.Function)] extern public static string SaveDataImagePath { get; set; }
            [NativeProperty("ps5SdkOverride", false, TargetType.Function)] extern private static string SdkOverrideInternal { get; set; }
            [NativeProperty("ps5BGMPath", false, TargetType.Function)] extern public static string BGMPath { get; set; }
            [NativeProperty("ps5ShareOverlayImagePath", false, TargetType.Function)] extern public static string ShareOverlayImagePath { get; set; }
            [NativeProperty("ps5NPConfigZipPath", false, TargetType.Function)] extern public static string npConfigZipPath { get; set; }
            [NativeProperty("ps5ScriptOptimizationLevel", false, TargetType.Field)] extern public static int scriptOptimizationLevel { get; set; }
            [NativeProperty("ps5disableAutoHideSplash", false, TargetType.Field)] extern public static bool disableAutoHideSplash { get; set; }
            [NativeProperty("ps5IncludedModules", false, TargetType.Field)] extern public static string[] includedModules { get; set; }
            [NativeProperty("ps5UpdateReferencePackage", false, TargetType.Function)] extern public static string updateReferencePackage { get; set; }
            [NativeProperty(TargetType = TargetType.Field)] extern public static bool enableApplicationExit { get; set; }
            [NativeProperty(TargetType = TargetType.Field)] extern public static bool resetTempFolder { get; set; }
            [NativeProperty(TargetType = TargetType.Field)] extern public static int playerPrefsMaxSize { get; set; }
            [NativeProperty("ps5OperatingSystemCanDisableSplashScreen", false, TargetType.Field)] extern public static bool operatingSystemCanDisableSplashScreen { get; set; }
        }
    }
}
