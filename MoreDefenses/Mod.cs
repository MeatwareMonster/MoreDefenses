// MoreDefenses
// a Valheim mod skeleton using Jötunn
// 
// File:    MoreDefenses.cs
// Project: MoreDefenses

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using MoreDefenses.Models;
using MoreDefenses.Services;
using UnityEngine;

namespace MoreDefenses
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class Mod : BaseUnityPlugin
  {
    public const string PluginGUID = "MeatwareMonster.MoreDefenses";
    public const string PluginName = "More Defenses";
    public const string PluginVersion = "1.2.0";

    public static ConfigEntry<int> TurretVolume;

    public static string ModLocation = Path.GetDirectoryName(typeof(Mod).Assembly.Location);

    private readonly Harmony m_harmony = new Harmony(PluginGUID);

    private readonly Dictionary<string, AssetBundle> m_assetBundles = new Dictionary<string, AssetBundle>();

    private void Awake()
    {
      TurretVolume = Config.Bind("General", "Turret Volume", 100, new ConfigDescription("Independent turret volume control.", new AcceptableValueRange<int>(0, 100)));

      LoadAssetBundles();
      AddTurrets();
      UnloadAssetBundles();

      m_harmony.PatchAll();
    }

    private void LoadAssetBundles()
    {
      foreach (var file in Directory.GetFiles($"{ModLocation}/Assets/AssetBundles").Where(file => Path.GetFileName(file) != "__folder_managed_by_vortex"))
      {
        m_assetBundles.Add(Path.GetFileName(file), AssetUtils.LoadAssetBundle(file));
      }
    }

    private void UnloadAssetBundles()
    {
      foreach (var assetBundle in m_assetBundles)
      {
        assetBundle.Value.Unload(false);
      }
    }

    private void AddTurrets()
    {
      var turretConfigs = new List<TurretConfig>();
      var customConfigFiles = Directory.Exists($"{ModLocation}/Assets/CustomConfigs") ? Directory.GetFiles($"{ModLocation}/Assets/CustomConfigs").Where(file => Path.GetFileName(file) != "__folder_managed_by_vortex").ToDictionary(file => Path.GetFileName(file)) : new Dictionary<string, string>();

      foreach (var file in Directory.GetFiles($"{ModLocation}/Assets/Configs").Where(file => Path.GetFileName(file) != "__folder_managed_by_vortex"))
      {
        string configPath;
        if (customConfigFiles.TryGetValue(Path.GetFileName(file), out var customConfigFile))
        {
          configPath = customConfigFile;
        }
        else
        {
          configPath = file;
        }

        turretConfigs.AddRange(TurretConfigManager.LoadTurretsFromJson(configPath));
      }

      turretConfigs.ForEach(turretConfig =>
      {
        if (turretConfig.enabled)
        {
          // Load prefab from asset bundle and apply config
          var prefab = m_assetBundles[turretConfig.bundleName].LoadAsset<GameObject>(turretConfig.prefabPath);
          var newPrefab = PrefabManager.Instance.CreateClonedPrefab(turretConfig.name, prefab);
          if (newPrefab != null)
          {
            prefab = newPrefab;
          }
          var turret = prefab.AddComponent<Turret>();
          turret.Initialize(turretConfig);
          var turretPiece = TurretConfig.Convert(prefab, turretConfig);

          // Jotunn code is currently not setting the description, potentially a bug
          //turretPiece.Piece.m_description = turretConfig.description;

          PieceManager.Instance.AddPiece(turretPiece);
        }
      });
    }
  }
}