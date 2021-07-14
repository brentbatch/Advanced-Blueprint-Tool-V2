using Assets.Scripts.Model.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UpgradeResolver
{
    private readonly Dictionary<string, string> UpgradeResources = new Dictionary<string, string>();

    public UpgradeResolver(string upgradeResourcesFile)
    {
        UpgradeResourcesData upgradeData = JsonConvert.DeserializeObject<UpgradeResourcesData>(File.ReadAllText(upgradeResourcesFile));
        foreach(var upgrade in upgradeData.Upgrade.SelectMany(listlist => listlist))
        {
            UpgradeResources.Add(upgrade[0], upgrade[1]);
        }
    }

    public string UpgradeResource(string path = "")
    {
        if (UpgradeResources.TryGetValue(path, out string upgrade))
        {
            return upgrade;
        }
        return path;
    }
}
