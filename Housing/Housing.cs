using System;

using Housing.Listeners;
using Housing.Managers;
using Housing.Types;

using UnityEngine;

namespace Housing
{
    public class Housing : MonoBehaviour
    {
        public static Housing Instance;
        public static House House;
        
        public static GameObject[] Transfer = new GameObject[2];
        
        public static AreaData AreaData => Game.Instance.AreaData;

        // Call this method to initialize a new instance of housing,
        // then attach to game object.
        public static void Load()
        {
            new GameObject().AddComponent<Housing>().name = "Housing";
            CacheManager.CacheHousing();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (House == null) return;

            if (Input.GetKeyDown(KeyCode.F1))
            {
                LoadFurnitureByName("Wooden Strongbox");
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
                            var instance = Instantiate(prefab);
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
        
        public static void LoadFurnitureByName(string name)
        {
            var furniture = House.Furnitures.Find(f => f.Name == name);
            if (furniture == null) return;

            var transform = Entities.Instance.me.wrapper.transform;
            var model = ModelManager.SpawnModel(furniture.Model, transform.position, transform.eulerAngles, "Furniture");
        }
    }
}