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
        public float mass;
        public float thrust;
        public float lift;
        public float drag;
        public float turnSpeed;
        public float cameraDistance;
        public bool enabled;
        public List<TurretConfigRequirement> resources;

        public static CustomPiece Convert(GameObject prefab, TurretConfig turretConfig)
        {
            return new CustomPiece(prefab,
                true,
                new PieceConfig
                {
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
