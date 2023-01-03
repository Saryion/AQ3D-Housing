using System.Collections.Generic;

namespace Housing.Config
{
    public class HouseConfig
    {
        public List<SavedInterior> SavedInteriors;
        
        public static HouseConfig Config = new HouseConfig
        {
            SavedInteriors = new List<SavedInterior>
            {
                new SavedInterior
                {
                    InteriorName = "Basic Interior",
                    Furnitures = new List<SavedFurniture>(),
                    Active = true
                }
            }
        };
    }
}