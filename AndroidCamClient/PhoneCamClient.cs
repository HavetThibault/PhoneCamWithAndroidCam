using AndroidCamClient.JpegStream;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Xml;
using System.Runtime.CompilerServices;

namespace AndroidCamClient
{
    public class PhoneCamClient : IDisposable
    {
        private string _phoneUrl;
        private JpegStreamDecoder _jpegStreamDecoder;

        public PhoneCamClient(string phoneIp)
        {
            _phoneUrl = $"http://{phoneIp}:4747";
            _jpegStreamDecoder = new();
        }

        public void Dispose() => _jpegStreamDecoder.Dispose();

        public Task<Stream> LaunchStream()
        {
            return _jpegStreamDecoder.InitMJpegStream(_phoneUrl + "/video?320x240");
        }

        public static JpegFrame GetStreamFrame(Stream mjpegStream)
        {
            return JpegStreamDecoder.ReadOneFrame(mjpegStream);
        }
    }
}