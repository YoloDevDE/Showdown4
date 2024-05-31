using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Showdown4.Utils;

public static class TableFormatter
{
    private static bool IsNumericOrTimestamp(string input)
    {
        string timestampRegex = @"^\d{1,2}:\d{2}(:\d{2})?(\.\d{1,3})?$";
        string numberRegex = @"^[+-]?\d+([\.,]\d+)?$";
        return Regex.IsMatch(input, numberRegex) || Regex.IsMatch(input, timestampRegex);
    }

    private static int[] CalculateColumnWidths(List<List<string>> table)
    {
        int[] columnWidths = new int[table[0].Count];
        for (int col = 0; col < table[0].Count; col++)
        {
            columnWidths[col] = table.Max(row => row[col].Length);
        }

        return columnWidths;
    }

    private static string FormatRow(List<string> row, int[] columnWidths, char delimiter)
    {
        string result = "";
        for (int col = 0; col < row.Count; col++)
        {
            if (IsNumericOrTimestamp(row[col]))
            {
                result += row[col].PadLeft(columnWidths[col]);
            }
            else
            {
                result += row[col].PadRight(columnWidths[col]);
            }

            if (col < row.Count - 1)
            {
                result += $" {delimiter} ";
            }
        }

        return result;
    }

    public static string FormattingScoreboard(List<List<string>> table, char delimiter = ' ')
    {
        if (table == null || table.Count == 0)
        {
            return "";
        }

        int[] columnWidths = CalculateColumnWidths(table);
        string result = "";

        foreach (List<string> row in table)
        {
            result += FormatRow(row, columnWidths, delimiter) + "<br>";
        }

        return result.TrimEnd("<br>".ToCharArray());
    }
}