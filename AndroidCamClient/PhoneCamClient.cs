
using AndroidCamClient;
using System;
using System.ComponentModel;
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

        public async Task<byte[]> MockPhoneVideoStream()
        {
            return await AsyncJpegStreamDecoder.GetFrameAsync(phoneUrl + "/video?320x240");
        }
    }
}