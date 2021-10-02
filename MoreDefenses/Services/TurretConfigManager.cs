using System.Collections.Generic;
using Jotunn.Utils;
using MoreDefenses.Models;

namespace MoreDefenses.Services
{
    class TurretConfigManager
    {
        public static List<TurretConfig> LoadTurretsFromJson(string turretConfigPath)
        {
            var json = AssetUtils.LoadText(turretConfigPath);
            return SimpleJson.SimpleJson.DeserializeObject<List<TurretConfig>>(json);
        }
    }
}
