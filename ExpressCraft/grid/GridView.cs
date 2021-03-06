﻿using Bridge;
using static Retyped.dom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Retyped.es5;
using System.Text;

namespace ExpressCraft
{
    public class GridView : Control, IPrintable
    {
        public HTMLDivElement GridFindPanel;

        public HTMLDivElement GridHeader;
        public HTMLDivElement GridHeaderContainer;
        public HTMLDivElement GridBodyContainer;
        public HTMLDivElement GridBody;

        private HTMLDivElement BottonOfTable;
        private HTMLDivElement RightOfTable;
        private HTMLDivElement RightOfTableHeader;

        public TextInput SearchTextInput;
        public SimpleButton btnFind;
        public SimpleButton btnClear;
        public SimpleButton btnClose;


        private bool _useInRowEditor;

        public bool UseInRowEditor
        {
            get { return _useInRowEditor; }
            set {
                if(_useInRowEditor != value)
                {
                    CloseEditor();
                    _useInRowEditor = value;
                    if(_useInRowEditor)
                    {
                        UseEditForm = false;
                    }
                }                
            }
        }

        private ContextItem _showFindPanelContextItem;

        private bool _findPanelVisible;

        public bool FindPanelVisible
        {
            get { return _findPanelVisible; }
            set {
                if(_findPanelVisible != value)
                {
                    if(value)
                        ShowFindPanel();
                    else
                        CloseFindPanel();
                                        
                }
                
            }
        }

        public bool ResolveSearchDataIndex()
        {
            return (VisibleRowHandles != null && VisibleRowHandles.Count > 0);
        }

        private bool _highlighSearchResults = true;

        public bool HighlighSearchResults
        {
            get { return _highlighSearchResults; }
            set {
                if(_highlighSearchResults != value)
                {
                    _highlighSearchResults = value;
                    RenderGrid();
                }                    
            }
        }

        public void ShowFindPanel()
        {
            if(!_findPanelVisible)
            {
                _showFindPanelContextItem.Caption = "Close Find Panel";
                _findPanelVisible = true;
                GridFindPanel.style.visibility = "inherit";

                SetDefaultSizes();

                RenderGrid();
            }
                
        }

        public void CloseFindPanel()
        {
            if(_findPanelVisible)
            {
                _showFindPanelContextItem.Caption = "Show Find Panel";
                _findPanelVisible = false;
                GridFindPanel.style.visibility = "hidden";

                SetDefaultSizes();

                RenderGrid();
            }            
        }


        private DataTable _dataSource = null;

        public Action<int, int> OnFocusedRowChanged = null;
        public Action<int, int> OnFocusedColumnChanged = null;
        public Action<int> OnRowDoubleClick = null;
        public Action<HTMLElement, int> OnCustomRowStyle = null;

        protected Action<MouseEvent> OnRowClick;
        protected Action<MouseEvent> OnDoubleClick;
        protected Action<MouseEvent> OnCellRowMouseDown;

        public HardSoftList<bool> SelectedRows = new HardSoftList<bool>(false);
        public List<int> VisibleRowHandles = null;

        public void SetVisibleRowHandles<T>(List<T> Cells, bool asc)
        {
            if(DataSource._searchActive)
            {
                if(asc)
                {
                    var sorted = Cells
                        .Select((x, i) => new KeyValuePair<int, T>(i, x))
                        .Where((p) => DataSource._searchResults.Contains(p.Key))
                        .OrderBy(x => x.Value)
                        .ToList();

                    VisibleRowHandles = sorted.Select(x => x.Key).ToList();
                }
                else
                {
                    var sorted = Cells
                        .Select((x, i) => new KeyValuePair<int, T>(i, x))
                        .Where((p) => DataSource._searchResults.Contains(p.Key))
                        .OrderByDescending(x => x.Value)
                        .ToList();

                    VisibleRowHandles = sorted.Select(x => x.Key).ToList();
                }
            }
            else
            {
                if(asc)
                {
                    var sorted = Cells
                        .Select((x, i) => new KeyValuePair<int, T>(i, x))                        
                        .OrderBy(x => x.Value)
                        .ToList();

                    VisibleRowHandles = sorted.Select(x => x.Key).ToList();
                }
                else
                {
                    var sorted = Cells
                        .Select((x, i) => new KeyValuePair<int, T>(i, x))
                        .OrderByDescending(x => x.Value)
                        .ToList();

                    VisibleRowHandles = sorted.Select(x => x.Key).ToList();
                }
            }
            
        }

        public bool _allowRowDrag = false;

        public bool AllowRowDrag
        {
            get { return _allowRowDrag; }
            set
            {
                if(_allowRowDrag != value)
                {
                    _allowRowDrag = value;
                    RenderGrid();
                }
            }
        }

        public bool AutoGenerateColumnsFromSource = true;
        public bool AllowMultiSelection = true;

        private bool showAutoFilterRow = false;

        public bool ShowAutoFilterRow
        {
            get { return showAutoFilterRow; }
            set
            {
                if(showAutoFilterRow != value)
                {
                    showAutoFilterRow = value;
                    if(!showAutoFilterRow)
                    {
                        // Remove Filter.
                        for(int i = 0; i < ColumnCount(); i++)
                        {
                            //FilterEdit = null;
                            Columns[i].FilterEdit = null;
                            Columns[i].FilterValue = null;
                        }
                        CalculateVisibleRows();
                    }
                    RenderGrid();
                }
            }
        }

        public void CalculateVisibleRows()
        {
            List<int> calcVisibleRows = new List<int>();

            for(int y = 0; y < RowCount(); y++)
            {
                bool AddIndex = true;

                for(int x = 0; x < ColumnCount(); x++)
                {
                    if(!Columns[x].ValueMatchFilter(y))
                    {
                        AddIndex = false;
                        break;
                    }
                }
                if(AddIndex)
                {
                    calcVisibleRows.Add(y);
                }
            }

            VisibleRowHandles = calcVisibleRows;
            RenderGrid();
        }

        public float UnitHeight = 28.0f;
        private bool _columnAutoWidth = false;
        
        private int _focusedcolumnHandle = -1;

        double cellChangeTimer = -1;

        private bool skipSetNewCell = false;

        public int FocusedColumnHandle
        {
            get
            {
                return _focusedcolumnHandle;
            }
            set
            {
                if(value != FocusedColumnHandle)
                {
                    var prev = _focusedcolumnHandle;
                    _focusedcolumnHandle = value;
                    setNewCell(value, FocusedDataHandle);
                    RenderGrid();
                    if (OnFocusedColumnChanged != null)
                        OnFocusedColumnChanged(_focusedcolumnHandle, prev);
                }
                else
                {
                    setNewCell(value, FocusedDataHandle);
                }
            }
        }

        int prevCellColIndex = -1;
        int prevRowCellIndex = -1;

        private DataRow dataRow = null;
        private int dataRowIndex = -1;
        private int dataColIndex = -1;
        private TextInput _activeEditor = null;
        private HTMLElement activeEditorElement = null;
        public TextInput ActiveEditor { get; }

        private bool isEditorShown = false;
        private bool isShowingEditor = false;

        public class ValidateInput
        {
            public bool IsValid = true;
            public string ErrorDescription;
        }

        public class ShowingEditor
        {
            public bool Cancel;
        }
        
        public Action<TextInput, ValidateInput> OnValidateInput = null;

        public void SetFocusedRowCellValue(int columnHandle, object value)
        {
            SetRowCellValue(FocusedDataHandle, columnHandle, value);
        }

        public void SetRowCellValue(int rowHandle, int columnHandle, object value)
        {

        }

        public void SetRowCellValue(int rowHandle, GridViewColumn column, object value)
        {

        }


        public void PostEditor()
        {
            if(isEditorShown)
            {
                dataRow[dataColIndex] = _activeEditor.GetEditValue();
                
                //SetRowCellValue(openedEditorRowIndex, openedEditorCol, _activeEditor.GetEditValue());

                CloseEditor();
            }
        }

        public void ValidateEditor()
        {
             if(isEditorShown)
            {
                var vi = new ValidateInput();
                // what we need to do is validate editor
                if (OnValidateInput != null)
                {
                    OnValidateInput(_activeEditor, vi);
                }
                if (!vi.IsValid)
                {
                    CloseEditor();
                    new MessageBoxForm(vi.ErrorDescription, MessageBoxLayout.Exclamation).ShowDialog();
                }
                else
                {
                    PostEditor();
                }
            }            
        }

        public void MoveNextCell()
        {

        }

