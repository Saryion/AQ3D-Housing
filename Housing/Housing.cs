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
        public static GameObject NewFurniture;
        public static GameObject SelectedFurniture;

        public static float SelectedFurnitureDistance = 1;
        public static EditingMode EditingMode = EditingMode.NONE;

        public static Camera Camera => Camera.main;
        public static Transform CameraTransform;
        
        private class FreeCamSettings
        {
            public float MainSpeed;
            public float ShiftAdd;
            public float MaxShift;
        }

        private static readonly FreeCamSettings[] freeCamSettings =
        {
            new FreeCamSettings
            {
                MainSpeed = 4f,
                ShiftAdd = 8f,
                MaxShift = 8f
            }
        };

        // Call this method to initialize a new instance of housing,
        // then attach to game object.
        public static void Load()
        { 
            new GameObject().AddComponent<Housing>().name = "Housing";
            CacheManager.CacheHousing();

            Config = ConfigManager.LoadConfig();
            UIFurnitureManager.Load();

            CameraTransform = Camera.transform.parent;
            Camera.GetComponent<FlyCam>().mainSpeed = freeCamSettings[0].MainSpeed;
            Camera.GetComponent<FlyCam>().shiftAdd = freeCamSettings[0].ShiftAdd;
            Camera.GetComponent<FlyCam>().maxShift = freeCamSettings[0].MaxShift;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (House == null) return;
            if (Config == null) return;

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
                ToggleBuildMode(false);
                UnloadInterior(House.Interiors[0]);
            }
            
            if (Input.GetKeyDown(KeyCode.F1) && IsInInterior())
            {
                ToggleBuildMode(BuildMode = !BuildMode);
            }

            if (BuildMode)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    UIFurnitureManager.Shown = !UIFurnitureManager.Shown;
                }
                
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    try
                    {
                        var ray = Camera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out var obj, 100))
                        {
                            var com = obj.transform.parent.GetComponent<ComFurniture>();
                            if (com != null)
                            {
                                Chat.Notify(obj.transform.parent.name + $" ({com.ID})");
                                DeleteFurniture(com.ID);
                            }
                        }
                    
                    } catch (NullReferenceException) { }
                }
            }
            
            if (NewFurniture != null)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    EditingMode = EditingMode.ROTATING;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Destroy(NewFurniture);
                    NewFurniture = null;
                    
                    UIMainMenu.ClearWindows();
                }
                
                var cameraTransform = Camera.transform;
                NewFurniture.transform.position = cameraTransform.position + cameraTransform.forward * 5;
            }

            if (NewFurniture != null && EditingMode == EditingMode.ROTATING)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0.0)
                {
                    NewFurniture.transform.Rotate(Vector3.up * 3f, Space.Self);
                }
            
                if (Input.GetAxis("Mouse ScrollWheel") < 0.0)
                {
                    NewFurniture.transform.Rotate(Vector3.down * 3f, Space.Self);
                }
            }

            if (Input.GetKeyDown(KeyCode.E) && NewFurniture != null)
            {
                var id = NewFurniture.GetComponent<ComFurniture>().ID;
                PlaceFurniture(House.Furnitures.Find(f => f.Model.Name + " (Furniture)" == NewFurniture.name),
                    id, NewFurniture.transform.position, NewFurniture.transform.eulerAngles, NewFurniture.transform.localScale);
                NewFurniture = null;
            }
        }

        public static void ToggleBuildMode(bool state)
        {
            BuildMode = state;
            
            if (Camera == null) return;

            if (!BuildMode)
            {
                Camera.transform.parent = CameraTransform;
                Camera.GetComponentInParent<CameraController>().enabled = true;
                Camera.GetComponent<FlyCam>().enabled = false;
                Camera.transform.localPosition = Vector3.zero;
                Camera.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                
                Entities.Instance.me.IsMovementDisabled = false;
            }
            else
            {
                Camera.GetComponentInParent<CameraController>().enabled = false;
                Camera.GetComponent<FlyCam>().enabled = true;
                Camera.transform.parent = null;
                
                Entities.Instance.me.IsMovementDisabled = true;
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
                            if (prefab == null) continue;
                            
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

        public static void LoadNewFurniture(string name)
        {
            if (!BuildMode) return;

            var model = ModelManager.SpawnModel(GetFurnitureByName(name).Model, Vector3.zero, Vector3.zero, "Furniture");
            
            var id = 0;
            for (var i = 1; id == 0; i++)
            {
                if (GetActiveSavedInterior().Furnitures.Find(f => f.ID == i) == null)
                {
                    id = i;
                }
            }
            
            var com = model.AddComponent<ComFurniture>();
            com.ID = id;
            com.Name = name;
            
            NewFurniture = model;
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

                model.AddComponent<MeshCollider>();
            }
        }
        
        public static void PlaceFurniture(Furniture furniture, int id, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (furniture == null) return;
            if (Config == null) return;

            var savedFurniture = new SavedFurniture()
            {
                ID = id,
                Name = furniture.Name,
                Position = new [] { position.x, position.y, position.z },
                Rotation = new [] { rotation.x, rotation.y, rotation.z },
                Scale = new [] { scale.x, scale.y, scale.z },
            };
            
            if (GetActiveSavedInterior().Furnitures.Find(f => f.ID == id) == null)
            {
                GetActiveSavedInterior().Furnitures.Add(savedFurniture);
            }
            else
            {
                var furnitures = GetActiveSavedInterior().Furnitures;
                furnitures[furnitures.FindIndex(f => f.ID == id)] = savedFurniture;
            }
            
            SaveConfig();
        }

        public static void DeleteFurniture(int id)
        {
            var model = GetFurnitureModelByID(id);
            if (model == null) return;
            
            DestroyImmediate(model);
            ModelManager.LoadedModels.Remove(model);

            var savedFurnitures = GetActiveSavedInterior().Furnitures;
            savedFurnitures.Remove(savedFurnitures.Find(f => f.ID == id));
            SaveConfig();
        }

        public static GameObject GetFurnitureModelByID(int id)
        {
            var models = ModelManager.LoadedModels;
            foreach (var model in models)
            {
                var com = model.GetComponent<ComFurniture>();
                if (com == null) continue;

                if (com.ID == id) return model;
            }

            return null;
        }
        
        public static void SaveConfig()
        {
            var config = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(@"S:\AQ3D\configs\housing.json", config);
        }
    }
}