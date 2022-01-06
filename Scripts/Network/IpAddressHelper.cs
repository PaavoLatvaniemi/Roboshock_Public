using System.IO;
using System.Net;

namespace Assets.Scripts.Network
{
    static class IpAddressHelper
    {
        public static string GetPublicIPV4Address()
        {
            string address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader streamR = new StreamReader(response.GetResponseStream()))
                {
                    address = streamR.ReadToEnd();
                }
            }
            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }
    }
}
