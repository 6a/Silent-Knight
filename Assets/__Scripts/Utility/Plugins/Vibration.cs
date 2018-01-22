using UnityEngine;
using System;

namespace SilentKnight.Utility
{
    /// <summary>
    /// Utility class for more explicit control of vibration on Android devices.
    /// </summary>
    public static class Vibration
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
        public static AndroidJavaClass unityPlayer;
        public static AndroidJavaObject currentActivity;
        public static AndroidJavaObject vibrator;
#endif

        /// <summary>
        /// Vibrate once with default settings. Fails silently if not Android.
        /// </summary>
        public static void Vibrate()
        {
            if (IsAndroid())
                vibrator.Call("vibrate");
        }

        /// <summary>
        /// Vibrate for a certain amount of time. Fails silently if not Android.
        /// </summary>
        public static void Vibrate(long milliseconds)
        {
            if (IsAndroid())
                vibrator.Call("vibrate", milliseconds);
        }

        /// <summary>
        /// Vibrate with an explicit PWM pattern. Fails silently if not Android. [Set repeat to -1 for one-shot].
        /// </summary>
        public static void Vibrate(long[] pattern, int repeat)
        {
            if (IsAndroid())
                vibrator.Call("vibrate", pattern, repeat);
        }

        /// <summary>
        /// Cancels the current vibration, if one exists
        /// </summary>
        public static void Cancel()
        {
            if (IsAndroid())
                vibrator.Call("cancel");
        }

        /// <summary>
        /// Returns a [long] PWN pattern based on the desired vibration intensity and duration. 
        /// </summary>
        /// SOURCE: paulscode @ stackoverflow - https://stackoverflow.com/a/20821575/6177635
        public static long[] GenVibratorPattern(float intensity, long duration)
        {
            float dutyCycle = Math.Abs((intensity * 2.0f) - 1.0f);
            long hWidth = (long)(dutyCycle * (duration - 1)) + 1;
            long lWidth = dutyCycle == 1.0f ? 0 : 1;

            int pulseCount = (int)(2.0f * ((float)duration / (float)(hWidth + lWidth)));
            long[] pattern = new long[pulseCount];

            for (int i = 0; i < pulseCount; i++)
            {
                pattern[i] = intensity < 0.5f ? (i % 2 == 0 ? hWidth : lWidth) : (i % 2 == 0 ? lWidth : hWidth);
            }

            return pattern;
        }

        /// <summary>
        /// Returns true if the device is an Android device.
        /// </summary>
        static bool IsAndroid()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
	return true;
#else
            return false;
#endif
        }
    }
}