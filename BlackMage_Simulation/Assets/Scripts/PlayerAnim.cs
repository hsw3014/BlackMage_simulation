using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    SpriteRenderer spr;
    public Animator anim;
    private PlayerMovement PlayerScript;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        PlayerScript = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
        Prone();
        Alert();
        //Adel_Divide();
    }

    void Alert()
    {
        if (PlayerScript.alert)
        {
            anim.SetBool("isAlert", true);
        }
        else
        {
            anim.SetBool("isAlert", false);
        }
    }

    void Prone()
    {
        //���λ��°� �Ǿ��ٸ� ���� ���, �ƴϸ� �ٸ���
        if (PlayerScript.state_prone)
        {
            anim.SetBool("isProne", true);
        }
        else
        {
            anim.SetBool("isProne", false);
        }
    }
    void Jump()
    {
        //���� �ѹ���
        if (!PlayerScript.onGround)  //���ƴϸ� ��� ü������
        {
            anim.SetBool("onAir", true);
            anim.SetBool("isStand", false);
        }
        else if(PlayerScript.onGround)
        {
            anim.SetBool("onAir", false);
            anim.SetBool("isStand", true);
            //Alert�߰�
        }
    }

    void Move()
    {
        //�¿� �����̴°�
        if(!PlayerScript.outofControl && !Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
        {
            anim.SetBool("isWalking", true);
            spr.flipX = false;
        }
        else if (!PlayerScript.outofControl && !Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            anim.SetBool("isWalking", true);
            spr.flipX = true;
        }
        /*
        if (PlayerScript.horizonInput > 0)
        {
            anim.SetBool("Walking", true);
            spr.flipX = true;
        }
        else if(PlayerScript.horizonInput < 0)
        {
            anim.SetBool("Walking", true);
            spr.flipX = false;
        }*/
        else
        {
            anim.SetBool("isWalking", false);
        }
    }

    void Adel_Divide()
    {
        //AŰ�������� �ִϸ��̼��� ����ǰ� ���� ������ ����
        if (Input.GetKeyDown(KeyCode.A) && !anim.GetCurrentAnimatorStateInfo(0).IsName("Divide"))
        {
            anim.SetBool("Divide", true);
        }
    }
}
