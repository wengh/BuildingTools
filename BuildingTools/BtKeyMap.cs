using BrilliantSkies.Localisation;
using BrilliantSkies.PlayerProfiles;
using InControl;
using UnityEngine;

namespace BuildingTools
{
    public class BtKeyMap : KeyMap<KeyInputsBt>
    {
        public static BtKeyMap Instance => ProfileManager.Instance.GetModule<BtKeyMap>();

        public override ModuleType ModuleType => ModuleType.Options;
        protected override string FilenameAndExtension => "profile.keymappingBt";

        protected override void FillAllVolatileData()
        {
            var category = new KeyAndEng("", "build mode", "");

            SetVolatile(KeyInputsBt.BuildModeTools,
                "Build mode tools",
                "Launch the Build Mode Tools window. Only works in build mode.",
                category,
                Q(Key.Backquote));


            category = new KeyAndEng("", "in game", "");

            SetVolatile(KeyInputsBt.Calculator,
                "Calculator",
                "Launch the Calculator.",
                category,
                Q(Key.Insert));

            SetVolatile(KeyInputsBt.ArmorVisualizer,
                "Armor visualizer",
                "Launch the Armor Visualizer mode. Will ask you with an alert to avoid losing unsaved progress.",
                category,
                Q(Key.Home));
        }

        public override Vector3 GetMovementDirection(bool smoothDigitalInput = true) => Vector3.zero;

        protected override int IdToInt(KeyInputsBt id) => (int) id;

        public BtKeyMap() : base(KeyInputsBt.MaxId)
        {
        }
    }

    public enum KeyInputsBt
    {
        BuildModeTools,
        Calculator,
        ArmorVisualizer,

        MaxId,
    }
}
