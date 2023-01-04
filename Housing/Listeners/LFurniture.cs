using System;
using Assets.Scripts.Game;
using UnityEngine;

namespace Housing.Listeners
{
    public class LFurniture : MonoBehaviour
    {
        private void OnMouseHover()
        {
            if (Housing.BuildMode)
            {
                CursorManager.Instance.SetCursor(CursorManager.Icon.Interact);
            }
        }
    }
}