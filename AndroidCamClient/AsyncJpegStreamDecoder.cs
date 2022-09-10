using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidCamClient
{
    public static class AsyncJpegStreamDecoder
    {
        public static readonly string HeaderSeparator = "\r\n";
        public static readonly string HeaderKeyValueSeparator = ": ";

        public static async Task<byte[]> GetFrameAsync(string uri)
        {
            using HttpClient client = new ();
            while (true)
            {
                using Stream stream = await client.GetStreamAsync(uri).ConfigureAwait(continueOnCapturedContext: false);
                while (true)
                {
                    Dictionary<string, string> headers = ConvertBytesHeaders(GetBytesHeaders(stream, out int contentOffset));
                    if (!headers.TryGetValue("Content-Length", out string strContentLength))
                        throw new IOException("The headers of the response don't contain the content length.");

                    int contentLength = int.Parse(strContentLength);
                    if (contentLength == 0)
                    {
                        break;
                    }

                    byte[] content = GetContent(stream, 320 * 240 * 4 + 5);
                    if (content == null)
                    {
                        break;
                    }

                    return content;
                }
            }
        }

        internal static byte[] GetBytesHeaders(Stream stream, out int offset)
        {
            bool firstCharCr = false;
            bool foundCr = false;
            bool foundJpegBeginning1 = false;
            bool foundJpegBeginning2 = false;
            bool foundJpegBeginning = false;
            byte b;
            MemoryStream mainMemoryStream = new ();
            MemoryStream subMemoryStream = new ();
            offset = 0;
            while (!foundJpegBeginning)
            {
                b = (byte)stream.ReadByte();
                subMemoryStream.WriteByte(b);
                if (foundCr)
                {
                    if (b == 255 && !foundJpegBeginning1)
                        foundJpegBeginning1 = true;
                    else if (b == 216 && !foundJpegBeginning2 && foundJpegBeginning1)
                        foundJpegBeginning2 = true;
                    else if (b == 255 && foundJpegBeginning1 && foundJpegBeginning2)
                        foundJpegBeginning = true;
                    else
                    {
                        foundJpegBeginning1 = false;
                        foundJpegBeginning2 = false;
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
            offset -= 3; // Less the 3 magic characters that begin a jpeg
            byte[] byteHeaders = mainMemoryStream.ToArray();
            mainMemoryStream.Close();
            subMemoryStream.Close();
            return byteHeaders;
        }

        internal static Dictionary<string,string> ConvertBytesHeaders(byte[] bytesHeaders)
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

                string[] headerKeyValueArray = headerKeyValue.Split(new string[2] { HeaderKeyValueSeparator, HeaderSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (headerKeyValueArray.Length != 2)
                    continue;

                string headerKey = headerKeyValueArray[0];
                string headerValue = headerKeyValueArray[1];

                headers.Add(headerKey, headerValue);
                mainMemoryStream.Close();
            }

            return headers;
        }

        internal static byte[] SliceJpegStream(Stream stream)
        {
            return SliceStream(stream, new byte[3] { 255, 216, 255}, new byte[2] { 255, 217 });
        }

        internal static byte[] SliceStream(Stream stream, byte[] beginPattern, byte[] endPattern)
        {
            int num = 0;
            bool flag = false;
            using MemoryStream memoryStream = new ();
            int i = 0;
            while(true)
            {
                int num2 = stream.ReadByte();
                if (num2 == -1)
                {
                    throw new IOException($"Can't read byte from the stream : {stream}");
                }

                if (!flag)
                {
                    if (num2 == beginPattern[num])
                    {
                        num++;
                        memoryStream.WriteByte((byte)num2);
                        if (num == beginPattern.Length)
                        {
                            flag = true;
                            num = 0;
                        }
                    }
                    else
                    {
                        memoryStream.SetLength(0L);
                        num = 0;
                    }

                    continue;
                }

                memoryStream.WriteByte((byte)num2);
                if (num2 == endPattern[num])
                {
                    num++;
                    if (num == endPattern.Length)
                    {
                        break;
                    }
                }
                else
                {
                    num = 0;
                }
                memoryStream.WriteByte((byte)stream.ReadByte());
                i++;
            }

            return memoryStream.ToArray();
        }

        internal static int GetContentLength(Stream stream)
        {
            int result = 0;
            byte[] array = SliceJpegStream(stream);
            if (array == null || array.Length < 7)
            {
                return 0;
            }

            for (int num = array.Length - 5; num >= 0; num--)
            {
                if (array[num] == 32)
                {
                    string @string = Encoding.UTF8.GetString(array, num, array.Length - num - 4);
                    if (!int.TryParse(@string, out result))
                    {
                        return 0;
                    }

                    break;
                }
            }

            string message = $"Frame header:\n{Encoding.UTF8.GetString(array, 0, array.Length - 4)}\nParsed Length: {result}\n";
            Debug.WriteLine(message);
            return result;
        }

        internal static byte[] GetContent(Stream stream, int contentLength)
        {
            byte[] bytesContent = new byte[contentLength];
            bytesContent[0] = 255;
            bytesContent[1] = 216;
            bytesContent[2] = 255;

            int num;
            for (int i = 3;  i != contentLength; i += num)
            {
                num = stream.Read(bytesContent, i, contentLength - i);
                if (num == 0)
                {
                    return null;
                }
            }

            return bytesContent;
        }
    }
}
