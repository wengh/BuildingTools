using BrilliantSkies.Localisation;
using BrilliantSkies.PlayerProfiles;
using UnityEngine;

namespace BuildingTools
{
    public class BtKeyMap : KeyMap<KeyInputsBt>
    {
        public static BtKeyMap Instance => ProfileManager.Instance.GetModule<BtKeyMap>();

        public override ModuleType ModuleType => ModuleType.Options;
        protected override string FilenameAndExtension => "profile.keymappingBt";
        
        protected override void FillDefaults(bool overrideExistingKey, bool overrideExistingDefault = false)
        {
            SetIfNull(KeyInputsBt.BuildModeTools, KeyCode.BackQuote,
                "Build mode tools",
                "Launch the Build Mode Tools window. Only works in Build Mode.",
                new KeyAndEng("", "Build mode", ""),
                overrideExistingKey);
            
            SetIfNull(KeyInputsBt.Calculator, KeyCode.Insert,
                "Calculator",
                "Launch the Calculator.",
                new KeyAndEng("", "In game", ""),
                overrideExistingKey);
            
            SetIfNull(KeyInputsBt.ArmorVisualizer, KeyCode.Home,
                "Armor visualizer",
                "Launch the Armor Visualizer mode. Will ask you with an alert to avoid losing unsaved progress",
                new KeyAndEng("", "In game", ""),
                overrideExistingKey);
        }

        public override Vector3 GetMovemementDirection() => Vector3.zero;
    }

    public enum KeyInputsBt
    {
        BuildModeTools,
        Calculator,
        ArmorVisualizer
    }
}
