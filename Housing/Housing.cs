using System;
using System.IO;
using System.Linq;

using Housing.Config;
using Housing.Listeners;
using Housing.Managers;
using Housing.Types;
using Housing.Components;

using Newtonsoft.Json;

using UnityEngine;

namespace Housing
{
    public class Housing : MonoBehaviour
    {
        public static Housing Instance;
        public static House House;
        
        public static GameObject[] Transfer = new GameObject[2];
        public static AreaData AreaData => Game.Instance.AreaData;

        public static HouseConfig Config;

        public static bool BuildMode;

        // Call this method to initialize a new instance of housing,
        // then attach to game object.
        public static void Load()
        {
            new GameObject().AddComponent<Housing>().name = "Housing";
            CacheManager.CacheHousing();

            Config = ConfigManager.LoadConfig();
            UIFurnitureManager.Load();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (House == null) return;
            if (Config == null) return;

            if (Input.GetKeyDown(KeyCode.F1))
            {
                UIFurnitureManager.Shown = !UIFurnitureManager.Shown;
            }

            if (IsInEntranceMap())
            {
                LoadTransferPad(House.Entrances.Find(e => e.MapID == AreaData.id));
            }
            
            if (!IsInEntranceMap() && !IsInInterior())
            {
                DestroyTransferPads();
            }

            if (IsInInterior())
            {
                LoadInterior(House.Interiors[0]);
                House.Entrances.ForEach(pad => pad.Loaded = false);
            }

            if (!IsInInterior())
            {
                UnloadInterior(House.Interiors[0]);
            }
        }
        
        public static void LoadModels()
        {
            var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            
            foreach (var furniture in House.Furnitures)
            {
                foreach (var obj in furniture.Model.Objects)
                {
                    var depends = obj.Dependency;
                    if (depends.Bundle == "" || depends.Prefab == "") continue;

                    if (!PrefabManager.IsPrefabLoaded(gameObjects, depends.Prefab))
                    {
                        var file = House.Bundles.Find(b => b.Name == depends.Bundle).File;
                        if (file == null) continue;

                        var bundle = BundleManager.GetBundle(file);
                        if (bundle != null)
                        {
                            var prefab = bundle.LoadAssetAsync<GameObject>(depends.Prefab).asset as GameObject;

                            GameObject instance;
                            if (depends.Path == "")
                                instance = Instantiate(prefab);
                            else
                            {
                                instance = Instantiate(prefab.transform.Find(depends.Path).gameObject);
                            }

                            instance.name = obj.Name + " (Prefab)";
                            instance.SetActive(false);
                            instance.transform.position = new Vector3(0f, 0f, 0f);
                            instance.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                            ModelManager.LoadedModels.Add(instance);
                        }
                    }
                }
            }
        }
        
        public static Interior GetInteriorByName(string name)
        {
            return House.Interiors.Find(i => i.Name == name);
        }
        
        public static void LoadInterior(Interior interior)
        {
            if (interior == null) return;
            if (interior.Loaded) return;

            LoadModels();
            LoadSavedFurnitures(GetActiveSavedInterior());
            
            interior.Loaded = true;
            
            LoadTransferPad(interior.TransferPad);
        }
        
        public static void UnloadInterior(Interior interior)
        {
            if (!interior.Loaded) return;

            ModelManager.DeleteModelsWithSuffix("Furniture");

            interior.Loaded = false;
            interior.TransferPad.Loaded = false;

        }

        public static void ChangeInterior(string name)
        {
            var interior = GetSavedInterior(name);
            if (interior == null) return;

            interior.Active = true;
            GetActiveSavedInterior().Active = false;

            ModelManager.DeleteModelsWithSuffix("Furniture");
            
            UnloadInterior(House.Interiors[0]);
            LoadInterior(House.Interiors[0]);
        }
        
        public static bool IsInEntranceMap()
        {
            return House.Entrances.Find(e => e.MapID == AreaData.id) != null;
        }
        
        public static bool IsInInterior()
        {
            var interiors = House.Interiors;
            var match = interiors.Find(i => i.MapID == AreaData.id);
            
            return match != null;
        }

