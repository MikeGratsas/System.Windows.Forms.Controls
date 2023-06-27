# System.Windows.Forms.Controls
This package includes types and controls that were removed from System.Windows.Forms in .NET Core 3.1/.NET 5 and later versions.

According to [Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core), starting with .NET Core 3.1, various Windows Forms controls are no longer available:

  * ContextMenu
  * DataGrid
  * DataGrid.HitTestType
  * DataGridBoolColumn
  * DataGridCell
  * DataGridColumnStyle
  * DataGridLineStyle
  * DataGridParentRowsLabelStyle
  * DataGridPreferredColumnWidthTypeConverter
  * DataGridTableStyle
  * DataGridTextBox
  * DataGridTextBoxColumn
  * GridColumnStylesCollection
  * GridTablesFactory
  * GridTableStylesCollection
  * IDataGridEditingService
  * IMenuEditorService
  * MainMenu
  * Menu
  * Menu.MenuItemCollection
  * MenuItem
  * ToolBar
  * ToolBarAppearance
  * ToolBarButton
  * ToolBar.ToolBarButtonCollection
  * ToolBarButtonClickEventArgs
  * ToolBarButtonStyle
  * ToolBarTextAlign
  * StatusBar
  * StatusBarDrawItemEventArgs
  * StatusBarDrawItemEventHandler
  * StatusBarPanel
  * StatusBarPanelAutoSize
  * StatusBarPanelBorderStyle
  * StatusBarPanelClickEventArgs
  * StatusBarPanelClickEventHandler
  * StatusBarPanelStyle

It's suggested you upgrade your code to replace legacy controls, but if case your projects include many Windows Forms containing deprecated controls you can use the controls with minimal code changes.


Since it is impossible to use extension properties in .NET, extension methods should be used instead of properties:

Control type:

  * ContextMenu ContextMenu { get; set; } replace to GetContextMenu(), SetContextMenu(ContextMenu value) methods

Form type:

  * MainMenu Menu { get; set; } replace to GetMenu(), SetMenu(MainMenu value) methods
  * MainMenu MergedMenu { get; } replace to GetMergedMenu(MainMenu value) methods
