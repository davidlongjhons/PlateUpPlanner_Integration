using KitchenLib;
using KitchenMods;
using PreferenceSystem;
using System.Reflection;
using TwitchLib.PubSub.Events;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace PlateUpPlannerIntegration
{
    public class Mod : BaseMod, IModSystem
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "davidlongjhons.plateup.plannerintegration";
        public const string MOD_NAME = "PlateUpPlanner Integration";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "davidlongjhons";
        public const string MOD_GAMEVERSION = ">=1.1.4";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif

        public static AssetBundle Bundle;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            GameObject = new GameObject("ImportMenu");
            ImportGUIManager = GameObject.AddComponent<ImportGUIManager>();
        }

        private void AddGameData()
        {
            LogInfo("Attempting to register game data...");

            // AddGameDataObject<MyCustomGDO>();

            LogInfo("Done loading game data.");
        }

        private static GameObject GameObject { get; set; }
        public static ImportGUIManager ImportGUIManager { get; private set; } 


            #region Logging
            public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
            public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
            public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
            public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
            public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
            public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
        private static PreferenceSystemManager PrefManager;
        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);
            PrefManager
                .AddLabel("Planner Integration")
                .AddButton("Open Menu", delegate (int _)
                {
                    ImportGUIManager.Show();
                });
                /*
                .AddSubmenu("Import Options", "importOptions")
                    .AddInfo("\"Check Importability\" will check that you have the appliances required to import")
                    .AddButton("Check Importability", delegate (int _)
                    {
                        KitchenLayoutImport.LayoutImporter.RequestImport();
                    })
                    .AddInfo("Note: Import button only works if import check passes.")
                    .AddButton("Import", delegate (int _)
                    {
                        KitchenLayoutImport.LayoutImporter.RequestImport();
                    })
                .SubmenuDone();
                */
                

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }
    }
}
