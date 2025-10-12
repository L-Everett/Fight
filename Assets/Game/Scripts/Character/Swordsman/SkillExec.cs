using System.Collections.Generic;
using UnityEngine;

public class SkillExec : MonoBehaviour, IMsgHandler
{
    public CharacterBase character;
    private BattleManager battleManager;
    private Transform effectRoot;
    //回合初执行的 - skillId
    private List<string> roundFreshSkills = new List<string>();
    //被动技能 - uid, skillId
    private Dictionary<int, string> passiveSkills = new Dictionary<int, string>();
    //主动技能 - skillId
    private List<string> activeSkills = new List<string>();

    // 方法名到委托的映射
    private delegate void ReleaseSkill();
    private Dictionary<string, ReleaseSkill> skills = new Dictionary<string, ReleaseSkill>();
    // 删除技能
    private delegate void DeleteSkill();
    private Dictionary<string, DeleteSkill> deleteSkills = new Dictionary<string, DeleteSkill>();
    // 主动技能的冷却值
    private delegate void SetCool();
    private Dictionary<string, SetCool> setCoolSkills = new Dictionary<string, SetCool>();
    // 主动技能的cd
    private delegate bool Cool(float time);
    private Dictionary<string, Cool> coolSkills = new Dictionary<string, Cool>();

    public void Handle(string msg, object obj)
    {
        #region 添加技能
        if (msg == Constant.MSG_NOTIFY_SKILL_ADD)
        {
            var data = ((string, int))obj;
            string cardID = data.Item1;
            var skillID = StaticDataInterface.Card.GetCard(cardID).SkillId;
            var skillData = StaticDataInterface.Skill.GetSkill(skillID);
            //主动技能
            if(skillData.Type == 1)
            {
                activeSkills.Add(skillID);
                ActiveSkillSetSetCool(skillID);
            }
            //被动技能
            else if (skillData.Type == 2)
            {
                passiveSkills.Add(data.Item2, skillID);
                RunPassiveSkill(skillID);
            }
            //回合技能
            else if (skillData.Type == 3)
            {
                roundFreshSkills.Add(skillID);
                RunSelectRoundSkill(skillID);
            }
        }
        #endregion

        #region 删除技能
        else if (msg == Constant.MSG_NOTIFY_SKILL_END)
        {
            var data = ((string, int))obj;
            string skillID = data.Item1;
            var skillData = StaticDataInterface.Skill.GetSkill(skillID);
            //被动技能
            if (skillData.Type == 2)
            {
                DeletePassiveSkill(skillID);
                passiveSkills.Remove(data.Item2);
            }
        }
        #endregion

        #region 回合增益技能刷新
        else if (msg == Constant.MSG_NOTIFY_ROUND_FRESH)
        {
            RunRoundSkills();
        }
        #endregion
    }

    private void Start()
    {
        battleManager = BattleManager.Instance;
        effectRoot = GameObject.Find("EffectRoot").transform;
        s002Ctrl = effectRoot.Find("S002").GetComponent<S002>();

        MsgManager.Instance.AddMsgListener(Constant.MSG_NOTIFY_SKILL_ADD, this);
        MsgManager.Instance.AddMsgListener(Constant.MSG_NOTIFY_SKILL_END, this);
        MsgManager.Instance.AddMsgListener(Constant.MSG_NOTIFY_ROUND_FRESH, this);

        //增加技能
        skills.Add("S001", S001);
        skills.Add("S002", S002);
        skills.Add("S003", S003);
        skills.Add("S004", S004);
        skills.Add("S005", S005);
        skills.Add("S006", S006);
        skills.Add("S007", S007);
        skills.Add("S008", S008);
        skills.Add("S009", S009);
        skills.Add("S010", S010);
        skills.Add("S011", S011);
        skills.Add("S012", S012);
        skills.Add("S013", S013);
        skills.Add("S014", S014);
        skills.Add("S015", S015);
        skills.Add("S016", S016);
        skills.Add("S017", S017);
        skills.Add("S018", S018);
        skills.Add("S019", S019);
        skills.Add("S020", S020);

        //删除技能
        deleteSkills.Add("S013", DS013);
        deleteSkills.Add("S014", DS014);
        deleteSkills.Add("S015", DS015);
        deleteSkills.Add("S019", DS019);
        deleteSkills.Add("S020", DS020);

        //主动技能设置CD
        setCoolSkills.Add("S002", S002SetCool);

        //主动技能冷却
        coolSkills.Add("S002", S002Cool);
    }

