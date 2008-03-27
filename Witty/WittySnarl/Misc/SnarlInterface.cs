using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Snarl
{    
    enum SNARL_COMMANDS
    {
        SNARL_SHOW = 1,
        SNARL_HIDE,
        SNARL_UPDATE,
        SNARL_IS_VISIBLE,
        SNARL_GET_VERSION,
        SNARL_REGISTER_CONFIG_WINDOW,
        SNARL_REVOKE_CONFIG_WINDOW,
        SNARL_REGISTER_ALERT,
        SNARL_REVOKE_ALERT,
        SNARL_REGISTER_CONFIG_WINDOW_2,
        SNARL_GET_VERSION_EX,

        SNARL_EX_SHOW = 0x20
    };

        
		unsafe struct SNARLSTRUCT {
			public int cmd;
            public int id;
            public int timeout;
            public int lngData2;
            public fixed byte title[SnarlInterface.SNARL_STRING_LENGTH];
            public fixed byte text[SnarlInterface.SNARL_STRING_LENGTH];
            public fixed byte icon[SnarlInterface.SNARL_STRING_LENGTH];
		};

        struct COPYDATASTRUCT
        {
            public int dwData;
            public int cbData;
            public IntPtr lpData;
        }

    class SnarlInterface
    {
        public const int SNARL_STRING_LENGTH = 1024;        

        private unsafe static void CopyToFixedArray(byte[] source, byte* destination)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = source[i];
            }
        }

        public unsafe static int SendMessage(string title, string message, string iconPath, int timeout)
        {
            SNARLSTRUCT snarlStruct = new SNARLSTRUCT();            

            snarlStruct.cmd = (int)SNARL_COMMANDS.SNARL_SHOW;

            CopyToFixedArray(ASCIIEncoding.ASCII.GetBytes(title.ToCharArray()), snarlStruct.title);
            CopyToFixedArray(ASCIIEncoding.ASCII.GetBytes(message.ToCharArray()), snarlStruct.text);
            CopyToFixedArray(ASCIIEncoding.ASCII.GetBytes(iconPath.ToCharArray()), snarlStruct.icon);

            snarlStruct.timeout = timeout;

            return Send(snarlStruct);

        }

        public static void HideMessage(int id)
        {
            SNARLSTRUCT snarlStruct = new SNARLSTRUCT();

            snarlStruct.cmd = (int)SNARL_COMMANDS.SNARL_HIDE;
            snarlStruct.id = id;

            int x = Send(snarlStruct);
        }        

        public static int GetVersionEx()
        {
            SNARLSTRUCT snarlStruct = new SNARLSTRUCT();
            snarlStruct.cmd = (int)SNARL_COMMANDS.SNARL_GET_VERSION_EX;
            return Send(snarlStruct);
        }


        private static int Send(SNARLSTRUCT snarlStruct)
        {
            int hWnd = Win32.FindWindow(null, "Snarl");

            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds.dwData = 2;
            cds.cbData = Marshal.SizeOf(snarlStruct);

            IntPtr snarlStructPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(snarlStruct));
            Marshal.StructureToPtr(snarlStruct, snarlStructPtr, true);

            cds.lpData = snarlStructPtr;

            IntPtr iPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(cds));
            Marshal.StructureToPtr(cds, iPtr, true);

            int id = Win32.SendMessage(hWnd, Win32.WM_COPYDATA, 0, iPtr);

            Marshal.FreeCoTaskMem(iPtr);
            Marshal.FreeCoTaskMem(snarlStructPtr);

            return id;
        }        
    }
}
