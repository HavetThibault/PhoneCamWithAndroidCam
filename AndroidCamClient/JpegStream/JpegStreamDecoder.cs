using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ImageProcessingUtils;

namespace AndroidCamClient.JpegStream
{
    public class JpegStreamDecoder : IDisposable
    {
        public static readonly string HEADER_SEPARATOR = "\r\n";
        public static readonly string HEADER_KEYVALUE_SEPARATOR = ": ";

        private HttpClient _httpClient;

        public JpegStreamDecoder()
        {
            _httpClient = new();
        }

        public void Dispose() => _httpClient.Dispose();

        public Task<Stream> InitMJpegStream(string uri)
        {
            return _httpClient.GetStreamAsync(uri);
        }

        public static JpegFrame ReadOneFrame(Stream mjpegStream)
        {
            Dictionary<string, string> headers = ConvertBytesHeaders(GetBytesHeaders(mjpegStream, out int offset));
            if (!headers.TryGetValue("Content-Length", out string strContentLength) || strContentLength == null || strContentLength.Length == 0)
                throw new JpegDecodingException("The headers of the response don't contain the content length.");

            int contentLength = int.Parse(strContentLength);
            if (contentLength == 0)
                throw new JpegDecodingException("Jpeg headers are empty.");

            return GetJpegFrame(mjpegStream, contentLength);
        }

        internal static byte[] GetBytesHeaders(Stream stream, out int offset)
        {
            bool firstCharCr = false;
            bool foundCr = false;
            bool foundJpegBeginning1 = false;
            bool foundJpegBeginning = false;
            byte b;
            MemoryStream mainMemoryStream = new();
            MemoryStream subMemoryStream = new();
            offset = 0;
            while (!foundJpegBeginning)
            {
                b = (byte)stream.ReadByte();
                subMemoryStream.WriteByte(b);
                if (foundCr)
                {
                    if (b == JpegMarkers.JPEG_START_OF_IMAGE[0] && !foundJpegBeginning1)
                        foundJpegBeginning1 = true;
                    else if (b == JpegMarkers.JPEG_START_OF_IMAGE[1] && foundJpegBeginning1)
                        foundJpegBeginning = true;
                    else
                    {
                        foundJpegBeginning1 = false;
                    }
                }

                if (b == 13)
                    firstCharCr = true;
                else if (b == 10 && firstCharCr)
                {
                    foundCr = true;
                    mainMemoryStream.Write(subMemoryStream.ToArray());
                    subMemoryStream.Close();
                    subMemoryStream = new();
                }
                else
                    firstCharCr = false;

                offset++;
            }
            offset -= 2; // Less the 2 magic characters that begin a jpeg
            byte[] byteHeaders = mainMemoryStream.ToArray();
            mainMemoryStream.Close();
            subMemoryStream.Close();
            return byteHeaders;
        }

        internal static Dictionary<string, string> ConvertBytesHeaders(byte[] bytesHeaders)
        {
            MemoryStream mainMemoryStream;
            var headers = new Dictionary<string, string>();

            bool firstCharCr, foundCr;
            int offset = 0;
            while (offset < bytesHeaders.Length)
            {
                mainMemoryStream = new();
                foundCr = false;
                firstCharCr = false;
                while (!foundCr && offset < bytesHeaders.Length)
                {
                    if (bytesHeaders[offset] == 13)
                        firstCharCr = true;
                    else if (bytesHeaders[offset] == 10 && firstCharCr)
                        foundCr = true;
                    else
                        firstCharCr = false;

                    mainMemoryStream.WriteByte(bytesHeaders[offset]);
                    offset++;
                }
                byte[] bytesOneHeaderKeyValue = mainMemoryStream.ToArray();
                if (bytesOneHeaderKeyValue.Length == 0)
                    continue;

                string headerKeyValue = Encoding.UTF8.GetString(bytesOneHeaderKeyValue, 0, bytesOneHeaderKeyValue.Length);

                if (headerKeyValue[0] == '-' && headerKeyValue[1] == '-')
                    continue;

                string[] headerKeyValueArray = headerKeyValue.Split(new string[2] { HEADER_KEYVALUE_SEPARATOR, HEADER_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                if (headerKeyValueArray.Length != 2)
                    continue;

                string headerKey = headerKeyValueArray[0];
                string headerValue = headerKeyValueArray[1];

                headers.Add(headerKey, headerValue);
                mainMemoryStream.Close();
            }

            return headers;
        }

        /// <summary>
        /// Stream must be set after the start of image marker
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        internal static JpegFrame GetJpegFrame(Stream stream, int length)
        {
            bool foundJpegFirstTag = false, foundJpegScanTag = false;
            MemoryStream memoryStream = new ();
            while(!foundJpegScanTag)
            {
                byte b = (byte)stream.ReadByte();
                memoryStream.WriteByte(b);
                if (b == JpegMarkers.JPEG_START_OF_SCAN[0])
                    foundJpegFirstTag = true;
                else if (b == JpegMarkers.JPEG_START_OF_SCAN[1] && foundJpegFirstTag)
                    foundJpegScanTag = true;
            }
            byte[] bytesHeader = memoryStream.ToArray();
            byte[] fullBytesHeader = new byte[bytesHeader.Length + 2];
            fullBytesHeader[0] = JpegMarkers.JPEG_START_OF_IMAGE[0];
            fullBytesHeader[1] = JpegMarkers.JPEG_START_OF_IMAGE[1];
            SIMDHelper.Copy(bytesHeader, fullBytesHeader, 2);

            int scanLength = length - fullBytesHeader.Length - 2;
            byte[] bytesScan = new byte[scanLength];
            int readBytesNbr;
            for (int i = 0; i < scanLength; i += readBytesNbr) // Do not read the 2 last jpeg marker
            {
                readBytesNbr = stream.Read(bytesScan, i, scanLength - i);
                if (readBytesNbr == 0)
                    throw new JpegDecodingException("Unable to read byte from mjpeg stream.");
            }

            return new JpegFrame(fullBytesHeader, bytesScan);
        }

        internal static byte[] GetContentWithoutJpegMarker(Stream stream, int contentLength)
        {
            byte[] bytesContent = new byte[contentLength];

            if (stream.ReadByte() != JpegMarkers.JPEG_START_OF_IMAGE[0] || stream.ReadByte() != JpegMarkers.JPEG_START_OF_IMAGE[1])
                throw new JpegDecodingException("The jpeg begenning of image marker doesn't exist.");

            int readBytesNbr;
            for (int i = 0; i < contentLength - 2; i += readBytesNbr) // Do not read the 2 last jpeg marker
            {
                readBytesNbr = stream.Read(bytesContent, i, contentLength - i);
                if (readBytesNbr == 0)
                    throw new JpegDecodingException("Unable to read byte from mjpeg stream.");
            }

            if (stream.ReadByte() != JpegMarkers.JPEG_END_OF_IMAGE[0] || stream.ReadByte() != JpegMarkers.JPEG_END_OF_IMAGE[1])
                throw new JpegDecodingException("The jpeg end of image marker doesn't exist.");

            return bytesContent;
        }
    }
}
