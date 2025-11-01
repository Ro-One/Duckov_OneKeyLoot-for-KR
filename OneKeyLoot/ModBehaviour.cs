using Duckov.Modding;
using HarmonyLib;
using UnityEngine;

namespace OneKeyLoot
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public readonly string Mod_Name = "DreamNya.OneKeyLoot";

        private Harmony harmony;

        private void Awake()
        {
            Debug.Log($"[OneKeyLoot]: {ModConfig.DisplayName} Awake");
        }

        private void OnEnable()
        {
            Debug.Log($"[OneKeyLoot]: {ModConfig.DisplayName} OnEnable");

            ModManager.OnModActivated += ModConfig.OnModActivated;
            i18n.Init();
            ModConfig.InitializeOnEnable();

            harmony = new Harmony(Mod_Name);
            harmony.PatchAll();
        }

        private void OnDisable()
        {
            Debug.Log($"[OneKeyLoot]: {ModConfig.DisplayName} OnDisable");

            ModManager.OnModActivated -= ModConfig.OnModActivated;
            ModConfig.OnDisableCleanup();

            harmony?.UnpatchAll(Mod_Name);
            i18n.Dispose();
        }
    }
}
