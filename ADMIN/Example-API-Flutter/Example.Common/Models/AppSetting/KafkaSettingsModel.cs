using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Models.AppSetting
{
    public class KafkaSettingsModel
    {
        public string Bootstrapservers { get; set; }
        public string SaslUsername { get; set; }
        public string SaslPassword { get; set; }
        public int SecurityProtocol { get; set; }
        public int SaslMechanism { get; set; }
        public string SslCa { get; set; }
    }
}
