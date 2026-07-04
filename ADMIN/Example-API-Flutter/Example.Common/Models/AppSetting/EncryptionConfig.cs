using System;
using System.Collections.Generic;
using System.Text;

namespace Example.Common.Models.AppSetting
{
    public class EncryptionConfig
    {
        public string AesKey { get; set; }
        public static string AesIV { get; set; }
        public string RSAPublicKey { get; set; }
        public string RSAPrivateKey { get; set; }
    }
}
