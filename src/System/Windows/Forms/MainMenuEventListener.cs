using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Interop;

namespace System.Windows.Forms
{
    // NativeWindow class to listen to operating system messages.
    internal class MainMenuEventListener : NativeWindow, IMessageFilter, IDisposable
    {
        static readonly ConditionalWeakTable<Form, MainMenuEventListener> MainMenus = new ConditionalWeakTable<Form, MainMenuEventListener>();

        private readonly Form owner;
        private MainMenu mainMenu;
        private MainMenu mergedMenu;
        private MainMenu currentMenu;
        private MainMenu dummyMenu;
        private bool disposed;

        private MainMenuEventListener(Form form)
        {
            owner = form;
            form.HandleCreated += new EventHandler(OnHandleCreated);
            form.HandleDestroyed += new EventHandler(OnHandleDestroyed);
            form.Disposed += new EventHandler(OnFormDisposed);
            Application.AddMessageFilter(this);
            if (form != null && form.IsHandleCreated)
                AssignHandle(form.Handle);
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~MainMenuEventListener()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public MainMenu MainMenu
        {
            get
            {
                return mainMenu;
            }
            set
            {
                MainMenu menu = mainMenu;

                if (menu != value)
                {
                    if (menu != null)
                    {
                        menu.SetForm(null);
                    }

                    mainMenu = value;

                    if (value != null)
                    {
                        Form previous = value.GetForm();
                        if (previous != null)
                        {
                            previous.SetMenu(null);
                        }
                        value.SetForm(owner);
                    }

                    if (!owner.IsHandleCreated)
                    {
                        owner.ClientSize = owner.ClientSize;
                    }
                    owner.MenuChanged(Windows.Forms.Menu.CHANGE_ITEMS, value);
                }
            }
        }

        public MainMenu CurrentMenu
        {
            get
            {
                return currentMenu;
            }
        }

        public MainMenu MergedMenu
        {
            get
            {
                MainMenu parentMenu = owner.MdiParent.GetMenu();

                if (mainMenu == null)
                {
                    return parentMenu;
                }

                if (parentMenu == null)
                {
                    return mainMenu;
                }

                // Create a menu that merges the two and save it for next time.
                mergedMenu = new MainMenu
                {
                    ownerForm = owner
                };
                mergedMenu.MergeMenu(parentMenu);
                mergedMenu.MergeMenu(mainMenu);
                return mergedMenu;
            }
        }

        internal void UpdateMenuHandles()
        {
            // Forget the current menu.
            if (currentMenu != null)
            {
                currentMenu = null;
            }

            if (owner.IsHandleCreated)
            {
                if (!owner.TopLevel)
                {
                    UpdateMenuHandles(null, true);
                }
                else
                {
                    Form child = owner.ActiveMdiChild;
                    if (child != null)
                    {
                        UpdateMenuHandles(child.GetMergedMenu(), true);
                    }
                    else
                    {
                        UpdateMenuHandles(owner.GetMenu(), true);
                    }
                }
            }
        }

        internal void UpdateMenuHandles(MainMenu menu, bool forceRedraw)
        {
            if (menu != null)
            {
                menu.SetForm(owner);
            }

            if (menu != null || currentMenu != null)
            {
                currentMenu = menu;
            }

            MdiClient ctlClient = typeof(Form).GetProperty("MdiClient", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner) as MdiClient;
            if (ctlClient == null || !ctlClient.IsHandleCreated)
            {
                if (menu != null)
                {
                    UnsafeNativeMethods.SetMenu(new HandleRef(owner, owner.Handle), new HandleRef(menu, menu.Handle));
                }
                else
                {
                    UnsafeNativeMethods.SetMenu(new HandleRef(owner, owner.Handle), NativeMethods.NullHandleRef);
                }
            }
            else
            {
                // when both MainMenuStrip and Menu are set, we honor the win32 menu over
                // the MainMenuStrip as the place to store the system menu controls for the maximized MDI child.

                MenuStrip mainMenuStrip = owner.MainMenuStrip;
                if (mainMenuStrip == null || menu != null)
                {  // We are dealing with a Win32 Menu; MenuStrip doesn't have control buttons.

                    // We have a MainMenu and we're going to use it

                    // We need to set the "dummy" menu even when a menu is being removed
                    // (set to null) so that duplicate control buttons are not placed on the menu bar when
                    // an ole menu is being removed.
                    // Make MDI forget the mdi item position.

                    if (dummyMenu == null)
                    {
                        dummyMenu = new MainMenu
                        {
                            ownerForm = owner
                        };
                    }
                    UnsafeNativeMethods.SendMessage(new HandleRef(ctlClient, ctlClient.Handle), WindowMessages.WM_MDISETMENU, dummyMenu.Handle, IntPtr.Zero);

                    if (menu != null)
                    {

                        // Microsoft, 5/2/1998 - don't use Win32 native Mdi lists...
                        //
                        UnsafeNativeMethods.SendMessage(new HandleRef(ctlClient, ctlClient.Handle), WindowMessages.WM_MDISETMENU, menu.Handle, IntPtr.Zero);
                    }
                }

                // (New fix: Only destroy Win32 Menu if using a MenuStrip)
                if (menu == null && mainMenuStrip != null)
                { // If MainMenuStrip, we need to remove any Win32 Menu to make room for it.
                    IntPtr hMenu = UnsafeNativeMethods.GetMenu(new HandleRef(owner, owner.Handle));
                    if (hMenu != IntPtr.Zero)
                    {

                        // We had a MainMenu and now we're switching over to MainMenuStrip

                        // Remove the current menu.
                        UnsafeNativeMethods.SetMenu(new HandleRef(owner, owner.Handle), NativeMethods.NullHandleRef);

                        // because we have messed with the child's system menu by shoving in our own dummy menu,
                        // once we clear the main menu we're in trouble - this eats the close, minimize, maximize gadgets
                        // of the child form. (See WM_MDISETMENU in MSDN)
                        Form activeMdiChild = owner.ActiveMdiChild;
                        if (activeMdiChild != null && activeMdiChild.WindowState == FormWindowState.Maximized)
                        {
                            typeof(Form).GetMethod("RecreateHandle", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(activeMdiChild, null);
                        }

                        // Since we're removing a menu but we possibly had a menu previously,
                        // we need to clear the cached size so that new size calculations will be performed correctly.
                        owner.ResumeLayout(false);
                    }
                }
            }
            if (forceRedraw)
            {
                SafeNativeMethods.DrawMenuBar(new HandleRef(owner, owner.Handle));
            }
        }

        private void OnFormDisposed(object sender, EventArgs e)
        {
            Dispose();
        }

        // Listen for the control's window creation and then hook into it.
        private void OnHandleCreated(object sender, EventArgs e)
        {
            // Window is now created, assign handle to NativeWindow.
            Form form = sender as Form;
            if (form != null)
            {
                AssignHandle(form.Handle);
                if (form.GetMenu() != null || !form.TopLevel || form.IsMdiContainer)
                {
                    UpdateMenuHandles();
                }
            }
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
                case WindowMessages.WM_INITMENUPOPUP:
                    WmInitMenuPopup(ref m);
                    break;
                case WindowMessages.WM_UNINITMENUPOPUP:
                    WmUnInitMenuPopup(ref m);
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
        ///  Processes a command key. Overrides Control.processCmdKey() to provide
        ///  additional handling of main menu command keys and Mdi accelerators.
        /// </summary>
        private bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            bool result = false;
            if (currentMenu != null && currentMenu.ProcessCmdKey(ref m, keyData))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        ///  WM_INITMENUPOPUP handler
        /// </summary>
        private void WmInitMenuPopup(ref Message m)
        {
            if (currentMenu == null || !currentMenu.ProcessInitMenuPopup(m.WParam))
            {
                base.WndProc(ref m);
            }
        }

        /// <summary>
        ///  Handles the WM_MENUCHAR message
        /// </summary>
        private void WmMenuChar(ref Message m)
        {
            if (currentMenu == null)
            {
                Form formMdiParent = owner.MdiParent;
                if (formMdiParent != null && formMdiParent.GetMenu() != null)
                {
                    UnsafeNativeMethods.PostMessage(new HandleRef(formMdiParent, formMdiParent.Handle), WindowMessages.WM_SYSCOMMAND, new IntPtr(NativeMethods.SC_KEYMENU), m.WParam);
                    m.Result = (IntPtr)NativeMethods.Util.MAKELONG(0, 1);
                    return;
                }
            }
            if (currentMenu != null)
            {
                currentMenu.WmMenuChar(ref m);
                if (m.Result != IntPtr.Zero)
                {
                    // This char is a mnemonic on our menu.
                    return;
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        ///  WM_UNINITMENUPOPUP handler
        /// </summary>
        private void WmUnInitMenuPopup(ref Message m)
        {
            if (mainMenu != null)
            {
                //Whidbey addition - also raise the MainMenu.Collapse event for the current menu
                mainMenu.OnCollapse(EventArgs.Empty);
            }
        }

        internal void DisposeMergedMenu()
        {
            if (mergedMenu != null)
            {
                if (mergedMenu.ownerForm == owner)
                {
                    mergedMenu.Dispose();
                }
                mergedMenu = null;
            }
        }

        private void DisposeMenus()
        {
            // We should only dispose this form's menus!
            if (mainMenu != null && mainMenu.ownerForm == owner)
            {
                mainMenu.Dispose();
                mainMenu = null;
            }

            currentMenu = null;

            owner.MenuChanged(Windows.Forms.Menu.CHANGE_ITEMS, null);

            if (dummyMenu != null)
            {
                dummyMenu.Dispose();
                dummyMenu = null;
            }

            if (mergedMenu != null)
            {
                if (mergedMenu.ownerForm == owner || mergedMenu.GetForm() == null)
                {
                    mergedMenu.Dispose();
                }
                mergedMenu = null;
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
                    MainMenus.Remove(owner);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                owner.HandleCreated -= new EventHandler(OnHandleCreated);
                owner.HandleDestroyed -= new EventHandler(OnHandleDestroyed);
                owner.Disposed -= new EventHandler(OnFormDisposed);
                Application.RemoveMessageFilter(this);
                disposed = true;
            }
        }

        internal static MainMenuEventListener GetEventListener(Form form)
        {
            if (MainMenus.TryGetValue(form, out MainMenuEventListener listener))
                return listener;
            return null;
        }

        internal static MainMenuEventListener CreateEventListener(Form form)
        {
            MainMenuEventListener listener = new MainMenuEventListener(form);
            MainMenus.AddOrUpdate(form, listener);
            return listener;
        }

        public bool PreFilterMessage(ref Message m)
        {
            bool result = false;
            Control target = Control.FromChildHandle(m.HWnd);
            if (target != null && owner == target.TopLevelControl)
            {
                switch (m.Msg)
                {
                    case WindowMessages.WM_KEYDOWN:
                    case WindowMessages.WM_SYSKEYDOWN:
                        Keys keyData = (Keys)(unchecked((int)(long)m.WParam) | (int)Control.ModifierKeys);
                        result = ProcessCmdKey(ref m, keyData);
                        break;
                }
            }
            return result;
        }
    }
}
