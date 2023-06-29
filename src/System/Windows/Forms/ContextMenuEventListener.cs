using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Interop;

namespace System.Windows.Forms
{
    // NativeWindow class to listen to operating system messages.
    internal class ContextMenuEventListener : NativeWindow, IDisposable
    {
        static readonly ConditionalWeakTable<Control, ContextMenuEventListener> ContextMenus = new ConditionalWeakTable<Control, ContextMenuEventListener>();

        private readonly Control owner;
        private ContextMenu contextMenu;
        private bool disposed;

        public ContextMenuEventListener(Control control)
        {
            owner = control;
            control.HandleCreated += new EventHandler(OnHandleCreated);
            control.HandleDestroyed += new EventHandler(OnHandleDestroyed);
            control.Disposed += new EventHandler(OnControlDisposed);
            if (control != null && control.IsHandleCreated)
                AssignHandle(control.Handle);
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ContextMenuEventListener()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ContextMenu ContextMenu
        {
            get
            {
                return contextMenu;
            }
            set
            {
                ContextMenu menu = contextMenu;

                if (menu != value)
                {
                    contextMenu = value;
                }
            }
        }

        private void OnControlDisposed(object sender, EventArgs e)
        {
            Dispose();
        }

        // Listen for the control's window creation and then hook into it.
        private void OnHandleCreated(object sender, EventArgs e)
        {
            // Window is now created, assign handle to NativeWindow.
            Control control = sender as Control;
            if (control != null)
                AssignHandle(control.Handle);
        }

        private void OnHandleDestroyed(object sender, EventArgs e)
        {
            // Window was destroyed, release hook.
            ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            // Listen for operating system messages

            switch (m.Msg)
            {
                case WindowMessages.WM_KEYDOWN:
                case WindowMessages.WM_SYSKEYDOWN:
                    Keys keyData = (Keys)(unchecked((int)(long)m.WParam) | (int)Control.ModifierKeys);
                    if (!ProcessCmdKey(ref m, keyData))
                        base.WndProc(ref m);
                    break;
                case WindowMessages.WM_CONTEXTMENU:
                    WmContextMenu(ref m, owner);
                    break;
                case WindowMessages.WM_INITMENUPOPUP:
                    WmInitMenuPopup(ref m);
                    break;
                case WindowMessages.WM_EXITMENULOOP:
                    WmExitMenuLoop(ref m);
                    break;
                case WindowMessages.WM_MENUCHAR:
                    WmMenuChar(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        ///  Processes a command key. This method is called during message
        ///  pre-processing to handle command keys. Command keys are keys that always
        ///  take precedence over regular input keys. Examples of command keys
        ///  include accelerators and menu shortcuts. The method must return true to
        ///  indicate that it has processed the command key, or false to indicate
        ///  that the key is not a command key.
        ///  processCmdKey() first checks if the control has a context menu, and if
        ///  so calls the menu's processCmdKey() to check for menu shortcuts. If the
        ///  command key isn't a menu shortcut, and if the control has a parent, the
        ///  key is passed to the parent's processCmdKey() method. The net effect is
        ///  that command keys are "bubbled" up the control hierarchy.
        ///  When overriding processCmdKey(), a control should return true to
        ///  indicate that it has processed the key. For keys that aren't processed by
        ///  the control, the result of "base.processCmdKey()" should be returned.
        ///  Controls will seldom, if ever, need to override this method.
        /// </summary>
        private bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            bool result = false;
            if (contextMenu != null && contextMenu.ProcessCmdKey(ref m, keyData, owner))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        ///  Handles the WM_EXITMENULOOP message. If this control has a context menu, its
        ///  Collapse event is raised.
        /// </summary>
        private void WmExitMenuLoop(ref Message m)
        {
            bool isContextMenu = unchecked((int)(long)m.WParam) != 0;

            if (isContextMenu)
            {
                if (contextMenu != null)
                {
                    contextMenu.OnCollapse(EventArgs.Empty);
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        ///  Handles the WM_CONTEXTMENU message
        /// </summary>
        private void WmContextMenu(ref Message m, Control sourceControl)
        {
            if (contextMenu != null)
            {
                int x = NativeMethods.Util.SignedLOWORD(m.LParam);
                int y = NativeMethods.Util.SignedHIWORD(m.LParam);
                Point client;
                bool keyboardActivated = false;
                // lparam will be exactly -1 when the user invokes the context menu
                // with the keyboard.
                //
                if (unchecked((int)(long)m.LParam) == -1)
                {
                    keyboardActivated = true;
                    client = new Point(owner.Width / 2, owner.Height / 2);
                }
                else
                {
                    client = owner.PointToClient(new Point(x, y));
                }

                // VisualStudio7 # 156, only show the context menu when clicked in the client area
                if (owner.ClientRectangle.Contains(client))
                {
                    if (contextMenu != null)
                    {
                        contextMenu.Show(sourceControl, client);
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                }
                else
                {
                    base.WndProc(ref m);
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        /// <summary>
        ///  WM_INITMENUPOPUP handler
        /// </summary>
        private void WmInitMenuPopup(ref Message m)
        {
            if (contextMenu == null || !contextMenu.ProcessInitMenuPopup(m.WParam))
            {
                base.WndProc(ref m);
            }
        }

        /// <summary>
        ///  Handles the WM_MENUCHAR message
        /// </summary>
        private void WmMenuChar(ref Message m)
        {
            if (contextMenu != null)
            {
                contextMenu.WmMenuChar(ref m);
                if (m.Result != IntPtr.Zero)
                {
                    // This char is a mnemonic on our menu.
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void DisposeMenus()
        {
            // We should only dispose this form's menus!
            if (contextMenu != null)
            {
                contextMenu.Dispose();
                contextMenu = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)

                    DisposeMenus();
                    ContextMenus.Remove(owner);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                owner.HandleCreated -= new EventHandler(OnHandleCreated);
                owner.HandleDestroyed -= new EventHandler(OnHandleDestroyed);
                owner.Disposed -= new EventHandler(OnControlDisposed);
                disposed = true;
            }
        }

        internal static ContextMenuEventListener GetEventListener(Control control)
        {
            if (ContextMenus.TryGetValue(control, out ContextMenuEventListener listener))
                return listener;
            return null;
        }

        internal static ContextMenuEventListener CreateEventListener(Control control)
        {
            ContextMenuEventListener listener = new ContextMenuEventListener(control);
            ContextMenus.AddOrUpdate(control, listener);
            return listener;
        }
    }
}
