using BrilliantSkies.Core.Networking;
using BrilliantSkies.PlayerProfiles;
using Harmony;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BuildingTools
{
    public static class CapsLock
    {
        public static bool State
        {
            get => GetCapsLock();
            set => SetCapsLock(value);
        }

        // KeyInputsFtd.IsKey
        public static void KeyMapPostfix(KeyInputsFtd codedInput, KeyMap<KeyInputsFtd> __instance, ref bool __result)
        {
            if (!BtSettings.Data.DisableCapsLock) return;

            var keyDef = __instance.GetKeyDef(codedInput);
            if (keyDef.Key == KeyCode.CapsLock && Input.GetKeyUp(keyDef.Key))
            {
                if (__result && !Event.current.capsLock)
                    __result = false;

                State = false;
            }
        }




        // https://answers.unity.com/questions/219017/toggling-caps-lock-or-even-just-the-indicator-ligh.html

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_CAPSLOCK = 0x14;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x1;
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int KEYEVENTF_KEYDOWN = 0x0;

        public static bool GetCapsLock()
        {
            return (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
        }

        public static void SetCapsLock(bool bState)
        {
            if (GetCapsLock() != bState)
            {
                keybd_event(VK_CAPSLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
                keybd_event(VK_CAPSLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            }
        }
    }
}