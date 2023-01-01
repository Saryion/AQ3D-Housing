using Housing.API;
using Housing.Managers;
using Housing.Types;

using Newtonsoft.Json;

namespace Housing.Managers
{
    public class CacheManager
    {
        public static async void CacheHousing()
        {
            var data = await APIManager.SendReq(APITypes.HOUSING);
            if (data == null) return;
            
            var housing = JsonConvert.DeserializeObject<House>(data);
            Housing.House = housing;
            
            foreach (var bundle in housing.Bundles)
            {
                if (bundle.File == "") continue;

                BundleManager.LoadBundle(bundle.File);
            }
        }
    }
}