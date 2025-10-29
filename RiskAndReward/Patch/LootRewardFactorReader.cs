// ******************************************************************
//       /\ /|       @file       LootRewardFactorReader.cs
//       \ V/        @brief      读取当前游戏规则的奖励因数
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 03:12:31
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using Duckov.Rules;
using RiskAndReward;
using RiskAndReward.Defs;

namespace RiskAndReward.Patch;

/// <summary>
/// 读取当前游戏规则对应的奖励因数。
/// </summary>
internal static class LootRewardFactorReader
{
    /// <summary>
    /// 读取当前游戏规则对应的奖励因数。若读取失败或缺省则返回 1。
    /// </summary>
    /// <returns>配置表中的 <c>rewardFactor</c>，默认 1</returns>
    public static float GetCurrentRewardFactor()
    {
        // 将难度映射到 Def 名称，然后从配置读取 rewardFactor
        var defName = GameRulesManager.SelectedRuleIndex switch
        {
            RuleIndex.Standard => DefLootReward.DStandard,
            RuleIndex.StandardChallenge => DefLootReward.DStandardChallenge,
            RuleIndex.Custom => DefLootReward.DCustom,
            RuleIndex.Easy => DefLootReward.DEasy,
            RuleIndex.ExtraEasy => DefLootReward.DExtraEasy,
            RuleIndex.Hard => DefLootReward.DHard,
            RuleIndex.ExtraHard => DefLootReward.DExtraHard,
            RuleIndex.Rage => DefLootReward.DRage,
            _ => DefLootReward.DStandard
        };

        try
        {
            var row = ConfigManager.Instance.cfgLootReward.Find(defName);
            return row.rewardFactor;
        }
        catch
        {
            return 1f;
        }
    }
}

