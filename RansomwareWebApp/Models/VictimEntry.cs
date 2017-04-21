using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace RansomwareWebApp.Models
{
    public class VictimEntry
    {
        private bool isActive = true;
        private DateTime _timestamp;
        private RSACryptoServiceProvider _cryptodata;
        public VictimEntry(RSACryptoServiceProvider rsa)
        {
            _cryptodata = rsa;
            _timestamp = DateTime.Now;
        }

        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }

            set
            {
                _timestamp = value;
            }
        }

        public RSACryptoServiceProvider RSA
        {
            get
            {
                return _cryptodata;
            }

            set
            {
                _cryptodata = value;
            }
        }
    }
}