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
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Mod : BaseUnityPlugin
    {
        public const string PluginGUID = "MeatwareMonster.MoreDefenses";
        public const string PluginName = "More Defenses";
        public const string PluginVersion = "1.0.0";

        public static ConfigEntry<float> TurretVolume;

        public static string ModLocation = Path.GetDirectoryName(typeof(Mod).Assembly.Location);

        private readonly Harmony harmony = new Harmony(PluginGUID);

        private readonly Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();

        private void Awake()
        {
            TurretVolume = Config.Bind("General", "Turret Volume", 100f, new ConfigDescription("Independent turret volume control.", new AcceptableValueRange<float>(0, 100)));

            LoadAssetBundles();
            AddTurrets();
            UnloadAssetBundles();

            harmony.PatchAll();
        }

        private void LoadAssetBundles()
        {
            foreach (var file in Directory.GetFiles($"{ModLocation}/Assets/AssetBundles").Where(file => Path.GetFileName(file) != "__folder_managed_by_vortex"))
            {
                AssetBundles.Add(Path.GetFileName(file), AssetUtils.LoadAssetBundle(file));
            }
        }

        private void UnloadAssetBundles()
        {
            foreach (var assetBundle in AssetBundles)
            {
                assetBundle.Value.Unload(false);
            }
        }

        private void AddTurrets()
        {
            var turretConfigs = new List<TurretConfig>();

            foreach (var file in Directory.GetFiles($"{ModLocation}/Assets/Configs").Where(file => Path.GetFileName(file) != "__folder_managed_by_vortex"))
            {
                turretConfigs.AddRange(TurretConfigManager.LoadTurretsFromJson(file));
            }

            turretConfigs.ForEach(turretConfig =>
            {
                if (turretConfig.enabled)
                {
                    // Load prefab from asset bundle and apply config
                    var prefab = AssetBundles[turretConfig.bundleName].LoadAsset<GameObject>(turretConfig.prefabPath);
                    var turret = prefab.AddComponent<Turret>();
                    turret.Range = turretConfig.range;
                    turret.Damage = turretConfig.damage;
                    turret.FireInterval = turretConfig.fireInterval;
                    var turretPiece = TurretConfig.Convert(prefab, turretConfig);

                    // Jotunn code is currently not setting the description, potentially a bug
                    turretPiece.Piece.m_description = turretConfig.description;

                    PieceManager.Instance.AddPiece(turretPiece);
                }
            });
        }
    }
}