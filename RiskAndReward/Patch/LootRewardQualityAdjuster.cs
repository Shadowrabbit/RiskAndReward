// ******************************************************************
//       /\ /|       @file       LootRewardQualityAdjuster.cs
//       \ V/        @brief      根据奖励因数调整品质权重
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 03:12:31
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using System.Collections.Generic;
using Duckov.Utilities;
using HarmonyLib;

namespace RiskAndReward.Patch;

/// <summary>
/// 根据奖励因数调整品质权重。
/// </summary>
internal static class LootRewardQualityAdjuster
{
    /// <summary>
    /// 根据奖励因数对品质权重容器进行倾斜调整，并刷新百分比显示。
    /// </summary>
    /// <param name="instance">目标对象</param>
    /// <param name="fieldName">字段名，通常为 <c>qualities</c></param>
    /// <param name="factor">奖励因数</param>
    public static void TryAdjustQualities(object instance, string fieldName, float factor)
    {
        var f = AccessTools.Field(instance.GetType(), fieldName);
        if (f == null) return;
        var container = f.GetValue(instance) as RandomContainer<int>;
        if (container?.entries == null || container.entries.Count == 0)
            return;
        var entries = container.entries;
        var count = entries.Count;

        // 记录Patch前的权重
        var beforeWeights = new float[count];
        var beforeSum = 0f;
        for (var i = 0; i < count; i++)
        {
            beforeWeights[i] = entries[i].weight;
            beforeSum += beforeWeights[i];
        }

        // 步骤1：计算权重加权平均品质（中心 m）
        var centerM = LootRewardQualityUtilities.CalculateWeightedQualityCenter(entries, count);
        if (centerM < 0f) return;
        // 步骤2：计算倾斜参数 k = β × ln(factor)
        var k = LootRewardQualityUtilities.CalculateTiltFactor(factor);
        // 步骤3：应用指数倾斜权重 w' = w × exp(k × (i - m))
        var newWeights = LootRewardQualityUtilities.ApplyTiltWeights(entries, count, centerM, k, out var sumWPrime);
        if (sumWPrime <= 0f) return;
        // 步骤4：最高品质概率护栏（防止"全金装"）
        LootRewardQualityGuard.ApplyTopQualityGuard(newWeights, count, entries, sumWPrime, out sumWPrime);
        // 步骤5：回写新权重并刷新百分比显示
        LootRewardQualityUtilities.WriteBackWeights(entries, count, newWeights);
        container.RefreshPercent();

        // 打印Patch前后对比
        LootRewardLogger.LogQualityComparison(instance.GetType().Name, entries, count, beforeWeights, beforeSum,
            newWeights, sumWPrime, centerM, k, factor);
    }
}

