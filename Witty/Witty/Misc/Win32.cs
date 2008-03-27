using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Snarl
{
    class Win32
    {        
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;
        public const int WM_COPYDATA = 0x4A;
        [DllImport("user32.dll")]
        public static extern int FindWindow(
        string lpClassName, // class name
        string lpWindowName // window name
        );
        [DllImport("user32.dll")]
        public static extern int SendMessage(
        int hWnd, // handle to destination window
        uint Msg, // message
        uint wParam, // first message parameter
        IntPtr lParam // second message parameter
        );        
    }
}