        public static void CreateTransferPad(int mapID, string notify)
        {
            var newTransferPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var newNotifyArea = GameObject.CreatePrimitive(PrimitiveType.Cube);

            DestroyImmediate(newTransferPad.GetComponent<MeshFilter>());
            newTransferPad.GetComponent<BoxCollider>().isTrigger = true;
            newTransferPad.AddComponent<Transfer>().Destination = mapID;
            
            DestroyImmediate(newNotifyArea.GetComponent<MeshFilter>());
            newNotifyArea.GetComponent<BoxCollider>().isTrigger = true;
            newNotifyArea.AddComponent<NotifyArea>().Message = notify;

            Transfer[0] = newTransferPad;
            Transfer[1] = newNotifyArea;
        }

        public static void DestroyTransferPads()
        {
            Destroy(Transfer[0]);
            Destroy(Transfer[1]);
            Transfer = new GameObject[2];
            
            House.Entrances.ForEach(pad => pad.Loaded = false);
            House.Interiors.ForEach(i => i.TransferPad.Loaded = false);
        }
        
        public static void LoadTransferPad(TransferPad transferPad)
        {
            if (transferPad == null) return;
            if (transferPad.Loaded) return;
            
            var notify = $"{Entities.Instance.me.name}'s House";
            if (IsInInterior())
            {
                notify = "Leave House";
            }

            var mapID = transferPad.Destination != 0 ? transferPad.Destination : transferPad.MapID;
            
            if (Transfer[0] == null || Transfer[1] == null)
                CreateTransferPad(mapID, notify);

            Transfer[0].GetComponent<Transfer>().Destination = mapID;
            Transfer[1].GetComponent<NotifyArea>().Message = notify;

            var transform = PrefabManager
                .ParseToTransformFromFArray(transferPad.TPPosition, transferPad.TPRotation, transferPad.TPScale);
            TransformManager.MergeGoTransform(Transfer[0], transform);
            
            transform = PrefabManager
                .ParseToTransformFromFArray(transferPad.NAPosition, transferPad.NARotation, transferPad.NAScale);
            TransformManager.MergeGoTransform(Transfer[1], transform);

            transferPad.Loaded = true;
        }

        public static SavedInterior GetSavedInterior(string name)
        {
            return Config.SavedInteriors.Find(i => i.InteriorName == name);
        }
        
        public static SavedInterior GetActiveSavedInterior()
        {
            return Config.SavedInteriors.Find(i => i.Active);
        }
        
        public static Furniture GetFurnitureByName(string name)
        {
            return House.Furnitures.Find(f => f.Name == name);
        }
        
        public static GameObject LoadFurnitureByName(string name, float[] position = null, float[] rotation = null, float[] scale = null)
        {
            var furniture = House.Furnitures.Find(f => f.Name == name);
            if (furniture == null) return null;

            Transform transform;
            if (position == null && rotation == null && scale == null)
            {
                transform = Entities.Instance.me.wrapper.transform;
            }
            else
            {
                transform = PrefabManager.ParseToTransformFromFArray(position, rotation, scale);
            }
            
            var m = ModelManager.SpawnModel(furniture.Model, transform.position, transform.eulerAngles, "Furniture");
            return m;
        }

        public static void LoadSavedFurnitures(SavedInterior interior)
        {
            if (!interior.Furnitures.Any()) return;

            foreach (var furniture in interior.Furnitures)
            {
                var transform = PrefabManager.ParseToTransformFromFArray(
                    furniture.Position,
                    furniture.Rotation,
                    furniture.Scale);
                
                var model = ModelManager.SpawnModel(
                    GetFurnitureByName(furniture.Name).Model,
                    transform.position,
                    transform.eulerAngles,
                    "Furniture");

                var com = model.AddComponent<ComFurniture>();
                com.ID = furniture.ID;
                com.Name = furniture.Name;
            }
        }
        
        public static void PlaceFurniture(Furniture furniture, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (furniture == null) return;
            if (Config == null) return;

            var id = 0;
            for (var i = 1; id == 0; i++)
            {
                if (GetActiveSavedInterior().Furnitures.Find(f => f.ID == i) == null)
                {
                    id = i;
                }
            }
            
            var savedFurniture = new SavedFurniture()
            {
                ID = id,
                Name = furniture.Name,
                Position = new [] { position.x, position.y, position.z },
                Rotation = new [] { rotation.x, rotation.y, rotation.z },
                Scale = new [] { scale.x, scale.y, scale.z },
            };
            
            GetActiveSavedInterior().Furnitures.Add(savedFurniture);
            SaveConfig();
        }

        public static void SaveConfig()
        {
            var config = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(@"S:\AQ3D\configs\housing.json", config);
        }
    }
}