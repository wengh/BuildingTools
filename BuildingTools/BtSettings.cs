using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Avatar.Skills;
using BrilliantSkies.PlayerProfiles;
using Harmony;
using UnityEngine;

namespace BuildingTools
{
    public class BtSettings : ProfileModule<BtSettings.InternalData>
    {
        public static InternalData Data => ProfileManager.Instance.GetModule<BtSettings>().Internal;

        public override ModuleType ModuleType => ModuleType.PlayerPreferences;

        protected override string FilenameAndExtension => "profile.buildingtools";
        
        public class InternalData
        {
            public bool DisableCapsLock { get; set; } = true;
            public bool EnableNewFeaturesReport { get; set; } = true;
            public bool DisableSkillsInDesigner { get; set; } = true;
            public bool CharacterInvincibilityInDesigner { get; set; } = true;
        }
    }
}
