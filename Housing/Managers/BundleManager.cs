using System.Collections.Generic;

using UnityEngine;

namespace Housing.Managers
{
    public class BundleManager
    {
        public static List<string> CachedBundles = new List<string>();
        
        public static AssetBundle GetBundle(string path)
        {
            return AssetBundleManager.GetBundle(Main.APPLICATION_PATH + "/gamefiles/build80/pc/" + path);
        }

        public static void LoadBundle(string file)
        {
            if (CachedBundles.Contains(file)) return;
            if (file != "" && GetBundle(file) == null)
            {
                AssetBundleManager.LoadAssetBundle(new BundleInfo
                    (file, 0, 0L, 0L, 0L));
                
                CachedBundles.Add(file);
            }
        }
    }
}