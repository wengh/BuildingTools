using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Texts;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Choices;
using BrilliantSkies.Ui.Tips;
using System.Linq;
using UnityEngine;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Layouts.DropDowns;

namespace BuildingTools
{
    public class BlockSearchUI : ConsoleUi<BlockSearch>
    {
        public BlockSearchUI(BlockSearch focus) : base(focus) { }

        private ItemDefinition[] results = new ItemDefinition[] { };

        protected override ConsoleWindow BuildInterface(string suggestedName = "")
        {
            var window = NewWindow("Search Blocks", WindowSizing.GetSizeCentral(0.5f, 0.8f));
            window.DisplayTextPrompt = false;

            var seg1 = window.Screen.CreateStandardSegment();
            seg1.AddInterpretter(TextInput<BlockSearch>.Quick(_focus, M.m<BlockSearch>(x => x.query), "Search query", new ToolTip("Search query"),
                (x, query) =>
                {
                    if (query != x.query)
                    {
                        if (query.Contains("`"))
                        {
                            DeactivateGui();
                            return;
                        }
                        results = x.SearchWithNewQuery(query).ToArray();
                        TriggerRebuild();
                    }
                }));

            var seg2 = window.Screen.CreateStandardSegment();
            foreach (var item in results)
            {
                var blocks = UnityEngine.Object.FindObjectOfType<cBuild>().C.iBlocks.AliveAndDead.Blocks;
                seg2.AddInterpretter(SubjectiveButton<BlockSearch>.Quick(_focus, item.ComponentId.Name, item.GetToolTip(), x =>
                {
                    x.SelectItem(item);
                    DeactivateGui();
                }));
            }
            return window;
        }
    }
}
