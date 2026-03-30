using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SimpleFolderIcon.Editor
{
    internal class IconDictionaryCreator : AssetPostprocessor
    {
        // Carpeta relativa dentro de Assets
        private const string AssetsPath = "Icons";

        internal static Dictionary<string, Texture> IconDictionary;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!ContainsIconAsset(importedAssets) &&
                !ContainsIconAsset(deletedAssets) &&
                !ContainsIconAsset(movedAssets) &&
                !ContainsIconAsset(movedFromAssetPaths))
            {
                return;
            }

            BuildDictionary();
        }

        private static bool ContainsIconAsset(string[] assets)
        {
            foreach (string str in assets)
            {
                var dir = Path.GetDirectoryName(str);
                if (dir == null) continue;

                if (ReplaceSeparatorChar(dir).Contains("Assets/" + AssetsPath))
                {
                    return true;
                }
            }
            return false;
        }

        private static string ReplaceSeparatorChar(string path)
        {
            return path.Replace("\\", "/");
        }

        internal static void BuildDictionary()
        {
            var dictionary = new Dictionary<string, Texture>();

            // Ruta REAL del sistema (E:\...\Assets\Icons)
            var fullPath = Path.Combine(Application.dataPath, AssetsPath);
            var dir = new DirectoryInfo(fullPath);

            if (!dir.Exists)
            {
                Debug.LogError(" No existe la carpeta: " + fullPath);
                IconDictionary = dictionary;
                return;
            }

            // Cargar PNG
            FileInfo[] pngFiles = dir.GetFiles("*.png");
            foreach (FileInfo f in pngFiles)
            {
                string assetPath = $"Assets/{AssetsPath}/{f.Name}";
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

                if (texture != null)
                {
                    dictionary[Path.GetFileNameWithoutExtension(f.Name)] = texture;
                }
            }

            // Cargar ScriptableObjects
            FileInfo[] soFiles = dir.GetFiles("*.asset");
            foreach (FileInfo f in soFiles)
            {
                string assetPath = $"Assets/{AssetsPath}/{f.Name}";
                var folderIconSO = AssetDatabase.LoadAssetAtPath<FolderIconSO>(assetPath);

                if (folderIconSO != null && folderIconSO.icon != null)
                {
                    foreach (string folderName in folderIconSO.folderNames)
                    {
                        if (!string.IsNullOrEmpty(folderName))
                        {
                            dictionary[folderName] = folderIconSO.icon;
                        }
                    }
                }
            }

            IconDictionary = dictionary;

            Debug.Log(" IconDictionary cargado con " + dictionary.Count + " íconos.");
        }
    }
}