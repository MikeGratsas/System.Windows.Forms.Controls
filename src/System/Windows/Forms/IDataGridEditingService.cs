// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms
{
    /// <summary>
    ///  The DataGrid exposes hooks to request editing commands via this interface.
    /// </summary>
    public interface IDataGridEditingService
    {
	/// <summary>Begins an edit operation.</summary>
	/// <param name="gridColumn">The <see cref="T:System.Windows.Forms.DataGridColumnStyle" /> to edit.</param>
	/// <param name="rowNumber">The index of the row to edit</param>
	/// <returns>
	///   <see langword="true" /> if the operation can be performed; otherwise <see langword="false" />.</returns>
        bool BeginEdit(DataGridColumnStyle gridColumn, int rowNumber);

	/// <summary>Ends the edit operation.</summary>
	/// <param name="gridColumn">The <see cref="T:System.Windows.Forms.DataGridColumnStyle" /> to edit.</param>
	/// <param name="rowNumber">The number of the row to edit</param>
	/// <param name="shouldAbort">True if an abort operation is requested</param>
	/// <returns>
	///   <see langword="true" /> if value is commited; otherwise <see langword="false" />.</returns>
        bool EndEdit(DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort);
    }
}
