// TextUtil.cs
// Solar Dynamics 2026

#region

using System;
using System.Linq;
using System.Text;

#endregion

namespace Solar.UI.Util;

public class TextUtil
{
    public static string[] FormatTable(string[] header, params string[][] data)
    {
        int columns = header.Length;
        int rows = data.Length;

        if (columns <= 0) throw new ArgumentException("Header must contain at least one column.");

        int[] maxLength = new int[columns];

        for (int i = 0; i < columns; i++) maxLength[i] = header[i].Length;

        for (int i = 0; i < rows; i++)
        for (int j = 0; j < columns; j++)
            maxLength[j] = Math.Max(maxLength[j], data[i][j].Length);

        StringBuilder line = new();

        string[] result = new string[rows + 2];

        // result[0] = string.Join(" | ", header);
        for (int c = 0; c < columns; c++)
        {
            line.Append(header[c].PadRight(maxLength[c]));
            if (c < columns - 1) line.Append(" | ");
        }

        result[0] = line.ToString();
        line.Clear();


        // string separator = string.Join(" | ", new string[columns].Select(_ => new string('-', 1)));
        for (int c = 0; c < columns; c++)
        {
            line.Append("".PadRight(maxLength[c], char.Parse("-")));
            if (c < columns - 1) line.Append(" | ");
        }

        result[1] = line.ToString();
        line.Clear();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                line.Append(data[i][j].PadLeft(maxLength[j]));
                if (j < columns - 1) line.Append(" | ");
            }

            result[i + 2] = line.ToString();
            line.Clear();
        }

        return result;
    }

    public static string FormatTableJoined(string[] header, params string[][] data)
    {
        return string.Join("\n", FormatTable(header, data).ToList().ConvertAll(line => $"| {line} |"));
    }

    private static StringBuilder sb = new();

    public static string FormatLabel(string left = "", string right = "")
    {
        sb.Clear();
        if (!string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right))
        {
            sb.Append($"{left}<line-height=0>\n<align=right>{right}");
            // sb.Append($"{left}\n<align=right>{right}");
        }
        else if (!string.IsNullOrEmpty(left))
        {
            sb.Append($"{left}");
        } else if (!string.IsNullOrEmpty(right))
        {
            sb.Append($"<align=right>{right}");
        }

        sb.Append("<line-height=1em>");
        // sb.Append("<align=left><color=reset>");
        return sb.ToString();
    }
}