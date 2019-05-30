using BrilliantSkies.PlayerProfiles;
using UnityEngine;

namespace BuildingTools
{
    public class BtKeyMap : KeyMap<KeyInputsBt>
    {
        public static BtKeyMap Instance => ProfileManager.Instance.GetModule<BtKeyMap>();

        public override ModuleType ModuleType => ModuleType.Options;
        protected override string FilenameAndExtension => "profile.keymappingBt";

        public BtKeyMap()
        {
            TipDictionary[KeyInputsBt.BuildModeTools]   = "Launch the Build Mode Tools window. Only works in Build Mode.";
            TipDictionary[KeyInputsBt.Calculator]       = "Launch the Calculator.";
            TipDictionary[KeyInputsBt.ArmorVisualizer]  = "Launch the Armor Visualizer mode. Will ask you with an alert to avoid losing unsaved progress";
        }

        protected override void FillDefaults(bool overrideExisting)
        {
            SetIfNull(KeyInputsBt.BuildModeTools,   KeyCode.BackQuote,  overrideExisting);
            SetIfNull(KeyInputsBt.Calculator,       KeyCode.Insert,     overrideExisting);
            SetIfNull(KeyInputsBt.ArmorVisualizer,  KeyCode.Home,       overrideExisting);
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
