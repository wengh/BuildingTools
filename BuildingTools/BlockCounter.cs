using System;
using System.Collections.Generic;
using System.Linq;
using BrilliantSkies.Ftd.Avatar.Build;

namespace BuildingTools
{
    public class BlockCounter
    {

        public static KeyValuePair<string, int>[] BlockCount { get; private set; }

        public static bool OrderByCount { get; set; } = true;

        public static KeyValuePair<string, int>[] Refresh()
        {
            var blocks = GetAllBlocks(cBuild.GetSingleton().GetC());
            var blockCount = new Dictionary<string, int>();

            foreach (var block in blocks)
            {
                string name = block.Name;

                if (blockCount.ContainsKey(name))
                    blockCount[name] += 1;
                else
                    blockCount[name] = 1;
            }

            if (OrderByCount)
                BlockCount = blockCount.OrderByDescending(x => x.Value).ToArray();
            else
                BlockCount = blockCount.OrderBy(x => x.Key).ToArray();

            return BlockCount;
        }

        public static IEnumerable<Block> GetAllBlocks(AllConstruct c)
        {
            var iBlocks = c.iBlocks;

            return iBlocks.AliveAndDead.Blocks
                .Concat(iBlocks.SubConstructList.SelectMany(x => GetAllBlocks(x)));
        }
    }
}