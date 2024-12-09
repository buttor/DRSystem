using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TYMDetector
{
    public partial class SdkInterface
    {


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_init", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_init(string ipaddress_local, string ipaddress_daq, int cmd_port, int data_port, ref int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_cleanup", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_cleanup(int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_inited", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_inited(ref bool inited,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_connected", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_connected(ref bool connected,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_version", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_version(StringBuilder sdkversion, int sdkversion_length);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_version_firmware", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_version_firmware(StringBuilder version,int buf_length,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_grab_start", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_grab_start(int instance,int count=0);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_grab_stop", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_grab_stop(int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_gain_range", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_gain_range(ref int gain_min, ref int gain_max,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_gain_low", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_gain_low(int card_no, int gain, int instance, int switch_code = 0);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_gain_low", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_gain_low(int card_no, IntPtr gain, int instance, int bufsize, ref int switch_code);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_gain_high", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_gain_high(int card_no, int gain, int instance, int switch_code = 0);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_gain_high", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_gain_high(int card_no, IntPtr gain, int instance, int bufsize, ref int switch_code);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_test_pattern_range", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_test_pattern_range(ref int pattern_min, ref int pattern_max,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_test_pattern", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_test_pattern(int pattern,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_test_pattern", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_test_pattern(ref int pattern,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_integral_time_range", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_integral_time_range(ref int time_min, ref int time_max,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_integral_time", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_integral_time(int time_us, int instance, int switch_code = 0);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_integral_time", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_integral_time(ref int time_us, int instance, int switch_code = 0);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_daq_ip", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_daq_ip(string ipaddress, int instance, int switch_code = 0);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_daq_ip", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_daq_ip(StringBuilder ipaddress, int ipaddress_length, int instance, int switch_code = 0);
		
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_daq_port", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_daq_port(int cmd_port, int img_port, int instance);
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_daq_port", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_daq_port(ref int cmd_port, ref int img_port, int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_cardnumber", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_cardnumber(int linkid, int number,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_cardnumber", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_cardnumber(int linkid, ref int number,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_pixelnum_percard", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_pixelnum_percard(int pixelnum,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_pixelnum_percard", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_pixelnum_percard(ref int pixelnum,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_pixelnum", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_pixelnum(ref int pixelnum,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_trigger_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_trigger_mode(int mode, int instance, IntPtr trig_info = default(IntPtr));


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_trigger_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_trigger_mode(ref int mode, int instance, IntPtr trig_info = default(IntPtr));


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_energy_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_energy_mode(int mode,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_energy_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_get_energy_mode(ref int mode,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_special_set", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_special_set(int cmd_id, int value, int bytes_this_cmd,int instance);


        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_special_get", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_special_get(int cmd_id, ref int value, int bytes_this_cmd,int instance);

		
        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_datacallback", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_datacallback(tymfn_datacallback callback,IntPtr user_data, int block_line_num, int instance, IntPtr data_format = default(IntPtr));
		
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymcan_doOffsetCalibration", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymcan_doOffsetCalibration(int instance);
		
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymcan_doGainCalibration", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymcan_doGainCalibration(uint target,int instance);
		
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymcan_setBaseLine", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymcan_setBaseLine(uint baseLine,int instance);
		
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymcan_set_sendCalibratedData_Enable", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymcan_set_sendCalibratedData_Enable(bool offset_enable, bool coe_enable, bool baseline_enable,int instance);

		[DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_pixel_order_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_pixel_order_mode(int mode,int instance);
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_get_systype", CallingConvention = CallingConvention.StdCall)]
		public static extern int tymscan_get_systype(ref int type,ref int bytes_per_pixel,int instance);
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_send_command", CallingConvention = CallingConvention.StdCall)]
		public static extern int tymscan_send_command(int cmdId, int op, string data, StringBuilder result, int maxResultLen,int instance);
		
		
		[DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_snap", CallingConvention = CallingConvention.StdCall)]
		public static extern int tymscan_snap(int count,int instance);

        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_save_config", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_save_config(int instance);

        [DllImport("TYMScannerLib.dll", EntryPoint = "tymscan_set_custom_pixnum", CallingConvention = CallingConvention.StdCall)]
        public static extern int tymscan_set_custom_pixnum(int pixelnum, int instance);


    }
}
