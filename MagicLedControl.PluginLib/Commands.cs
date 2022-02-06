﻿namespace MagicLedControl.PluginLib
{
    public static class Commands
    {
        public static readonly byte[] TURN_ON = new byte[] { 0x71, 0x23, 0x0F }; //0x71 - Command to send data, 0x23 - value (ON), 0x0F - 0x0F means that the packet comes from a local network, wwhile 0xF0 would mean that the packet was sent from the internet.
        public static readonly byte[] TURN_OFF = new byte[] { 0x71, 0x24, 0x0F }; //same goes here except that 0x24 is OFF
        public static readonly byte[] SET_COLOR = new byte[7] { 0x31, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x0F }; //0x31 - Code, 0xFF- Red, 0xFF - Green, 0xFF - Blue, 0x00 - White, 0x00 - idk, 0x00 - idk, 0x0F - Local Packet, 0xZZ - Checksum
        public static readonly byte[] SET_CUSTOM_FUNCTION = new byte[] { 0x51,  0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00,
            0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03,
            0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x10, 0x3B, 0xFF, 0x0F }; //0x51 - Code, 0xFF - Red1, 0x00 - Green1, 0x00 - Blue1, 0x00 - White1, 0x00 - Red2, 0x00 - Green2, 0xFF - Blue2, 0x00 - White2, 0x00 - Red3, 0xFF - Green3, 0x00 - Blue3, 0x00 - White3.... 
                                                                                                                                                                    //0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00... to fill size of 16 colors (4 Bytes for each color (RBGW)). Then 0x10 - speed (32 - speed), 0x3B - mode (A, 0xsmooth, B, 0xinstant, C, 0xStroboscopic), 0xFF, 0xFF, idk, 0x0F - Local packet, 0xEC - Checksum
        public static readonly byte[] SET_DISCO_FUNCTION = new byte[] { 0x51, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0xFF, 0xFF, 0xFF,
            0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01,
            0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x3B, 0xFF, 0x0F };

        public static readonly byte[] SET_DISCO_FUNCTION2 = new byte[] { 0x51, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0xFF, 0xFF, 0xFF,
            0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01,
            0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x02, 0x03, 0x00, 0x01, 0x3B, 0xFF, 0x0F };

        public static readonly byte[] REQUEST_DATA = new byte[] { 0x81, 0x8A, 0x8B }; //This command requests data from the controller Refer to Stucts.Controller for more info

        public static readonly string DISCOVERY_MESSAGE = "HF-A11ASSISTHREAD"; //also:  48 46 2d 41 31 31 41 53 53 49 53 54 48 52 45 41 44
        /*
         * Discovery Conversation:
         * 
        Message (what the phone app sends):
            HF-A11ASSISTHREAD - contents

        Disocovery Response:
            192.168.0.*, - device IP
            16A6BDEF677D, - device Mac
            AK001-ZJ2101 - device Code (Always the same? I think so)
        */
    }
}
