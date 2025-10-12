using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StaticUtils 
{
    public static List<string[]> LoadCSV(string fileName)
    {
        string csvPath = Application.dataPath + "\\Game\\CSV";
        if (!Directory.Exists(csvPath))
        {
            Debug.LogError("当前路径不存在" + csvPath);
            return null;
        }

        string filePath = csvPath + "\\" + fileName + ".csv";
        List<string[]> dataList = ParseCSV(filePath, ',');
        return dataList;
    }

    public static List<string[]> ParseCSV(string csvPath, char sep)
    {
        List<string[]> dataList = new List<string[]>();
        if (!File.Exists(csvPath))
        {
            Debug.LogError("当前路径不存在" + csvPath);
            return dataList;
        }
        StreamReader reader = new StreamReader(csvPath, System.Text.Encoding.GetEncoding("utf-8"));
        string stringData = reader.ReadToEnd();
        string[] strArray = stringData.Split('\n');
        for (int i = 0; i < strArray.Length; i++)
        {
            string str = strArray[i];
            if (str == "") continue;
            string[] strA = str.Split('\r');
            string[] strB = strA[0].Split(sep);
            dataList.Add(strB);
        }
        reader.Close();
        return dataList;
    }
}
