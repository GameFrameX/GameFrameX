#if !UNITY_EDITOR
#if UNITY_IOS || (UNITY_STANDALONE_OSX && ENABLE_IL2CPP)
#define SENTRY_NATIVE_COCOA
#elif UNITY_ANDROID
#define SENTRY_NATIVE_ANDROID
#elif UNITY_64 && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX)
#define SENTRY_NATIVE
#elif UNITY_WEBGL
#define SENTRY_WEBGL
#endif
#endif

#if ENABLE_IL2CPP && UNITY_2020_3_OR_NEWER && (SENTRY_NATIVE_COCOA || SENTRY_NATIVE_ANDROID || SENTRY_NATIVE)
#define IL2CPP_LINENUMBER_SUPPORT
#endif

using System;
#if UNITY_2020_3_OR_NEWER
using System.Buffers;
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using UnityEngine.Scripting;

#if SENTRY_NATIVE_COCOA
using Sentry.Unity.iOS;
#elif SENTRY_NATIVE_ANDROID
using Sentry.Unity.Android;
#elif SENTRY_NATIVE
using Sentry.Unity.Native;
#elif SENTRY_WEBGL
using Sentry.Unity.WebGL;
#elif SENTRY_DEFAULT
using Sentry.Unity.Default;
#endif

[assembly: AlwaysLinkAssembly]

