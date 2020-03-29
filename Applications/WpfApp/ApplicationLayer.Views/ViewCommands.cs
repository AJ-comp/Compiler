using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ApplicationLayer.Views
{
    public static class ViewCommands
    {
        /// This command shows parsing table
        public static readonly RelayCommand<Tuple<object, object, object, RoutedEventArgs>> ShowParsingTableCommand =
            new RelayCommand<Tuple<object, object, object, RoutedEventArgs>>((param) =>
            {
                int recentRowIdx = -1;
                int recentColIdx = -1;
                ToolTip toolTip = new ToolTip();

                var windowsFormsHost = param.Item2 as WindowsFormsHost;

                windowsFormsHost.Child = new DataGridView();
                var dataGridView = windowsFormsHost.Child as DataGridView;
                dataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dataGridView.DataSource = param.Item1 as DataView;
                dataGridView.CellMouseEnter += (s, e) =>
                {
                    if (recentColIdx == e.ColumnIndex && recentRowIdx == e.RowIndex) return;
                    recentColIdx = e.ColumnIndex;
                    recentRowIdx = e.RowIndex;

                    toolTip.Hide(dataGridView);
                    if (e.ColumnIndex != 0 || e.RowIndex == -1) return;

                    dataGridView.ShowCellToolTips = false;

                    var cell = dataGridView[e.ColumnIndex, e.RowIndex];

                    var tooltipData = param.Item3 as List<string>;

                    var data = tooltipData[(Convert.ToInt32(cell.Value.ToString().Substring(1)))];
                    var lineCount = Regex.Matches(data, Environment.NewLine).Count;
                    if (lineCount == 0 || lineCount == -1) lineCount = 1;

                    var popDelay = 3000 * lineCount;
                    if (popDelay > 30000) popDelay = 30000;
                    toolTip.Show(data, dataGridView, popDelay);
                };
            }, (condition) =>
            {
                return true;
            });
    }
}
