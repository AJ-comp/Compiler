﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace AJ.Common.Helpers
{
    public static class DataTableHelper
    {
        public static void ToCSV(this DataTable dataTable, string fileFullPath)
        {
            var lines = new List<string>();

            string[] columnNames = dataTable.Columns
                                                              .Cast<DataColumn>()
                                                              .Select(column => column.ColumnName)
                                                              .ToArray();

            var header = string.Join(",", columnNames.Select(name => $"\"{name}\""));
            lines.Add(header);

            var valueLines = dataTable.AsEnumerable()
                .Select(row => string.Join(",", row.ItemArray.Select(val => $"\"{val}\"")));

            lines.AddRange(valueLines);

//            using var writer = new StreamWriter(fileFullPath);
            File.WriteAllLines(fileFullPath, lines);
        }
    }
}
