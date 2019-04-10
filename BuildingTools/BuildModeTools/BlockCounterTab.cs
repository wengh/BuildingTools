using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Tips;
using UnityEngine;

namespace BuildingTools
{
    public class BlockCounterTab : SuperScreen
    {
        public override Content Name => new Content("Block Counter", new ToolTip("View amount of instances of every block on your vehicle"), "counter");

        public BlockCounterTab(ConsoleWindow window) : base(window) { }

        public override void Build()
        {
            var results = BlockCounter.Refresh();

            var seg1 = CreateStandardSegment(InsertPosition.ZeroCursor);
            seg1.AddInterpretter(Button.Quick(
                "Order by " + (BlockCounter.OrderByCount
                ? "name instead of by count"
                : "count instead of by name"
                ), new ToolTip("Change order"), () =>
            {
                BlockCounter.OrderByCount = !BlockCounter.OrderByCount;
                Segments.Clear();
                Build();
            }));

            var seg2 = CreateTableSegment(2, results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                var name = new StringDisplay(M.m(results[i].Key.ComponentId.Name), M.m(results[i].Key.GetToolTip()));
                name.Justify = TextAnchor.MiddleLeft;

                var count = StringDisplay.Quick(results[i].Value.ToString());
                count.Justify = TextAnchor.MiddleRight;

                seg2.AddInterpretter(name, i, 0);
                seg2.AddInterpretter(count, i, 1);
            }
        }
    }
}
