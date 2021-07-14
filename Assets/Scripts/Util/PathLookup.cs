using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    // case insensitive path lookup for solving linux compatibility issues.
    public sealed class PathLookup
    {

        private static readonly PathLookup instance = new PathLookup();
        public static PathLookup Instance => instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static PathLookup() { }

#if UNITY_STANDALONE_LINUX //|| true // true for testing purposes

        #region stuff
        public interface IPath
        {
            string CaseSensitiveName { get; set; }
        }
        public struct Folder : IPath
        {
            public Dictionary<string, IPath> SubPath { get; set; }
            public string CaseSensitiveName { get; set; }
        }
        public struct File : IPath
        {
            public string CaseSensitiveName { get; set; }
        }
        #endregion
        public Dictionary<string, Folder> RootFolderCache { get; }
        private PathLookup()
        {
            RootFolderCache = new Dictionary<string, Folder>();
        }

        public static string Transform(string path)
        {
            try
            {
                return instance.TransformPath(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"An error occurred while loading path: {path}");
                Debug.LogException(e);
                return path; // whatever, probably doesn't exist anyway
            }
        }

        private string TransformPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            path = path
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            Stack<string> splitPath = new Stack<string>( path.ToLower().Split(Path.DirectorySeparatorChar).Reverse() );
            string root = splitPath.Pop();

            StringBuilder stringBuilder = new StringBuilder();

            if (RootFolderCache.TryGetValue(root, out Folder rootFolder))
            {
                stringBuilder.Append(rootFolder.CaseSensitiveName);
                Lookup(rootFolder, splitPath, stringBuilder);
                return stringBuilder.ToString();
            }
            else
            {
                // new root dir
                string name = path.Split(Path.DirectorySeparatorChar)[0]; // we are assuming the root folder is always cased correctly
                Folder folder = new Folder
                {
                    CaseSensitiveName = name,
                    SubPath = new Dictionary<string, IPath>()
                };
                RootFolderCache.Add(root, folder);
                Lookup(folder, splitPath, stringBuilder);
                Debug.Log($"{path} -> {stringBuilder}");
                return stringBuilder.ToString();
            }
        }

        private void Lookup(Folder folder, Stack<string> subPaths, StringBuilder stringBuilder)
        {
            string next = subPaths.Pop();

            stringBuilder.Append(Path.DirectorySeparatorChar);

            if (folder.SubPath.TryGetValue(next, out IPath subPath))
            {
                stringBuilder.Append(subPath.CaseSensitiveName);
                if (subPath is Folder subFolder)
                {
                    if (subPaths.Count > 0)
                        Lookup(subFolder, subPaths, stringBuilder);
                }
                else if (subPaths.Count > 0)
                {
                    throw new InvalidOperationException("path lookup: folder expected but got file!");
                }
            }
            else
            {
                lock (folder.SubPath)
                {
                    string currentPath = stringBuilder.ToString();
                    foreach (string filePath in Directory.GetFiles(currentPath))
                    {
                        string fileName = Path.GetFileName(filePath);
                        string fileNameLower = fileName.ToLower();
                        if (folder.SubPath.ContainsKey(fileNameLower))
                        {
                            if (next == fileNameLower)
                            {
                                stringBuilder.Append(folder.SubPath[fileNameLower].CaseSensitiveName);
                                return;
                            }
                            continue;
                        }

                        File file = new File
                        {
                            CaseSensitiveName = fileName
                        };
                        folder.SubPath.Add(fileNameLower, file);
                        if (next == fileNameLower)
                        {
                            stringBuilder.Append(fileName);
                            if (subPaths.Count > 0)
                            {
                                throw new InvalidOperationException("path lookup: folder expected but got file!");
                            }
                            return;
                        }
                    }

                    foreach (string folderPath in Directory.GetDirectories(currentPath))
                    {
                        string folderName = Path.GetFileName(folderPath);
                        string folderNameLower = folderName.ToLower();
                        if (folder.SubPath.ContainsKey(folderNameLower))
                        {
                            if (next == folderNameLower) // FUK threading
                            {
                                Folder subpath = (Folder)folder.SubPath[folderNameLower];
                                stringBuilder.Append(subpath.CaseSensitiveName);
                                if (subPaths.Count > 0)
                                    Lookup(subpath, subPaths, stringBuilder);
                                return;
                            }
                            continue;
                        }

                        Folder subfolder = new Folder
                        {
                            CaseSensitiveName = folderName,
                            SubPath = new Dictionary<string, IPath>()
                        };
                        folder.SubPath.Add(folderNameLower, subfolder);
                        if (next == folderNameLower)
                        {
                            stringBuilder.Append(folderName);
                            if (subPaths.Count > 0)
                                Lookup(subfolder, subPaths, stringBuilder);
                            return;
                        }
                    }
                }
                stringBuilder.Append(next);
                throw new FileNotFoundException($"{stringBuilder} doesn't exist");
            }
        }

#else
        private PathLookup()
        {
        }

        // windows is case insensitive , no bullshit required:
        public string Transform(string path) => path

#endif
    }
}
