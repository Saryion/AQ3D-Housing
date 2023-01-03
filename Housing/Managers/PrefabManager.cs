using System.Collections.Generic;
using System.Linq;

using Housing.Types;

using UnityEngine;

namespace Housing.Managers
{
    public class PrefabManager : MonoBehaviour
    {
        public static GameObject LoadPrefabFromBundle(string fileName, string  prefabName, string path)
        {
            var loadedBundle = BundleManager.GetBundle(fileName);
            if (loadedBundle == null) return null;
            
            var prefab = loadedBundle.LoadAssetAsync<GameObject>(prefabName).asset as GameObject;
            var instance = Instantiate(prefab);

            return instance;
            
        }
        
        public static bool IsPrefabLoaded(IEnumerable<GameObject> objects, string name)
        {
            return objects.FirstOrDefault(o => name == o.name) ?? false;
        }

        public static Vector3 ParseFArrayToVector3(float[] array)
        {
            return new Vector3(array[0], array[1], array[2]);
        }

        public static Transform ParseToTransform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Transform transform = new GameObject().transform;
            transform.position = position;
            transform.eulerAngles = rotation;
            transform.localScale = scale;

            return transform;
        }
        
        public static Transform ParseToTransformFromFArray(float[] position, float[] rotation, float[] scale)
        {
            Transform transform = new GameObject().transform;
            transform.position = ParseFArrayToVector3(position);
            transform.eulerAngles = ParseFArrayToVector3(rotation);
            transform.localScale = ParseFArrayToVector3(scale);

            return transform;
        }
        
        public static float[] MergeVectorFArray(float[] array1, float[] array2)
        {
            var array3 = new [] { array1[0] + array2[0], array1[1] + array2[1], array1[2] + array2[2] };
            return array3;
        }
    }
}