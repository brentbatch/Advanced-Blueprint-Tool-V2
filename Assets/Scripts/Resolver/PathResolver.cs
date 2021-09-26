using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Extensions;
using Assets.Scripts.Model.Data;
using Assets.Scripts.Steam;
using Assets.Scripts.Unity;
using Assets.Scripts.Util;
using Microsoft.Win32;
using Newtonsoft.Json;
using SFB;
using UnityEngine;

namespace Assets.Scripts.Resolver
{
    public sealed class PathResolver
    {
        private static readonly PathResolver instance = new PathResolver();
        public static PathResolver Instance => instance;


        // regexes for path matching:
        private readonly Regex dataRegex = new Regex(@"^\.\./data", RegexOptions.IgnoreCase);
        private readonly Regex modDataRegex = new Regex(@"^\$mod_data", RegexOptions.IgnoreCase);
        private readonly Regex contentDataRegex = new Regex(@"^\$content_data", RegexOptions.IgnoreCase);
        private readonly Regex contentDataUuidRegex = new Regex(@"(?i)(^\$content_data_)([0-9A-F]{8}[-](?:[0-9A-F]{4}[-]){3}[0-9A-F]{12})", RegexOptions.IgnoreCase);
        private readonly Regex gameDataRegex = new Regex(@"^\$game_data", RegexOptions.IgnoreCase);
        private readonly Regex survivalDataRegex = new Regex(@"^\$survival_data", RegexOptions.IgnoreCase);
        private readonly Regex challengeDataRegex = new Regex(@"^\$challenge_data", RegexOptions.IgnoreCase);

        private readonly UpgradeResolver upgradeResolver;
        private readonly Dictionary<string, string> workshopModUuidPaths = new Dictionary<string, string>();

        private string steamInstallationPath;

        private string workShopPath;
        private string scrapMechanicPath;
        private string scrapMechanicAppdataUserPath;

        private readonly string scrapDataPath;
        private readonly string scrapSurvivalPath;
        private readonly string scrapChallengePath;


        public static string WorkShopPath => Instance.workShopPath;
        public static string ScrapMechanicPath => Instance.scrapMechanicPath;
        public static string ScrapMechanicAppdataUserPath => Instance.scrapMechanicAppdataUserPath;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static PathResolver() { }
        private PathResolver()
        {
            FindAppdataPath();
            FindPaths();
            ValidateAndCorrectPaths();

            scrapDataPath = Path.Combine(scrapMechanicPath, "Data");
            scrapSurvivalPath = Path.Combine(scrapMechanicPath, "Survival");
            scrapChallengePath = Path.Combine(scrapMechanicPath, "ChallengeData");
            CollectWorkshopModPaths();

            string upgradeFilePath = PathLookup.Transform(Resolve(GameController.Instance.upgradeResourcesPath));
            upgradeResolver = new UpgradeResolver(upgradeFilePath);
        }


        public static string ResolvePath(string path, string parentPath = null)
        {
            path = Instance.Resolve(path, parentPath);
            return PathLookup.Transform(path);
        }

        private string Resolve(string path, string parentPath = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";

            if (upgradeResolver != null) // have to resolve the path for the upgraderesolver , so it will be null the first time.
            {
                path = upgradeResolver.UpgradeResource(path);
            }

            path = path
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            if (dataRegex.IsMatch(path) || modDataRegex.IsMatch(path) || contentDataRegex.IsMatch(path))
            {
                if (parentPath == null)
                    throw new InvalidDataException("Invalid path or parent path not provided");
                return path
                    .Replace(dataRegex, parentPath)
                    .Replace(modDataRegex, parentPath)
                    .Replace(contentDataRegex, parentPath);
            }
        
            if (contentDataUuidRegex.IsMatch(path))
            {
                MatchCollection matchCollection = contentDataRegex.Matches(path);
                Match uuidMatch = matchCollection[1];

                if (workshopModUuidPaths.TryGetValue(uuidMatch.Value, out string workshopModPath))
                {
                    return path.Replace(contentDataUuidRegex, workshopModPath);
                }
                else
                {
                    throw new InvalidDataException($"{path} is invalid");
                }
            }


            return path
                .Replace(gameDataRegex, scrapDataPath)
                .Replace(survivalDataRegex, scrapSurvivalPath)
                .Replace(challengeDataRegex, scrapChallengePath);
        }


