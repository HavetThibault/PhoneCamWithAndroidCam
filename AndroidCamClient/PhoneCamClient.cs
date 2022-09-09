
using AndroidCamClient;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Xml;

namespace WebAPIClients
{
    public class PhoneCamClient
    {
        private string phoneUrl;

        public PhoneCamClient(string phoneIp)
        {
            phoneUrl = $"http://{phoneIp}:4747";
        }

        public async void MockPhoneVideoStream()
        {
            IAsyncEnumerable<byte[]> enumerable = AsyncStreamDecoder.GetFrameAsync(phoneUrl + "/video?320x240");
            IAsyncEnumerator<byte[]> enumerator = enumerable.GetAsyncEnumerator();

            Console.WriteLine((await enumerator.MoveNextAsync()).ToString());

            byte[] frame = enumerator.Current;
        }
    }
}