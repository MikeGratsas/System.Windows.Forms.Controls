using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms
{
    internal static class CurrencyManagerHelper
    {
        internal static string GetListName(this CurrencyManager currencyManager)
        {
            return typeof(CurrencyManager).GetMethod("GetListName", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(currencyManager, null) as string;
        }

        /// <summary>
        ///  Gets a value indicating
        ///  whether items can be added to the list.
        /// </summary>
        internal static bool AllowAdd(this CurrencyManager currencyManager)
        {
            IList list = currencyManager.List;
            if (list is IBindingList)
            {
                return ((IBindingList)list).AllowNew;
            }

            if (list is null)
            {
                return false;
            }

            return !list.IsReadOnly && !list.IsFixedSize;
        }

        /// <summary>
        ///  Gets a value
        ///  indicating whether edits to the list are allowed.
        /// </summary>
        internal static bool AllowEdit(this CurrencyManager currencyManager)
        {
            IList list = currencyManager.List;
            if (list is IBindingList)
            {
                return ((IBindingList)list).AllowEdit;
            }

            if (list is null)
            {
                return false;
            }

            return !list.IsReadOnly;
        }

        /// <summary>
        ///  Gets a value indicating whether items can be removed from the list.
        /// </summary>
        internal static bool AllowRemove(this CurrencyManager currencyManager)
        {
            IList list = currencyManager.List;
            if (list is IBindingList)
            {
                return ((IBindingList)list).AllowRemove;
            }

            if (list is null)
            {
                return false;
            }

            return !list.IsReadOnly && !list.IsFixedSize;
        }
    }
}
