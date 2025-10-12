using System.Collections.Generic;

public class StaticSkill
{
    public Dictionary<string, Static_Skill_t> mDatas = new Dictionary<string, Static_Skill_t>();
    public bool Init()
    {
        CSVReader reader = StaticDataInterface.LoadCSV("Skill");
        int iRowCount = reader.GetRowCount();
        mDatas = new Dictionary<string, Static_Skill_t>();
        for (int i = 0; i < iRowCount; i++)
        {
            Dictionary<string, string> data = reader.GetRow(i);
            Static_Skill_t info = new Static_Skill_t(ref data);
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

    public Static_Skill_t GetSkill(string id)
    {
        mDatas.TryGetValue(id, out Static_Skill_t static_Skill_T);
        return static_Skill_T;
    }
}
