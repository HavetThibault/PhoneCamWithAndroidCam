using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.Threads
{
    internal class ReceivePicturesThread
    {
        private MultipleBuffering _multipleBuffering;

        public ReceivePicturesThread(MultipleBuffering multipleBuffering)
        {
            _multipleBuffering = multipleBuffering;
            new Thread(ReceivePictures).Start();
        }

        public void ReceivePictures()
        {

        }
    }
}
