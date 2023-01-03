using System.Linq;
using UnityEngine;

namespace Housing.Managers
{
    public class UIFurnitureManager : MonoBehaviour
    {
        public UIFurnitureManager Instance;

        public static bool Shown;
        
        public static void Load()
        {
            new GameObject().AddComponent<UIFurnitureManager>().name = "UIFurniture";
        }
        
        private void Awake()
        {
            Instance = this;
        }
        
        public static void ShowAvailableFurnitures()
        {
            if (!Housing.House.Furnitures.Any()) return;
            
            var boxX = 1500f;
            var boxY = 120f;
            var boxW = 400f;
            var boxH = 800f;
            var boxPadding = 30f;
            var buttonSpacing = 20f;
            var buttonLength = boxW - (boxPadding * 2);
            var buttonSize = 60f;
            
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
            };

            style.normal.textColor = Color.white;
            
            var positionSize = new Rect(boxX, boxY, boxW, boxH);
            GUI.Box(positionSize, "Available Furniture", style);

            var i = 1;
            foreach (var furniture in Housing.House.Furnitures)
            {
                style = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 20,
                };
                
                if (i > 9) continue;

                var padding = i == 1 ? boxPadding : 0f;
                
                positionSize = new Rect(boxX + boxPadding, boxY + (boxPadding + buttonSpacing * 2f) * i, buttonLength, buttonSize);
                if (GUI.Button(positionSize, furniture.Name, style))
                {
                    var transform = Entities.Instance.me.wrapper.transform;
                    Housing.LoadFurnitureByName(furniture.Name);
                    Housing.PlaceFurniture(furniture, transform.localPosition, transform.localEulerAngles, transform.localScale);
                }
                ++i;
            }
            
        }
        
        private void OnGUI()
        {
            if (Shown)
            {
                ShowAvailableFurnitures();
            }
        }
    }
}