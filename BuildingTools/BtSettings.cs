using BrilliantSkies.PlayerProfiles;

namespace BuildingTools
{
    public class BtSettings : ProfileModule<BtSettings.InternalData>
    {
        public static InternalData Data => ProfileManager.Instance.GetModule<BtSettings>().Internal;

        public override ModuleType ModuleType => ModuleType.Options;
        protected override string FilenameAndExtension => "profile.buildingtools";
        
        public class InternalData
        {
            public bool EnableNewFeaturesReport { get; set; } = true;
        }
    }
}
