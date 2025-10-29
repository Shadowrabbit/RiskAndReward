// ******************************************************************
//       /\ /|       @file       LootRewardQualityGuard.cs
//       \ V/        @brief      最高品质概率护栏逻辑
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 03:12:31
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using System.Collections.Generic;
using System.Linq;
using Duckov.Utilities;

namespace RiskAndReward.Patch;

/// <summary>
/// 最高品质概率护栏逻辑，防止"全金装"情况。
/// </summary>
internal static class LootRewardQualityGuard
{
    /// <summary>
    /// 对最高品质概率设置上限护栏，若超额则按比例回灌至其它品质。
    /// </summary>
    /// <param name="newWeights">新权重数组</param>
    /// <param name="count">条目数量</param>
    /// <param name="entries">品质-权重条目</param>
    /// <param name="sumWPrime">当前新权重总和</param>
    /// <param name="finalSum">输出：护栏后的新权重总和</param>
    public static void ApplyTopQualityGuard(float[] newWeights, int count,
        List<RandomContainer<int>.Entry> entries, float sumWPrime, out float finalSum)
    {
        finalSum = sumWPrime;
        // 检查是否需要应用护栏
        if (!ShouldApplyTopQualityGuard(newWeights, count, entries, sumWPrime, out var idxTop))
            return;
        // 先计算其他有效品质的权重总和
        var sumValidOthers = CalculateSumOfValidOthers(newWeights, count, entries, idxTop);
        if (sumValidOthers < 1e-6f)
        {
            // 特殊情况：除了最高品质，其他有效品质权重几乎为0
            ApplyTopQualityGuardSpecialCase(newWeights, idxTop, out finalSum);
        }
        else
        {
            // 正常情况：将超额概率按比例回灌给其他有效品质
            ApplyTopQualityGuardNormalCase(newWeights, count, idxTop, sumWPrime, sumValidOthers, entries, out finalSum);
        }
    }

    /// <summary>
    /// 检查是否需要应用最高品质概率护栏。
    /// </summary>
    /// <param name="newWeights">新权重数组</param>
    /// <param name="count">条目数量</param>
    /// <param name="entries">品质-权重条目</param>
    /// <param name="sumWPrime">当前新权重总和</param>
    /// <param name="idxTop">输出：最高品质索引</param>
    /// <returns>如果需要应用护栏返回 true，否则返回 false</returns>
    private static bool ShouldApplyTopQualityGuard(float[] newWeights, int count,
        List<RandomContainer<int>.Entry> entries, float sumWPrime,
        out int idxTop)
    {
        idxTop = LootRewardQualityUtilities.FindTopQualityIndex(entries, count);
        if (idxTop < 0)
        {
            return false;
        }

        // 防御性检查：避免除零错误
        if (sumWPrime <= 0f)
        {
            return false;
        }

        var wTopOld = newWeights[idxTop];
        var pTop = wTopOld / sumWPrime;
        return pTop > LootRewardConstants.MaxTopQualityProb;
    }

    /// <summary>
    /// 计算除最高品质外的其他有效品质权重总和。
    /// </summary>
    /// <param name="newWeights">新权重数组</param>
    /// <param name="count">条目数量</param>
    /// <param name="entries">品质-权重条目</param>
    /// <param name="idxTop">最高品质索引</param>
    /// <returns>其他有效品质权重总和</returns>
    private static float CalculateSumOfValidOthers(float[] newWeights, int count,
        List<RandomContainer<int>.Entry> entries, int idxTop)
    {
        var sum = 0f;
        for (var i = 0; i < count; i++)
        {
            if (i == idxTop) continue;
            // 只计算有效品质的权重
            if (LootRewardQualityUtilities.IsValidQuality(entries[i].value))
            {
                sum += newWeights[i];
            }
        }

        return sum;
    }

    /// <summary>
    /// 处理最高品质护栏的特殊情况：除了最高品质其余几乎为0。
    /// </summary>
    /// <param name="newWeights">新权重数组</param>
    /// <param name="idxTop">最高品质索引</param>
    /// <param name="finalSum">输出：护栏后的新权重总和</param>
    private static void ApplyTopQualityGuardSpecialCase(float[] newWeights, int idxTop, out float finalSum)
    {
        // 先计算当前总和（包括最高品质和其他可能存在的无效品质权重）
        var currentSum = newWeights.Sum();
        // 将最高品质权重设为上限值，确保概率不超过 MaxTopQualityProb
        // 如果总和为0，设为默认值
        if (currentSum <= 0f)
        {
            newWeights[idxTop] = LootRewardConstants.MaxTopQualityProb;
            finalSum = 1f;
            return;
        }

        // 将最高品质权重设为：MaxTopQualityProb * (当前总和 - 原最高品质权重 + 新最高品质权重)
        // 解方程：新最高品质权重 = MaxTopQualityProb * (当前总和 - 原最高品质权重 + 新最高品质权重)
        // 简化：新最高品质权重 = MaxTopQualityProb * (当前总和 - 原最高品质权重) / (1 - MaxTopQualityProb)
        var otherSum = currentSum - newWeights[idxTop];
        // 如果其他权重总和为0（只有最高品质有权重），直接设置最高品质权重为上限值
        if (otherSum <= 1e-6f)
        {
            newWeights[idxTop] = LootRewardConstants.MaxTopQualityProb;
            finalSum = 1f;
        }
        else
        {
            newWeights[idxTop] = LootRewardConstants.MaxTopQualityProb * otherSum / (1f - LootRewardConstants.MaxTopQualityProb);
            // 重新计算总和
            finalSum = otherSum + newWeights[idxTop];
        }
    }

    /// <summary>
    /// 处理最高品质护栏的正常情况：将超额概率按比例回灌给其他有效品质（无效品质保持不变）。
    /// </summary>
    /// <param name="newWeights">新权重数组</param>
    /// <param name="count">条目数量</param>
    /// <param name="idxTop">最高品质索引</param>
    /// <param name="sumWPrime">当前新权重总和</param>
    /// <param name="sumValidOthers">其他有效品质权重总和</param>
    /// <param name="entries">品质-权重条目</param>
    /// <param name="finalSum">输出：护栏后的新权重总和</param>
    private static void ApplyTopQualityGuardNormalCase(float[] newWeights, int count, int idxTop,
        float sumWPrime, float sumValidOthers, List<RandomContainer<int>.Entry> entries,
        out float finalSum)
    {
        // 将超额概率按比例回灌给其他有效品质
        var targetTop = LootRewardConstants.MaxTopQualityProb * sumWPrime;
        var scaleValidOthers = sumValidOthers > 1e-6f ? (sumWPrime - targetTop) / sumValidOthers : 1f;
        for (var i = 0; i < count; i++)
        {
            if (i == idxTop) continue;
            // 只对有效品质进行回灌缩放，无效品质保持不变
            if (LootRewardQualityUtilities.IsValidQuality(entries[i].value))
            {
                newWeights[i] *= scaleValidOthers;
            }
        }

        newWeights[idxTop] = targetTop;
        // 重新计算总和
        finalSum = LootRewardQualityUtilities.RecalculateWeightSum(newWeights, count);
    }
}

