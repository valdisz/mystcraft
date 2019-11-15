namespace atlantis
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ReportMatcher {
        public ReportMatcher(IAsyncEnumerable<MultiLineBlock> source) {
            this.source = source;
        }

        private readonly IAsyncEnumerable<MultiLineBlock> source;
        private readonly HashSet<ReportBlockType> unmatchedBlocks = new HashSet<ReportBlockType> {
            ReportBlockType.Header,
            ReportBlockType.FactionStatus,
            ReportBlockType.Error,
            ReportBlockType.Battle,
            ReportBlockType.Event,
            ReportBlockType.SkillLore,
            ReportBlockType.ItemLore,
            ReportBlockType.ObjectLore,
            ReportBlockType.DefaultAttitude,
            ReportBlockType.Attitude,
            ReportBlockType.UnclaimedSilver,
            ReportBlockType.Region,
            ReportBlockType.OrdersTemplate
        };

        public enum BlockAction {
            None, Skip, Break
        }

        public async IAsyncEnumerable<ReportBlock> CategorizeAsync() {
            await using var cursor = new Cursor<MultiLineBlock>(5, source);

            ReportBlockType lastBlock = ReportBlockType.None;
            List<MultiLineBlock> list = new List<MultiLineBlock>();

            while (await cursor.NextAsync()) {
                var (nextBlock, action) = await DetermineBlockTypeAsync(lastBlock, cursor);
                if (lastBlock != nextBlock) {
                    if (list.Count > 0) {
                        yield return new ReportBlock(lastBlock, list.ToArray());
                        list.Clear();
                    }
                }

                switch (action) {
                    case BlockAction.Skip:
                        lastBlock = nextBlock;
                        continue;

                    case BlockAction.Break:
                        if (list.Count > 0) {
                            yield return new ReportBlock(lastBlock, list.ToArray());
                            list.Clear();
                        }
                        break;
                }

                if (nextBlock == ReportBlockType.Region) {
                    var region = new RegionMatcher(cursor);
                    yield return await region.MatchAsync();
                }
                else {
                    list.Add(cursor.Value);
                }

                lastBlock = nextBlock;
            }

            if (list.Count > 0) {
                yield return new ReportBlock(lastBlock, list.ToArray());
            }
        }

        private void MarkBlockMatched(ReportBlockType type) {
            unmatchedBlocks.Remove(type);
        }

        private async Task<ReportBlockType> MatchPossibleBlock(Cursor<MultiLineBlock> cursor) {
            foreach (var block in unmatchedBlocks) {
                bool isMatch = false;
                switch (block) {
                    case ReportBlockType.Header: isMatch = cursor.MatchReportStart(); break;
                    case ReportBlockType.FactionStatus: isMatch = cursor.MatchFactionStatus(); break;
                    case ReportBlockType.Error: isMatch = cursor.MatchErrors(); break;
                    case ReportBlockType.Battle: isMatch = cursor.MatchBattles(); break;
                    case ReportBlockType.Event: isMatch = cursor.MatchEvents(); break;
                    case ReportBlockType.SkillLore: isMatch = cursor.MatchSkills(); break;
                    case ReportBlockType.ItemLore: isMatch = cursor.MatchItems(); break;
                    case ReportBlockType.ObjectLore: isMatch = cursor.MatchObjects(); break;
                    case ReportBlockType.DefaultAttitude: isMatch = cursor.MatchDefaultAttitude(); break;
                    case ReportBlockType.Attitude: isMatch = cursor.MatchAttitue(); break;
                    case ReportBlockType.UnclaimedSilver: isMatch = cursor.MatchUnclaimed(); break;
                    case ReportBlockType.Region: isMatch = await cursor.MatchRegionStart(); break;
                    case ReportBlockType.OrdersTemplate: isMatch = cursor.MatchOrdersTemplate(); break;
                }

                if (isMatch) return block;
            }

            return ReportBlockType.Other;
        }

        private static readonly ReportBlockType[] blocksWithHeaders = {
            ReportBlockType.Header,
            ReportBlockType.FactionStatus,
            ReportBlockType.Error,
            ReportBlockType.Battle,
            ReportBlockType.Event,
            ReportBlockType.SkillLore,
            ReportBlockType.ItemLore,
            ReportBlockType.ObjectLore,
            ReportBlockType.OrdersTemplate
        };

        private async Task<(ReportBlockType type, BlockAction action)> DetermineBlockTypeAsync(ReportBlockType lastBlock, Cursor<MultiLineBlock> cursor) {
            ReportBlockType nextBlock = lastBlock;
            BlockAction action = BlockAction.None;

            switch (lastBlock) {
                case ReportBlockType.None:
                    if (cursor.MatchReportStart()) {
                        nextBlock = ReportBlockType.Header;
                    }
                    else {
                        action = BlockAction.Skip;
                    }
                    break;

                case ReportBlockType.FactionStatus:
                    if (!cursor.MatchFactionStatusItem()) {
                        nextBlock = await MatchPossibleBlock(cursor);
                    }
                    else {
                        action = BlockAction.Break;
                    }
                    break;

                case ReportBlockType.Header:
                case ReportBlockType.Error:
                case ReportBlockType.Event:
                case ReportBlockType.ItemLore:
                case ReportBlockType.SkillLore:
                case ReportBlockType.ObjectLore:
                case ReportBlockType.Attitude:
                    nextBlock = await MatchPossibleBlock(cursor);
                    if (nextBlock == ReportBlockType.Other) {
                        nextBlock = lastBlock;
                        action = BlockAction.Break;
                    }
                    break;

                case ReportBlockType.Battle:
                    nextBlock = await MatchPossibleBlock(cursor);
                    if (nextBlock == ReportBlockType.Other) {
                        nextBlock = lastBlock;
                        if (cursor.MatchBattle()) {
                            action = BlockAction.Break;
                        }
                    }
                    break;

                case ReportBlockType.Region:
                    if (cursor.MatchOrdersTemplate()) {
                        nextBlock = ReportBlockType.OrdersTemplate;
                    }
                    else if (await cursor.MatchRegionStart()) {
                        action = BlockAction.Break;
                    }
                    break;

                case ReportBlockType.OrdersTemplate:
                    action = BlockAction.None;
                    break;

                default:
                    nextBlock = await MatchPossibleBlock(cursor);
                    if (nextBlock == ReportBlockType.Other) {
                        action = BlockAction.Break;
                    }
                    break;
            }

            if (unmatchedBlocks.Contains(nextBlock) && blocksWithHeaders.Contains(nextBlock)) {
                action = BlockAction.Skip;
            }
            MarkBlockMatched(nextBlock);

            return (nextBlock, action);
        }
    }
}
