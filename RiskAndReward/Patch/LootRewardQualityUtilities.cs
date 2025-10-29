// ******************************************************************
//       /\ /|       @file       LootRewardQualityUtilities.cs
//       \ V/        @brief      品质权重调整的辅助工具函数
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 03:12:31
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

namespace RiskAndReward.Patch;

/// <summary>
/// 品质权重调整的辅助工具函数。
/// </summary>
internal static class LootRewardQualityUtilities
{
    /// <summary>
    /// 检查品质值是否在有效范围内（White=1 到 Red=6）。
    /// </summary>
    /// <param name="quality">品质值</param>
    /// <returns>有效返回 true，否则返回 false</returns>
    public static bool IsValidQuality(int quality)
    {
        return quality is >= LootRewardConstants.MinValidQuality and <= LootRewardConstants.MaxValidQuality;
    }

    /// <summary>
    /// 计算品质分布的加权中心值，用作倾斜的参考点（仅考虑有效品质范围 1-6）。
    /// </summary>
    /// <param name="entries">品质-权重条目</param>
    /// <param name="count">条目数量</param>
    /// <returns>加权中心，若无效则返回 -1</returns>
    public static float CalculateWeightedQualityCenter(
        List<RandomContainer<int>.Entry> entries, int count)
    {
        var sumW = 0f;
        var sumQiWi = 0f;
        for (var i = 0; i < count; i++)
        {
            var e = entries[i];
            // 只考虑有效品质范围（White=1 到 Red=6）
            if (!IsValidQuality(e.value)) continue;

            var w = Mathf.Max(0.0001f, e.weight); // 防止0权重
            sumW += w;
            sumQiWi += e.value * w;
        }

        return sumW > 0f ? sumQiWi / sumW : -1f;
    }

    /// <summary>
    /// 计算品质倾斜系数 k = β × ln(factor)。
    /// </summary>
    /// <param name="factor">奖励因数</param>
    /// <returns>倾斜系数 k</returns>
    public static float CalculateTiltFactor(float factor)
    {
        return LootRewardConstants.BetaQuality * Mathf.Log(Mathf.Max(factor, 1e-4f));
    }

    /// <summary>
    /// 应用指数倾斜，得到新权重数组 w' = w × exp(k × (i - m))（无效品质保持原权重不变）。
    /// </summary>
    /// <param name="entries">品质-权重条目</param>
    /// <param name="count">条目数量</param>
    /// <param name="centerM">加权中心 m</param>
    /// <param name="k">倾斜系数</param>
    /// <param name="sumWPrime">输出：新权重总和</param>
    /// <returns>新权重数组</returns>
    public static float[] ApplyTiltWeights(List<RandomContainer<int>.Entry> entries,
        int count, float centerM, float k, out float sumWPrime)
    {
        sumWPrime = 0f;
        var newWeights = new float[count];
        for (var i = 0; i < count; i++)
        {
            var e = entries[i];
            var baseW = Mathf.Max(0.0001f, e.weight);

            // 无效品质保持原权重不变，不进行倾斜
            if (!IsValidQuality(e.value))
            {
                newWeights[i] = baseW;
            }
            else
            {
                var tilt = Mathf.Exp(k * (e.value - centerM));
                var wp = Mathf.Max(0.00001f, baseW * tilt); // 下限保护
                newWeights[i] = wp;
            }

            sumWPrime += newWeights[i];
        }

        return newWeights;
    }

    /// <summary>
    /// 将计算后的新权重写回容器条目列表。
    /// </summary>
    /// <param name="entries">品质-权重条目</param>
    /// <param name="count">条目数量</param>
    /// <param name="newWeights">新权重数组</param>
    public static void WriteBackWeights(List<RandomContainer<int>.Entry> entries,
        int count, float[] newWeights)
    {
        for (var i = 0; i < count; i++)
        {
            var e = entries[i];
            e.weight = newWeights[i];
            entries[i] = e;
        }
    }

    /// <summary>
    /// 查找最高品质所在的索引（动态查找容器中的最大品质值，仅考虑有效品质范围 1-6）。
    /// </summary>
    /// <param name="entries">品质条目</param>
    /// <param name="count">条目数量</param>
    /// <returns>最高品质的索引，未找到返回 -1</returns>
    public static int FindTopQualityIndex(List<RandomContainer<int>.Entry> entries,
        int count)
    {
        if (count == 0) return -1;

        var maxQuality = int.MinValue;
        var maxIndex = -1;
        for (var i = 0; i < count; i++)
        {
            var quality = entries[i].value;
            // 只考虑有效品质范围（White=1 到 Red=6）
            if (!IsValidQuality(quality) || quality <= maxQuality) continue;
            maxQuality = quality;
            maxIndex = i;
        }

        return maxIndex;
    }

    /// <summary>
    /// 重新计算权重数组的总和。
    /// </summary>
    /// <param name="newWeights">权重数组</param>
    /// <param name="count">数组长度</param>
    /// <returns>权重总和</returns>
    public static float RecalculateWeightSum(float[] newWeights, int count)
    {
        var sum = 0f;
        for (var i = 0; i < count; i++)
            sum += newWeights[i];
        return sum;
    }
}