        private void FindAppdataPath()
        {
            try
            {
                string userdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Axolot Games",  "Scrap Mechanic", "User");

                // find steam user scrap save dir:
                DateTime lasthigh = DateTime.MinValue;
                foreach (string subdir in Directory.GetDirectories(userdir)) //get user_numbers folder that is last used
                {
                    DirectoryInfo fi1 = new DirectoryInfo(Path.Combine(subdir, "Save"));
                    DateTime created = fi1.LastWriteTime;

                    if (created > lasthigh)
                    {
                        scrapMechanicAppdataUserPath = subdir;
                        lasthigh = created;
                    }
                }

            }
            catch
            {
                Debug.LogWarning("Failed finding scrap mechanic user directory. Opening open folder dialog.");
                while(string.IsNullOrEmpty(scrapMechanicAppdataUserPath) || !Directory.Exists(scrapMechanicAppdataUserPath))
                {
                    string[] paths = StandaloneFileBrowser.OpenFolderPanel("Please select your Scrap Mechanic USER_ directory", Environment.CurrentDirectory, false);
                    string path = paths?.FirstOrDefault();
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(Path.Combine(path, "Save")) && Directory.Exists(Path.Combine(path, "Blueprints")))
                    {
                        scrapMechanicAppdataUserPath = path;
                    }
                }
            }
        }

        private bool IsScrapMechanicPath(string path)
        {
            return File.Exists(Path.Combine(path??"", "Release", "ScrapMechanic.exe"));
        }

        private void FindPaths()
        {
            try
            {
                // find steam folder
                steamInstallationPath = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", "").ToString().Replace('/', Path.DirectorySeparatorChar);

                if (string.IsNullOrEmpty(steamInstallationPath))
                {
                    // maybe proton on linux is running this tool?
                    steamInstallationPath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? ".", ".steam", "steam");
                }

                if (!string.IsNullOrEmpty(steamInstallationPath))
                {
                    // could use this to load persona name:
                    //string VdfFilePath = Path.Combine(steamInstallationPath, "config", "config.vdf");
                }

                // check base steam install folder:
                string steamApps = Path.Combine(steamInstallationPath, "steamapps");
                if (Directory.Exists(steamApps))
                {
                    this.workShopPath = Path.Combine(steamApps, "workshop", "content", "387990");
                    if (!Directory.Exists(workShopPath)) // linux?
                        this.workShopPath = Path.Combine(steamApps, "compatdata", "387990");

                    string scrapMechanicPath = Path.Combine(steamApps, "common", "Scrap Mechanic");
                    if (IsScrapMechanicPath(scrapMechanicPath))
                    {
                        this.scrapMechanicPath = scrapMechanicPath;
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
                                scrapMechanicPath = Path.Combine(library, "steamapps", "common", "Scrap Mechanic");
                                if (IsScrapMechanicPath(scrapMechanicPath))
                                {
                                    this.scrapMechanicPath = scrapMechanicPath;
                                    this.workShopPath = Path.Combine(library, "steamapps", "workshop", "content", "387990");
                                    if (!Directory.Exists(workShopPath)) // linux?
                                        this.workShopPath = Path.Combine(library, "steamapps", "compatdata", "387990");
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
        }

        private void ValidateAndCorrectPaths()
        {
            string openFolder = string.IsNullOrEmpty(steamInstallationPath) ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : steamInstallationPath;
            while (!IsScrapMechanicPath(this.scrapMechanicPath))
            {
                string[] paths = StandaloneFileBrowser.OpenFolderPanel("Please select the Scrap Mechanic folder", openFolder, false);
                string path = paths.FirstOrDefault();

                if (!IsScrapMechanicPath(path))
                    continue;

                this.scrapMechanicPath = path;
                // try locate workshop dir if not set yet:
                if (!Directory.Exists(this.workShopPath))
                {
                    this.workShopPath = Path.Combine(
                        Directory.GetParent(Directory.GetParent(this.scrapMechanicPath).FullName).FullName,
                        "workshop", "content", "387990");
                }
            }

            while (!Directory.Exists(this.workShopPath))
            {
                string[] paths = StandaloneFileBrowser.OpenFolderPanel("Please select the Workshop folder containing workshop blueprints", openFolder, false);
                if (!string.IsNullOrEmpty(paths?.FirstOrDefault()))
                {
                    this.workShopPath = paths?.FirstOrDefault();
                }
            }
        }

        private void CollectWorkshopModPaths()
        {
            foreach (string dir in Directory.GetDirectories(workShopPath))
            {
                string description = Path.Combine(dir, "description.json");
                if (!File.Exists(description))
                    continue;
                try
                {
                    DescriptionData descriptionData = JsonConvert.DeserializeObject<DescriptionData>(File.ReadAllText(description));
                    if (!string.IsNullOrEmpty(descriptionData.LocalId)
                        && !workshopModUuidPaths.ContainsKey(descriptionData.LocalId)
                        && descriptionData.Type == "Blocks and Parts")
                    {
                        workshopModUuidPaths.Add(descriptionData.LocalId, dir);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not load {description}\nError: {e}");
                }
            }
        }
    }
}