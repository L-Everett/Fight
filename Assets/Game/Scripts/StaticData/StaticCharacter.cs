using System.Collections.Generic;

public class StaticCharacter
{
    public Dictionary<string, Static_Character_t> mDatas = new Dictionary<string, Static_Character_t>();
    public bool Init()
    {
        CSVReader reader = StaticDataInterface.LoadCSV("Character");
        int iRowCount = reader.GetRowCount();
        mDatas = new Dictionary<string, Static_Character_t>();
        for (int i = 0; i < iRowCount; i++)
        {
            Dictionary<string, string> data = reader.GetRow(i);
            Static_Character_t info = new Static_Character_t(ref data);
            if (info.Id == string.Empty) continue;
            mDatas.Add(info.Id, info);
        }
        return true;
    }

    public void DeInit()
    {
        mDatas.Clear();
    }

    public int GetCount() { return mDatas.Count; }

    public Static_Character_t GetCharacter(string id)
    {
        mDatas.TryGetValue(id, out Static_Character_t static_Character_T);
        return static_Character_T;
    }

    public List<string> GetAllIds()
    {
        List<string> list = new List<string>();
        foreach (string id in mDatas.Keys)
        {
            list.Add(id);
        }
        return list;
    }
}
