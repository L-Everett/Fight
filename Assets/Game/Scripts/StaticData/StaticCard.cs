using System.Collections.Generic;

public class StaticCard
{
    public Dictionary<string, Static_Card_t> mDatas = new Dictionary<string, Static_Card_t>();
    public bool Init()
    {
        CSVReader reader = StaticDataInterface.LoadCSV("Card");
        int iRowCount = reader.GetRowCount();
        mDatas = new Dictionary<string, Static_Card_t>();
        for (int i = 0; i < iRowCount; i++)
        {
            Dictionary<string, string> data = reader.GetRow(i);
            Static_Card_t info = new Static_Card_t(ref data);
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

    public Static_Card_t GetCard(string id)
    {
        mDatas.TryGetValue(id, out Static_Card_t static_Card_T);
        return static_Card_T;
    }

    public (List<string>, List<int>) GetCardsByType(CardType cardType)
    {
        List<string> ids = new List<string>();
        List<int> weights = new List<int>();
        int type = (int)cardType;
        foreach (var card in mDatas)
        {
            if(card.Value.Type == type)
            {
                ids.Add(card.Value.Id); 
                weights.Add(card.Value.Weight);
            }
        }
        return (ids, weights);
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
