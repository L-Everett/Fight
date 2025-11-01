using System.Collections.Generic;

public class StaticTalent
{
    public Dictionary<int, Static_Talent_t> mDatas = new Dictionary<int, Static_Talent_t>();
    public bool Init()
    {
        CSVReader reader = StaticDataInterface.LoadCSV("Talent");
        int iRowCount = reader.GetRowCount();
        mDatas = new Dictionary<int, Static_Talent_t>();
        for (int i = 0; i < iRowCount; i++)
        {
            Dictionary<string, string> data = reader.GetRow(i);
            Static_Talent_t info = new Static_Talent_t(ref data);
            mDatas.Add(info.Id, info);
        }
        return true;
    }

    public void DeInit()
    {
        mDatas.Clear();
    }

    public int GetCount() { return mDatas.Count; }

    public Dictionary<int, Static_Talent_t> GetTalentById(string id)
    {
        Dictionary<int, Static_Talent_t> res = new Dictionary<int, Static_Talent_t>();
        foreach (var item in mDatas)
        {
            if(item.Value.Character == id)
            {
                res.Add(item.Value.Lv, item.Value);
            }
        }
        return res;
    }
}
