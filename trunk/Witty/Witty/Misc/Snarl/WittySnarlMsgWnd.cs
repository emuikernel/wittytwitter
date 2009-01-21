using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NativeWindowApplication
{

    // Summary description for WittySnarlMsgWnd.
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class WittySnarlMsgWnd : NativeWindow
    {
        CreateParams cp = new CreateParams();

        public WittySnarlMsgWnd()
        {
            // Create the actual window
            this.CreateHandle(cp);
        }

        protected override void WndProc(ref Message m)
        {

            /*       if (m.Msg == 1234)
                   {
                       // Do something here in response to messages
            
            
                       Snarl.SnarlConnector.ShowMessage("Time out", "trala", 20, "", IntPtr.Zero, Snarl.WindowsMessage.WM_USER + 56);
                       return;
                   } else
                   {
                       // Do something here in response to messages
                       Snarl.WindowsMessage bla = new Snarl.WindowsMessage();
                       Snarl.SnarlConnector.ShowMessage(m.Msg.ToString(), m.WParam.ToString(), 20, "", IntPtr.Zero, bla);
                       base.WndProc(ref m);
                       return;
                   }*/
            base.WndProc(ref m);

        }

    }

}
