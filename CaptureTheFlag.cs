using BepInEx;
using HarmonyLib;
using UnboundLib;
using UnityEngine;
using UnboundLib.Utils.UI;
using UnboundLib.GameModes;
using TMPro;
using CaptureTheFlag.GameModeHandler;
using CaptureTheFlag.GameMode;
using UnboundLib.Cards;
using UnboundLib.Utils;

namespace CaptureTheFlag
{
    class CaptureTheFlag : BaseUnityPlugin
    {


        private const string ModId = "pykessandbosssloth.rounds.plugins.gamemodecollection";
        private const string ModName = "Game Mode Collection";
        public const string Version = "0.0.0";
        private static string CompatibilityModName => ModName.Replace(" ", "");

        public static CaptureTheFlag instance;

        private Harmony harmony;


#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif
        internal static void Log(object msg)
        {
            if (DEBUG)
            {
                UnityEngine.Debug.Log($"[{ModName}] {msg.ToString()}");
            }
        }
        internal static void LogWarning(object msg)
        {
            if (DEBUG)
            {
                UnityEngine.Debug.LogWarning($"[{ModName}] {msg.ToString()}");
            }
        }
    }
}
