using System.Collections.Generic;
using UnityEngine;

public class StaticUtils
{
    public static List<string[]> LoadCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/{fileName}");

        if (csvFile == null)
        {
            Debug.LogError($"无法加载CSV文件: CSV/{fileName}");
            return null;
        }

        List<string[]> dataList = ParseCSVContent(csvFile.text, ',');
        return dataList;
    }

    public static List<string[]> ParseCSVContent(string csvContent, char sep)
    {
        List<string[]> dataList = new List<string[]>();

        if (string.IsNullOrEmpty(csvContent))
        {
            Debug.LogError("CSV内容为空");
            return dataList;
        }

        // 按行分割
        string[] lines = csvContent.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 处理可能的\r字符
            if (line.Contains("\r"))
            {
                line = line.Replace("\r", "");
            }

            string[] fields = line.Split(sep);
            dataList.Add(fields);
        }

        return dataList;
    }
}