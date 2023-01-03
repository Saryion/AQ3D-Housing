using System;
using System.IO;
using System.Text;

using Housing.Config;

using Newtonsoft.Json;

namespace Housing.Managers
{
    public class ConfigManager
    {
        public static string PATH = @"S:\AQ3D\configs\housing.json";
        
        public static HouseConfig LoadConfig()
        {
            if (!File.Exists(PATH))
            {
                return CreateConfig("housing", HouseConfig.Config);
            }
            
            var file = File.ReadAllText(PATH, Encoding.UTF8);

            try
            {
                var config = JsonConvert.DeserializeObject<HouseConfig>(file);

                return config;
            }
            catch
            {
                return null;
            }
        }

        public static HouseConfig CreateConfig(string file, HouseConfig config)
        {
            try
            {
                using (var sw = File.CreateText(PATH))    
                {    
                    sw.Write(JsonConvert.SerializeObject(config, Formatting.Indented));
                }
                
                return config;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}