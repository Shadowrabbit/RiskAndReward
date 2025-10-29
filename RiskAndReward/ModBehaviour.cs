// ******************************************************************
//       /\ /|       @file       ModBehaviour.cs
//       \ V/        @brief      
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-10-29 04:39:44
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using HarmonyLib;
using UnityEngine;

namespace RiskAndReward;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    /// <summary>
    /// Harmony实例，用于应用补丁
    /// </summary>
    private Harmony _harmony = null!;

    protected override void OnAfterSetup()
    {
        base.OnAfterSetup();
        ConfigManager.Instance.Init(info.path);
        ConfigManager.Instance.GenerateConfigs();
        // 初始化 Harmony 补丁
        _harmony = new Harmony("RiskAndReward");
        _harmony.PatchAll();
        // 输出初始化日志
        Debug.Log("[RiskAndReward] Mod initialization completed, Harmony patches applied");
    }

    /// <summary>
    /// Mod销毁方法
    /// </summary>
    protected override void OnBeforeDeactivate()
    {
        // 移除我们添加的补丁
        _harmony.UnpatchAll("RiskAndReward");
        Debug.Log("[RiskAndReward] Mod unloaded, Harmony patches removed");
    }
}