namespace Sentry.Unity
{
    public static class SentryInitialization
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            var sentryUnityInfo = new SentryUnityInfo();
            var options = ScriptableSentryUnityOptions.LoadSentryUnityOptions(sentryUnityInfo);
            if (options.ShouldInitializeSdk())
            {
                Exception nativeInitException = null;

                try
                {
#if SENTRY_NATIVE_COCOA
                    SentryNativeCocoa.Configure(options, sentryUnityInfo);
#elif SENTRY_NATIVE_ANDROID
                    SentryNativeAndroid.Configure(options, sentryUnityInfo);
#elif SENTRY_NATIVE
                    SentryNative.Configure(options);
#elif SENTRY_WEBGL
                    SentryWebGL.Configure(options);
#endif
                }
                catch (DllNotFoundException e)
                {
                    nativeInitException = new Exception(
                        "Sentry native-error capture configuration failed to load a native library. This usually " +
                        "means the library is missing from the application bundle or the installation directory.", e);
                }
                catch (Exception e)
                {
                    nativeInitException = new Exception("Sentry native error capture configuration failed.", e);
                }

                SentryUnity.Init(options);
                if (nativeInitException != null)
                {
                    SentrySdk.CaptureException(nativeInitException);
                }
            }
        }
    }

    public class SentryUnityInfo : ISentryUnityInfo
    {
        public bool IL2CPP
        {
            get =>
#if ENABLE_IL2CPP
               true;
#else
               false;
#endif
        }

        public string Platform
        {
            get =>
#if UNITY_IOS || UNITY_STANDALONE_OSX
                "macho"
#elif UNITY_ANDROID || UNITY_STANDALONE_LINUX
                "elf"
#elif UNITY_STANDALONE_WIN
                "pe"
#else
                "unknown"
#endif
            ;
        }

        public Il2CppMethods Il2CppMethods => _il2CppMethods;

        private Il2CppMethods _il2CppMethods
            // Lowest supported version to have all required methods below
#if !IL2CPP_LINENUMBER_SUPPORT
            ;
#else
            = new Il2CppMethods(
                il2cpp_gchandle_get_target,
                Il2CppNativeStackTraceShim,
                il2cpp_free);

#pragma warning disable 8632
        // The incoming `IntPtr` is a native `char*`, a pointer to a
        // nul-terminated C string. This function converts it to a C# string,
        // and also byte-swaps/truncates on ELF platforms.
        private static string? SanitizeDebugId(IntPtr debugIdPtr)
        {
            if (debugIdPtr == IntPtr.Zero)
            {
                return null;
            }

#if UNITY_ANDROID || UNITY_STANDALONE_LINUX
            // For ELF platforms, the 20-byte `NT_GNU_BUILD_ID` needs to be
            // turned into a "little-endian GUID", which means the first three
            // components need to be byte-swapped appropriately.
            // See: https://getsentry.github.io/symbolicator/advanced/symbol-server-compatibility/#identifiers

            // We unconditionally byte-flip these as we assume that we only
            // ever run on little-endian platforms. Additionally, we truncate
            // this down from a 40-char build-id to a 32-char debug-id as well.
            SwapHexByte(debugIdPtr, 0, 3);
            SwapHexByte(debugIdPtr, 1, 2);
            SwapHexByte(debugIdPtr, 4, 5);
            SwapHexByte(debugIdPtr, 6, 7);
            Marshal.WriteByte(debugIdPtr, 32, 0);

            // This will swap the two hex-encoded bytes at offsets 1 and 2.
            // Internally, it treats these as Int16, as the hex-encoding means
            // they occupy 2 bytes each.
            void SwapHexByte(IntPtr buffer, Int32 offset1, Int32 offset2)
            {
                var a = Marshal.ReadInt16(buffer, offset1 * 2);
                var b = Marshal.ReadInt16(buffer, offset2 * 2);
                Marshal.WriteInt16(buffer, offset2 * 2, a);
                Marshal.WriteInt16(buffer, offset1 * 2, b);
            }

            // All other platforms we care about (Windows, macOS) already have
            // an appropriate debug-id format for that platform so no modifications
            // are needed.
#endif

            return Marshal.PtrToStringAnsi(debugIdPtr);
        }

        // Available in Unity `2019.4.34f1` (and later)
        // Il2CppObject* il2cpp_gchandle_get_target(uint32_t gchandle)
        [DllImport("__Internal")]
        private static extern IntPtr il2cpp_gchandle_get_target(int gchandle);

        // Available in Unity `2019.4.34f1` (and later)
        // void il2cpp_free(void* ptr)
        [DllImport("__Internal")]
        private static extern void il2cpp_free(IntPtr ptr);

#if UNITY_2021_3_OR_NEWER
        private static void Il2CppNativeStackTraceShim(IntPtr exc, out IntPtr addresses, out int numFrames, out string? imageUUID, out string? imageName)
        {
            var uuidBuffer = IntPtr.Zero;
            il2cpp_native_stack_trace(exc, out addresses, out numFrames, out uuidBuffer, out imageName);

            try
            {
                imageUUID = SanitizeDebugId(uuidBuffer);
            }
            finally
            {
                il2cpp_free(uuidBuffer);
            }
        }

        // Definition from Unity `2021.3` (and later):
        // void il2cpp_native_stack_trace(const Il2CppException * ex, uintptr_t** addresses, int* numFrames, char** imageUUID, char** imageName)
        [DllImport("__Internal")]
        private static extern void il2cpp_native_stack_trace(IntPtr exc, out IntPtr addresses, out int numFrames, out IntPtr imageUUID, out string? imageName);
#else
        private static void Il2CppNativeStackTraceShim(IntPtr exc, out IntPtr addresses, out int numFrames, out string? imageUUID, out string? imageName)
        {
            imageName = null;
            // Unity 2020 does not *return* a newly allocated string as out-parameter,
            // but rather expects a pre-allocated buffer it writes into.
            // That buffer needs to have space for either:
            // - A hex-encoded `LC_UUID` on MacOS (32)
            // - A hex-encoded GUID + Age on Windows (40)
            // - A hex-encoded `NT_GNU_BUILD_ID` on ELF (Android/Linux) (40)
            // plus a terminating nul-byte.
            var uuidBuffer = il2cpp_alloc(40 + 1);
            il2cpp_native_stack_trace(exc, out addresses, out numFrames, uuidBuffer);

            try
            {
                imageUUID = SanitizeDebugId(uuidBuffer);
            }
            finally
            {
                il2cpp_free(uuidBuffer);
            }
        }

        // Available in Unity `2020.3` (possibly even sooner)
        // void* il2cpp_alloc(size_t size)
        [DllImport("__Internal")]
        private static extern IntPtr il2cpp_alloc(uint size);

        // Definition from Unity `2020.3`:
        // void il2cpp_native_stack_trace(const Il2CppException * ex, uintptr_t** addresses, int* numFrames, char* imageUUID)
        [DllImport("__Internal")]
        private static extern void il2cpp_native_stack_trace(IntPtr exc, out IntPtr addresses, out int numFrames, IntPtr imageUUID);
#endif
#pragma warning restore 8632
#endif
    }
}
