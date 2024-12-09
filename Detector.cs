using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TYMDetector;

namespace DRSystem
{
    public class Detector
    {
        const int DISCONNECTED = -1;
        SdkInterface.tymfn_datacallback tymfn_Datacallback;

        public int imageWidth = 3072;
        public int imageHeight = 512;
        public int blockHeight = 8;
        public int systemType = 0;
        public int channelNumber = 4;

        

        public enum SystemType
        {
            DAQ = 1,
            DAQII = 2,
            DAQIIEX = 4,
            PANGUC = 5,//single energy
            FCM_G = 10,//single energy
            _04X8 = 11//single energy
        }
    }
}
