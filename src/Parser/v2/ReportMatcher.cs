namespace atlantis {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class AtlantisReportConverter {
        public AtlantisReportConverter(IAsyncEnumerable<MultiLineBlock> source) {
            this.source = source;
        }

        private ReportState state = ReportState.None;
        private readonly Dictionary<ReportState, HashSet<ReportState>> nextState =
            new Dictionary<ReportState, HashSet<ReportState>> {
                { ReportState.None, new HashSet<ReportState> { ReportState.Header } }
            };

        private async IAsyncEnumerable<TextParser> AsTextParser() {
            await foreach (var block in source) {
                yield return new TextParser(1, block.Text);
            }
        }

        public async Task ConvertReportToJson(JsonWriter writer) {
            await using var cursor = new Cursor<TextParser>(5, AsTextParser());

            while (await cursor.NextAsync() && !cursor.Value.Match("Atlantis Report For")) {
            }

            while (await cursor.NextAsync() && !await IsRegion(cursor)) {
            }
        }

        private async Task<bool> IsRegion(Cursor<TextParser> cursor) {
            var isRegionHeader = await cursor.NextAsync() && cursor.Value.Match("-----").Success;
            cursor.Back();

            return isRegionHeader;
        }

        private readonly IAsyncEnumerable<MultiLineBlock> source;
        private readonly HashSet<ReportState> unmatchedBlocks = new HashSet<ReportState> {
            ReportState.Header,
            ReportState.FactionStatus,
            ReportState.Error,
            ReportState.Battle,
            ReportState.Event,
            ReportState.SkillLore,
            ReportState.ItemLore,
            ReportState.ObjectLore,
            ReportState.DefaultAttitude,
            ReportState.Attitude,
            ReportState.UnclaimedSilver,
            ReportState.Region,
            ReportState.OrdersTemplate
        };

        public enum BlockAction {
            None, Skip, Break
        }

        public async IAsyncEnumerable<ReportBlock> CategorizeAsync() {
            await using var cursor = new Cursor<MultiLineBlock>(5, source);

            ReportState lastBlock = ReportState.None;
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

                if (nextBlock == ReportState.Region) {
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

        private void MarkBlockMatched(ReportState type) {
            unmatchedBlocks.Remove(type);
        }

        private async Task<ReportState> MatchPossibleBlock(Cursor<MultiLineBlock> cursor) {
            foreach (var block in unmatchedBlocks) {
                bool isMatch = false;
                switch (block) {
                    case ReportState.Header: isMatch = cursor.MatchReportStart(); break;
                    case ReportState.FactionStatus: isMatch = cursor.MatchFactionStatus(); break;
                    case ReportState.Error: isMatch = cursor.MatchErrors(); break;
                    case ReportState.Battle: isMatch = cursor.MatchBattles(); break;
                    case ReportState.Event: isMatch = cursor.MatchEvents(); break;
                    case ReportState.SkillLore: isMatch = cursor.MatchSkills(); break;
                    case ReportState.ItemLore: isMatch = cursor.MatchItems(); break;
                    case ReportState.ObjectLore: isMatch = cursor.MatchObjects(); break;
                    case ReportState.DefaultAttitude: isMatch = cursor.MatchDefaultAttitude(); break;
                    case ReportState.Attitude: isMatch = cursor.MatchAttitue(); break;
                    case ReportState.UnclaimedSilver: isMatch = cursor.MatchUnclaimed(); break;
                    case ReportState.Region: isMatch = await cursor.MatchRegionStart(); break;
                    case ReportState.OrdersTemplate: isMatch = cursor.MatchOrdersTemplate(); break;
                }

                if (isMatch) return block;
            }

            return ReportState.Other;
        }

        private static readonly ReportState[] blocksWithHeaders = {
            ReportState.Header,
            ReportState.FactionStatus,
            ReportState.Error,
            ReportState.Battle,
            ReportState.Event,
            ReportState.SkillLore,
            ReportState.ItemLore,
            ReportState.ObjectLore,
            ReportState.OrdersTemplate
        };

        private async Task<(ReportState type, BlockAction action)> DetermineBlockTypeAsync(ReportState lastBlock, Cursor<MultiLineBlock> cursor) {
            ReportState nextBlock = lastBlock;
            BlockAction action = BlockAction.None;

            switch (lastBlock) {
                case ReportState.None:
                    if (cursor.MatchReportStart()) {
                        nextBlock = ReportState.Header;
                    }
                    else {
                        action = BlockAction.Skip;
                    }
                    break;

                case ReportState.FactionStatus:
                    if (!cursor.MatchFactionStatusItem()) {
                        nextBlock = await MatchPossibleBlock(cursor);
                    }
                    else {
                        action = BlockAction.Break;
                    }
                    break;

                case ReportState.Header:
                case ReportState.Error:
                case ReportState.Event:
                case ReportState.ItemLore:
                case ReportState.SkillLore:
                case ReportState.ObjectLore:
                case ReportState.Attitude:
                    nextBlock = await MatchPossibleBlock(cursor);
                    if (nextBlock == ReportState.Other) {
                        nextBlock = lastBlock;
                        action = BlockAction.Break;
                    }
                    break;

                case ReportState.Battle:
                    nextBlock = await MatchPossibleBlock(cursor);
                    if (nextBlock == ReportState.Other) {
                        nextBlock = lastBlock;
                        if (cursor.MatchBattle()) {
                            action = BlockAction.Break;
                        }
                    }
                    break;

                case ReportState.Region:
                    if (cursor.MatchOrdersTemplate()) {
                        nextBlock = ReportState.OrdersTemplate;
                    }
                    else if (await cursor.MatchRegionStart()) {
                        action = BlockAction.Break;
                    }
                    break;

                case ReportState.OrdersTemplate:
                    action = BlockAction.None;
                    break;

                default:
                    nextBlock = await MatchPossibleBlock(cursor);
                    if (nextBlock == ReportState.Other) {
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
