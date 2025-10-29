// ******************************************************************
//       /\ /|       @file       LootRewardPatch.cs
//       \ V/        @brief      Patch: 按奖励因数调整预设/刷点箱的品质与数量
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 03:12:31
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using Duckov.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace RiskAndReward.Patch;

/// <summary>
/// Harmony Patch：按奖励因数调整预设/刷点箱的品质与数量。
/// </summary>
[HarmonyPatch]
[UsedImplicitly]
public static class LootRewardPatch
{
    [HarmonyPatch(typeof(LootBoxLoader), nameof(LootBoxLoader.Setup))]
    [HarmonyPrefix]
    [UsedImplicitly]
    private static void LootBoxLoader_Setup_Prefix(LootBoxLoader __instance)
    {
        ApplyFactorToLootBoxLoader(__instance);
    }

    [HarmonyPatch(typeof(LootSpawner), nameof(LootSpawner.Setup))]
    [HarmonyPrefix]
    [UsedImplicitly]
    private static void LootSpawner_Setup_Prefix(LootSpawner __instance)
    {
        ApplyFactorToLootSpawner(__instance);
    }

    /// <summary>
    /// 针对 <see cref="LootBoxLoader"/> 应用奖励因数的包装函数。
    /// </summary>
    /// <param name="instance">刷箱加载器实例</param>
    private static void ApplyFactorToLootBoxLoader(LootBoxLoader instance)
    {
        ApplyFactorToInstance(instance);
    }

    /// <summary>
    /// 针对 <see cref="LootSpawner"/> 应用奖励因数的包装函数。
    /// </summary>
    /// <param name="instance">刷怪点掉落生成器实例</param>
    private static void ApplyFactorToLootSpawner(LootSpawner instance)
    {
        ApplyFactorToInstance(instance);
    }

    /// <summary>
    /// 按当前奖励因数对目标对象的掉落数量区间与品质权重进行调整。
    /// </summary>
    /// <param name="instance">包含 <c>randomCount</c> 与 <c>qualities</c> 字段的对象</param>
    private static void ApplyFactorToInstance(object instance)
    {
        var factor = LootRewardFactorReader.GetCurrentRewardFactor();
        if (Mathf.Approximately(factor, 1f)) return;
        LootRewardQuantityScaler.TryScaleRandomCount(instance, "randomCount", factor);
        LootRewardQualityAdjuster.TryAdjustQualities(instance, "qualities", factor);
    }
}