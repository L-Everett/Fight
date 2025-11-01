using System.Collections.Generic;
using UnityEngine;


public class CSVReader
{
    int mCount = 0;
    Dictionary<string, List<string>> mDatas = new Dictionary<string, List<string>>();
    public void LoadCSVFile(List<string[]> texts)
    {
        //获取数据表的键值
        foreach (string text in texts[1])
        {
            mDatas.Add(text, new List<string>());
        }
        //只保留数据
        texts.RemoveRange(0, 3);
        mCount = texts.Count;
        int j;
        foreach (string[] datas in texts)
        {
            j = 0;
            foreach (var item in mDatas)
            {
                if (j >= datas.Length)
                {
                    item.Value.Add("");
                }
                else
                {
                    item.Value.Add(datas[j]);
                    j++;
                }
            }
        }
    }

    public int GetRowCount()
    {
        return mCount;
    }

    public Dictionary<string, string> GetRow(int index)
    {
        if (index > mCount) return null;
        Dictionary<string, string> text = new Dictionary<string, string>();
        foreach (var item in mDatas)
        {
            text.Add(item.Key, item.Value[index]);
        }
        return text;
    }
}

public class StaticDataInterface : MonoBehaviour
{
    private static bool isCreated = false;

    private void Awake()
    {
        if (isCreated)
        {
            Destroy(gameObject);
            return;
        }
        isCreated = true;
        DontDestroyOnLoad(this);
        Init();
    }

    public static StaticCard Card = new StaticCard();
    public static StaticEnemy Enemy = new StaticEnemy();
    public static StaticCharacter Character = new StaticCharacter();
    public static StaticRound Round = new StaticRound();
    public static StaticSkill Skill = new StaticSkill();
    public static StaticTalent Talent = new StaticTalent();

    public static void Init()
    {
        Card.Init();
        Enemy.Init();
        Character.Init();
        Round.Init();
        Skill.Init();
        Talent.Init();
    }

    public static CSVReader LoadCSV(string csvName)
    {
        CSVReader reader = new CSVReader();
        List<string[]> dataList = StaticUtils.LoadCSV(csvName);
        //Debug.Log(csvName);
        reader.LoadCSVFile(dataList);
        return reader;
    }
}

