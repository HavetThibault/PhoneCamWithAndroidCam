using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidCamClient.JpegStream
{
    public class JpegDecodingException : Exception
    {
        public JpegDecodingException(string message) : base(message) { }
    }
}
