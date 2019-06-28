using System;
using System.Collections.Generic;
using System.Linq;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Modding.Types;

namespace BuildingTools
{
    public class BlockCounter
    {

        public static KeyValuePair<ItemDefinition, int>[] BlockCount { get; private set; }

        public static bool OrderByCount { get; set; } = true;

        public static KeyValuePair<ItemDefinition, int>[] Refresh()
        {
            var blocks = GetAllBlocks(cBuild.GetSingleton().GetC());
            var blockCount = new Dictionary<ItemDefinition, int>();

            foreach (var block in blocks)
            {
                if (blockCount.ContainsKey(block.item))
                    blockCount[block.item] += 1;
                else
                    blockCount[block.item] = 1;
            }

            if (OrderByCount)
                BlockCount = blockCount.OrderByDescending(x => x.Value).ToArray();
            else
                BlockCount = blockCount.OrderBy(x => x.Key.ComponentId.Name).ToArray();

            return BlockCount;
        }

        public static IEnumerable<Block> GetAllBlocks(AllConstruct c)
        {
            var iBlocks = c.AllBasics;

            return iBlocks.AliveAndDead.Blocks
                .Concat(iBlocks.SubConstructList.SelectMany(x => GetAllBlocks(x)));
        }
    }
}