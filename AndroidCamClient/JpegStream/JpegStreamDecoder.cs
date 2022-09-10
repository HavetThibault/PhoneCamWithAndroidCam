using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AndroidCamClient.JpegStream
{
    public class JpegStreamDecoder : IDisposable
    {
        public static readonly string HEADER_SEPARATOR = "\r\n";
        public static readonly string HEADER_KEYVALUE_SEPARATOR = ": ";
        public static readonly byte[] JPEG_START_OF_IMAGE = new byte[2] { 255, 216 };
        public static readonly byte[] JPEG_END_OF_IMAGE = new byte[2] { 255, 217 };

        private HttpClient _httpClient;

        public JpegStreamDecoder()
        {
            _httpClient = new();
        }

        public void Dispose() => _httpClient.Dispose();

        public async Task<byte[]> GetFrameAsync(string uri)
        {
            while (true)
            {
                using Stream stream = await _httpClient.GetStreamAsync(uri).ConfigureAwait(continueOnCapturedContext: false);
                while (true)
                {
                    Dictionary<string, string> headers = ConvertBytesHeaders(GetBytesHeaders(stream, out int contentOffset));
                    if (!headers.TryGetValue("Content-Length", out string strContentLength))
                        throw new IOException("The headers of the response don't contain the content length.");

                    if (strContentLength == null || strContentLength.Length == 0)
                        throw new IOException("The headers of the response don't contain the content length.");

                    int contentLength = int.Parse(strContentLength);
                    if (contentLength == 0)
                    {
                        break;
                    }

                    byte[] content = GetContent(stream, 320 * 240 * 4 + 5, contentLength);
                    if (content == null)
                    {
                        break;
                    }

                    return content;
                }
            }
        }

        public Task<Stream> InitMJpegStream(string uri)
        {
            return _httpClient.GetStreamAsync(uri);
        }

        public static JpegFrame GetJpegFrame(Stream mjpegStream)
        {
            Dictionary<string, string> headers = ConvertBytesHeaders(GetBytesHeaders(mjpegStream, out _));
            if (!headers.TryGetValue("Content-Length", out string strContentLength) || strContentLength == null || strContentLength.Length == 0)
                throw new JpegDecodingException("The headers of the response don't contain the content length.");

            int contentLength = int.Parse(strContentLength);
            if (contentLength == 0)
                throw new JpegDecodingException("Jpeg headers are empty.");

            byte[] bytesJpegHeaders = new byte[contentLength];
            bytesJpegHeaders[0] = JPEG_START_OF_IMAGE[0];
            bytesJpegHeaders[1] = JPEG_START_OF_IMAGE[1];
            int readBytesNbr;
            for (int i = 0; i < contentLength - 2; i += readBytesNbr)
            {
                readBytesNbr = mjpegStream.Read(bytesJpegHeaders, i, contentLength - 2 - i);
                if (readBytesNbr == 0)
                    throw new JpegDecodingException("Unable to read byte from mjpeg stream.");
            }

            byte[] bytesJpegContent = GetContent(mjpegStream, 320 * 240 * 4 + 4, contentLength);
            return new(bytesJpegHeaders, bytesJpegContent);
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
                    if (b == JPEG_START_OF_IMAGE[0] && !foundJpegBeginning1)
                        foundJpegBeginning1 = true;
                    else if (b == JPEG_START_OF_IMAGE[1] && foundJpegBeginning1)
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

        internal static byte[] GetContent(Stream stream, int contentLength, int offset)
        {
            byte[] dummyArray = new byte[offset];
            int num;
            for (int i = 0; i < offset; i += num)
            {
                num = stream.Read(dummyArray, i, offset - i);
                if (num == 0)
                    throw new JpegDecodingException("Unable to read byte from mjpeg stream.");
            }

            byte[] bytesContent = new byte[contentLength];
            
            for (int i = 0; i < contentLength; i += num)
            {
                num = stream.Read(bytesContent, i, contentLength - i);
                if (num == 0)
                    throw new JpegDecodingException("Unable to read byte from mjpeg stream.");
            }

            return bytesContent;
        }

        internal static byte[] GetContentWithoutJpegMarker(Stream stream, int contentLength)
        {
            byte[] bytesContent = new byte[contentLength];

            if (stream.ReadByte() != JPEG_START_OF_IMAGE[0] || stream.ReadByte() != JPEG_START_OF_IMAGE[1])
                throw new JpegDecodingException("The jpeg begenning of image marker doesn't exist.");

            int readBytesNbr;
            for (int i = 0; i < contentLength - 2; i += readBytesNbr) // Do not read the 2 last jpeg marker
            {
                readBytesNbr = stream.Read(bytesContent, i, contentLength - i);
                if (readBytesNbr == 0)
                    throw new JpegDecodingException("Unable to read byte from mjpeg stream.");
            }

            if (stream.ReadByte() != JPEG_END_OF_IMAGE[0] || stream.ReadByte() != JPEG_END_OF_IMAGE[1])
                throw new JpegDecodingException("The jpeg end of image marker doesn't exist.");

            return bytesContent;
        }
    }
}
