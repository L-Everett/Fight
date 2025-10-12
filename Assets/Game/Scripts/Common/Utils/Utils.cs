using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class Utils
{
    /// <summary>
    /// 安全地将字符串转换为int，转换失败返回0
    /// </summary>
    public static int IntParseByString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return 0;

        return int.TryParse(str, out int result) ? result : 0;
    }

    /// <summary>
    /// 安全地将字符串转换为float，转换失败返回0
    /// </summary>
    public static float FloatParseByString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return 0f;

        return float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result : 0f;
    }

    /// <summary>
    /// 安全地将字符串转换为bool，支持"true"/"false"、"1"/"0"等常见格式
    /// </summary>
    public static bool BooleanParseByString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        // 尝试标准布尔解析
        if (bool.TryParse(str, out bool result))
            return result;

        // 尝试数字格式的布尔值 (1=true, 0=false)
        if (int.TryParse(str, out int intValue))
            return intValue != 0;

        // 尝试小写字符串比较
        string lowerStr = str.ToLowerInvariant();
        if (lowerStr == "true" || lowerStr == "yes" || lowerStr == "y" || lowerStr == "1")
            return true;
        if (lowerStr == "false" || lowerStr == "no" || lowerStr == "n" || lowerStr == "0")
            return false;

        return false;
    }

    /// <summary>
    /// 安全地将字符串转换为long，转换失败返回0
    /// </summary>
    public static long LongParseByString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return 0L;

        return long.TryParse(str, out long result) ? result : 0L;
    }

    /// <summary>
    /// 将分号分隔的字符串解析为int列表
    /// </summary>
    public static List<int> ParseIntStrings(string str, char separator = ';')
    {
        List<int> result = new List<int>();

        if (string.IsNullOrEmpty(str))
            return result;

        string[] parts = str.Split(separator);
        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part.Trim()))
                continue;

            if (int.TryParse(part.Trim(), out int value))
                result.Add(value);
        }

        return result;
    }

    /// <summary>
    /// 将分号分隔的字符串解析为float列表
    /// </summary>
    public static List<float> ParseFloatStrings(string str, char separator = ';')
    {
        List<float> result = new List<float>();

        if (string.IsNullOrEmpty(str))
            return result;

        string[] parts = str.Split(separator);
        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part.Trim()))
                continue;

            if (float.TryParse(part.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                result.Add(value);
        }

        return result;
    }

    /// <summary>
    /// 将分号分隔的字符串解析为bool列表
    /// </summary>
    public static List<bool> ParseBooleanStrings(string str, char separator = ';')
    {
        List<bool> result = new List<bool>();

        if (string.IsNullOrEmpty(str))
            return result;

        string[] parts = str.Split(separator);
        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part.Trim()))
                continue;

            result.Add(BooleanParseByString(part.Trim()));
        }

        return result;
    }

    /// <summary>
    /// 将分号分隔的字符串解析为long列表
    /// </summary>
    public static List<long> ParseLongStrings(string str, char separator = ';')
    {
        List<long> result = new List<long>();

        if (string.IsNullOrEmpty(str))
            return result;

        string[] parts = str.Split(separator);
        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part.Trim()))
                continue;

            if (long.TryParse(part.Trim(), out long value))
                result.Add(value);
        }

        return result;
    }

    /// <summary>
    /// 将分号分隔的字符串解析为string列表（去除空白字符）
    /// </summary>
    public static List<string> ParseStrings(string str, char separator = ';')
    {
        if (string.IsNullOrEmpty(str))
            return new List<string>();

        return str.Split(separator)
                 .Select(s => s.Trim())
                 .Where(s => !string.IsNullOrEmpty(s))
                 .ToList();
    }

    // 以下是一些额外的实用方法，可能会对你有帮助

    /// <summary>
    /// 安全转换为int，支持默认值
    /// </summary>
    public static int IntParseByString(string str, int defaultValue)
    {
        if (string.IsNullOrEmpty(str))
            return defaultValue;

        return int.TryParse(str, out int result) ? result : defaultValue;
    }

    /// <summary>
    /// 检查字符串是否表示真值（支持多种格式）
    /// </summary>
    public static bool IsTrueValue(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        string lowerStr = str.ToLowerInvariant();
        return lowerStr == "true" || lowerStr == "yes" || lowerStr == "y" || lowerStr == "1";
    }

    /// <summary>
    /// 轮盘随机
    /// </summary>
    /// <param name="weights"></param>
    /// <returns></returns>
    public static int GetRandomIndex(List<int> weights)
    {
        if (weights == null || weights.Count == 0)
        {
            return -1;
        }

        int totalWeight = 0;
        foreach (int weight in weights)
        {
            if (weight < 0)
            {
                return -1;
            }
            totalWeight += weight;
        }

        if (totalWeight == 0)
        {
            return UnityEngine.Random.Range(0, weights.Count);
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue < cumulativeWeight)
            {
                return i;
            }
        }

        return weights.Count - 1;
    }
}