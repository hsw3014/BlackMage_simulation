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
        //숙인상태가 되었다면 눕기 재생, 아니면 다르게
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
        //점프 한번만
        if (!PlayerScript.onGround)  //땅아니면 상시 체공상태
        {
            anim.SetBool("onAir", true);
            anim.SetBool("isStand", false);
        }
        else if(PlayerScript.onGround)
        {
            anim.SetBool("onAir", false);
            anim.SetBool("isStand", true);
            //Alert추가
        }
    }

    void Move()
    {
        //좌우 움직이는것
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
        //A키눌렀을때 애니메이션이 재생되고 있지 않으면 시행
        if (Input.GetKeyDown(KeyCode.A) && !anim.GetCurrentAnimatorStateInfo(0).IsName("Divide"))
        {
            anim.SetBool("Divide", true);
        }
    }
}
