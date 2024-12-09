using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TYMDetector
{
    public partial class SdkInterface
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct tymdata_buffer
        {
            public int line_num;
            public int pixels_per_line;
            public int bytes_per_pixel;
            public int bytes_per_line;
            public IntPtr data;//unsigned char*
        };



        [StructLayout(LayoutKind.Sequential)]
        public struct trigger_info
        {
	        public int pulse_level;//表示触发脉冲电平 默认:高电平
	        public int input_pulse_num;//输入脉冲个数,默认:1
	        public int output_pulse_width;//输出脉冲宽度，默认:50ms
	        public int outputLinenum_per_trigger;//表明每触发一次trigger上传多少行数据，默认:512

         };



        [StructLayout(LayoutKind.Sequential)]
        public struct userdata_pangu
        {
            public int height;
			public int blocknum;
            public IntPtr lineNumber; //unsigned int*
            public IntPtr temperature; //unsigned short*
            public IntPtr humidity; //unsigned short*
            public IntPtr voltage1; //unsigned short*
            public IntPtr voltage2; //unsigned short*
            public IntPtr voltage3; //unsigned short*
            public IntPtr voltage4; //unsigned short*

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct userdata_panguc
        {
            public userdata_pangu usrdata_pangu;
            public IntPtr xray_flag;//unsigned char*
            public IntPtr data_mode;//unsigned char*

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct info_header_setting {

	        public int null_byte_num;
            public IntPtr buf_item_order;//unsigned char*
	        public int bufsize_item_order;
	        public enum item_id{ id_linenum,id_null_byte};
            
        }

        public const int id_linenum = 0;
        public const int id_null_byte = 1;

       [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void tymfn_datacallback(IntPtr buffer,IntPtr extra_info, IntPtr user_data,int instance);
    }
}
