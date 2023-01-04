using System.Collections.Generic;
using System.Linq;
using Housing.Types;
using UnityEngine;

namespace Housing.Managers
{
    public class ModelManager : MonoBehaviour
    {
        public static List<GameObject> LoadedModels = new List<GameObject>();
        
        public static GameObject SpawnModel(Model model, Vector3 position, Vector3 rotation, Vector3 scale, string suffix)
        {
            var modelGo = new GameObject(model.Name + $" ({suffix})");
            var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

            foreach (var obj in model.Objects)
            {
                var match = gameObjects.FirstOrDefault(go => go.name == obj.Name);
                if (match == null) continue;

                var instance = Instantiate(match, modelGo.transform);
                instance.SetActive(true);
                instance.name = obj.Name + $" ({suffix})";

                if (instance.GetComponent<BoxCollider>() == null)
                {
                    instance.AddComponent<BoxCollider>();
                }
                
                instance.transform.position = PrefabManager.ParseFArrayToVector3(obj.Position);
                instance.transform.eulerAngles = PrefabManager.ParseFArrayToVector3(obj.Rotation);
                instance.transform.localScale = PrefabManager.ParseFArrayToVector3(obj.Scale);
            }
            
            if (modelGo.transform.childCount <= 0) return null;
            
            modelGo.transform.position = position;
            modelGo.transform.eulerAngles = rotation;
            modelGo.transform.localScale = scale;
            
            LoadedModels.Add(modelGo);
            return modelGo;
        }
        
        public static void DeleteModelsWithSuffix(string suffix)
        {
            var destroyed = 0;
            foreach (var model in LoadedModels)
            {
                if (model == null) continue;
                if (model.name.Contains($"({suffix})"))
                {
                    DestroyImmediate(model);
                    LoadedModels.Remove(model);
                    ++destroyed;
                }
            }
        }
    }
}