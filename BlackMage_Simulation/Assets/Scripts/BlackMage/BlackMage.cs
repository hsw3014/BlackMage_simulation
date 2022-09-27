using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackMage : MonoBehaviour
{
    BMAI bmai_script;

    public Animator bm_anim;
    public Transform target;
    public float moveSpeed;
    public float fieldVision;
    public float atkSpeed;
    public float skillSpeed;
    public float skillCooldown;
    public float kickpower;
    public float magnetpower;

    public bool atk_trig;

    float attackDelay;

    // Start is called before the first frame update
    void Start()
    {
        bmai_script = GetComponent<BMAI>();
        fieldVision = 777.0f;
        bm_anim = transform.GetChild(0).gameObject.GetComponent<Animator>();
        moveSpeed = 0.15f;
        atkSpeed = 3.665f;
        skillSpeed = 1.805f;
        skillCooldown = 20.0f;
        kickpower = 470.0f;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
