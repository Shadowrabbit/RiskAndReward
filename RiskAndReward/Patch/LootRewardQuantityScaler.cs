// ******************************************************************
//       /\ /|       @file       LootRewardQuantityScaler.cs
//       \ V/        @brief      根据奖励因数缩放掉落数量区间
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 03:12:31
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using HarmonyLib;
using UnityEngine;

namespace RiskAndReward.Patch;

/// <summary>
/// 根据奖励因数缩放掉落数量区间。
/// </summary>
internal static class LootRewardQuantityScaler
{
    /// <summary>
    /// 根据奖励因数按幂函数缩放 <c>Vector2Int randomCount</c> 的最小/最大值，并设置护栏。
    /// </summary>
    /// <param name="instance">目标对象</param>
    /// <param name="fieldName">字段名，通常为 <c>randomCount</c></param>
    /// <param name="factor">奖励因数</param>
    public static void TryScaleRandomCount(object instance, string fieldName, float factor)
    {
        var f = AccessTools.Field(instance.GetType(), fieldName);
        if (f == null || f.FieldType != typeof(Vector2Int)) return;
        var old = (Vector2Int)f.GetValue(instance);
        // 如果原值为0，则保持为0（不进行缩放）
        if (old.y <= 0)
        {
            return;
        }

        // 幂函数放大量级（边际递减），避免爆量
        var scaleFactor = Mathf.Pow(factor, LootRewardConstants.AlphaQuantity);
        var nx = Mathf.Max(1, Mathf.CeilToInt(old.x * scaleFactor));
        var ny = Mathf.Max(1, Mathf.CeilToInt(old.y * scaleFactor));
        // 护栏：不超过原max的 MaxQuantityMultiplier 倍
        var hardMax = Mathf.CeilToInt(old.y * LootRewardConstants.MaxQuantityMultiplier);
        if (ny > hardMax) ny = hardMax;
        if (nx > ny) ny = nx; // 保证区间有效

        var newValue = new Vector2Int(nx, ny);

        // 打印Patch前后对比
        Debug.Log($"[LootRewardPatch] 数量区间调整 [{instance.GetType().Name}] | " +
                  $"Before: [{old.x}, {old.y}] | After: [{newValue.x}, {newValue.y}] | " +
                  $"Factor: {factor:F2}, ScaleFactor: {scaleFactor:F3}");

        f.SetValue(instance, newValue);
    }
}

