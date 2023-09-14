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
        public string FilePath { get; set; }

        public DisplayStreamViewModelInfo() { }

        public DisplayStreamViewModelInfo(string filePath, string phoneIp) 
        {
            FilePath = filePath;
            PhoneIp = phoneIp;
        }

        public void Serialize()
        {
            XmlSerializerHelper.Serialize(FilePath, this);
        }
    }
}
