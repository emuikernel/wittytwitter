using System;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct SNARLSTRUCT
    {
        public int cmd;
        public int id;
        public int timeout;
        public int lngData2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlInterface.SNARL_STRING_LENGTH)]
        public char[] title;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlInterface.SNARL_STRING_LENGTH)]
        public char[] text;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlInterface.SNARL_STRING_LENGTH)]
        public char[] icon;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct SNARLSTRUCTEX
    {
        public SNARLSTRUCT snarlStruct;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlInterface.SNARL_STRING_LENGTH)]
        public char[] snarlClass;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlInterface.SNARL_STRING_LENGTH)]
        public char[] extra;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlInterface.SNARL_STRING_LENGTH)]
        public char[] extra2;
        public int reserved1;
        public int reserved2;
    }

    struct COPYDATASTRUCT
    {
        public int dwData;
        public int cbData;
        public IntPtr lpData;
    }

    class SnarlInterface
    {
        public const int SNARL_STRING_LENGTH = 1024;

        public static int SendMessage(string title, string message, string iconPath, int timeout)
        {
            SNARLSTRUCT snarlStruct = new SNARLSTRUCT();

            snarlStruct.cmd = (int)SNARL_COMMANDS.SNARL_SHOW;

            snarlStruct.title = title.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStruct.text = message.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStruct.icon = iconPath.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();

            snarlStruct.timeout = timeout;

            return Send(snarlStruct);

        }

        public static int SendMessage(string alertClass, string title, string message, string iconPath, int timeout, string soundPath)
        {
            SNARLSTRUCTEX snarlStructEx = new SNARLSTRUCTEX();
            snarlStructEx.snarlStruct.cmd = (int)SNARL_COMMANDS.SNARL_EX_SHOW;
            snarlStructEx.snarlStruct.title = title.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStructEx.snarlStruct.text = message.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStructEx.snarlStruct.icon = iconPath.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStructEx.snarlClass = alertClass.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStructEx.extra = soundPath.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStructEx.snarlStruct.timeout = timeout;

            return Send(snarlStructEx);
        }

        public static int RegisterConfig(string appName, int hWnd, string iconPath)
        {
            SNARLSTRUCT snarlStruct = new SNARLSTRUCT();
            snarlStruct.cmd = (int)SNARL_COMMANDS.SNARL_REGISTER_CONFIG_WINDOW_2;
            snarlStruct.lngData2 = hWnd;
            snarlStruct.title = appName.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStruct.icon = iconPath.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStruct.id = Win32.WM_USER;

            return Send(snarlStruct);
        }

        public static int RevokeConfig(int hWnd)
        {
            SNARLSTRUCT snarlStruct = new SNARLSTRUCT();
            snarlStruct.cmd = (int)SNARL_COMMANDS.SNARL_REVOKE_CONFIG_WINDOW;
            snarlStruct.lngData2 = hWnd;

            return Send(snarlStruct);
        }

        public static int RegisterAlert(string appName, string alertName)
        {
            SNARLSTRUCT snarlStruct = new SNARLSTRUCT();
            snarlStruct.cmd = (int)SNARL_COMMANDS.SNARL_REGISTER_ALERT;
            snarlStruct.title = appName.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();
            snarlStruct.text = alertName.PadRight(SNARL_STRING_LENGTH, '\0').ToCharArray();

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

        public static bool SnarlIsActive()
        {
            return Win32.FindWindow(null, "Snarl") != 0;
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

        private static int Send(SNARLSTRUCTEX snarlStruct)
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
