using System.IO;
using UnityEditor;
using UnityEngine;

namespace lilAvatarUtils.Utils
{
    internal class PackageJsonReader
    {
        private const string GUID_PACKAGE_JSON = "f5675109c3092d74a86101426568fc4e";
        internal static string GetVersion()
        {
            string path = AssetDatabase.GUIDToAssetPath(GUID_PACKAGE_JSON);
            if(string.IsNullOrEmpty(path)) return "";
            return JsonUtility.FromJson<PackageInfos>(File.ReadAllText(path)).version;
           
        }

        private class PackageInfos
        {
            public string version = "";
        }
    }
}
