using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace MoreDefenses.Models
{
    [Serializable]
    public class TurretConfigRequirement
    {
        public string item;
        public int amount;

        public static RequirementConfig Convert(TurretConfigRequirement turretConfigRequirement)
        {
            return new RequirementConfig()
            {
                Amount = turretConfigRequirement.amount,
                Item = turretConfigRequirement.item,
                Recover = true
            };
        }
    }

    [Serializable]
    public class TurretConfig
    {
        public string name;
        public string bundleName;
        public string prefabPath;
        public string description;
        public string pieceTable;
        public bool enabled;
        public string type;
        public float fireInterval;
        public float damage;
        public float pierceDamage;
        public float fireDamage;
        public float frostDamage;
        public float lightningDamage;
        public float poisonDamage;
        public float spiritDamage;
        public float range;
        public float damageRadius;
        public bool canShootFlying;
        public bool isContinuous;
        public List<TurretConfigRequirement> resources;

        public static CustomPiece Convert(GameObject prefab, TurretConfig turretConfig)
        {
            return new CustomPiece(prefab,
                true,
                new PieceConfig
                {
                    Name = turretConfig.name,
                    Description = turretConfig.description,
                    Enabled = turretConfig.enabled,
                    PieceTable = turretConfig.pieceTable,
                    Category = "More Defenses",
                    Requirements = turretConfig.resources.Select(TurretConfigRequirement.Convert).ToArray()
                }
            );
        }
    }
}
