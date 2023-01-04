using Housing.Components;
using UnityEngine;

namespace Housing.Managers
{
    public class UIBuildMode: MonoBehaviour
    {
        private float _menuPositionX = 100f;
        private float _menuPositionY = 100f;
        private float _menuSizeX = 280f;
        private float _menuSizeY = 150f;

        private bool BuildMode => Housing.BuildMode;

        private void LoadUI()
        {
            var positionSize = new Rect(_menuPositionX, _menuPositionY, _menuSizeX, _menuSizeY);
            GUI.Box(positionSize, "");

            var style = new GUIStyle
            {
                fontSize = 20
            };
            style.normal.textColor = Color.white;

            positionSize.x += 75f;
            positionSize.y += -25f;
            GUI.Label(positionSize, "BUILD MODE", style);

            
            style.fontSize = 16;
            style.alignment = TextAnchor.UpperCenter;

            positionSize.x += -75f;
            positionSize.y += 33f;
            if (Housing.HoveringFurniture != null && Housing.NewFurniture == null)
            {
                var com = Housing.HoveringFurniture.GetComponent<ComFurniture>();
                
                GUI.Label(positionSize, $"{com.Name} ({com.ID})", style);
            }

            if (Housing.NewFurniture != null)
            {
                var com = Housing.NewFurniture.GetComponent<ComFurniture>();
                
                GUI.Label(positionSize, $"Editing: {com.Name} ({com.ID})", style);
            }
            
            style.fontSize = 19;

            positionSize.x += -35f;
            positionSize.y += 45f;
            GUI.Label(positionSize, "1        Furniture Menu", style);
        }

        private void OnGUI()
        {
            if (BuildMode)
            {
                LoadUI();
            }
        }
    }
}