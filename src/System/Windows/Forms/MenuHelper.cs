using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Interop;

namespace System.Windows.Forms
{
    /// <summary>
    ///  This is the class to contain the extension method for all menu components (MainMenu and ContextMenu).
    /// </summary>
    public static class MenuHelper
    {
        internal static MainMenu GetCurrentMenu(this Form form)
        {
            MainMenuEventListener listener = MainMenuEventListener.GetEventListener(form);
            return listener != null ? listener.CurrentMenu : null;
        }

        /// <summary>
        ///  Gets the <see cref='MainMenu'/>
        ///  that is displayed in the form.
        /// </summary>
        public static MainMenu GetMenu(this Form form)
        {
            MainMenuEventListener listener = MainMenuEventListener.GetEventListener(form);
            return listener != null ? listener.MainMenu: null;
        }

        /// <summary>
        ///  Sets the <see cref='MainMenu'/>
        ///  that is displayed in the form.
        /// </summary>
        public static void SetMenu(this Form form, MainMenu value)
        {
            MainMenuEventListener listener = MainMenuEventListener.GetEventListener(form);
            if (value != null)
            {
                if (listener == null)
                    listener = MainMenuEventListener.CreateEventListener(form);
                listener.MainMenu = value;
            }
            else
            {
                if (listener != null)
                {
                    listener.Dispose();
                    listener = null;
                }
            }
        }

        /// <summary>
        ///  Gets the merged menu for the form.
        /// </summary>
        public static MainMenu GetMergedMenu(this Form form)
        {
            Form formMdiParent = form.MdiParent;
            if (formMdiParent == null)
            {
                return null;
            }

            MainMenuEventListener listener = MainMenuEventListener.GetEventListener(form);
            if (listener == null)
                listener = MainMenuEventListener.CreateEventListener(form);
            return listener.MergedMenu;
        }

        // Package scope for menu interop
        internal static void MenuChanged(this Form form, int change, Menu menu)
        {
            Form parForm = form.ParentForm;
            if (parForm != null && form == parForm.ActiveMdiChild)
            {
                parForm.MenuChanged(change, menu);
                return;
            }
            MainMenuEventListener listener = MainMenuEventListener.GetEventListener(form);
            MdiClient ctlClient = typeof(Form).GetProperty("MdiClient", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form) as MdiClient;
            switch (change)
            {
                case Windows.Forms.Menu.CHANGE_ITEMS:
                case Windows.Forms.Menu.CHANGE_MERGE:
                    if (ctlClient == null || !ctlClient.IsHandleCreated)
                    {
                        if (menu == form.GetMenu() && change == Windows.Forms.Menu.CHANGE_ITEMS)
                        {
                            listener.UpdateMenuHandles();
                        }

                        break;
                    }

                    // Tell the children to toss their mergedMenu.
                    if (form.IsHandleCreated)
                    {
                        listener.UpdateMenuHandles(null, false);
                    }

                    Control.ControlCollection children = ctlClient.Controls;
                    for (int i = children.Count; i-- > 0;)
                    {
                        Form child = children[i] as Form;
                        if (child != null)
                        {
                            MainMenuEventListener childListener = MainMenuEventListener.GetEventListener(child);
                            if (childListener != null)
                                childListener.DisposeMergedMenu();
                        }
                    }

                    listener.UpdateMenuHandles();
                    break;
                case Windows.Forms.Menu.CHANGE_VISIBLE:
                    if (menu == form.GetMenu() || (form.ActiveMdiChild != null && menu == form.ActiveMdiChild.GetMenu()))
                    {
                        listener.UpdateMenuHandles();
                    }
                    break;
                case Windows.Forms.Menu.CHANGE_MDI:
                    if (ctlClient != null && ctlClient.IsHandleCreated)
                    {
                        listener.UpdateMenuHandles();
                    }
                    break;
            }
        }

        /// <summary>
        ///  Get the contextMenu associated with this control. The contextMenu
        ///  will be shown when the user right clicks the mouse on the control.
        ///
        ///  In all cases where both a context menu
        ///  and a context menu strip are assigned, context menu will be shown instead of context menu strip.
        /// </summary>
        public static ContextMenu GetContextMenu(this Control control)
        {
            ContextMenuEventListener listener = ContextMenuEventListener.GetEventListener(control);
            return listener != null ? listener.ContextMenu : null;
        }

        /// <summary>
        ///  Set the contextMenu associated with this control. The contextMenu
        ///  will be shown when the user right clicks the mouse on the control.
        ///
        ///  In all cases where both a context menu
        ///  and a context menu strip are assigned, context menu will be shown instead of context menu strip.
        /// </summary>
        public static void SetContextMenu(this Control control, ContextMenu value)
        {
            ContextMenuEventListener listener = ContextMenuEventListener.GetEventListener(control);
            if (value != null)
            {
                if (listener == null)
                    listener = ContextMenuEventListener.CreateEventListener(control);
                listener.ContextMenu = value;
            }
            else
            {
                if (listener != null)
                {
                    listener.Dispose();
                    listener = null;
                }
            }
        }
    }
}