    private void Update()
    {
        if (battleManager.currentStage != BattleStage.Battle) return;
        RunActivateSkills();
    }

    private void OnDestroy()
    {
        MsgManager.Instance.RemoveALLMsgListener(this);
    }

    #region 回合初运行
    //运行已获得的
    void RunRoundSkills()
    {
        foreach (var skill in roundFreshSkills)
        {
            if (skills.TryGetValue(skill, out ReleaseSkill method))
            {
                method();
            }
        }
    }

    //运行指定的
    void RunSelectRoundSkill(string skillID)
    {
        if (skills.TryGetValue(skillID, out ReleaseSkill method))
        {
            method();
        }
    }

    #region S001 - 万剑归宗
    void S001()
    {
        if (roundFreshSkills.Contains("S001"))
        {
            MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "万剑归宗");
            float rate = StaticDataInterface.Skill.GetSkill("S001").Value;
            character.AddAttributeModifier(new AttributeModifier(AttributeType.Attack, rate, true));
        }
    }
    #endregion

    #region S003 - 疗愈
    void S003()
    {
        if (roundFreshSkills.Contains("S003"))
        {
            MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "疗愈");
            float rate = StaticDataInterface.Skill.GetSkill("S003").Value;
            float heal = character.attributes.GetFinalAttr(AttributeType.Hp, character.attributeModifiers) * rate;
            character.Heal(heal);
        }
    }
    #endregion

    #endregion

    #region 被动技能
    void RunPassiveSkill(string skillID)
    {
        if (skills.TryGetValue(skillID, out ReleaseSkill release))
        {
            release();
        }
    }
    void DeletePassiveSkill(string skillID)
    {
        if (deleteSkills.TryGetValue(skillID, out DeleteSkill delete))
        {
            delete();
        }
    }

    #region S004 - 体质+
    void S004()
    {
        float value = StaticDataInterface.Skill.GetSkill("S004").Value;
        float heal = value * character.attributes.GetBaseAttr(AttributeType.Hp);
        character.AddAttributeModifier(new AttributeModifier(AttributeType.Hp, value, true));
        character.Heal(heal);
    }
    #endregion

    #region S005 - 体质++
    void S005()
    {
        float value = StaticDataInterface.Skill.GetSkill("S005").Value;
        float heal = value * character.attributes.GetBaseAttr(AttributeType.Hp);
        character.AddAttributeModifier(new AttributeModifier(AttributeType.Hp, value, true));
        character.Heal(heal);
    }
    #endregion

    #region S006 - 体质+++
    void S006()
    {
        float value = StaticDataInterface.Skill.GetSkill("S006").Value;
        float heal = value * character.attributes.GetBaseAttr(AttributeType.Hp);
        character.AddAttributeModifier(new AttributeModifier(AttributeType.Hp, value, true));
        character.Heal(heal);
    }

    #endregion

    #region S007 - 小幅强化
    void S007()
    {
        float value = StaticDataInterface.Skill.GetSkill("S007").Value;
        character.AddAttributeModifier(new AttributeModifier(AttributeType.DamageAddRate, value, false));
    }
    #endregion

    #region S008 - 中幅强化
    void S008()
    {
        float value = StaticDataInterface.Skill.GetSkill("S008").Value;
        character.AddAttributeModifier(new AttributeModifier(AttributeType.DamageAddRate, value, false));
    }
    #endregion

    #region S009 - 超限强化
    void S009()
    {
        float value = StaticDataInterface.Skill.GetSkill("S009").Value;
        character.AddAttributeModifier(new AttributeModifier(AttributeType.DamageAddRate, value, false));
    }
    #endregion

    #region S010 - 速攻
    void S010()
    {
        float value = StaticDataInterface.Skill.GetSkill("S010").Value;
        character.AddAttributeModifier(new AttributeModifier(AttributeType.AttackSpeed, value, true));
    }
    #endregion

    #region S011 - 超级速攻
    void S011()
    {
        float value = StaticDataInterface.Skill.GetSkill("S011").Value;
        character.AddAttributeModifier(new AttributeModifier(AttributeType.AttackSpeed, value, true));
    }
    #endregion

    #region S012 - 彻底疯狂
    void S012()
    {
        float value = StaticDataInterface.Skill.GetSkill("S012").Value;
        character.AddAttributeModifier(new AttributeModifier(AttributeType.AttackSpeed, value, true));
    }
    #endregion

    #region S013 - 锋刃
    private int s013UID = -1;
    void S013()
    {
        float value = StaticDataInterface.Skill.GetSkill("S013").Value;
        s013UID = character.AddAttributeModifier(new AttributeModifier(AttributeType.DamageAddRate, value, false));
    }
    void DS013()
    {
        character.RemoveAttributeModifier(s013UID);
    }
    #endregion

    #region S014 - 鸡肋
    private int s014UID = -1;
    void S014()
    {
        float value = StaticDataInterface.Skill.GetSkill("S013").Value;
        s014UID = character.AddAttributeModifier(new AttributeModifier(AttributeType.Attack, value, true));
    }
    void DS014()
    {
        character.RemoveAttributeModifier(s014UID);
    }
    #endregion

    #region S015 - 兵贵神速
    private int s015UID = -1;
    void S015()
    {
        float value = StaticDataInterface.Skill.GetSkill("S013").Value;
        s015UID = character.AddAttributeModifier(new AttributeModifier(AttributeType.AttackSpeed, value, true));
    }
    void DS015()
    {
        character.RemoveAttributeModifier(s015UID);
    }
    #endregion

    #region S016 - 治疗1
    void S016()
    {
        float rate = StaticDataInterface.Skill.GetSkill("S016").Value;
        float heal = character.attributes.GetFinalAttr(AttributeType.Hp, character.attributeModifiers) * rate;
        character.Heal(heal);
    }
    #endregion

    #region S017 - 治疗2
    void S017()
    {
        float rate = StaticDataInterface.Skill.GetSkill("S017").Value;
        float heal = character.attributes.GetFinalAttr(AttributeType.Hp, character.attributeModifiers) * rate;
        character.Heal(heal);
    }
    #endregion

    #region S018 - 治疗3
    void S018()
    {
        float rate = StaticDataInterface.Skill.GetSkill("S018").Value;
        float heal = character.attributes.GetFinalAttr(AttributeType.Hp, character.attributeModifiers) * rate;
        character.Heal(heal);
    }
    #endregion

    #region S019 - 临时强化
    private int s019UID = -1;
    void S019()
    {
        float value = StaticDataInterface.Skill.GetSkill("S019").Value;
        s019UID = character.AddAttributeModifier(new AttributeModifier(AttributeType.Attack, value, true));
    }
    void DS019()
    {
        character.RemoveAttributeModifier(s019UID);
    }
    #endregion

    #region S020 - 点金手
    void S020()
    {
        battleManager.coinAdd += StaticDataInterface.Skill.GetSkill("S020").Value;
    }
    void DS020()
    {
        battleManager.coinAdd -= StaticDataInterface.Skill.GetSkill("S020").Value;
    }
    #endregion

    #endregion

    #region 主动技能
    private float time_;
    void RunActivateSkills()
    {
        time_ = Time.deltaTime * battleManager.currentTimeSpeed;
        foreach (var skill in activeSkills)
        {
            //更新CD
            if (coolSkills.TryGetValue(skill, out Cool cool))
            {
                bool coolDown = cool(time_);
                //冷却好释放
                if (coolDown)
                {
                    if (skills.TryGetValue(skill, out ReleaseSkill release))
                    {
                        release();
                    }
                }
            }
        }
    }

    void ActiveSkillSetSetCool(string skill)
    {
        if (setCoolSkills.TryGetValue(skill, out SetCool setCool))
        {
            setCool();
        }
    }

    # region S002 - 陨星剑
    private float s002CoolTime; //冷却
    private float s002CoolTimer;//冷却计时
    public S002 s002Ctrl;
    void S002()
    {
        if (character.currentTarget == null) return;
        s002Ctrl.Init(transform.position + Vector3.up * 100, character.currentTarget.position);
    }
    void S002SetCool()
    {
        s002CoolTimer = 0;
        s002CoolTime = StaticDataInterface.Skill.GetSkill("S002").Cool;
        float rate = StaticDataInterface.Skill.GetSkill("S002").Value;
        s002Ctrl.SetCallback(() =>
        {
            if (character.currentTarget == null) return;
            var target = character.currentTarget.GetComponent<CharacterBase>();
            if (character.currentTarget != null && !target.isDead)
            {
                float damage = character.CalculateDamage(out bool isCritical, rate);
                target.TakeDamage(damage);

                // 显示伤害数字
                Vector3 popupPosition = character.currentTarget.position + Vector3.up * 2f;
                DamagePopupManager.Instance.ShowDamage(popupPosition, damage, isCritical);
            }
        });
    }
    bool S002Cool(float time)
    {
        s002CoolTimer += time;
        if(s002CoolTimer > s002CoolTime)
        {
            s002CoolTimer = 0;
            return true;
        }
        return false;
    }
    #endregion

    #endregion
}
