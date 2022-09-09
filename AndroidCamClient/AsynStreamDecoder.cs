using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidCamClient
{
    public static class AsyncStreamDecoder
    {
        public static async IAsyncEnumerable<byte[]> GetFrameAsync(string uri)
        {
            using HttpClient client = new HttpClient();
            while (true)
            {
                using Stream stream = await client.GetStreamAsync(uri).ConfigureAwait(continueOnCapturedContext: false);
                while (true)
                {
                    int contentLength = GetContentLength(stream);
                    if (contentLength == 0)
                    {
                        break;
                    }

                    byte[] content = GetContent(stream, contentLength);
                    if (content == null)
                    {
                        break;
                    }

                    yield return content;
                }
            }
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
            int i = 0;
            byte[] array = new byte[contentLength];
            int num;
            for (; i != contentLength; i += num)
            {
                num = stream.Read(array, i, contentLength - i);
                if (num == 0)
                {
                    return null;
                }
            }

            return array;
        }
    }
}
