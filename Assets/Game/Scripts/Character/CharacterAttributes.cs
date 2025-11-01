using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterAttributes
{
    // 基础属性字典
    public Dictionary<AttributeType, float> baseAttr = new Dictionary<AttributeType, float>();

    // 缓存最终属性值
    private Dictionary<AttributeType, float> finalAttrCache = new Dictionary<AttributeType, float>();

    // 脏标记 - 标识是否需要重新计算
    private bool isDirty = true;

    public void Init(string id, bool isEnemy = false)
    {
        if (isEnemy)
        {
            var data = StaticDataInterface.Enemy.GetEnemy(id);
            baseAttr.Clear();
            baseAttr.Add(AttributeType.MoveSpeed, data.MoveSpeed);
            baseAttr.Add(AttributeType.Attack, data.ATK);
            baseAttr.Add(AttributeType.CritRate, data.Crit);
            baseAttr.Add(AttributeType.CritDamage, data.CritDamage);
            baseAttr.Add(AttributeType.Hp, data.HP);
            baseAttr.Add(AttributeType.AttackSpeed, data.ATKSpeed);
            baseAttr.Add(AttributeType.AttackRange, data.ATKRange);
            baseAttr.Add(AttributeType.DamageAddRate, 0);
        }
        else
        {
            var data = StaticDataInterface.Character.GetCharacter(id);
            baseAttr.Clear();
            baseAttr.Add(AttributeType.MoveSpeed, data.MoveSpeed);
            baseAttr.Add(AttributeType.Attack, data.ATK);
            baseAttr.Add(AttributeType.CritRate, data.Crit);
            baseAttr.Add(AttributeType.CritDamage, data.CritDamage);
            baseAttr.Add(AttributeType.Hp, data.HP);
            baseAttr.Add(AttributeType.AttackSpeed, data.ATKSpeed);
            baseAttr.Add(AttributeType.AttackRange, data.ATKRange);
            baseAttr.Add(AttributeType.DamageAddRate, 0);

            //神赐
            RunningManager.Instance.mTalentAdd.TryGetValue(id, out var talentAdd);
            if(talentAdd != null)
            {
                foreach(var talent in talentAdd)
                {
                    baseAttr[talent.Key] += talent.Value;
                }
            }
        }

        // 初始化后标记为需要重新计算
        MarkDirty();
    }

    // 标记属性需要重新计算
    public void MarkDirty()
    {
        isDirty = true;
    }

    // 获取最终属性值
    public float GetFinalAttr(AttributeType attributeType, Dictionary<int, AttributeModifier> modifiers)
    {
        // 如果缓存无效或该属性尚未缓存，重新计算所有属性
        if (isDirty || !finalAttrCache.ContainsKey(attributeType))
        {
            RecalculateAllAttributes(modifiers);
        }

        return finalAttrCache[attributeType];
    }

    public float GetBaseAttr(AttributeType attributeType)
    {
        baseAttr.TryGetValue(attributeType, out float value);
        return value;
    }

    // 重新计算所有属性并缓存结果
    private void RecalculateAllAttributes(Dictionary<int, AttributeModifier> modifiers)
    {
        // 清空缓存
        finalAttrCache.Clear();

        // 按属性类型分组修饰符
        var groupedModifiers = GroupModifiersByType(modifiers);

        // 计算每个属性的最终值
        foreach (AttributeType type in System.Enum.GetValues(typeof(AttributeType)))
        {
            if (!baseAttr.TryGetValue(type, out float baseValue))
            {
                Debug.LogWarning($"Base attribute {type} not found!");
                continue;
            }

            float finalValue = baseValue;
            float percentBonus = 0f;

            // 处理该类型的所有修饰符
            if (groupedModifiers.TryGetValue(type, out List<AttributeModifier> typeModifiers))
            {
                foreach (var mod in typeModifiers)
                {
                    if (mod.isPercentage)
                        percentBonus += mod.value;
                    else
                        finalValue += mod.value;
                }
            }

            // 应用百分比加成
            finalValue *= (1 + percentBonus);

            // 特殊属性处理
            switch (type)
            {
                case AttributeType.CritRate:
                    finalValue = Mathf.Clamp(finalValue, 0f, 1f); // 确保暴击率在0-1范围内
                    break;
                case AttributeType.CritDamage:
                    finalValue = Mathf.Max(finalValue, 1f); 
                    break;
            }

            // 缓存结果
            finalAttrCache[type] = finalValue;
        }

        isDirty = false;
    }

    // 按属性类型分组修饰符
    private Dictionary<AttributeType, List<AttributeModifier>> GroupModifiersByType(Dictionary<int, AttributeModifier> modifiers)
    {
        var grouped = new Dictionary<AttributeType, List<AttributeModifier>>();

        foreach (var mod in modifiers)
        {
            if (!grouped.ContainsKey(mod.Value.attributeType))
            {
                grouped[mod.Value.attributeType] = new List<AttributeModifier>();
            }
            grouped[mod.Value.attributeType].Add(mod.Value);
        }

        return grouped;
    }
}

public enum AttributeType
{
    MoveSpeed = 0, 
    Attack = 1, 
    CritRate = 2, 
    CritDamage = 3, 
    Hp = 4, 
    AttackSpeed = 5, 
    AttackRange = 6,
    DamageAddRate
}

[System.Serializable]
public struct AttributeModifier
{
    public AttributeType attributeType;
    public float value;
    public bool isPercentage;
    public AttributeModifier(AttributeType type, float value, bool isPer)
    {
        this.attributeType = type;
        this.value = value;
        this.isPercentage = isPer;
    }
}