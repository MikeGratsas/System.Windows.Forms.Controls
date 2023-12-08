// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    ///  Represents the table drawn by the <see cref='Forms.DataGrid'/> control at run time.
    /// </summary>
    [
    ToolboxItem(false),
    DesignTimeVisible(false),
    //DefaultProperty("GridTableName")
    ]
    public class DataGridTableStyle : Component, IDataGridEditingService
    {
        // internal for DataGridColumn accessibility...
        //
        internal DataGrid dataGrid = null;

        // relationship UI
        private int relationshipHeight = 0;
        internal const int relationshipSpacing = 1;
        private Rectangle relationshipRect = Rectangle.Empty;
        private int focusedRelation = -1;
        private int focusedTextWidth;

        // will contain a list of relationships that this table has
        private readonly ArrayList relationsList = new ArrayList(2);

        // the name of the table
        private string mappingName = string.Empty;
        private readonly GridColumnStylesCollection gridColumns = null;
        private bool readOnly = false;
        private readonly bool isDefaultTableStyle = false;

        private static readonly object EventAllowSorting = new object();
        private static readonly object EventGridLineColor = new object();
        private static readonly object EventGridLineStyle = new object();
        private static readonly object EventHeaderBackColor = new object();
        private static readonly object EventHeaderForeColor = new object();
        private static readonly object EventHeaderFont = new object();
        private static readonly object EventLinkColor = new object();
        private static readonly object EventLinkHoverColor = new object();
        private static readonly object EventPreferredColumnWidth = new object();
        private static readonly object EventPreferredRowHeight = new object();
        private static readonly object EventColumnHeadersVisible = new object();
        private static readonly object EventRowHeaderWidth = new object();
        private static readonly object EventSelectionBackColor = new object();
        private static readonly object EventSelectionForeColor = new object();
        private static readonly object EventMappingName = new object();
        private static readonly object EventAlternatingBackColor = new object();
        private static readonly object EventBackColor = new object();
        private static readonly object EventForeColor = new object();
        private static readonly object EventReadOnly = new object();
        private static readonly object EventRowHeadersVisible = new object();

        // add a bunch of properties, taken from the dataGrid
        //

        // default values
        //
        private const bool defaultAllowSorting = true;
        private const DataGridLineStyle defaultGridLineStyle = DataGridLineStyle.Solid;
        private const int defaultPreferredColumnWidth = 75;
        private const int defaultRowHeaderWidth = 35;
        internal static readonly Font defaultFont = Control.DefaultFont;
        internal static readonly int defaultFontHeight = defaultFont.Height;

        // the actual place holders for properties
        //
        private bool allowSorting = defaultAllowSorting;
        private SolidBrush alternatingBackBrush = DefaultAlternatingBackBrush;
        private SolidBrush backBrush = DefaultBackBrush;
        private SolidBrush foreBrush = DefaultForeBrush;
        private SolidBrush gridLineBrush = DefaultGridLineBrush;
        private DataGridLineStyle gridLineStyle = defaultGridLineStyle;
        internal SolidBrush headerBackBrush = DefaultHeaderBackBrush;
        internal Font headerFont = null; // this is ambient property to Font value.
        internal SolidBrush headerForeBrush = DefaultHeaderForeBrush;
        internal Pen headerForePen = DefaultHeaderForePen;
        private SolidBrush linkBrush = DefaultLinkBrush;
        internal int preferredColumnWidth = defaultPreferredColumnWidth;
        private int preferredRowHeight = defaultFontHeight + 3;
        private SolidBrush selectionBackBrush = DefaultSelectionBackBrush;
        private SolidBrush selectionForeBrush = DefaultSelectionForeBrush;
        private int rowHeaderWidth = defaultRowHeaderWidth;
        private bool rowHeadersVisible = true;
        private bool columnHeadersVisible = true;

        // the dataGrid would need to know when the ColumnHeaderVisible, RowHeadersVisible, RowHeaderWidth
        // and preferredColumnWidth, preferredRowHeight properties are changed in the current dataGridTableStyle
        // also: for GridLineStyle, GridLineColor, ForeColor, BackColor, HeaderBackColor, HeaderFont, HeaderForeColor
        // LinkColor, LinkHoverColor
        //

        /// <summary>Indicates whether sorting is allowed on the grid table when this <see cref="T:System.Windows.Forms.DataGridTableStyle" /> is used.</summary>
        /// <returns>
        ///   <see langword="true" /> if sorting is allowed; otherwise, <see langword="false" />. The default is <see langword="true" />.
        /// </returns>
        [
        SRCategory(nameof(SR.CatBehavior)),
        DefaultValue(defaultAllowSorting),
        SRDescription(nameof(SR.DataGridAllowSortingDescr))
        ]
        public bool AllowSorting
        {
            get
            {
                return allowSorting;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(AllowSorting)));
                }

                if (allowSorting != value)
                {
                    allowSorting = value;
                    OnAllowSortingChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///  [To be  supplied]
        /// </summary>
        public event EventHandler AllowSortingChanged
        {
            add => Events.AddHandler(EventAllowSorting, value);
            remove => Events.RemoveHandler(EventAllowSorting, value);
        }

        /// <summary>Gets or sets the background color of odd-numbered rows of the grid.</summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the background color of odd-numbered rows. The default is <see cref="P:System.Drawing.SystemBrushes.Window" /></returns>
        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridAlternatingBackColorDescr))
        ]
        public Color AlternatingBackColor
        {
            get
            {
                return alternatingBackBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(AlternatingBackColor)));
                }

                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(SR.DataGridTableStyleTransparentAlternatingBackColorNotAllowed, nameof(value));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(AlternatingBackColor)), nameof(value));
                }
                if (!alternatingBackBrush.Color.Equals(value))
                {
                    alternatingBackBrush = new SolidBrush(value);
                    InvalidateInside();
                    OnAlternatingBackColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.AlternatingBackColor" /> value changes.</summary>
        public event EventHandler AlternatingBackColorChanged
        {
            add => Events.AddHandler(EventAlternatingBackColor, value);
            remove => Events.RemoveHandler(EventAlternatingBackColor, value);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.AlternatingBackColor" /> property to its default value.</summary>
        public void ResetAlternatingBackColor()
        {
            if (ShouldSerializeAlternatingBackColor())
            {
                AlternatingBackColor = DefaultAlternatingBackBrush.Color;
                InvalidateInside();
            }
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.AlternatingBackColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool ShouldSerializeAlternatingBackColor()
        {
            return !AlternatingBackBrush.Equals(DefaultAlternatingBackBrush);
        }

        internal SolidBrush AlternatingBackBrush
        {
            get
            {
                return alternatingBackBrush;
            }
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.BackColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected bool ShouldSerializeBackColor()
        {
            return !System.Windows.Forms.DataGridTableStyle.DefaultBackBrush.Equals(backBrush);
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.ForeColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected bool ShouldSerializeForeColor()
        {
            return !System.Windows.Forms.DataGridTableStyle.DefaultForeBrush.Equals(foreBrush);
        }

        internal SolidBrush BackBrush
        {
            get
            {
                return backBrush;
            }
        }

        /// <summary>Gets or sets the background color of even-numbered rows of the grid.</summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the background color of odd-numbered rows.</returns>
        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.ControlBackColorDescr))
        ]
        public Color BackColor
        {
            get
            {
                return backBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(BackColor)));
                }

                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(SR.DataGridTableStyleTransparentBackColorNotAllowed, nameof(value));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(BackColor)), nameof(value));
                }
                if (!backBrush.Color.Equals(value))
                {
                    backBrush = new SolidBrush(value);
                    InvalidateInside();
                    OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.BackColor" /> value changes.</summary>
        public event EventHandler BackColorChanged
        {
            add => Events.AddHandler(EventBackColor, value);
            remove => Events.RemoveHandler(EventBackColor, value);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.BackColor" /> property to its default value.</summary>
        public void ResetBackColor()
        {
            if (!backBrush.Equals(DefaultBackBrush))
            {
                BackColor = DefaultBackBrush.Color;
            }
        }

        internal int BorderWidth
        {
            get
            {
                DataGrid dataGrid = DataGrid;
                if (dataGrid == null)
                {
                    return 0;
                }
                // if the user set the GridLineStyle property on the dataGrid.
                // then use the value of that property
                DataGridLineStyle gridStyle;
                int gridLineWidth;
                if (IsDefault)
                {
                    gridStyle = DataGrid.GridLineStyle;
                    gridLineWidth = DataGrid.GridLineWidth;
                }
                else
                {
                    gridStyle = GridLineStyle;
                    gridLineWidth = GridLineWidth;
                }

                if (gridStyle == DataGridLineStyle.None)
                {
                    return 0;
                }

                return gridLineWidth;
            }
        }

        internal static SolidBrush DefaultAlternatingBackBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.Window;
            }
        }
        internal static SolidBrush DefaultBackBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.Window;
            }
        }
        internal static SolidBrush DefaultForeBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.WindowText;
            }
        }
        private static SolidBrush DefaultGridLineBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.Control;
            }
        }
        private static SolidBrush DefaultHeaderBackBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.Control;
            }
        }
        private static SolidBrush DefaultHeaderForeBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.ControlText;
            }
        }
        private static Pen DefaultHeaderForePen
        {
            get
            {
                return new Pen(SystemColors.ControlText);
            }
        }
        private static SolidBrush DefaultLinkBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.HotTrack;
            }
        }
        private static SolidBrush DefaultSelectionBackBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.ActiveCaption;
            }
        }
        private static SolidBrush DefaultSelectionForeBrush
        {
            get
            {
                return (SolidBrush)SystemBrushes.ActiveCaptionText;
            }
        }

        internal int FocusedRelation
        {
            get
            {
                return focusedRelation;
            }
            set
            {
                if (focusedRelation != value)
                {
                    focusedRelation = value;
                    if (focusedRelation == -1)
                    {
                        focusedTextWidth = 0;
                    }
                    else
                    {
                        Graphics g = DataGrid.CreateGraphics();
                        focusedTextWidth = (int)Math.Ceiling(g.MeasureString(((string)RelationsList[focusedRelation]), DataGrid.LinkFont).Width);
                        g.Dispose();
                    }
                }
            }
        }

        internal int FocusedTextWidth
        {
            get
            {
                return focusedTextWidth;
            }
        }

        /// <summary>Gets or sets the foreground color of the grid table.</summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the foreground color of the grid table.</returns>
        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.ControlForeColorDescr))
        ]
        public Color ForeColor
        {
            get
            {
                return foreBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(ForeColor)));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(ForeColor)), nameof(value));
                }
                if (!foreBrush.Color.Equals(value))
                {
                    foreBrush = new SolidBrush(value);
                    InvalidateInside();
                    OnForeColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.ForeColor" /> value changes.</summary>
        public event EventHandler ForeColorChanged
        {
            add => Events.AddHandler(EventForeColor, value);
            remove => Events.RemoveHandler(EventForeColor, value);
        }

        internal SolidBrush ForeBrush
        {
            get
            {
                return foreBrush;
            }
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.ForeColor" /> property to its default value.</summary>
        public void ResetForeColor()
        {
            if (!foreBrush.Equals(DefaultForeBrush))
            {
                ForeColor = DefaultForeBrush.Color;
            }
        }

        /// <summary>Gets or sets the color of grid lines.</summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the grid line color.</returns>
        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridGridLineColorDescr))
        ]
        public Color GridLineColor
        {
            get
            {
                return gridLineBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(GridLineColor)));
                }

                if (gridLineBrush.Color != value)
                {
                    if (value.IsEmpty)
                    {
                        throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(GridLineColor)), nameof(value));
                    }

                    gridLineBrush = new SolidBrush(value);
                    OnGridLineColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.GridLineColor" /> value changes.</summary>
        public event EventHandler GridLineColorChanged
        {
            add => Events.AddHandler(EventGridLineColor, value);
            remove => Events.RemoveHandler(EventGridLineColor, value);
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.GridLineColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool ShouldSerializeGridLineColor()
        {
            return !GridLineBrush.Equals(DefaultGridLineBrush);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.GridLineColor" /> property to its default value.</summary>
        public void ResetGridLineColor()
        {
            if (ShouldSerializeGridLineColor())
            {
                GridLineColor = DefaultGridLineBrush.Color;
            }
        }

        internal SolidBrush GridLineBrush
        {
            get
            {
                return gridLineBrush;
            }
        }

        internal int GridLineWidth
        {
            get
            {
                Debug.Assert(GridLineStyle == DataGridLineStyle.Solid || GridLineStyle == DataGridLineStyle.None, "are there any other styles?");
                return GridLineStyle == DataGridLineStyle.Solid ? 1 : 0;
            }
        }

        /// <summary>Gets or sets the style of grid lines.</summary>
        /// <returns>One of the <see cref="T:System.Windows.Forms.DataGridLineStyle" /> values. The default is <see langword="DataGridLineStyle.Solid" />.</returns>
        [
         SRCategory(nameof(SR.CatAppearance)),
         DefaultValue(defaultGridLineStyle),
         SRDescription(nameof(SR.DataGridGridLineStyleDescr))
        ]
        public DataGridLineStyle GridLineStyle
        {
            get
            {
                return gridLineStyle;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(GridLineStyle)));
                }

                //valid values are 0x0 to 0x1.
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)DataGridLineStyle.None, (int)DataGridLineStyle.Solid))
                {
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DataGridLineStyle));
                }
                if (gridLineStyle != value)
                {
                    gridLineStyle = value;
                    OnGridLineStyleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.GridLineStyle" /> value changes.</summary>
        public event EventHandler GridLineStyleChanged
        {
            add => Events.AddHandler(EventGridLineStyle, value);
            remove => Events.RemoveHandler(EventGridLineStyle, value);
        }

        /// <summary>Gets or sets the background color of headers.</summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the background color of headers.</returns>
        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridHeaderBackColorDescr))
        ]
        public Color HeaderBackColor
        {
            get
            {
                return headerBackBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(HeaderBackColor)));
                }

                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(SR.DataGridTableStyleTransparentHeaderBackColorNotAllowed, nameof(value));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(HeaderBackColor)), nameof(value));
                }

                if (!value.Equals(headerBackBrush.Color))
                {
                    headerBackBrush = new SolidBrush(value);
                    OnHeaderBackColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.HeaderBackColor" /> value changes.</summary>
        public event EventHandler HeaderBackColorChanged
        {
            add => Events.AddHandler(EventHeaderBackColor, value);
            remove => Events.RemoveHandler(EventHeaderBackColor, value);
        }

        internal SolidBrush HeaderBackBrush
        {
            get
            {
                return headerBackBrush;
            }
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.HeaderForeColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool ShouldSerializeHeaderBackColor()
        {
            return !HeaderBackBrush.Equals(DefaultHeaderBackBrush);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.HeaderBackColor" /> property to its default value.</summary>
        public void ResetHeaderBackColor()
        {
            if (ShouldSerializeHeaderBackColor())
            {
                HeaderBackColor = DefaultHeaderBackBrush.Color;
            }
        }

        /// <summary>Gets or sets the font used for header captions.</summary>
        /// <returns>The <see cref="T:System.Drawing.Font" /> used for captions.</returns>
        [
         SRCategory(nameof(SR.CatAppearance)),
         Localizable(true),
         AmbientValue(null),
         SRDescription(nameof(SR.DataGridHeaderFontDescr))
        ]
        public Font HeaderFont
        {
            get
            {
                return (headerFont ?? (DataGrid == null ? Control.DefaultFont : DataGrid.Font));
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(HeaderFont)));
                }

                if (value == null && headerFont != null || (value != null && !value.Equals(headerFont)))
                {
                    headerFont = value;
                    OnHeaderFontChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.HeaderFont" /> value changes.</summary>
        public event EventHandler HeaderFontChanged
        {
            add => Events.AddHandler(EventHeaderFont, value);
            remove => Events.RemoveHandler(EventHeaderFont, value);
        }

        private bool ShouldSerializeHeaderFont()
        {
            return (headerFont != null);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.HeaderFont" /> property to its default value.</summary>
        public void ResetHeaderFont()
        {
            if (headerFont != null)
            {
                headerFont = null;
                OnHeaderFontChanged(EventArgs.Empty);
            }
        }

        /// <summary>Gets or sets the foreground color of headers.</summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the foreground color of headers.</returns>
        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridHeaderForeColorDescr))
        ]
        public Color HeaderForeColor
        {
            get
            {
                return headerForePen.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(HeaderForeColor)));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(HeaderForeColor)), nameof(value));
                }

                if (!value.Equals(headerForePen.Color))
                {
                    headerForePen = new Pen(value);
                    headerForeBrush = new SolidBrush(value);
                    OnHeaderForeColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.HeaderForeColor" /> value changes.</summary>
        public event EventHandler HeaderForeColorChanged
        {
            add => Events.AddHandler(EventHeaderForeColor, value);
            remove => Events.RemoveHandler(EventHeaderForeColor, value);
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.HeaderForeColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool ShouldSerializeHeaderForeColor()
        {
            return !HeaderForePen.Equals(DefaultHeaderForePen);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.HeaderForeColor" /> property to its default value.</summary>
        public void ResetHeaderForeColor()
        {
            if (ShouldSerializeHeaderForeColor())
            {
                HeaderForeColor = DefaultHeaderForeBrush.Color;
            }
        }

        internal SolidBrush HeaderForeBrush
        {
            get
            {
                return headerForeBrush;
            }
        }

        internal Pen HeaderForePen
        {
            get
            {
                return headerForePen;
            }
        }

        /// <summary>Gets or sets the color of link text.</summary>
        /// <returns>The <see cref="T:System.Drawing.Color" /> of link text.</returns>
        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridLinkColorDescr))
        ]
        public Color LinkColor
        {
            get
            {
                return linkBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(LinkColor)));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(LinkColor)), nameof(value));
                }

                if (!linkBrush.Color.Equals(value))
                {
                    linkBrush = new SolidBrush(value);
                    OnLinkColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.LinkColor" /> value changes.</summary>
        public event EventHandler LinkColorChanged
        {
            add => Events.AddHandler(EventLinkColor, value);
            remove => Events.RemoveHandler(EventLinkColor, value);
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.LinkColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool ShouldSerializeLinkColor()
        {
            return !LinkBrush.Equals(DefaultLinkBrush);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.LinkColor" /> property to its default value.</summary>
        public void ResetLinkColor()
        {
            if (ShouldSerializeLinkColor())
            {
                LinkColor = DefaultLinkBrush.Color;
            }
        }

        internal Brush LinkBrush
        {
            get
            {
                return linkBrush;
            }
        }

        /// <summary>Gets or sets the color displayed when hovering over link text.</summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the hover color.</returns>
        [
         SRDescription(nameof(SR.DataGridLinkHoverColorDescr)),
         SRCategory(nameof(SR.CatColors)),
         Browsable(false),
         EditorBrowsable(EditorBrowsableState.Never)
        ]
        public Color LinkHoverColor
        {
            get
            {
                return LinkColor;
            }
            set
            {
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.LinkHoverColor" /> value changes.</summary>
        public event EventHandler LinkHoverColorChanged
        {
            add => Events.AddHandler(EventLinkHoverColor, value);
            remove => Events.RemoveHandler(EventLinkHoverColor, value);
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.LinkHoverColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool ShouldSerializeLinkHoverColor()
        {
            return false;
        }

        internal Rectangle RelationshipRect
        {
            get
            {
                if (relationshipRect.IsEmpty)
                {
                    ComputeRelationshipRect();
                }
                return relationshipRect;
            }
        }

        private Rectangle ComputeRelationshipRect()
        {
            if (relationshipRect.IsEmpty && DataGrid.AllowNavigation)
            {
                Debug.WriteLineIf(CompModSwitches.DGRelationShpRowLayout.TraceVerbose, "GetRelationshipRect grinding away");
                Graphics g = DataGrid.CreateGraphics();
                relationshipRect = new Rectangle
                {
                    X = 0 //indentWidth;
                };
                // relationshipRect.Y = base.Height - BorderWidth;

                // Determine the width of the widest relationship name
                int longestRelationship = 0;
                for (int r = 0; r < RelationsList.Count; ++r)
                {
                    int rwidth = (int)Math.Ceiling(g.MeasureString(((string)RelationsList[r]), DataGrid.LinkFont).Width)
;
                    if (rwidth > longestRelationship)
                    {
                        longestRelationship = rwidth;
                    }
                }

                g.Dispose();

                relationshipRect.Width = longestRelationship + 5;
                relationshipRect.Width += 2; // relationshipRect border;
                relationshipRect.Height = BorderWidth + relationshipHeight * RelationsList.Count;
                relationshipRect.Height += 2; // relationship border
                if (RelationsList.Count > 0)
                {
                    relationshipRect.Height += 2 * relationshipSpacing;
                }
            }
            return relationshipRect;
        }

        internal void ResetRelationsUI()
        {
            relationshipRect = Rectangle.Empty;
            focusedRelation = -1;
            relationshipHeight = dataGrid.LinkFontHeight + relationshipSpacing;
        }

        internal int RelationshipHeight
        {
            get
            {
                return relationshipHeight;
            }
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.LinkHoverColor" /> property to its default value.</summary>
        public void ResetLinkHoverColor()
        {
        }

        /// <summary>Gets or sets the width used to create columns when a new grid is displayed.</summary>
        /// <returns>The width used to create columns when a new grid is displayed.</returns>
        [
         DefaultValue(defaultPreferredColumnWidth),
         SRCategory(nameof(SR.CatLayout)),
         Localizable(true),
         SRDescription(nameof(SR.DataGridPreferredColumnWidthDescr)),
         TypeConverter(typeof(DataGridPreferredColumnWidthTypeConverter))
        ]
        public int PreferredColumnWidth
        {
            get
            {
                return preferredColumnWidth;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(PreferredColumnWidth)));
                }

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, SR.DataGridColumnWidth);
                }

                if (preferredColumnWidth != value)
                {
                    preferredColumnWidth = value;
                    OnPreferredColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.PreferredColumnWidth" /> property value changes.</summary>
        public event EventHandler PreferredColumnWidthChanged
        {
            add => Events.AddHandler(EventPreferredColumnWidth, value);
            remove => Events.RemoveHandler(EventPreferredColumnWidth, value);
        }

        /// <summary>Gets or sets the height used to create a row when a new grid is displayed.</summary>
        /// <returns>The height of a row, in pixels.</returns>
        [
         SRCategory(nameof(SR.CatLayout)),
         Localizable(true),
         SRDescription(nameof(SR.DataGridPreferredRowHeightDescr))
        ]
        public int PreferredRowHeight
        {
            get
            {
                return preferredRowHeight;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(PreferredRowHeight)));
                }

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, SR.DataGridRowRowHeight);
                }

                if (preferredRowHeight != value)
                {
                    preferredRowHeight = value;
                    OnPreferredRowHeightChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.PreferredRowHeight" /> value changes.</summary>
        public event EventHandler PreferredRowHeightChanged
        {
            add => Events.AddHandler(EventPreferredRowHeight, value);
            remove => Events.RemoveHandler(EventPreferredRowHeight, value);
        }

        private void ResetPreferredRowHeight()
        {
            PreferredRowHeight = defaultFontHeight + 3;
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.PreferredRowHeight" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected bool ShouldSerializePreferredRowHeight()
        {
            return preferredRowHeight != defaultFontHeight + 3;
        }

        /// <summary>Gets or sets a value indicating whether column headers are visible.</summary>
        /// <returns>
        ///   <see langword="true" /> if column headers are visible; otherwise, <see langword="false" />. The default is <see langword="true" />.
        /// </returns>
        [
         SRCategory(nameof(SR.CatDisplay)),
         DefaultValue(true),
         SRDescription(nameof(SR.DataGridColumnHeadersVisibleDescr))
        ]
        public bool ColumnHeadersVisible
        {
            get
            {
                return columnHeadersVisible;
            }
            set
            {
                if (columnHeadersVisible != value)
                {
                    columnHeadersVisible = value;
                    OnColumnHeadersVisibleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.ColumnHeadersVisible" /> value changes.</summary>
        public event EventHandler ColumnHeadersVisibleChanged
        {
            add => Events.AddHandler(EventColumnHeadersVisible, value);
            remove => Events.RemoveHandler(EventColumnHeadersVisible, value);
        }

        /// <summary>Gets or sets a value indicating whether row headers are visible.</summary>
        /// <returns>
        ///   <see langword="true" /> if row headers are visible; otherwise, <see langword="false" />. The default is <see langword="true" />.
        /// </returns>
        [
         SRCategory(nameof(SR.CatDisplay)),
         DefaultValue(true),
         SRDescription(nameof(SR.DataGridRowHeadersVisibleDescr))
        ]
        public bool RowHeadersVisible
        {
            get
            {
                return rowHeadersVisible;
            }
            set
            {
                if (rowHeadersVisible != value)
                {
                    rowHeadersVisible = value;
                    OnRowHeadersVisibleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.RowHeadersVisible" /> value changes.</summary>
        public event EventHandler RowHeadersVisibleChanged
        {
            add => Events.AddHandler(EventRowHeadersVisible, value);
            remove => Events.RemoveHandler(EventRowHeadersVisible, value);
        }

        /// <summary>Gets or sets the width of row headers.</summary>
        /// <returns>The width of row headers, in pixels.</returns>
        [
         SRCategory(nameof(SR.CatLayout)),
         DefaultValue(defaultRowHeaderWidth),
         Localizable(true),
         SRDescription(nameof(SR.DataGridRowHeaderWidthDescr))
        ]
        public int RowHeaderWidth
        {
            get
            {
                return rowHeaderWidth;
            }
            set
            {
                if (DataGrid != null)
                {
                    value = Math.Max(DataGrid.MinimumRowHeaderWidth(), value);
                }

                if (rowHeaderWidth != value)
                {
                    rowHeaderWidth = value;
                    OnRowHeaderWidthChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.RowHeaderWidth" /> value changes.</summary>
        public event EventHandler RowHeaderWidthChanged
        {
            add => Events.AddHandler(EventRowHeaderWidth, value);
            remove => Events.RemoveHandler(EventRowHeaderWidth, value);
        }

        /// <summary>Gets or sets the background color of selected cells.</summary>
        /// <returns>The <see cref="T:System.Drawing.Color" /> that represents the background color of selected cells.</returns>
        [
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridSelectionBackColorDescr))
        ]
        public Color SelectionBackColor
        {
            get
            {
                return selectionBackBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(SelectionBackColor)));
                }

                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(SR.DataGridTableStyleTransparentSelectionBackColorNotAllowed, nameof(value));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(SelectionBackColor)), nameof(value));
                }

                if (!value.Equals(selectionBackBrush.Color))
                {
                    selectionBackBrush = new SolidBrush(value);
                    InvalidateInside();
                    OnSelectionBackColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.SelectionBackColor" /> value changes.</summary>
        public event EventHandler SelectionBackColorChanged
        {
            add => Events.AddHandler(EventSelectionBackColor, value);
            remove => Events.RemoveHandler(EventSelectionBackColor, value);
        }

        internal SolidBrush SelectionBackBrush
        {
            get
            {
                return selectionBackBrush;
            }
        }

        internal SolidBrush SelectionForeBrush
        {
            get
            {
                return selectionForeBrush;
            }
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.SelectionBackColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected bool ShouldSerializeSelectionBackColor()
        {
            return !DefaultSelectionBackBrush.Equals(selectionBackBrush);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.SelectionBackColor" /> property to its default value.</summary>
        public void ResetSelectionBackColor()
        {
            if (ShouldSerializeSelectionBackColor())
            {
                SelectionBackColor = DefaultSelectionBackBrush.Color;
            }
        }

        /// <summary>Gets or sets the foreground color of selected cells.</summary>
        /// <returns>The <see cref="T:System.Drawing.Color" /> that represents the foreground color of selected cells.</returns>
        [
         Description("The foreground color for the current data grid row"),
         SRCategory(nameof(SR.CatColors)),
         SRDescription(nameof(SR.DataGridSelectionForeColorDescr))
        ]
        public Color SelectionForeColor
        {
            get
            {
                return selectionForeBrush.Color;
            }
            set
            {
                if (isDefaultTableStyle)
                {
                    throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(SelectionForeColor)));
                }

                if (value.IsEmpty)
                {
                    throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(SelectionForeColor)), nameof(value));
                }

                if (!value.Equals(selectionForeBrush.Color))
                {
                    selectionForeBrush = new SolidBrush(value);
                    InvalidateInside();
                    OnSelectionForeColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.SelectionForeColor" /> value changes.</summary>
        public event EventHandler SelectionForeColorChanged
        {
            add => Events.AddHandler(EventSelectionForeColor, value);
            remove => Events.RemoveHandler(EventSelectionForeColor, value);
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Forms.DataGridTableStyle.SelectionForeColor" /> property should be persisted.</summary>
        /// <returns>
        ///   <see langword="true" /> if the property value has changed from its default; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool ShouldSerializeSelectionForeColor()
        {
            return !SelectionForeBrush.Equals(DefaultSelectionForeBrush);
        }

        /// <summary>Resets the <see cref="P:System.Windows.Forms.DataGridTableStyle.SelectionForeColor" /> property to its default value.</summary>
        public void ResetSelectionForeColor()
        {
            if (ShouldSerializeSelectionForeColor())
            {
                SelectionForeColor = DefaultSelectionForeBrush.Color;
            }
        }

        // will need this function from the dataGrid
        //
        private void InvalidateInside()
        {
            if (DataGrid != null)
            {
                DataGrid.InvalidateInside();
            }
        }

        /// <summary>Gets the default table style.</summary>
        public static readonly DataGridTableStyle DefaultTableStyle = new DataGridTableStyle(true);

        /// <summary>
        ///  Initializes a new instance of the <see cref='DataGridTableStyle'/> class.
        /// </summary>
        public DataGridTableStyle(bool isDefaultTableStyle)
        {
            gridColumns = new GridColumnStylesCollection(this, isDefaultTableStyle);
            gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
            this.isDefaultTableStyle = isDefaultTableStyle;
        }

        /// <summary>Initializes a new instance of the <see cref='DataGridTableStyle' /> class.</summary>
        public DataGridTableStyle() : this(false)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref='DataGridTableStyle'/> class with the specified
        /// <see cref='CurrencyManager'/>.
        /// </summary>
        public DataGridTableStyle(CurrencyManager listManager) : this()
        {
            Debug.Assert(listManager != null, "the DataGridTabel cannot use a null listManager");
            mappingName = listManager.GetListName();
            // set up the Relations and the columns
            SetGridColumnStylesCollection(listManager);
        }

        internal void SetRelationsList(CurrencyManager listManager)
        {
            PropertyDescriptorCollection propCollection = listManager.GetItemProperties();
            Debug.Assert(!IsDefault, "the grid can set the relations only on a table that was manually added by the user");
            int propCount = propCollection.Count;
            if (relationsList.Count > 0)
            {
                relationsList.Clear();
            }

            for (int i = 0; i < propCount; i++)
            {
                PropertyDescriptor prop = propCollection[i];
                Debug.Assert(prop != null, "prop is null: how that happened?");
                if (PropertyDescriptorIsARelation(prop))
                {
                    // relation
                    relationsList.Add(prop.Name);
                }
            }
        }

        internal void SetGridColumnStylesCollection(CurrencyManager listManager)
        {
            // when we are setting the gridColumnStyles, do not handle any gridColumnCollectionChanged events
            gridColumns.CollectionChanged -= new CollectionChangeEventHandler(OnColumnCollectionChanged);

            PropertyDescriptorCollection propCollection = listManager.GetItemProperties();

            // we need to clear the relations list
            if (relationsList.Count > 0)
            {
                relationsList.Clear();
            }

            Debug.Assert(propCollection != null, "propCollection is null: how that happened?");
            int propCount = propCollection.Count;
            for (int i = 0; i < propCount; i++)
            {
                PropertyDescriptor prop = propCollection[i];
                Debug.Assert(prop != null, "prop is null: how that happened?");
                // do not take into account the properties that are browsable.
                if (!prop.IsBrowsable)
                {
                    continue;
                }

                if (PropertyDescriptorIsARelation(prop))
                {
                    // relation
                    relationsList.Add(prop.Name);
                }
                else
                {
                    // column
                    DataGridColumnStyle col = CreateGridColumn(prop, isDefaultTableStyle);
                    if (isDefaultTableStyle)
                    {
                        gridColumns.AddDefaultColumn(col);
                    }
                    else
                    {
                        col.MappingName = prop.Name;
                        col.HeaderText = prop.Name;
                        gridColumns.Add(col);
                    }
                }
            }

            // now we are able to handle the collectionChangeEvents
            gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
        }

        private static bool PropertyDescriptorIsARelation(PropertyDescriptor prop)
        {
            return typeof(IList).IsAssignableFrom(prop.PropertyType) && !typeof(Array).IsAssignableFrom(prop.PropertyType);
        }

        /// <summary>Creates a <see cref="T:System.Windows.Forms.DataGridColumnStyle" />, using the specified property descriptor.</summary>
        /// <param name="prop">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> used to create the column style object.</param>
        /// <returns>The newly created <see cref="T:System.Windows.Forms.DataGridColumnStyle" />.</returns>
        internal protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop)
        {
            return CreateGridColumn(prop, false);
        }

        /// <summary>Creates a <see cref="T:System.Windows.Forms.DataGridColumnStyle" /> using the specified property descriptor. Specifies whether the <see cref="T:System.Windows.Forms.DataGridColumnStyle" /> is a default column style.</summary>
        /// <param name="prop">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> used to create the column style object.</param>
        /// <param name="isDefault">Specifies whether the <see cref="T:System.Windows.Forms.DataGridColumnStyle" /> is a default column style. This parameter is read-only.</param>
        /// <returns>The newly created <see cref="T:System.Windows.Forms.DataGridColumnStyle" />.</returns>
        internal protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop, bool isDefault)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop));
            }

            DataGridColumnStyle ret = null;
            Type dataType = prop.PropertyType;

            if (dataType.Equals(typeof(bool)))
            {
                ret = new DataGridBoolColumn(prop, isDefault);
            }
            else if (dataType.Equals(typeof(string)))
            {
                ret = new DataGridTextBoxColumn(prop, isDefault);
            }
            else if (dataType.Equals(typeof(DateTime)))
            {
                ret = new DataGridTextBoxColumn(prop, "d", isDefault);
            }
            else if (dataType.Equals(typeof(short)) ||
                     dataType.Equals(typeof(int)) ||
                     dataType.Equals(typeof(long)) ||
                     dataType.Equals(typeof(ushort)) ||
                     dataType.Equals(typeof(uint)) ||
                     dataType.Equals(typeof(ulong)) ||
                     dataType.Equals(typeof(decimal)) ||
                     dataType.Equals(typeof(double)) ||
                     dataType.Equals(typeof(float)) ||
                     dataType.Equals(typeof(byte)) ||
                     dataType.Equals(typeof(sbyte)))
            {
                ret = new DataGridTextBoxColumn(prop, "G", isDefault);
            }
            else
            {
                ret = new DataGridTextBoxColumn(prop, isDefault);
            }
            return ret;
        }

        internal void ResetRelationsList()
        {
            if (isDefaultTableStyle)
            {
                relationsList.Clear();
            }
        }

        // =------------------------------------------------------------------
        // =        Properties
        // =------------------------------------------------------------------

        /// <summary>
        ///  Gets the name of this grid table.
        /// </summary>
        [Editor("System.Windows.Forms.Design.DataGridTableStyleMappingNameEditor, " + AssemblyRef.SystemDesign, typeof(Drawing.Design.UITypeEditor)), DefaultValue("")]
        public string MappingName
        {
            get
            {
                return mappingName;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                if (value.Equals(mappingName))
                {
                    return;
                }

                string originalMappingName = MappingName;
                mappingName = value;

                // this could throw
                try
                {
                    if (DataGrid != null)
                    {
                        DataGrid.TableStyles.CheckForMappingNameDuplicates(this);
                    }
                }
                catch
                {
                    mappingName = originalMappingName;
                    throw;
                }
                OnMappingNameChanged(EventArgs.Empty);
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.MappingName" /> value changes.</summary>
        public event EventHandler MappingNameChanged
        {
            add => Events.AddHandler(EventMappingName, value);
            remove => Events.RemoveHandler(EventMappingName, value);
        }

        /// <summary>
        ///  Gets the
        ///  list of relation objects for the grid table.
        /// </summary>
        internal ArrayList RelationsList
        {
            get
            {
                return relationsList;
            }
        }

        /// <summary>
        ///  Gets the collection of columns drawn for this table.
        /// </summary>
        [
        Localizable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
        ]
        public virtual GridColumnStylesCollection GridColumnStyles
        {
            get
            {
                return gridColumns;
            }
        }

        /// <summary>
        ///  Gets or sets the <see cref='Forms.DataGrid'/>
        ///  control displaying the table.
        /// </summary>
        internal void SetInternalDataGrid(DataGrid dG, bool force)
        {
            if (dataGrid != null && dataGrid.Equals(dG) && !force)
            {
                return;
            }
            else
            {
                dataGrid = dG;
                if (dG != null && dG.Initializing)
                {
                    return;
                }

                int nCols = gridColumns.Count;
                for (int i = 0; i < nCols; i++)
                {
                    gridColumns[i].SetDataGridInternalInColumn(dG);
                }
            }
        }

        /// <summary>
        ///  Gets or sets the <see cref='Forms.DataGrid'/> control for the drawn table.
        /// </summary>
        [Browsable(false)]
        public virtual DataGrid DataGrid
        {
            get
            {
                return dataGrid;
            }
            set
            {
                SetInternalDataGrid(value, true);
            }
        }

        /// <summary>
        ///  Gets or sets a value indicating whether columns can be
        ///  edited.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool ReadOnly
        {
            get
            {
                return readOnly;
            }
            set
            {
                if (readOnly != value)
                {
                    readOnly = value;
                    OnReadOnlyChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>Occurs when the <see cref="P:System.Windows.Forms.DataGridTableStyle.ReadOnly" /> value changes.</summary>
        public event EventHandler ReadOnlyChanged
        {
            add => Events.AddHandler(EventReadOnly, value);
            remove => Events.RemoveHandler(EventReadOnly, value);
        }

        // =------------------------------------------------------------------
        // =        Methods
        // =------------------------------------------------------------------

        /// <summary>
        ///  Requests an edit operation.
        /// </summary>
        public bool BeginEdit(DataGridColumnStyle gridColumn, int rowNumber)
        {
            DataGrid grid = DataGrid;
            if (grid == null)
            {
                return false;
            }
            else
            {
                return grid.BeginEdit(gridColumn, rowNumber);
            }
        }

        /// <summary>
        ///  Requests an end to an edit
        ///  operation.
        /// </summary>
        public bool EndEdit(DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
        {
            DataGrid grid = DataGrid;
            if (grid == null)
            {
                return false;
            }
            else
            {
                return grid.EndEdit(gridColumn, rowNumber, shouldAbort);
            }
        }

        internal void InvalidateColumn(DataGridColumnStyle column)
        {
            int index = GridColumnStyles.IndexOf(column);
            if (index >= 0 && DataGrid != null)
            {
                DataGrid.InvalidateColumn(index);
            }
        }

        private void OnColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            gridColumns.CollectionChanged -= new CollectionChangeEventHandler(OnColumnCollectionChanged);

            try
            {
                DataGrid grid = DataGrid;
                DataGridColumnStyle col = e.Element as DataGridColumnStyle;
                if (e.Action == CollectionChangeAction.Add)
                {
                    if (col != null)
                    {
                        col.SetDataGridInternalInColumn(grid);
                    }
                }
                else if (e.Action == CollectionChangeAction.Remove)
                {
                    if (col != null)
                    {
                        col.SetDataGridInternalInColumn(null);
                    }
                }
                else
                {
                    // refresh
                    Debug.Assert(e.Action == CollectionChangeAction.Refresh, "there are only Add, Remove and Refresh in the CollectionChangeAction");
                    // if we get a column in this collectionChangeEventArgs it means
                    // that the propertyDescriptor in that column changed.
                    if (e.Element != null)
                    {
                        for (int i = 0; i < gridColumns.Count; i++)
                        {
                            gridColumns[i].SetDataGridInternalInColumn(null);
                        }
                    }
                }

                if (grid != null)
                {
                    grid.OnColumnCollectionChanged(this, e);
                }
            }
            finally
            {
                gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
            }
        }

#if false
        /// <summary>
        ///  The DataColumnCollection class actually wires up this
        ///  event handler to the PropertyChanged events of
        ///  a DataGridTable's columns.
        /// </summary>
        internal void OnColumnChanged(object sender, PropertyChangedEvent event) {
            if (event.PropertyName.Equals("Visible"))
                GenerateVisibleColumnsCache();
        }
#endif

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.ReadOnlyChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnReadOnlyChanged(EventArgs e)
        {
            if (Events[EventReadOnly] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.MappingNameChanged" /> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnMappingNameChanged(EventArgs e)
        {
            if (Events[EventMappingName] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.AlternatingBackColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnAlternatingBackColorChanged(EventArgs e)
        {
            if (Events[EventAlternatingBackColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.ForeColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnForeColorChanged(EventArgs e)
        {
            if (Events[EventForeColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.BackColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnBackColorChanged(EventArgs e)
        {
            if (Events[EventBackColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.AllowSortingChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnAllowSortingChanged(EventArgs e)
        {
            if (Events[EventAllowSorting] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.GridLineColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnGridLineColorChanged(EventArgs e)
        {
            if (Events[EventGridLineColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.GridLineStyleChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnGridLineStyleChanged(EventArgs e)
        {
            if (Events[EventGridLineStyle] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.HeaderBackColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnHeaderBackColorChanged(EventArgs e)
        {
            if (Events[EventHeaderBackColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.HeaderFontChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnHeaderFontChanged(EventArgs e)
        {
            if (Events[EventHeaderFont] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.HeaderForeColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnHeaderForeColorChanged(EventArgs e)
        {
            if (Events[EventHeaderForeColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.LinkColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnLinkColorChanged(EventArgs e)
        {
            if (Events[EventLinkColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see langword="LinkHoverColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnLinkHoverColorChanged(EventArgs e)
        {
            if (Events[EventLinkHoverColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.PreferredRowHeightChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnPreferredRowHeightChanged(EventArgs e)
        {
            if (Events[EventPreferredRowHeight] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.PreferredRowHeightChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnPreferredColumnWidthChanged(EventArgs e)
        {
            if (Events[EventPreferredColumnWidth] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.ColumnHeadersVisibleChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnColumnHeadersVisibleChanged(EventArgs e)
        {
            if (Events[EventColumnHeadersVisible] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.RowHeadersVisibleChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnRowHeadersVisibleChanged(EventArgs e)
        {
            if (Events[EventRowHeadersVisible] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.RowHeaderWidthChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnRowHeaderWidthChanged(EventArgs e)
        {
            if (Events[EventRowHeaderWidth] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.SelectionForeColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnSelectionForeColorChanged(EventArgs e)
        {
            if (Events[EventSelectionForeColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridTableStyle.SelectionBackColorChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected virtual void OnSelectionBackColorChanged(EventArgs e)
        {
            if (Events[EventSelectionBackColor] is EventHandler eh)
            {
                eh(this, e);
            }
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the <see cref="T:System.Windows.Forms.DataGridTableStyle" />.
        /// </summary>
        /// <param name="disposing">
        ///   <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GridColumnStylesCollection cols = GridColumnStyles;
                if (cols != null)
                {
                    for (int i = 0; i < cols.Count; i++)
                    {
                        cols[i].Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }

        internal bool IsDefault
        {
            get
            {
                return isDefaultTableStyle;
            }
        }

    }
}
