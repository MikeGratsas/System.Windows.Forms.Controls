using System.Reflection;
using static Interop;

namespace System.Windows.Forms
{
    internal static class ControlHelper
    {
        internal static void SetState(this Control control, int flag, bool value)
        {
            typeof(Control).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, new object[] { flag, value });
        }

        internal static void SuspendDrawing(this Control control)
        {
            if (control.IsHandleCreated)
            {
                User32.SendMessageW(control, WindowMessages.WM_SETREDRAW, (IntPtr)BOOL.FALSE, IntPtr.Zero);
            }
        }

        internal static void ResumeDrawing(this Control control)
        {
            ResumeDrawing(control, true);
        }

        internal static void ResumeDrawing(this Control control, bool invalidate)
        {
            if (control.IsHandleCreated)
            {
                User32.SendMessageW(control, WindowMessages.WM_SETREDRAW, (IntPtr)BOOL.TRUE, IntPtr.Zero);
                if (invalidate)
                {
                    control.Invalidate();
                }
            }
        }
    }
}
