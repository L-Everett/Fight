using System.Collections;
using UnityEngine;

public class EB1Base : EnemyBase
{
    protected override IEnumerator DelaySkill(int skillID)
    {
        float time = 0.1f;
        if (skillID == 2)
        {
            time = 0.18f;
        }
        if (battleManager.currentTimeSpeed == 2) time /= 2;
        yield return new WaitForSeconds(time);
        skillctrl.RunSelectSkill(skillID);
    }
}
