using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentCtrl : MonoBehaviour
{
    public Image[] cHeads;
    public TextMeshProUGUI cName;
    public Material hight;
    private List<string> cIds;
    private int curId;
    private Dictionary<string, bool> needUpdate;

    //最终属性展示
    [Header("最终加成")]
    public TextMeshProUGUI finalHp;
    public TextMeshProUGUI finalAtk;
    public TextMeshProUGUI finalCrit;
    public TextMeshProUGUI finalCritDam;

    [Header("内容")]
    public TextMeshProUGUI lvText;
    public TextMeshProUGUI contentText;

    [Header("升级相关")]
    public TextMeshProUGUI needGem;
    public GameObject gemIcon;
    public TextMeshProUGUI curGem;

    private void Start()
    {
        cIds = StaticDataInterface.Character.GetAllIds();
        curId = 0;
        needUpdate = new Dictionary<string, bool>();
        if(DataModel.Instance.mTalentAdd.Count == 0)
        {
            for(int i = 0; i < cIds.Count; i++)
            {
                DataModel.Instance.mTalentAdd.Add(cIds[i], null);
                DataModel.Instance.mTalentLv.Add(cIds[i], 0);
            }
        }
        for (int i = 0; i < cIds.Count; i++)
        {
            needUpdate.Add(cIds[i], true);
        }
        SelectCharacter(0);
        FreshGemUI();
        CloseTalentTree();
    }

    //刷新最终天赋树加成
    void FreshFinalAdd()
    {
        string id = cIds[curId];
        cName.text = StaticDataInterface.Character.GetCharacter(id).Name;
        float hpAdd = 0;
        float atkAdd = 0;
        float critAdd = 0;
        float critDamAdd = 0;
        //临时有值就直接拿
        RunningManager.Instance.mTalentAdd.TryGetValue(id, out var attrAdd);
        if(attrAdd != null && !needUpdate[id])
        {
            hpAdd = attrAdd[AttributeType.Hp];
            atkAdd = attrAdd[AttributeType.Attack];
            critAdd = attrAdd[AttributeType.CritRate];
            critDamAdd = attrAdd[AttributeType.CritDamage];
        }
        //没有就先计算再存临时
        else
        {
            needUpdate[id] = false;
            var add = DataModel.Instance.mTalentAdd[id];
            if (add != null)
            {
                foreach (var attr in add)
                {
                    if (attr.attributeType == AttributeType.Hp)
                    {
                        hpAdd += attr.value;
                    }
                    else if (attr.attributeType == AttributeType.Attack)
                    {
                        atkAdd += attr.value;
                    }
                    else if (attr.attributeType == AttributeType.CritRate)
                    {
                        critAdd += attr.value;
                    }
                    else if (attr.attributeType == AttributeType.CritDamage)
                    {
                        critDamAdd += attr.value;
                    }
                }
            }
            RunningManager.Instance.mTalentAdd[id] = new Dictionary<AttributeType, float>();
            RunningManager.Instance.mTalentAdd[id].Add(AttributeType.Hp, hpAdd);
            RunningManager.Instance.mTalentAdd[id].Add(AttributeType.Attack, atkAdd);
            RunningManager.Instance.mTalentAdd[id].Add(AttributeType.CritRate, critAdd);
            RunningManager.Instance.mTalentAdd[id].Add(AttributeType.CritDamage, critDamAdd);
        }

        finalHp.text = hpAdd.ToString("F0");
        finalAtk.text = atkAdd.ToString("F0");
        finalCrit.text = (critAdd * 100).ToString("F0") + "%";
        finalCritDam.text = (critDamAdd * 100).ToString("F0") + "%";
    }

    void FreshNextAttr()
    {
        string id = cIds[curId];
        var lvDatas = StaticDataInterface.Talent.GetTalentById(id);
        int curLv = DataModel.Instance.mTalentLv[id];
        lvText.text = "Lv." + curLv;
        StringBuilder des = new StringBuilder();
        if (curLv == lvDatas.Count)
        {
            des.Append("神赐已满级！");
        }
        else
        {
            //查找下一级的数据
            var curLvData = lvDatas[curLv + 1];
            for (int i = 0; i < curLvData.Attr.Count; i++)
            {
                if ((AttributeType)curLvData.Attr[i] == AttributeType.Hp)
                {
                    des.Append($"基础生命值 +{curLvData.Value[i]}\n");
                }
                else if ((AttributeType)curLvData.Attr[i] == AttributeType.Attack)
                {
                    des.Append($"基础攻击力 +{curLvData.Value[i]}\n");
                }
                else if ((AttributeType)curLvData.Attr[i] == AttributeType.CritRate)
                {
                    des.Append($"基础暴击率 +{curLvData.Value[i] * 100 : F0}%\n");
                }
                else if ((AttributeType)curLvData.Attr[i] == AttributeType.CritDamage)
                {
                    des.Append($"基础暴击伤害 +{curLvData.Value[i] * 100 : F0}%\n");
                }
            }
        }
        contentText.text = des.ToString();
    }

    public void CloseTalentTree()
    {
        gameObject.SetActive(false);
    }

    public void SelectCharacter(int i)
    {
        curId = i;
        FreshFinalAdd();
        FreshNextAttr();
        for (int j = 0; j < cHeads.Length; j++)
        {
            if (i == j)
            {
                cHeads[j].material = hight;
            }
            else
            {
                cHeads[j].material = null;
            }
        }
    }

    public void UpLv()
    {
        string id = cIds[curId];
        var lvDatas = StaticDataInterface.Talent.GetTalentById(id);
        int curLv = DataModel.Instance.mTalentLv[id];
        //已满级
        if (curLv == lvDatas.Count)
        {
            MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "已满级！");
            return;
        }
        var nextLvData = StaticDataInterface.Talent.GetTalentById(id)[curLv + 1];
        var dataModel = DataModel.Instance;
        //钻石够就升级，存入属性
        if (dataModel.mGemCount >= nextLvData.Gem)
        {
            dataModel.mGemCount -= nextLvData.Gem;
            if (dataModel.mTalentAdd[id] == null)
            {
                dataModel.mTalentAdd[id] = new List<AttributeModifier>();
            }
            for (int i = 0; i < nextLvData.Attr.Count; i++)
            {
                dataModel.mTalentAdd[id].Add(
                        new AttributeModifier()
                        {
                            attributeType = (AttributeType)nextLvData.Attr[i],
                            value = nextLvData.Value[i],
                            isPercentage = false
                        });
            }
            dataModel.mTalentLv[id]++;
            //刷新UI
            needUpdate[id] = true;
            FreshGemUI();
            FreshFinalAdd();
            FreshNextAttr();
        }
        else
        {
            MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "钻石不足！");
        }
    }

    void FreshGemUI()
    {
        string id = cIds[curId];
        var lvDatas = StaticDataInterface.Talent.GetTalentById(id);
        int curLv = DataModel.Instance.mTalentLv[id];
        //已满级
        if (curLv == lvDatas.Count)
        {
            needGem.text = "";
            gemIcon.SetActive(false);
        }
        else
        {
            gemIcon.SetActive(true);
            var nextLvData = StaticDataInterface.Talent.GetTalentById(id)[curLv + 1];
            needGem.text = "X" + nextLvData.Gem;
        }
        curGem.text = DataModel.Instance.mGemCount.ToString();
    }
}
