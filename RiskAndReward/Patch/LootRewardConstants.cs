// ******************************************************************
//       /\ /|       @file       LootRewardConstants.cs
//       \ V/        @brief      掉落奖励调整的常量定义
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 03:12:31
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

namespace RiskAndReward.Patch;

/// <summary>
/// 掉落奖励调整的常量定义。
/// </summary>
internal static class LootRewardConstants
{
    public const float AlphaQuantity = 0.75f; // 数量增长幂指数（边际递减）
    public const float BetaQuality = 1.2f; // 品质敏感度
    public const float MaxQuantityMultiplier = 1.8f; // 数量上限的保护倍数
    public const float MaxTopQualityProb = 0.25f; // 最高品质的概率上限护栏

    public const int MinValidQuality = 1; // White
    public const int MaxValidQuality = 6; // Red
}