        public void ShowEditor()
        {
            if(!isEditorShown && UseInRowEditor)
            {
                if (FocusedColumn == null || !FocusedColumn.AllowEdit)
                    return;

                var cancelEven = new ShowingEditor();
                if (OnShowingEditor != null)
                {
                    OnShowingEditor(cancelEven);
                }
                if (cancelEven.Cancel)
                    return;

                var input = FocusedColumn.GetNewInput();

                if(input == null)
                {
                    return;
                }
                
                dataRowIndex = FocusedDataHandle;
                dataRow = DataSource[dataRowIndex];

                dataColIndex = FocusedColumn.GetDataColumnIndex();

                _activeEditor = input;

                var inpute = _activeEditor.GetInput();
                
                inpute.onkeydown = (ev) => {
                    if (ev.keyCode == 9)
                    {
                        ev.preventDefault();

                        ValidateEditor();

                        MoveNextCell();
                    }
                    else if (ev.keyCode == 13 || ev.keyCode == 27)
                    {
                        ValidateEditor();
                    }
                };

                _activeEditor.SetEditValue(dataRow[dataColIndex]);

                activeEditorElement = (input.Controller != null ? input.Controller : input).Content;
                activeEditorElement.style.zIndex = "100";
                GridBody.appendChild(activeEditorElement);

                isEditorShown = true;
                
                RenderGrid();

                inpute.tabIndex = -1;
                isShowingEditor = true;
                inpute.focus();
            }
        }               

        public GridViewColumn FocusedColumn => FocusedColumnHandle < 0 ? null : Columns[FocusedColumnHandle];

        public Action<ShowingEditor> OnShowingEditor = null;

        public void CloseEditor()
        {
            if(isEditorShown)
            {                
                _activeEditor = null;
                dataColIndex = -1;
                dataRow = null;
                dataRowIndex = -1;
                isEditorShown = false;

                GridBody.removeChild(activeEditorElement);
                activeEditorElement = null;
                RenderGrid();
            }
        }

        /// <summary>
        /// int Col, int Row
        /// </summary>
        public Action<int, int> OnFocusedCellChanged;
        
        private void setNewCell(int col, int row)
        {
            if(col == -1 || row == -1 || skipSetNewCell)
            {
                prevCellColIndex = col;
                prevRowCellIndex = row;
                return;
            }
            if(col != prevCellColIndex || prevRowCellIndex != row)
            {                
                // changed..
                if (cellChangeTimer > -1)
                {
                    window.clearTimeout(cellChangeTimer);
                    cellChangeTimer = -1;
                }

                cellChangeTimer = window.setTimeout((obj) =>
                {
                    // what we need to do is set new index.
                    prevCellColIndex = col;
                    prevRowCellIndex = row;

                    if (OnFocusedCellChanged != null)
                    {
                        OnFocusedCellChanged(col, row);
                    }
                    ValidateEditor();
                    ShowEditor();
                    cellChangeTimer = -1;
                }, 25);
            }
            else
            {
                // reason for where there are the same is when you click again on same cell.... after close..
                if (cellChangeTimer > -1)
                {
                    window.clearTimeout(cellChangeTimer);
                    cellChangeTimer = -1;
                }

                cellChangeTimer = window.setTimeout((obj) =>
                {                    
                    ValidateEditor();
                    ShowEditor();
                    cellChangeTimer = -1;
                }, 25);
            }
        }

        private int _focusedDataHandle = -1;
        
        public int FocusedDataHandle
        {
            get
            {
                return _focusedDataHandle;
            }
            set
            {
                if(value != _focusedDataHandle)
                {                    
                    var prev = _focusedDataHandle;
                    
                    _focusedDataHandle = value;
                    setNewCell(FocusedColumnHandle, value);
                    RenderGrid();
                    if(OnFocusedRowChanged != null)
                        OnFocusedRowChanged(_focusedDataHandle, prev);
                }
                else
                {
                    setNewCell(FocusedColumnHandle, value);
                }
            }
        }

        public void SetDefaultSizes()
        {
            if(_columnHeadersVisible)
            {
                GridHeader.style.visibility = "inherit";
                GridHeaderContainer.style.visibility = "inherit";

                if(FindPanelVisible)
                {
                    GridHeaderContainer.SetBounds(0, 47, "100%", UnitHeight + 1);
                    GridBodyContainer.SetBounds(0, UnitHeight + 2 + 47, "100%", "(100% - " + (UnitHeight + 2 + 47) + "px)");
                }
                else
                {
                    GridHeaderContainer.SetBounds(0, 0, "100%", UnitHeight + 1);
                    GridBodyContainer.SetBounds(0, UnitHeight + 2, "100%", "(100% - " + (UnitHeight + 2) + "px)");                    
                }                
            }
            else
            {
                GridHeader.style.visibility = "hidden";
                GridHeaderContainer.style.visibility = "hidden";

                if(FindPanelVisible)
                {
                    GridBodyContainer.SetBounds(0, 1 + 46, "100%", "(100% - " + (1 + 46)  + "px)");
                }
                else
                {
                    GridBodyContainer.SetBounds(0, 1, "100%", "(100% - 1px)");
                }                
            }
        }

        private bool _columnHeadersVisible = true;

        public bool ColumnHeadersVisible
        {
            get
            {
                return _columnHeadersVisible;
            }
            set
            {
                if(value != _columnHeadersVisible)
                {
                    _columnHeadersVisible = value;

                    SetDefaultSizes();

                    RenderGrid();
                }
            }
        }

        public bool ColumnAutoWidth
        {
            get
            {
                return _columnAutoWidth;
            }
            set
            {
                if(value)
                    GridBodyContainer.style.overflowX = "hidden";
                else
                    GridBodyContainer.style.overflowX = "auto";

                if(_columnAutoWidth != value)
                {
                    _columnAutoWidth = value;
                    RenderGrid();
                }
            }
        }

        private bool _useEditForm = true;

        public bool UseEditForm
        {
            get
            {
                return _useEditForm;
            }
            set
            {
                if(value != _useEditForm)
                {
                    _useEditForm = value;
                    if(_useEditForm)
                    {
                        UseInRowEditor = false;
                    }
                    RenderGrid();
                }
            }
        }

        private SortSetting SortSettings;

        public void SortColumn()
        {
            if(SortSettings != null)
            {
                SortColumn(SortSettings.Column, SortSettings.SortMode);
            }
        }

        public void ClearSortColumn()
        {
            if(SortSettings != null)
            {
                SortColumn(SortSettings.Column, GridViewSortMode.None);
            }
        }

        public void SortColumn(GridViewColumn column, GridViewSortMode sort = GridViewSortMode.Asc)
        {
            column.SortedMode = sort;

            if(SortSettings != null && SortSettings.Column != column)
            {
                SortSettings.Column.SortedMode = GridViewSortMode.None;
                VisibleRowHandles = null;
            }

            if(sort == GridViewSortMode.None)
            {
                VisibleRowHandles = null;
            }
            else
            {
                bool sort1 = sort == GridViewSortMode.Asc;

                switch(column.Column.DataType)
                {
                    default:
                    case DataType.Object:
                        SetVisibleRowHandles((column.Column as DataColumnObject).Cells, sort1);
                        break;

                    case DataType.Bool:
                        SetVisibleRowHandles((column.Column as DataColumnBool).Cells, sort1);
                        break;

                    case DataType.DateTime:
                        SetVisibleRowHandles((column.Column as DataColumnDateTime).Cells, sort1);
                        break;

                    case DataType.String:
                        SetVisibleRowHandles((column.Column as DataColumnString).Cells, sort1);
                        break;

                    case DataType.Byte:
                        SetVisibleRowHandles((column.Column as DataColumnByte).Cells, sort1);
                        break;

                    case DataType.Short:
                        SetVisibleRowHandles((column.Column as DataColumnShort).Cells, sort1);
                        break;

                    case DataType.Integer:
                        SetVisibleRowHandles((column.Column as DataColumnInteger).Cells, sort1);
                        break;

                    case DataType.Long:
                        SetVisibleRowHandles((column.Column as DataColumnLong).Cells, sort1);
                        break;

                    case DataType.Float:
                        SetVisibleRowHandles((column.Column as DataColumnFloat).Cells, sort1);
                        break;

                    case DataType.Double:
                        SetVisibleRowHandles((column.Column as DataColumnDouble).Cells, sort1);
                        break;

                    case DataType.Decimal:
                        SetVisibleRowHandles((column.Column as DataColumnDecimal).Cells, sort1);
                        break;
                }
            }

            RenderGrid();
            SortSettings = new SortSetting()
            {
                Column = column,
                SortMode = sort
            };
        }

