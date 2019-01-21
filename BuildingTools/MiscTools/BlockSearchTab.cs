using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Texts;
using BrilliantSkies.Ui.Tips;
using UnityEngine;

namespace BuildingTools
{
    public class BlockSearchTab : SuperScreen<BlockSearch>
    {
        protected readonly Action deactivate;

        public override Content Name => new Content("Block Search", new ToolTip("Quickly pick blocks in the inventory"), "search");

        public BlockSearchTab(Action deactivate, ConsoleWindow window, BlockSearch focus) : base(window, focus)
        {
            this.deactivate = deactivate;
        }

        protected ItemDefinition[] results;

        public override void Build()
        {
            if (results == null)
                results = _focus.SearchWithNewQuery("").ToArray();

            var seg1 = CreateStandardSegment(InsertPosition.ZeroCursor);
            seg1.AddInterpretter(TextInput<BlockSearch>.Quick(_focus, M.m<BlockSearch>(x => x.query), "Search query", new ToolTip("Search query"),
                (x, query) =>
                {
                    if (query != x.query)
                    {
                        if (query.Contains('`'))
                        {
                            deactivate();
                            return;
                        }
                        results = x.SearchWithNewQuery(query).ToArray();
                        
                        Segments.Clear();
                        Build();
                    }
                }));

            var seg2 = CreateStandardSegment();
            foreach (var item in results)
            {
                var button = SubjectiveButton<BlockSearch>.Quick(_focus, item.ComponentId.Name, item.GetToolTip(), x =>
                {
                    x.SelectItem(item);
                    deactivate();
                });
                button.Justify = TextAnchor.MiddleLeft;
                seg2.AddInterpretter(button);
            }
        }
    }
}
