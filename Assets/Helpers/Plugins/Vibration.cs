using UnityEngine;
using System.Collections;
using System;

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

    public static void Vibrate()
    {
        if (IsAndroid())
            vibrator.Call("vibrate");
        else
            Handheld.Vibrate();
    }


    public static void Vibrate(long milliseconds)
    {
        if (IsAndroid())
            vibrator.Call("vibrate", milliseconds);
        else
            Handheld.Vibrate();
    }

    public static void Vibrate(long[] pattern, int repeat)
    {
        if (IsAndroid())
            vibrator.Call("vibrate", pattern, repeat);
        else
            Handheld.Vibrate();
    }

    public static bool HasVibrator()
    {
        return IsAndroid();
    }

    public static void Cancel()
    {
        if (IsAndroid())
            vibrator.Call("cancel");
    }

    // https://stackoverflow.com/a/20821575/6177635
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

    // Dummy to trigger manifest generation for permissions
    static void Dummy()
    {
        Handheld.Vibrate();
    }

    private static bool IsAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
	return true;
#else
        return false;
#endif
    }
}