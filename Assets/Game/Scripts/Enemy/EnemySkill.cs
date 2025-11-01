using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill : MonoBehaviour
{
    private Transform effectRoot;
    // Start is called before the first frame update
    void Start()
    {
        effectRoot = GameObject.Find("EffectRoot").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
