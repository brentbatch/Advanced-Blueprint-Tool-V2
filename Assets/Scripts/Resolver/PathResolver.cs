using Assets.Scripts.Model.Data;
using Assets.Scripts.Unity;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFB;
using SLMFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class PathResolver
{

    private static bool pathsLoaded;
    /*
    public delegate void PathsLoaded();
    public static event PathsLoaded OnPathsLoaded;

    public static void WhenPathsLoaded(PathsLoaded task)
    {
        if (pathsLoaded)
            task();
        else
            OnPathsLoaded += task;
    }*/

    private static UpgradeResolver upgradeResolver;
    private static readonly Dictionary<string, string> workshopModUuids = new Dictionary<string, string>(); // todo: fill this array
    private static string scrapMechanicPath;
    private static string scrapDataPath;
    private static string scrapSurvivalPath;
    private static string scrapChallengePath;
    private static string workShopPath;

    private static string scrapMechanicAppdataPath1;
    public static string ScrapMechanicAppdataUserPath
    {
        get
        {
            if (scrapMechanicAppdataPath1 == null)
                FindAppdataPath();
            return scrapMechanicAppdataPath1;
        }
        private set => scrapMechanicAppdataPath1 = value;
    }

    public static string WorkShopPath
    {
        get
        {
            if (workShopPath == null)
                Initialize();
            return workShopPath;
        }
        set => workShopPath = value;
    }

    public static void Initialize()
    {
        FindAppdataPath();
        FindPaths();
        string upgradeFilePath = ResolvePath(GameController.Instance.upgradeResourcesPath);
        upgradeResolver = new UpgradeResolver(upgradeFilePath);
    }

    public static string ResolvePath(string path, string parentPath = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "";

        if (upgradeResolver != null) // have to resolve the path for the upgraderesolver , so it will be null the first time.
        {
            path = upgradeResolver.UpgradeResource(path);
        }
        path = path.ToLower().Replace(@"/", @"\");

        if (path.Contains(@"..\data") || path.Contains("$mod_data") || path.Contains("$content_data"))
        {
            if (parentPath == null)
                throw new InvalidDataException("Invalid path or parent path not provided");
            return path
                .Replace(@"..\data", parentPath)
                .Replace("$mod_data", parentPath)
                .Replace("$content_data", parentPath);
        }
        if (!pathsLoaded)
            Initialize();

        if (path.Contains("$content_data_"))
        {
            string uuid = path.Substring("$content_data_".Length, "23f3a760-bb11-46b0-b2ac-2370aedd10be".Length);
            return path
                .Replace($"$content_data_{uuid}", workshopModUuids[uuid]);
        }

        return path
            .Replace("$game_data", scrapDataPath)
            .Replace("$survival_data", scrapSurvivalPath)
            .Replace("$challenge_data", scrapChallengePath);
    }


    private static void FindAppdataPath()
    {
        string userdir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Axolot Games\\Scrap Mechanic\\User";

        // find steam user scrap save dir:
        DateTime lasthigh = DateTime.MinValue;
        foreach (string subdir in Directory.GetDirectories(userdir)) //get user_numbers folder that is last used
        {
            DirectoryInfo fi1 = new DirectoryInfo(subdir + @"\Save");
            DateTime created = fi1.LastWriteTime;

            if (created > lasthigh)
            {
                ScrapMechanicAppdataUserPath = subdir;
                lasthigh = created;
            }
        }
    }

    private static bool IsScrapMechanicPath(string path)
    {
        return File.Exists(Path.Combine(path, "Release", "ScrapMechanic.exe"));
    }

    private static void FindPaths()
    {
        string steamInstallationPath = "";
        try
        {
            // find steam folder
            steamInstallationPath = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", "").ToString().Replace('/', Path.DirectorySeparatorChar);
            
            if (!string.IsNullOrEmpty(steamInstallationPath))
            {
                // could use this to load persona name:
                //string VdfFilePath = Path.Combine(steamInstallationPath, "config", "config.vdf");
            }

            // check base steam install folder:
            string steamApps = Path.Combine(steamInstallationPath, "steamapps");
            if (Directory.Exists(steamApps))
            {
                PathResolver.workShopPath = Path.Combine(steamApps, "workshop", "content", "387990");

                string scrapMechanicPath = Path.Combine(steamApps, "common", "Scrap Mechanic");
                if (IsScrapMechanicPath(scrapMechanicPath))
                {
                    PathResolver.scrapMechanicPath = scrapMechanicPath;
                }
                else
                {
                    var libraryFile = new KeyValue();
                    libraryFile.ReadFileAsText(Path.Combine(steamApps, "libraryfolders.vdf"));

                    for (int i = 1; i < 8; i++)
                    {
                        string library = libraryFile[i.ToString()].Value;
                        if (Directory.Exists(library))
                        {
                            scrapMechanicPath = Path.Combine(library, "steamApps", "common", "Scrap Mechanic");
                            if (IsScrapMechanicPath(scrapMechanicPath))
                            {
                                PathResolver.scrapMechanicPath = scrapMechanicPath;
                                PathResolver.workShopPath = Path.Combine(
                                    Directory.GetParent(
                                        Directory.GetParent(
                                            PathResolver.scrapMechanicPath).FullName).FullName,
                                    "workshop", "content", "387990");
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        string folder = steamInstallationPath == "" ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : steamInstallationPath;
        while (!IsScrapMechanicPath(PathResolver.scrapMechanicPath))
        {
            StandaloneFileBrowser.OpenFolderPanelAsync("Please select the Scrap Mechanic folder", steamInstallationPath, false, (string[] paths) =>
            {
                if (IsScrapMechanicPath(paths.First()))
                {
                    PathResolver.scrapMechanicPath = paths.First();
                    if (!Directory.Exists(PathResolver.workShopPath))
                    {
                        // try find workshop
                        PathResolver.workShopPath = Path.Combine(
                            Directory.GetParent(
                                Directory.GetParent(
                                    PathResolver.scrapMechanicPath).FullName).FullName, 
                            "workshop", "content", "387990");
                    }
                }
            });
        }

        while (!Directory.Exists(PathResolver.workShopPath))
        {
            StandaloneFileBrowser.OpenFolderPanelAsync("Please select the Workshop folder containing workshop blueprints", steamInstallationPath, false, (string[] paths) =>
            {
                if (!string.IsNullOrEmpty(paths?.First()))
                {
                    PathResolver.workShopPath = paths?.First();
                }
            });
        }

        while (!Directory.Exists(PathResolver.ScrapMechanicAppdataUserPath))
        {
            StandaloneFileBrowser.OpenFolderPanelAsync("Please select your User_<numbersHere> folder", steamInstallationPath, false, (string[] paths) =>
            {
                if (!string.IsNullOrEmpty(paths?.First()))
                {
                    PathResolver.ScrapMechanicAppdataUserPath = paths?.First();
                }
            });
        }

        scrapDataPath = Path.Combine(scrapMechanicPath, "Data");
        scrapSurvivalPath = Path.Combine(scrapMechanicPath, "Survival");
        scrapChallengePath = Path.Combine(scrapMechanicPath, "ChallengeData");

        foreach (string dir in Directory.GetDirectories(workShopPath))
        {
            string description = Path.Combine(dir, "description.json");
            if (File.Exists(description))
            {
                try
                {
                    DescriptionData descriptionData = JsonConvert.DeserializeObject<DescriptionData>(File.ReadAllText(description));
                    if(!string.IsNullOrEmpty(descriptionData.LocalId) 
                            && !workshopModUuids.ContainsKey(descriptionData.LocalId)
                            && descriptionData.Type == "Blocks and Parts")
                        workshopModUuids.Add(descriptionData.LocalId, dir);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not load {description}\nError: {e}");
                }
            }
        }

        pathsLoaded = true;
    }

}