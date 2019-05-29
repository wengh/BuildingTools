using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Choices;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingTools
{
    public class BtPanel : SuperScreen<BtSettings.InternalData>
    {
        public override Content Name => new Content("BT Settings", "Change the behaviour of the BuildingTools mod");

        public BtPanel(ConsoleWindow window) : base(window, BtSettings.Data) { }

        public override void Build()
        {
            var seg = CreateTableSegment(2, 2);
            seg.AddInterpretter(SubjectiveToggle<BtSettings.InternalData>.Quick(_focus,
                "Disable CapsLock",
                new ToolTip("Automagically revert CapsLock when you press it, so you won't accidentally type cAPITALIZED lETTERS"),
                (x, val) => x.DisableCapsLock = val,
                x => x.DisableCapsLock));
            seg.AddInterpretter(SubjectiveToggle<BtSettings.InternalData>.Quick(_focus,
                "Enable changelog report",
                new ToolTip("After each update, show a list of new changes made to FtD at start, if any"),
                (x, val) => x.EnableNewFeaturesReport = val,
                x => x.EnableNewFeaturesReport));
            seg.AddInterpretter(SubjectiveToggle<BtSettings.InternalData>.Quick(_focus,
                "Disable skills in designer",
                new ToolTip("Ignore all skill levels in designer mode, to stop you from thinking that you're building overpowered crafts when they are garbage in reality", 330),
                (x, val) => {
                    x.DisableSkillsInDesigner = val;
                    BuildingToolsPlugin.RefreshSkills();
                },
                x => x.DisableSkillsInDesigner));
            seg.AddInterpretter(SubjectiveToggle<BtSettings.InternalData>.Quick(_focus,
                "Invincible character in designer",
                new ToolTip("Prevent the rambot from receiving any damage", 210),
                (x, val) => {
                    x.CharacterInvincibilityInDesigner = val;
                    BuildingToolsPlugin.RefreshSkills();
                },
                x => x.CharacterInvincibilityInDesigner));
        }
    }
}
