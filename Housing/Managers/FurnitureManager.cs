using System;

using Housing.Components;
using Housing.Types;
using UnityEngine;

namespace Housing.Managers
{
    public class FurnitureManager : MonoBehaviour
    {
        private bool BuildMode => Housing.BuildMode;

        private GameObject NewFurniture => Housing.NewFurniture;
        private GameObject HoveringFurniture => Housing.HoveringFurniture;

        private EditingMode EditingMode => Housing.EditingMode;
        
        private void Update()
        {
            if (BuildMode)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    UIFurnitureManager.Shown = !UIFurnitureManager.Shown;
                }
                
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    var hover = Housing.GetFurnitureByHover();
                    if (hover != null)
                    {
                        var com = hover.GetComponent<ComFurniture>();
                        if (com == null) return;
                        
                        Housing.DeleteFurniture(com.ID);
                    }
                }

                if (NewFurniture != null)
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        Housing.EditingMode = EditingMode.ROTATING;
                    }

                    if (EditingMode == EditingMode.ROTATING)
                    {
                        if (Input.GetAxis("Mouse ScrollWheel") > 0.0)
                        {
                            Housing.NewFurniture.transform.Rotate(Vector3.up * 3f, Space.Self);
                        }
            
                        if (Input.GetAxis("Mouse ScrollWheel") < 0.0)
                        {
                            Housing.NewFurniture.transform.Rotate(Vector3.down * 3f, Space.Self);
                        }
                    }
                
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        Housing.EditingMode = EditingMode.SCALING;
                        UIMenuWindow.ClearWindows();
                    }

                    if (EditingMode == EditingMode.SCALING)
                    {
                        if (Input.GetAxis("Mouse ScrollWheel") > 0.0)
                        {
                            if (Housing.FurnitureScale <= 2)
                            {
                                Housing.FurnitureScale += 0.1f;
                                Housing.NewFurniture.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
                            }
                        }
            
                        if (Input.GetAxis("Mouse ScrollWheel") < 0.0)
                        {
                            if (Housing.FurnitureScale >= 0.5)
                            {
                                Housing.FurnitureScale -= 0.1f;
                                Housing.NewFurniture.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                            }
                        }
                    }
                    
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        Destroy(NewFurniture);
                        Housing.NewFurniture = null;
                        Housing.FurnitureScale = 1f;
                    
                        UIMainMenu.ClearWindows();
                    }
                    
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        var id = NewFurniture.GetComponent<ComFurniture>().ID;
                        Housing.PlaceFurniture(Housing.House.Furnitures.Find(f => f.Model.Name + " (Furniture)" == NewFurniture.name),
                            id, NewFurniture.transform.position, NewFurniture.transform.eulerAngles, NewFurniture.transform.localScale);
                
                        Housing.NewFurniture = null;
                        Housing.FurnitureScale = 1f;
                    }
                
                    var cameraTransform = Housing.Camera.transform;
                    NewFurniture.transform.position = cameraTransform.position + cameraTransform.forward * 5;
                }

                if (NewFurniture == null)
                {
                    var hover = Housing.GetFurnitureByHover();
                    if (hover == null) Housing.HoveringFurniture = null;

                    Housing.HoveringFurniture = hover;
                }
            }
        }
    }
}