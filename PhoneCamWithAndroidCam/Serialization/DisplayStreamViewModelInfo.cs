using Helper.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.Serialization
{
    public class DisplayStreamViewModelInfo
    {
        public static DisplayStreamViewModelInfo? TryLoad(string filePath)
        {
            return XmlSerializerHelper.Deserialize<DisplayStreamViewModelInfo>(filePath);
        }

        public string PhoneIp { get; set; }

        public DisplayStreamViewModelInfo() { }

        public DisplayStreamViewModelInfo(string phoneIp) 
        {
            PhoneIp = phoneIp;
        }

        public void Serialize(string filePath)
        {
            XmlSerializerHelper.Serialize(filePath, this);
        }
    }
}
