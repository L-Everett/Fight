using TMPro;
using UnityEngine;

public class CharacterInfoCtrl : MonoBehaviour
{
    public GameObject root;
    [Header("基础")]
    public TextMeshProUGUI hpBase;
    public TextMeshProUGUI atkBase;
    public TextMeshProUGUI critBase;
    public TextMeshProUGUI critDmgBase;
    [Header("神赐")]
    public TextMeshProUGUI hpTalent;
    public TextMeshProUGUI atkTalent;
    public TextMeshProUGUI critTalent;
    public TextMeshProUGUI critDmgTalent;
    [Header("局内增益")]
    public TextMeshProUGUI hpBuff;
    public TextMeshProUGUI atkBuff;
    public TextMeshProUGUI critBuff;
    public TextMeshProUGUI critDmgBuff;
    public TextMeshProUGUI dmgAddBuff;
    [Header("最终")]
    public TextMeshProUGUI hpFinal;
    public TextMeshProUGUI atkFinal;
    public TextMeshProUGUI critFinal;
    public TextMeshProUGUI critDmgFinal;
    public TextMeshProUGUI dmgAddFinal;

    private bool isUIVisible = false;
    private BattleManager battleManager;

    private void Start()
    {
        battleManager = BattleManager.Instance;
        HideInfo();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowInfo();
            isUIVisible = true;
        }

        if (Input.GetKeyUp(KeyCode.Tab) && isUIVisible)
        {
            HideInfo();
            isUIVisible = false;
        }
    }

    void ShowInfo()
    {
        root.SetActive(true);
        FreshUI();
    }

    void HideInfo()
    {
        root.SetActive(false);
    }

    void FreshUI()
    {
        string cId = RunningManager.Instance.mCurrentCharacter;
        //基础
        var cData = StaticDataInterface.Character.GetCharacter(cId);
        hpBase.text = cData.HP.ToString("F0");
        atkBase.text = cData.ATK.ToString("F0");
        critBase.text = (cData.Crit * 100).ToString("F0") + "%";
        critDmgBase.text = (cData.CritDamage * 100).ToString("F0") + "%";

        //神赐
        var tData = RunningManager.Instance.mTalentAdd[cId];
        if (tData != null)
        {
            tData.TryGetValue(AttributeType.Hp, out float tHp);
            tData.TryGetValue(AttributeType.Attack, out float tAtk);
            tData.TryGetValue(AttributeType.CritRate, out float tCrit);
            tData.TryGetValue(AttributeType.CritDamage, out float tCritDmg);
            hpTalent.text = "+" + tHp.ToString("F0");
            atkTalent.text = "+" + tAtk.ToString("F0");
            critTalent.text = "+" + (tCrit * 100).ToString("F0") + "%";
            critDmgTalent.text = "+" + (tCritDmg * 100).ToString("F0") + "%";
        }

        //加上神赐后的基础值
        var character = battleManager.character;
        var attr = character.attributes;
        float hpBaseValue = attr.GetBaseAttr(AttributeType.Hp);
        float atkBaseValue = attr.GetBaseAttr(AttributeType.Attack);
        float critBaseValue = attr.GetBaseAttr(AttributeType.CritRate) * 100;
        float critDmgBaseValue = attr.GetBaseAttr(AttributeType.CritDamage) * 100;

        //最终
        float finalHp = attr.GetFinalAttr(AttributeType.Hp, character.attributeModifiers);
        float finalAtk = attr.GetFinalAttr(AttributeType.Attack, character.attributeModifiers);
        float finalCrit = attr.GetFinalAttr(AttributeType.CritRate, character.attributeModifiers) * 100;
        float finalCritDmg = attr.GetFinalAttr(AttributeType.CritDamage, character.attributeModifiers) * 100;
        float finalDmgAdd = attr.GetFinalAttr(AttributeType.DamageAddRate, character.attributeModifiers) * 100;
        hpFinal.text = finalHp.ToString("F0");
        atkFinal.text = finalAtk.ToString("F0");
        critFinal.text = finalCrit.ToString("F0") + "%";
        critDmgFinal.text = finalCritDmg.ToString("F0") + "%";
        dmgAddFinal.text = finalDmgAdd.ToString("F0") + "%";

        //局内增益
        float hpAdd = finalHp - hpBaseValue;
        float atkAdd = finalAtk - atkBaseValue;
        float critAdd = finalCrit - critBaseValue;
        float critDmgAdd = finalCritDmg - critDmgBaseValue;
        hpBuff.text = "+" + hpAdd.ToString("F0");
        atkBuff.text = "+" + atkAdd.ToString("F0");
        critBuff.text = "+" + critAdd.ToString("F0") + "%";
        critDmgBuff.text = "+" + critDmgAdd.ToString("F0") + "%";
        dmgAddBuff.text = "+" + finalDmgAdd.ToString("F0") + "%";
    }
}
