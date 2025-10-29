// ******************************************************************
//       /\ /|       @file       LootRewardFactor.cs
//       \ V/        @brief      难度→奖励因数(rewardFactor) 映射与读取
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-10-29 03:12:31
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using Duckov.Rules;
using RiskAndReward.Defs;

namespace RiskAndReward.Utils
{
    public static class LootRewardFactor
    {
        public static float DefaultFactor = 1f;

        public static float GetCurrentRewardFactor()
        {
            var key = MapRuleIndexToDefName(GameRulesManager.SelectedRuleIndex);
            return GetFactorByKey(key);
        }

        public static float GetFactorByKey(string defName)
        {
            try
            {
                var row = ConfigManager.Instance.cfgLootReward.Find(defName);
                return row.rewardFactor;
            }
            catch
            {
                return DefaultFactor;
            }
        }

        private static string MapRuleIndexToDefName(RuleIndex idx)
        {
            return idx switch
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
        }
    }
}