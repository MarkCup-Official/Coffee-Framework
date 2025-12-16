using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class CSVHelper
{
    /// <summary>
    /// 读取 CSV 文件
    /// </summary>
    /// <param name="filePath">CSV 文件路径</param>
    /// <param name="hasHeader">是否包含表头</param>
    /// <returns>每一行是一个 string 列表</returns>
    public static List<List<string>> Read(string filePath, bool hasHeader = true)
    {
        var result = new List<List<string>>();

        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        using (var reader = new StreamReader(filePath, Encoding.UTF8))
        {
            bool skipHeader = hasHeader;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;

                if (skipHeader)
                {
                    skipHeader = false;
                    continue;
                }

                result.Add(ParseLine(line));
            }
        }

        return result;
    }

    public static List<List<string>> ReadFromText(string csvText, bool hasHeader = true)
    {
        var result = new List<List<string>>();

        if (string.IsNullOrEmpty(csvText))
            return result;

        using (var reader = new StringReader(csvText))
        {
            bool skipHeader = hasHeader;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                    break;

                if (string.IsNullOrEmpty(line))
                    continue;

                if (skipHeader)
                {
                    skipHeader = false;
                    continue;
                }

                result.Add(ParseLine(line));
            }
        }

        return result;
    }

    /// <summary>
    /// 写入 CSV 文件
    /// </summary>
    /// <param name="filePath">输出路径</param>
    /// <param name="rows">数据</param>
    /// <param name="header">表头（可选）</param>
    public static void Write(string filePath, List<List<string>> rows, List<string> header = null)
    {
        using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            if (header != null)
                writer.WriteLine(BuildLine(header));

            foreach (var row in rows)
                writer.WriteLine(BuildLine(row));
        }
    }

    // ------------------ 内部方法 ------------------

    private static List<string> ParseLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());
        return result;
    }

    private static string BuildLine(List<string> fields)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < fields.Count; i++)
        {
            if (i > 0)
                sb.Append(",");

            sb.Append(Escape(fields[i]));
        }

        return sb.ToString();
    }

    private static string Escape(string field)
    {
        if (field == null)
            return "";

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}