        public int ColumnCount()
        {
            return Columns.Count;
        }

        public int RowCount()
        {
            if(_dataSource == null)
                return 0;
            return _dataSource.RowCount;
        }

        public void ScrollToBottom()
        {
            GridBodyContainer.scrollTop = GridBody.clientHeight - GridBodyContainer.clientHeight;
        }

        public void ScrollToTop()
        {
            GridBodyContainer.scrollTop = 0;
        }

        public DataTable DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                FocusedDataHandle = -1;
                SelectedRows = new HardSoftList<bool>(false);
                VisibleRowHandles = new List<int>();

                if(_dataSource != null)
                {
                    _dataSource.OnDataSourceChanged -= DataSource_OnDataSourceChanged;
                }

                _dataSource = value;

                if(_dataSource != null)
                {
                    _dataSource.OnDataSourceChanged += DataSource_OnDataSourceChanged;

                    if(Columns.Count == 0 && AutoGenerateColumnsFromSource)
                    {
                        var sw = Stopwatch.StartNew();

                        for(int i = 0; i < _dataSource.ColumnCount; i++)
                        {
                            var sw1 = Stopwatch.StartNew();

                            var gvc = new GridViewColumn(this);
                            gvc.Caption = _dataSource.Columns[i].FieldName;
                            gvc.Column = _dataSource.Columns[i];
                            gvc.Visible = true;

                            switch(_dataSource.Columns[i].DataType)
                            {
                                case DataType.Byte:
                                case DataType.Short:
                                case DataType.Integer:
                                case DataType.Long:
                                case DataType.Float:
                                case DataType.Double:
                                case DataType.Decimal:
                                    gvc.BodyApparence.Alignment = "right";
                                    break;

                                case DataType.DateTime:
                                    if(Settings.GridViewAutoColumnFormatDates)
                                    {
                                        if(Settings.GridViewAutoColumnGenerateFormatAsDate)
                                            gvc.FormatString = "{0:d}";
                                        else
                                            gvc.FormatString = "{0:yyyy-MM-dd}";
                                    }

                                    break;

                                case DataType.Bool:
                                    gvc.CellDisplay = new GridViewCellDisplayCheckBox();
                                    break;
                            }

                            Columns.Add(gvc);

                            sw.Stop();
                            System.Console.WriteLine("DataSource AddColumn Auto: " + sw1.ElapsedMilliseconds);
                        }

                        sw.Stop();
                        System.Console.WriteLine("DataSource AutoColumns: " + sw.ElapsedMilliseconds);
                    }
                    RenderGrid();
                }
            }
        }

        public List<GridViewColumn> Columns = new List<GridViewColumn>();

        public GridViewColumn GetColumn(int i)
        {
            return Columns[i];
        }

        public object GetFocusedRowCellValue(int columnIndex)
        {
            return GetFocusedRowCellValue(Columns[columnIndex]);
        }

        public GridViewColumn GetGridViewColumnByFieldName(string FieldName)
        {
            for(int i = 0; i < ColumnCount(); i++)
            {
                if(Columns[i].Column.FieldName == FieldName)
                {
                    return Columns[i];
                }
            }
            return null;
        }

        public object GetFocusedRowCellValue(string FieldName)
        {
            return GetFocusedRowCellValue(GetColumnByFieldName(FieldName));
        }

        public object GetFocusedRowCellValue(GridViewColumn column)
        {
            return GetRowCellValue(FocusedDataHandle, column);
        }

        public object GetFocusedRowCellValue(DataColumn column)
        {
            return GetRowCellValue(FocusedDataHandle, column);
        }

        public object GetRowCellValue(int Datahandle, GridViewColumn column)
        {
            return GetRowCellValue(Datahandle, column.Column);
        }

        public object GetRowCellValue(int Datahandle, DataColumn column)
        {
            if(Datahandle == -1)
                return null;
            return column.GetCellValue(Datahandle);
        }

        public object GetRowCellValue(int Datahandle, string FieldName)
        {
            return GetRowCellValue(Datahandle, GetColumnByFieldName(FieldName));
        }

        public object GetRowCellValue(int Datahandle, int columnIndex)
        {
            return GetRowCellValue(Datahandle, Columns[columnIndex]);
        }

        public GridViewColumn GetGridViewColumnByFieldName(string fieldName, bool IgnoreCase = false)
        {
            for(int i = 0; i < ColumnCount(); i++)
            {
                if(Columns[i] != null && Columns[i].Column != null &&
                    string.Compare(Columns[i].Column.FieldName, fieldName, IgnoreCase) == 0)
                    return Columns[i];
            }

            return null;
        }

        public DataColumn GetColumnByFieldName(string fieldName, bool IgnoreCase = false)
        {
            if(DataSource == null)
                return null;

            for(int i = 0; i < DataSource.ColumnCount; i++)
            {
                if(DataSource.Columns[i] != null &&
                    string.Compare(DataSource.Columns[i].FieldName, fieldName, IgnoreCase) == 0)
                    return DataSource.Columns[i];
            }

            return null;
        }

        public void AddColumn(string caption, string fieldname, int width = 100, string formatstring = "", string alignment = "left", string forecolor = null, bool isBold = false)
        {
            var col = GetColumnByFieldName(fieldname);
            if(col == null)
                return;
            AddColumn(caption, col, width, formatstring, alignment, forecolor, isBold);
        }

        public void AddColumn(string caption, DataColumn column, int width = 100, string formatstring = "", string alignment = "left", string forecolor = null, bool isBold = false)
        {
            AddColumn(new GridViewColumn(this, width) { Caption = caption, BodyApparence = new GridViewCellApparence(isBold, alignment, forecolor), FormatString = formatstring, Column = column });
        }

        public void AddColumn(GridViewColumn column)
        {
            if(column == null)
                return;

            Columns.Add(column);

            RenderGrid();
        }

        public void AddColumns(params GridViewColumn[] columns)
        {
            if(columns == null || columns.Length == 0)
                return;

            Columns.AddRange(columns);

            RenderGrid();
        }

        public void RemoveColumn(GridViewColumn column)
        {
            Columns.Remove(column);

            RenderGrid();
        }

        public int GetDataSourceRow(int i)
        {            
            if(VisibleRowHandles == null || VisibleRowHandles.Count == 0)
            {
                if(DataSource._searchActive)
                {
                    return  DataSource._searchResults[i];
                }
                return i;
            }
                
            return VisibleRowHandles[i];
        }

        public float GetColumnWidths()
        {
            if(_columnAutoWidth)
            {
                return (float)GridBodyContainer.clientWidth;
            }
            else
            {
                float width = 0.0f;
                for(int i = 0; i < Columns.Count; i++)
                {
                    if(Columns[i].Visible)
                        width += Columns[i].Width;
                }
                return width;
            }
        }

        public void ClearSelection()
        {
            SelectedRows = new HardSoftList<bool>(false);
            RenderGrid();
        }

        public void SelectAllRows()
        {
            int length = RowCount();
            if(length == 0)
            {
                SelectedRows.ClearAll();
            }
            else
            {
                int[] index = new int[length];
                for(int i = 0; i < length; i++)
                {
                    index[i] = GetDataSourceRow(i);
                }
                SelectedRows.ClearAllSetHardRange(true, index);
            }
            RenderGrid();
        }

        private int PrevRenderGridScrollId = -1;

        public void DelayedRenderGrid(bool renderNoLag = false)
        {
            if(renderNoLag)
            {
                RenderGrid(false);
            }
            else
            {
                if(Settings.GridViewScrollDelayed)
                {
                    if(PrevRenderGridScrollId != -1)
                    {
                        clearTimeout(PrevRenderGridScrollId);
                        PrevRenderGridScrollId = -1;
                    }
                    PrevRenderGridScrollId = (int)setTimeout((a) =>
                    {
                        RenderGrid();
                    }, System.Math.Max(1, Settings.GridViewScrollDelayMS));
                }
                else
                {
                    RenderGrid();
                }
            }
            
        }

        private Stopwatch clickTimeDiff = null;

        public DataRow GetFocusedRow()
        {
            if(FocusedDataHandle > -1)
            {
                return DataSource[GetDataSourceRow(FocusedDataHandle)];
            }
            else
            {
                return null;
            }
        }

        public int GetVisibleCount()
        {
            if(Columns == null || Columns.Count == 0)
                return 0;
            int length = Columns.Count;
            int length1 = Columns.Count;

            for(int i = 0; i < length; i++)
            {
                if(!Columns[i].Visible)
                    length1--;
            }
            return length1;
        }

        public int GetBestFitForColumn(GridViewColumn column, bool includeColumnHeader = false)
        {
            if(!column.Visible)
                return 0;

            int maxLength = 0;
            string maxStr = "";

            if(includeColumnHeader && !string.IsNullOrWhiteSpace(column.Caption))
            {
                maxStr = column.Caption;
                maxLength = column.Caption.Length;
            }
            
            for(int i = 0; i < RowCount(); i++)
            {
                string value = column.GetDisplayValueByDataRowHandle(i);
                if(!string.IsNullOrWhiteSpace(value))
                {
                    int v = value.Length;
                    if(v > maxLength)
                    {
                        maxLength = v;
                        maxStr = value;
                    }
                }
            }

            if(maxLength > 0)
            {
                return (int)GetTextWidth(maxStr, Settings.DefaultFont) + 20;
            }else
            {
                return 0;
            }
        }

        public void BestFitAllColumns(bool includeColumnHeader = false)
        {
            _disableRender = true;
            for(int i = 0; i < Columns.Count; i++)
            {
                if(Columns[i].Visible)
                {
                    Columns[i].Width = GetBestFitForColumn(Columns[i], includeColumnHeader);
                }
            }
            _disableRender = false;
            RenderGrid();
        }

        private string headingClass;
        private string cellClass;

        private Dictionary<int, HTMLElement> CacheRow = new Dictionary<int, HTMLElement>();
        int CountOfDeletion = 0;

        private int _searchTimer = -1;
        private void _search()
        {
            if(this.DataSource == null || !FindPanelVisible)
                return;
            this.DataSource.Search(SearchTextInput.Text, this);
        }

        public void MakeRowVisible(int rowHandle)
        {
            if(rowHandle < 0)
                return;

            var getTopMostRowIndex = GetRawTopRowIndex();

            if(rowHandle < getTopMostRowIndex)
            {
                GridBodyContainer.scrollTop -= (int)((getTopMostRowIndex - rowHandle) * PixelsPerRow(RowCount()));                
            }
            else
            {
                getTopMostRowIndex = GetRawVisibleRowCount() + getTopMostRowIndex;
                if(rowHandle >= getTopMostRowIndex)
                {
                    GridBodyContainer.scrollTop += (int)(((rowHandle - getTopMostRowIndex) + 1) * PixelsPerRow(RowCount()));                    
                }
            }
        }

        private bool _useDrawNotDom;

        public bool UseDrawNotDom
        {
            get { return _useDrawNotDom; }
            set {
                if(_useDrawNotDom != value)
                {
                    _useDrawNotDom = value;
                    RenderGrid();
                }
            }
        }

        public GridView() : this(true, false)
        {

        }

        public void ExportToXLS(string fileName)
        {

            var builder = new StringBuilder();

            // Grid is empty...
            if(ColumnCount() == 0 || RowCount() == 0)
                return;
            int columnLength = ColumnCount();

            builder.Append("<table><thead><tr>");

            for(int i = 0; i < columnLength; i++)
            {
                var col = Columns[i];
                if(col.Visible)
                {
                    builder.Append($"<th>{Columns[i].Caption}</th>");
                } 
            }

            builder.Append("</tr></thead>");

            builder.Append("<tbody>");

            int rowLength = RowCount();

            for(int y = 0; y < rowLength; y++)
            {
                builder.Append("<tr>");
                var DataRowhandle = GetDataSourceRow(y);
                

                for(int x = 0; x < columnLength; x++)
                {
                    var col = Columns[x];
                    if(!col.Visible)
                        continue;

                    var displayValue = col.GetDisplayValueByDataRowHandle(DataRowhandle);

                    builder.Append($"<td>{displayValue}</td>");
                }

                builder.Append("</tr>");
            }
                        

            builder.Append("</tbody>");


            builder.Append("</table>");

            var ua = window.navigator.userAgent;
            var msie = ua.IndexOf("MSIE ");

            if(msie > 0)      // If Internet Explorer
            {
                var iframe = new HTMLIFrameElement();
                iframe.contentDocument.open("txt/html", "replace");
                iframe.contentDocument.write(builder.ToString());
                iframe.contentDocument.close();
                iframe.focus();
                iframe.contentDocument.execCommand("SaveAs", true, fileName);
            }
            else                 //other browser not tested on IE 11
                window.open("data:application/vnd.ms-excel," + encodeURIComponent(builder.ToString()));            
        }

        public GridView(bool autoGenerateColumns = true, bool columnAutoWidth = false) : base("grid")
        {
            if(Helper.NotDesktop)
            {
                UnitHeight = 53;
                headingClass = "heading heading-responsive";

                cellClass = "cell cell-responsive";
            }
            else
            {
                UnitHeight = 20;
                headingClass = "heading";
                cellClass = "cell";
            }

            this.Content.style.overflow = "hidden";
            // #FIND #RENDER#
            renderGridInternal = () =>
            {
                if(_disableRender)
                    return;

                int StartedWith = RenderTime;

                GridHeaderContainer.scrollLeft = GridBodyContainer.scrollLeft;
                if(Settings.GridViewBlurOnScroll)
                    ProcessBlur();

                ValidateGridSize();

                if(ColumnCount() == 0)
                {
                    ClearGrid();
                    return;
                }

                int RawLeftCellIndex = 0;
                float RawLeftCellScrollPadding = 0;

                int RawLeftCellCount = Columns.Count;

                float LeftLocation = 0;
                bool foundLeftLocation = false;
                bool foundRightLocation = false;

                float ClientWidth = (float)GridBodyContainer.clientWidth;

                #region "Columns"

                float ViewWidth = (float)GridBodyContainer.scrollLeft + ClientWidth;
                float _columnAutoWidthSingle = 0.0f;

                if(_columnAutoWidth)
                {
                    _columnAutoWidthSingle = ClientWidth == 0 ? 0.0f : ClientWidth / GetVisibleCount();
                }

                float MaxWidth;
                float LastWidth;

                for(int x = 0; x < Columns.Count; x++)
                {
                    if(!Columns[x].Visible)
                        continue;

                    Columns[x].CachedX = LeftLocation;
                    LastWidth = _columnAutoWidth ? _columnAutoWidthSingle : Columns[x].Width;
                    LeftLocation += LastWidth;
                    if(!foundLeftLocation && LeftLocation >= GridBodyContainer.scrollLeft)
                    {
                        foundLeftLocation = true;
                        RawLeftCellIndex = x;
                        RawLeftCellScrollPadding = (float)(LeftLocation - GridBodyContainer.scrollLeft);
                    }
                    if(foundLeftLocation && !foundRightLocation && LeftLocation >= ViewWidth)
                    {
                        foundRightLocation = true;
                        RawLeftCellCount = x + 1;
                        break;
                    }
                    if(StartedWith != RenderTime)
                    {
                        return;
                    }
                }

                MaxWidth = LeftLocation;

                var colFragment = document.createDocumentFragment();

                int uboundRowCount = RawLeftCellCount - 1;

                if(_columnHeadersVisible)
                {
                    for(int x = RawLeftCellIndex; x < RawLeftCellCount; x++)
                    {
                        if(x >= Columns.Count)
                            break;
                        if(!Columns[x].Visible)
                            continue;

                        var gcol = Columns[x];
                        var colIndex = x;
                        var apparence = gcol.HeadingApparence;

                        var col = Label(gcol.Caption,
                            (_columnAutoWidth ? gcol.CachedX : gcol.CachedX), 0, (_columnAutoWidth ? _columnAutoWidthSingle : gcol.Width) - (x == uboundRowCount ? 0 : 1),
                            apparence.IsBold, false, headingClass, apparence.Alignment, apparence.Forecolor);
                        if(gcol.SortedMode != GridViewSortMode.None)
                        {
                            var sortImage = Div(gcol.SortedMode == GridViewSortMode.Asc ? "grid-sort-up" : "grid-sort-down");
                            sortImage.SetBounds("(100% - 13px)", 7, 9, 5);
                            col.AppendChild(sortImage);
                        }

                        SetupColumn(col, x, gcol);

                        colFragment.appendChild(col);

                        if(StartedWith != RenderTime)
                        {
                            return;
                        }
                    }
                }

                #endregion "Columns"

                if(_dataSource == null || _dataSource.RowCount == 0 || _dataSource.ColumnCount == 0)
                {
                    ClearGrid();
                    GridHeader.AppendChild(colFragment);
                    return;
                }

                var ppr = PixelsPerRow(_dataSource.RowCount);

                float RawTopRowIndex = GetRawTopRowIndex();
                float RawTopRowScrollPadding = RawTopRowIndex % 1.0f;
                float RawVisibleRowCount = GetRawVisibleRowCount();

                int Length = (int)(RawVisibleRowCount + RawTopRowIndex) + 1;
                int start = (int)RawTopRowIndex;

                #region "Selection"

                for(int x = SelectedRows.SL.Count - 1; x >= 0; x--)
                {
                    bool Found = false;
                    for(int i = start; i < Length; i++)
                    {
                        if(i < DataSource.RowCount)
                        {
                            var DataRowhandle = GetDataSourceRow(i);
                            if(SelectedRows.GetIndexValueByHardListIndex(SelectedRows.SL[x]).Index == DataRowhandle)
                            {
                                Found = true;
                                break;
                            }
                        }
                        if(StartedWith != RenderTime)
                        {
                            return;
                        }
                    }
                    if(StartedWith != RenderTime)
                    {
                        return;
                    }
                    if(!Found)
                    {
                        SelectedRows.SL.RemoveAt(x);
                    }
                }

                #endregion "Selection"

                var rowFragment = document.createDocumentFragment();

                if(Settings.GridViewRowScrollPadding > 0)
                {
                    start -= Settings.GridViewRowScrollPadding;
                    Length += Settings.GridViewRowScrollPadding;
                }

                float Y = (start * ppr);// + RawTopRowScrollPadding;

                if(ShowAutoFilterRow)
                {
                    Length -= 1;
                    Y += UnitHeight;
                }

                this.Content.onblur = (ev) => {
                    if(isEditorShown && !isShowingEditor)
                        ValidateEditor();
                    isShowingEditor = false;
                };
                
                // #TODO - CLEAN...
                if(start < 0)
                    start = 0;
                if(Length > DataSource.RowCount)
                    Length = DataSource.RowCount;

                if(CacheRow.Count > 10)
                {                   
                    if(CountOfDeletion > 8)
                    {
                        CacheRow = new Dictionary<int, HTMLElement>();
                        CountOfDeletion = 0;
                    }
                    else
                    {
                        int MaxDelete = CacheRow.Count - 10;
                        int __length = CacheRow.Count;
                        List<int> KeysToDelete = new List<int>();
                        for(int i = 0; i < __length; i++)
                        {
                            int fieldIndex = CacheRow.ElementAt(i).Key;
                            if(fieldIndex < start || fieldIndex >= Length)
                            {
                                KeysToDelete.Add(fieldIndex);
                                if(KeysToDelete.Count > MaxDelete)
                                {                                    
                                    break;
                                }
                            }
                        }
                        __length = KeysToDelete.Count;
                        if(__length > 0)
                            CountOfDeletion++;
                        for(int i = 0; i < __length; i++)
                        {
                            if(CacheRow.ContainsKey(KeysToDelete[i]))
                            {
                                var x = CacheRow[KeysToDelete[i]];
                                x.onclick = null;
                                x.ondblclick = null;
                                x.Empty();
                                x.ondragstart = null;
                                x.Delete();

                                CacheRow.Remove(KeysToDelete[i]);
                            }                            
                        }
                    }                    
                }

                int prevRowCache = CacheRow.Count;
                
                for(int i = start; i < Length; i++)
                {
                    if(!CacheRow.ContainsKey(i))
                    {
                        var DataRowhandle = GetDataSourceRow(i);
                        var dr = document.createElement("row"); // Div();
                        dr.className = (i % 2 == 0 ? "cellrow even" : "cellrow") + (SelectedRows.GetValue(DataRowhandle, true) ? " cellrow-selected" : "") + (DataRowhandle == FocusedDataHandle ? " focusedrow" : "");
                        dr.style.position = "absolute";
                        dr.SetBounds(0, Y, _columnAutoWidth ? ClientWidth : MaxWidth + 1, UnitHeight);
                        dr.setAttribute("i", Convert.ToString(DataRowhandle));

                        

                        dr.onclick = new HTMLElement.onclickFn(OnRowClick);
                        if(Settings.IsChrome)
                        {
                            dr.ondblclick = new HTMLElement.onclickFn(OnDoubleClick);
                        }
                        var docFrag = document.createDocumentFragment();

                        for(int x = RawLeftCellIndex; x < RawLeftCellCount; x++)
                        {
                            var col = Columns[x];
                            if(!col.Visible)
                                continue;

                            

                            var apparence = col.BodyApparence;
                            bool useDefault = false;
                            HTMLElement cell = null;
                            if(col.CellDisplay == null || (useDefault = col.CellDisplay.UseDefaultElement))
                            {
                                var displayValue = col.GetDisplayValueByDataRowHandle(DataRowhandle);

                                cell = document.createElement("cell");// new HTMLSpanElement();
                                cell.className = cellClass;// + " control";
                                cell.style.position = "absolute";
                                cell.style.left = col.CachedX + "px";
                                cell.style.width = (_columnAutoWidth ? _columnAutoWidthSingle : col.Width + (x == Columns.Count - 1 ? 1 : 0)) + "px";
                                cell.setAttribute("x", Convert.ToString(x));
                                cell.onclick = new HTMLElement.onclickFn(OnCellRowMouseDown);

                                if(!string.IsNullOrWhiteSpace(displayValue))
                                {
                                    cell.textContent = displayValue;
                                    if(apparence.Alignment != "left")
                                    {
                                        if(apparence.Alignment == "right")
                                        {
                                            cell.style.direction = "rtl";
                                        }
                                        else
                                        {
                                            cell.style.textAlign = apparence.Alignment;
                                        }
                                    }
                                    if(apparence.IsBold)
                                    {
                                        cell.style.fontWeight = "bold";
                                    }

                                    if(apparence.Forecolor != null)
                                    {
                                        cell.style.color = apparence.Forecolor;
                                    }
                                }

                                var newCell = useDefault ?
                                    col.CellDisplay.OnCreateDefault(cell, this, DataRowhandle, x) :
                                    cell;

                                if(_highlighSearchResults && DataSource._searchActive && !useDefault
                                    && !string.IsNullOrWhiteSpace(displayValue)
                                    && displayValue.ToLower().StartsWith(DataSource.SearchString))
                                {
                                    newCell.Empty();
                                    var markelement = document.createElement("mark");
                                    int Slength = DataSource.SearchString.Length;
                                    markelement.textContent = displayValue.Substring(0, Slength);
                                    newCell.AppendChildren(markelement, document.createTextNode(displayValue.Substring(Slength)));
                                }

                                docFrag.appendChild(newCell);
                            }
                            else
                            {
                                cell = col.CellDisplay.OnCreate(this, DataRowhandle, x);                                
                                cell.style.left = col.CachedX + "px";
                                cell.style.width = (_columnAutoWidth ? _columnAutoWidthSingle : col.Width).ToPx();

                                docFrag.appendChild(cell);
                            }

                            if (isEditorShown && dataRowIndex == DataRowhandle && col.GetDataColumnIndex() == dataColIndex)
                            {
                                activeEditorElement.style.left = cell.style.left;
                                activeEditorElement.style.width = cell.style.width;
                                activeEditorElement.style.top = dr.style.top;
                                activeEditorElement.style.height = UnitHeight.ToPx();
                            }
                        }

                        dr.AppendChild(docFrag);

                        if(AllowRowDrag)
                        {
                            dr.setAttribute("draggable", "true");

                            dr.ondragstart = new HTMLElement.ondragFn(OnRowDragStart);
                        }

                        rowFragment.appendChild(dr);

                        CacheRow[i] = dr;   
                    }
                    
                    if (StartedWith != RenderTime)
                    {
                        if(prevRowCache == 0)
                            ClearGrid();

                        GridBody.AppendChild(rowFragment);

                        return;
                    }

                    Y += UnitHeight;
                }
                if(prevRowCache == 0)
                    ClearGrid();

                if(OnCustomRowStyle != null && rowFragment.childNodes != null)
                {
                    var count = rowFragment.childNodes.length;
                    for(int i = 0; i < count; i++)
                    {
                        if(StartedWith != RenderTime)
                        {
                            GridBody.AppendChild(rowFragment);

                            return;
                        }

                        try
                        {
                            var child = (HTMLElement)rowFragment.childNodes[i];
                            OnCustomRowStyle(child, Script.ParseInt(child.getAttribute("i")));
                        }
                        catch(Exception ex)
                        {
                            if(Application.AplicationDefition == ApplicationDefitnion.ExpressCraftConsole)
                                ConsoleForm.Log(ex.ToString(), ConsoleLogType.Error);
                        }
                    }
                }

                GridHeader.AppendChild(colFragment);
                GridBody.AppendChild(rowFragment);

                if(StartedWith != RenderTime)
                {
                    return;
                }

                RenderTime = -1;
            };

            GridHeaderContainer = Div("heading-container");

            GridHeader = Div();
            GridHeader.SetBounds(0, 0, 0, "29px");
            GridBodyContainer = Div();

            GridBodyContainer.style.overflowX = "auto !important";
            GridBodyContainer.style.overflowY = "auto !important";

            GridHeaderContainer.style.overflow = "hidden";

            GridBody = Div();
            GridBody.SetBounds(0, 0, 0, 0);

            GridBodyContainer.AppendChild(GridBody);
            GridHeaderContainer.AppendChild(GridHeader);

            GridFindPanel = Div("heading-container");
            GridFindPanel.style.visibility = "hidden";
            GridFindPanel.SetBounds(0, 0, "100%", 46);

            SearchTextInput = new TextInput() { OnTextChanged = (sender) => {
                if(_searchTimer > -1)
                {
                    clearTimeout(_searchTimer);
                }
                if(string.IsNullOrWhiteSpace(SearchTextInput.Text))
                    _search();
                else
                    _searchTimer = (int)setTimeout((a) => { _search(); }, 500);
            }, OnKeyDown = (sender, ev) => {
                if(ev.keyCode == KeyCodes.Enter)
                {
                    btnFind.Content.click();
                }
            } };
            SearchTextInput.Bounds = new Vector4(30, 13, 350, 22);
            SearchTextInput.SetAttribute("placeholder", "Enter text to search...");

            btnFind = new SimpleButton() { Text = "Find", ItemClick = (sender) => {
                if(_searchTimer > -1)
                {
                    clearTimeout(_searchTimer);
                }
                _search();
            }, Bounds = new Vector4(385, 13, 60, 22) };
            btnClear = new SimpleButton() { Text = "Clear", ItemClick = (sender) => {
                if(_searchTimer > -1)
                {
                    clearTimeout(_searchTimer);
                }
                SearchTextInput.Text = string.Empty;
            }, Bounds = new Vector4(449, 13, 60, 22) };

            btnClose = new SimpleButton() { Bounds = new Vector4(7, 15, 18, 18), ItemClick = (sender) => {
                btnClear.Content.click();
                CloseFindPanel();
            } };
            btnClose.Content.innerHTML = "&times;";


            GridFindPanel.AppendChildren(btnClose, SearchTextInput, btnFind, btnClear);

            SetDefaultSizes();

            Content.onmouseup = (ev) =>
            {
                if(ResizeIndex < 0 || Bridge.Script.IsNaN(ResizeIndex))
                    return;
                
                
                int x = (int)ev.pageX;
                
                x = Columns[ResizeIndex].Width + (x - ResizePageX);
                if(x < 24)
                    x = 24;
                Columns[ResizeIndex].Width = x;

                Form.SetCursor("default");

                ev.preventDefault();
                ev.stopImmediatePropagation();
                ev.stopPropagation();

                ResizeIndex = -1;
                ResizeSpan = null;
            };

            OnResize = (ev) =>
            {
                CacheRow = new Dictionary<int, HTMLElement>();
                DelayedRenderGrid();
            };

            int prevleft = 0;
            if(Settings.IsChrome && !Helper.NotDesktop)
            {
                GridBodyContainer.onmousewheel = (ev) =>
                {
                    ev.preventDefault();

                    if (ev.deltaY != 0)
                    {
                        GridBodyContainer.scrollTop += (UnitHeight * (ev.deltaY / 100.00d)) * 3;
                    }
                };
            }
            
            bool ignoreScroll = false;
            GridBodyContainer.onscroll = (ev) =>
            {
                if (ignoreScroll)
                    return;

                if(prevleft != GridBodyContainer.scrollLeft)
                {
                    CacheRow = new Dictionary<int, HTMLElement>();
                    prevleft = (int)GridBodyContainer.scrollLeft;
                    DelayedRenderGrid();
                }else
                {
                    if(Settings.IsChrome && !Helper.NotDesktop)
                    {
                        //(window.innerHeight + window.pageYOffset) >= document.body.offsetHeight - 2
                        if(GridBodyContainer.scrollTop != 0 && GridBodyContainer.scrollTop + GridBodyContainer.clientHeight != GridBodyContainer.scrollHeight)
                        {
                            var diff = GridBodyContainer.scrollTop % UnitHeight;
                            if (diff != 0)
                            {
                                ignoreScroll = true;
                                GridBodyContainer.scrollTop -= diff;
                                ignoreScroll = false;
                            }
                        }
                    }
                    
                    DelayedRenderGrid(true);
                }
            };
            OnLoaded = (ev) =>
            {
                RenderGrid();
            };
            OnCellRowMouseDown = (ev) =>
            {
                FocusedColumnHandle = Script.ParseInt(ev.currentTarget.As<HTMLElement>().getAttribute("x"));

            };
            OnRowClick = (ev) =>
            {
                if(!Settings.IsChrome)
                {
                    if(clickTimeDiff == null)
                    {
                        clickTimeDiff = Stopwatch.StartNew();
                    }
                    else
                    {
                        clickTimeDiff.Stop();
                        var ems = clickTimeDiff.ElapsedMilliseconds;
                        clickTimeDiff = null;

                        if(ems < 200)
                        {
                            OnDoubleClick(ev);
                        }
                    }
                }

                var DataRowHandle = Script.ParseInt(ev.currentTarget.As<HTMLElement>().getAttribute("i"));
                
                var mev = ev.As<MouseEvent>();
                if(AllowMultiSelection)
                {
                    if(mev.ctrlKey)
                    {
                        SelectedRows.AddOrSet(true, DataRowHandle, true);
                        RenderGrid();
                    }
                    else if(mev.shiftKey && FocusedDataHandle > -1)
                    {                        
                        _disableRender = true;
                        SelectedRows.ClearAll();
                        if(DataRowHandle < FocusedDataHandle)
                        {
                            for(int i = DataRowHandle; i < FocusedDataHandle + 1; i++)
                            {
                                SelectedRows.AddOrSet(true, i, true);
                            }
                        }else
                        {
                            for(int i = FocusedDataHandle; i < DataRowHandle + 1; i++)
                            {
                                SelectedRows.AddOrSet(true, i, true);
                            }
                        }
                        _disableRender = false;
                        RenderGrid();
                        return;
                    }
                }
                SelectedRows.ClearAndAddOrSet(true, DataRowHandle, true);
                if(DataRowHandle != _focusedDataHandle)
                {
                    FocusedDataHandle = DataRowHandle;
                }
                else
                {
                    FocusedDataHandle = DataRowHandle;
                    RenderGrid();
                }
            };
            Content.tabIndex = 0;
            OnDoubleClick = (ev) =>
            {
                int drh = Script.ParseInt(ev.currentTarget.As<HTMLElement>().getAttribute("i"));
                if(OnRowDoubleClick != null)
                    OnRowDoubleClick(drh);

                if(_useEditForm)
                {
                    var idr = DataSource[drh];

                    var fdre = new DataRowEditForm(idr, this, true);
                    fdre.ShowDialog();
                }
            };

            Content.onkeydown = (ev) =>
            {
                var kev = ev.As<KeyboardEvent>();
                //Global.Alert("CONTROL + A");
                if(AllowMultiSelection && kev.ctrlKey && (kev.keyCode == 65 || kev.keyCode == 97))
                {
                    // keyCode == 65 || keyCode == 97
                    //Global.Alert("AllowMultiSelection = TRUE");
                    SelectAllRows();
                }
                else
                {
                    if(kev.keyCode == KeyCodes.Up || kev.keyCode == KeyCodes.Down)
                    {
                        _disableRender = true;
                        var prevFocused = FocusedDataHandle;
                        if(kev.keyCode == KeyCodes.Up)
                        {
                            if(!(FocusedDataHandle - 1 < 0))
                                FocusedDataHandle--;
                        }
                        else if(kev.keyCode == KeyCodes.Down)
                        {
                            if(!(FocusedDataHandle > RowCount()))
                                FocusedDataHandle++;
                        }
                        if(prevFocused != FocusedDataHandle)
                        {
                            if(kev.shiftKey)
                            {
                                SelectedRows.AddOrSet(true, FocusedDataHandle, true);
                            }
                            else
                            {
                                SelectedRows.ClearAndAddOrSet(true, FocusedDataHandle, true);
                            }
                            
                            MakeRowVisible(FocusedDataHandle);

                            _disableRender = false;

                            RenderGrid();
                        }else
                        {
                            _disableRender = false;
                        }                        
                    }
                                 
                    //Global.Alert("AllowMultiSelection = FALSE");
                }
            };

            ContextMenu = new ContextMenu();

            ContextMenu.ContextItems.AddRange(new ContextItem[] {
                new ContextItem("Sort Ascending", (cm) => {
                    if(FocusedColumnHandle > -1)
                    {
                        SortColumn(Columns[FocusedColumnHandle], GridViewSortMode.Asc);
                    }
                }),
                new ContextItem("Sort Descending", (cm) => {
                    if(FocusedColumnHandle > -1)
                    {
                        SortColumn(Columns[FocusedColumnHandle], GridViewSortMode.Desc);
                    }
                }),
                new ContextItem("Clear All Sorting", (cm) => {
                    ClearSortColumn();
                },  true),
                //new ContextItem("Group By This Column"),
                //new ContextItem("Hide Group By Box", true),
                new ContextItem("Hide This Column", (ci) => {
                    if(FocusedColumnHandle > -1)
                    {
                        Columns[FocusedColumnHandle].Visible = false;
                        RenderGrid();
                    }
                }),
                new ContextItem("View Columns", (ci) => {
                    if(this.ColumnCount() == 0)
                    {
                        new MessageBoxForm("This grid control is empty.", MessageBoxLayout.Information).ShowDialog();
                    }
                    else
                    {
                        var x = new Form();
                        x.StartPosition = FormStartPosition.Center;
                        x.Size = new Vector2(200, 400);
                        x.Text = "View Columns";
                        x.ShowMaximize = false;
                        x.ShowMinimize = false;
                        int index = 10;
                        foreach (var item in this.Columns)
                        {
                            var gridItem = item;

                            var checkEdit = new CheckEdit(gridItem.Caption) { Checked = gridItem.Visible, OnCheckChanged = (chk) => {
                                    gridItem.Visible = chk.Checked;
                                    this.RenderGrid();
                                }, Location = new Vector2(10, index), Width = "(100% - 20px)", Height = 22
                            };

                            x.AppendChild(checkEdit);
                            index += 24;
	                    }

                        x.Body.style.overflow = "auto";

                        x.Show();

                    }
                }),
                //new ContextItem("View Columns"),
                //new ContextItem("Save Column Layout"),
                new ContextItem("Best Fit", (ci) => {
                    if(FocusedColumnHandle > -1)
                    {
                        Columns[FocusedColumnHandle].Width = GetBestFitForColumn(Columns[FocusedColumnHandle]);                        
                    }
                }) ,
                new ContextItem("Best Fit (all columns)", (ci) => {
                    BestFitAllColumns();
                }, true),
                new ContextItem("Export to Excel", (ci) => {
                    this.ExportToXLS("export.xls");
                }, true),
                //new ContextItem("Filter Editor...", true),
                _showFindPanelContextItem = new ContextItem("Show Find Panel", true) {
                    OnItemClick = (sender) => {
                        if(FindPanelVisible)
                        {                            
                            FindPanelVisible = false;
                        }else
                        {                            
                            FindPanelVisible = true;
                        }
                    }
                },                
                new ContextItem("Select All", (cm) => { SelectAllRows(); }),
                new ContextItem("Unselect All", (cm) => { ClearSelection(); })
            });

            Content.oncontextmenu = (ev) =>
            {
                if(Helper.NotDesktop)
                {
                    ev.preventDefault();
                    ev.stopPropagation();

                    OnDoubleClick(ev);
                }
                else
                {
                    if(ContextMenu != null)
                    {
                        ContextMenu.Show(Helper.GetClientMouseLocation(ev));
                        ev.preventDefault();
                        ev.stopPropagation();
                    }
                }
            };

            OnColumnOnClick = (ev) =>
            {
                if(ResizeIndex >= 0)
                    return;

                var gcol = Columns[Script.ParseInt(ev.currentTarget.As<HTMLElement>().getAttribute("i"))];

                for(int i = 0; i < ColumnCount(); i++)
                {
                    if(Columns[i] != gcol)
                    {
                        Columns[i].SortedMode = GridViewSortMode.None;
                    }
                }
                switch(gcol.SortedMode)
                {
                    default:
                    case GridViewSortMode.None:
                        SortColumn(gcol, GridViewSortMode.Asc);
                        break;

                    case GridViewSortMode.Asc:
                        SortColumn(gcol, GridViewSortMode.Desc);
                        break;

                    case GridViewSortMode.Desc:
                        SortColumn(gcol, GridViewSortMode.None);
                        break;
                }
            };
            OnColumnDragStart = (ev) =>
            {
                Script.Call("ev.dataTransfer.setData", "gridviewColumnDrag", ev.currentTarget.As<HTMLElement>().getAttribute("i"));
            };
            OnColumnDragOver = (ev) =>
            {
                ev.preventDefault();
            };
            OnColumnDrop = (ev) =>
            {
                if(ev.target == null || !(ev.target is HTMLSpanElement))
                    return;

                var target = ev.target.As<HTMLSpanElement>();

                if(target.parentElement != GridHeader)
                    return;

                var HoverIndex = Script.ParseInt(target.getAttribute("i"));
                var SelectedIndex = Script.Write<int>("parseInt(ev.dataTransfer.getData(\"gridviewColumnDrag\"));");
                if(SelectedIndex == HoverIndex)
                    return;

                if(HoverIndex < 0)
                    return;

                int x = Script.Write<int>("ev.layerX");
                x -= (int)target.clientLeft;
                int w = (int)target.clientWidth / 2;

                if(HoverIndex == SelectedIndex - 1 && x > w)
                    return;
                if(HoverIndex == SelectedIndex + 1 && x < w)
                    return;

                if(x < w)
                {
                    DragIndex = HoverIndex;
                }
                else
                {
                    DragIndex = HoverIndex + 1;
                }

                if(DragIndex < 0 || SelectedIndex < 0)
                    return;
                var col = Columns[SelectedIndex];
                if(DragIndex == Columns.Count)
                {
                    Columns.Remove(col);
                    Columns.Add(col);
                }
                else
                {
                    var col1 = Columns[DragIndex];
                    Columns.Remove(col);
                    Columns.Insert(Columns.IndexOf(col1), col);
                }

                RenderGrid();
            };
            OnColumnMouseDown = (ev) =>
            {
                int x = Script.Write<int>("ev.layerX");
                var target = ev.target.As<HTMLSpanElement>();
                x -=(int)target.clientLeft;
                ResizePageX = Script.Write<int>("ev.pageX");
                skipSetNewCell = true;
                FocusedColumnHandle = Script.ParseInt(ev.currentTarget.As<HTMLElement>().getAttribute("i"));
                skipSetNewCell = false;

                if (x >= target.clientWidth - 2)
                {
                    ResizeIndex = Script.ParseInt(target.getAttribute("i"));
                    ResizeSpan = target;
                    Form.SetCursor("east-west-resize");

                    ev.preventDefault();
                }
                else
                {
                    ResizeSpan = null;
                    ResizeIndex = -1;
                }
            };
            OnColumnMouseMove = (ev) =>
            {
                if(ResizeIndex == -1)
                {
                    int x = Script.Write<int>("ev.layerX");
                    var target = ev.target.As<HTMLSpanElement>();
                    x -= (int)target.clientLeft;

                    if(x >= target.clientWidth - 2)
                    {
                        Form.SetCursor("east-west-resize");
                        return;
                    }
                    Form.SetCursor("default");
                }
            };

            OnColumnMouseLeave = (ev) =>
            {
                if(ResizeIndex == -1)
                {
                    Form.SetCursor("default");
                }
            };

            OnRowDragStart = (ev) =>
            {
                Script.Call("ev.dataTransfer.setData", "gridviewRowDrag", JSON.stringify(DataSource[Script.ParseInt(ev.currentTarget.As<HTMLElement>().getAttribute("i"))].GetOfflineDataRow()));
            };

            Content.AppendChildren(GridFindPanel, GridHeaderContainer, GridBodyContainer);

            FilterRowOnChange = (te) =>
            {
                Columns[Script.ParseInt(te.Content.getAttribute("i"))].FilterValue = te.Text;
            };

            AutoGenerateColumnsFromSource = autoGenerateColumns;
            ColumnAutoWidth = columnAutoWidth;
        }

        private void DataSource_OnDataSourceChanged(object sender, EventArgs e)
        {
            SortColumn();
            RenderGrid();
        }

        public override void Render()
        {
            base.Render();
            HasRendered = true;
            RenderGrid();

            if(Content.parentElement != null)
            {
            }
        }

        public float GetRawVisibleRowCount()
        {
            return (float)(GridBodyContainer.clientHeight == 0 ? 0.0f : GridBodyContainer.clientHeight / UnitHeight);
        }

        public float GetRawTopRowIndex()
        {
            return (float)(GridBodyContainer.scrollTop == 0 ? 0.0f : GridBodyContainer.scrollTop / PixelsPerRow(RowCount()));
        }

        public void ValidateGridWidth()
        {
            var width = GetColumnWidths();
            GridBody.style.width = (width).ToPx();
            GridHeader.style.width = ((width) + 24).ToPx(); // (width).ToPx();
            if(RightOfTable == null)
            {
                RightOfTable = Div();
                GridBody.AppendChild(RightOfTable);
            }
            if(RightOfTableHeader == null)
            {
                RightOfTableHeader = Div();
                GridHeader.AppendChild(RightOfTableHeader);
            }
            RightOfTable.SetBounds(width - 1, 0, 1, 1);
            RightOfTableHeader.SetBounds(width - 1, 0, 1, 1);
        }

        public float PixelsPerRow(int rowCount)
        {
            if(rowCount > Settings.MaximumPixelScrollingRows)
            {
                return 3.0f;
            }
            else
            {
                return UnitHeight;
            }
        }

        public void ValidateGridHeight()
        {
            var i = RowCount();
            var ppr = PixelsPerRow(i);
            var height = ppr * (i);

            if(i > Settings.MaximumPixelScrollingRows && GridBodyContainer.clientHeight > 0)
            {
                height += (float)((GridBodyContainer.clientHeight / UnitHeight) * ppr);
            }
            
            GridBody.style.height = height.ToPx();
            if(BottonOfTable == null)
            {
                BottonOfTable = Div();
                GridBody.AppendChild(BottonOfTable);
            }
            BottonOfTable.SetBounds(0, height, 1, 1);
        }

        public void ValidateGridSize()
        {
            ValidateGridHeight();
            ValidateGridWidth();
        }

        public void ClearHeader()
        {
            GridHeader.Empty();
            GridHeader.AppendChild(RightOfTableHeader);
        }

        public void ClearColumns()
        {
            Columns = new List<GridViewColumn>();
        }

        public void ClearView()
        {
            _disableRender = true;
            Columns = new List<GridViewColumn>();
            VisibleRowHandles = new List<int>();
            SelectedRows = new HardSoftList<bool>(false);
            _dataSource = null;
            _disableRender = false;
            RenderGrid();
        }

        public void ClearBody()
        {
            if(isEditorShown)
            {
                GridBody.Empty(activeEditorElement);
            }
            else
            {
                GridBody.Empty();
            }
            
            GridBody.AppendChildren(RightOfTable, BottonOfTable);
        }

        public void ClearGrid()
        {
            ClearHeader();
            ClearBody();
        }

        public int[] GetSelectedRowHandles()
        {
            List<int> listOfInt = new List<int>();
            int rowCount = RowCount();        
            for(int i = 0; i < rowCount; i++)
            {                
                int index = GetDataSourceRow(i);
                if(SelectedRows.GetValue(index, false))
                {
                    listOfInt.Add(i);
                }
            }
            return listOfInt.ToArray();
        }

        public int[] GetSelectedDataRowHandles()
        {
            List<int> listOfInt = new List<int>();
            int rowCount = RowCount();
            for(int i = 0; i < rowCount; i++)
            {
                int index = GetDataSourceRow(i);
                if(SelectedRows.GetValue(index, false))
                {
                    listOfInt.Add(index);
                }
            }
            return listOfInt.ToArray();
        }

        private int DragIndex = -1;
        private int ResizeIndex = -1;
        private int ResizePageX = 0;
        private HTMLSpanElement ResizeSpan = null;

        private Action<MouseEvent> OnColumnOnClick;
        private Action<Event> OnColumnDragStart;
        private Action<Event> OnColumnDragOver;
        private Action<Event> OnColumnDrop;
        private Action<MouseEvent> OnColumnMouseDown;
        private Action<MouseEvent> OnColumnMouseMove;
        private Action<MouseEvent> OnColumnMouseLeave;

        private Action<Event> OnRowDragStart;

        private void SetupColumn(HTMLSpanElement se, int index, GridViewColumn gcol)
        {
            se.setAttribute("i", Convert.ToString(index));
            se.setAttribute("draggable", "true");
            se.onclick = new HTMLElement.onclickFn(OnColumnOnClick);
            se.ondragstart = new HTMLElement.ondragFn(OnColumnDragStart);
            se.ondragover = new HTMLElement.ondragFn(OnColumnDragOver);
            se.ondrop = new HTMLElement.ondragFn(OnColumnDrop);
            se.onmousedown = new HTMLElement.onclickFn(OnColumnMouseDown);
            se.onmousemove = new HTMLElement.onclickFn(OnColumnMouseMove);
            se.onmouseleave = new HTMLElement.onclickFn(OnColumnMouseLeave);
        }

        private int lastId = -1;

        private int PrevScroll = -1;

        private void ProcessBlur()
        {
            if(PrevScroll != GridBodyContainer.scrollTop)
            {
                GridBody.classList.add("blur");
                if(lastId != -1)
                {
                    clearTimeout(lastId);
                    lastId = -1;
                }

                lastId = (int)setTimeout((a) =>
                {
                    GridBody.classList.remove("blur");
                }, 100);
            }
            PrevScroll = (int)GridBodyContainer.scrollTop;
        }

        private Action<TextInput> FilterRowOnChange;

        private int RenderTime = -1;
        private Action renderGridInternal;
        private bool _disableRender = false;
        public void RenderGrid(bool clear = true)
        {
            if(_disableRender)
                return;

            if(clear)
                CacheRow = new Dictionary<int, HTMLElement>();
            
            if(RenderTime > -1)
            {
                clearTimeout(RenderTime);
                RenderTime = (int)setTimeout(renderGridInternal, 1);
            }
            else
            {
                renderGridInternal();
            }
        }

        public List<Page> GetPages(Layout pageLayout, PageSize pageSize)
        {
            List<Page> Pages = new List<Page>();

            Page activePage = null;

            float yp = 0;
            float xp = 0;

            float width;
            float height;

            if(pageSize == PageSize.A4)
            {
                if(pageLayout == Layout.Landscape)
                {
                    width = 29.7f;
                    height = 21f; 
                }
                else
                {
                    height = 29.7f;
                    width = 21f;
                }
            }else
            {
                if(pageLayout == Layout.Landscape)
                {
                    width = 42f;
                    height = 29.7f;
                }
                else
                {
                    height = 42f;
                    width = 29.7f;
                }
            }

            activePage = new Page();
            Pages.Add(activePage);

            float sep = 0;

            if(ColumnAutoWidth)
                sep = (width / GetVisibleCount());

            int rowHeight = 28;

            for(int x = 0; x < ColumnCount(); x++)
            {                
                if(Columns[x].Visible)
                {
                    var colWidth = ColumnAutoWidth ? sep : Columns[x].Width;
                    yp = 0;

                    var colHeader = new Control();
                    colHeader.Bounds = new Vector4(xp, yp, colWidth, rowHeight);
                    colHeader.Content.textContent = Columns[x].Caption;
                    colHeader.Style.border = "1px solid grey";
                    colHeader.Style.backgroundColor = "lightgrey";
                    colHeader.Style.color = "white";

                    activePage.AppendChild(colHeader);

                    yp += rowHeight;

                    for(int y = 0; y < RowCount(); y++)
                    {
                        var rowCell = new Control();
                        rowCell.Bounds = new Vector4(xp, yp, colWidth, rowHeight);
                        rowCell.Content.textContent =  Columns[x].GetDisplayValueByDataRowHandle(GetDataSourceRow(y));
                        rowCell.Style.border = "1px solid lightgrey";
                        activePage.AppendChild(rowCell);

                        yp += rowHeight;
                    }
                    xp += colWidth;
                }
            }

            return Pages;
        }
    }

    public class SortSetting
    {
        public GridViewColumn Column;
        public GridViewSortMode SortMode;
    }
}