// ******************************************************************
//       /\ /|       @file       LootRewardLogger.cs
//       \ V/        @brief      掉落奖励调整的日志输出
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
/// 掉落奖励调整的日志输出。
/// </summary>
internal static class LootRewardLogger
{
    /// <summary>
    /// 打印品质权重调整的对比日志。
    /// </summary>
    /// <param name="instanceName">实例类型名称</param>
    /// <param name="entries">品质条目</param>
    /// <param name="count">条目数量</param>
    /// <param name="beforeWeights">调整前的权重数组</param>
    /// <param name="beforeSum">调整前的权重总和</param>
    /// <param name="afterWeights">调整后的权重数组</param>
    /// <param name="afterSum">调整后的权重总和</param>
    /// <param name="centerM">加权中心</param>
    /// <param name="k">倾斜系数</param>
    /// <param name="factor">奖励因数</param>
    public static void LogQualityComparison(string instanceName, List<RandomContainer<int>.Entry> entries,
        int count, float[] beforeWeights, float beforeSum, float[] afterWeights, float afterSum,
        float centerM, float k, float factor)
    {
        var logLines = new List<string>
        {
            $"[LootRewardPatch] 品质权重调整 [{instanceName}]",
            $"Factor: {factor:F3}, CenterM: {centerM:F3}, K: {k:F3}",
            "Before:"
        };

        for (var i = 0; i < count; i++)
        {
            var quality = entries[i].value;
            var beforeWeight = beforeWeights[i];
            var beforeProb = beforeSum > 0f ? beforeWeight / beforeSum : 0f;
            var validMark = LootRewardQualityUtilities.IsValidQuality(quality) ? "" : "[无效]";
            logLines.Add($"  Q{quality}: Weight={beforeWeight:F3}, Prob={beforeProb:P2} {validMark}");
        }

        logLines.Add($"  Sum: {beforeSum:F3}");

        logLines.Add("After:");
        for (var i = 0; i < count; i++)
        {
            var quality = entries[i].value;
            var afterWeight = afterWeights[i];
            var afterProb = afterSum > 0f ? afterWeight / afterSum : 0f;
            var validMark = LootRewardQualityUtilities.IsValidQuality(quality) ? "" : "[无效]";
            var change = beforeWeights[i] > 0f ? (afterWeight / beforeWeights[i] - 1f) * 100f : 0f;
            logLines.Add(
                $"  Q{quality}: Weight={afterWeight:F3}, Prob={afterProb:P2}, Change={change:+#0.0;-#0.0;0}% {validMark}");
        }

        logLines.Add($"  Sum: {afterSum:F3}");
        Debug.Log(string.Join("\n", logLines));
    }
}

