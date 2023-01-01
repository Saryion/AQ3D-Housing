using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Housing.API;

namespace Housing.Managers
{
    public class APIManager
    {
        public static readonly string URL = "https://api.saryion.com/aq3d/housing/";

        public static async Task<string> SendReq(APITypes type)
        {
            var url = $"{URL}{type.ToString().ToLower()}.json";

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            var res = (HttpWebResponse)await Task.Factory
                .FromAsync(req.BeginGetResponse, req.EndGetResponse, null);

            try
            {
                var stream = res.GetResponseStream();

                try
                {
                    var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
                catch
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}