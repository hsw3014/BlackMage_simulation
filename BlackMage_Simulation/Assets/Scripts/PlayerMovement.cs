using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rigid;
    public SpriteRenderer spr;

    GameObject PlayerBody;

    GameObject headBody;
    GameObject standBody;
    GameObject proneLeftBody;
    GameObject proneRightBody;

    BoxCollider2D headCol;
    BoxCollider2D standCol;
    BoxCollider2D proneLeftCol;
    BoxCollider2D proneRightCol;

    PlayerAnim PlayerAnimScript;    //자식의 애니메이션 오브젝트
    Vector3 pos;

    public string state; //Stand, Prone, Move, onAir, Sit
    public float force_num = 0;
    public float gravity_num = 0;

    public float player_velo;
    public int jump_cnt;

    public float airpower;
    public float speedvalue;
    public float maxSpeed;
    public float jumpMaxSpeed;
    private float jumpspeed;
    private float maxGravity;
    public float facedir;    // <- 음수, -> 양수

    public bool onGround;
    public bool muzok;
    public bool alert;
    public bool AlertSwitch;
    public bool isknockback;

    public bool outofControl;

    public float alert_timer;

    public bool state_prone;

    public float horizonInput;

    // Start is called before the first frame update
    void Start()
    {
        state = "Stand";
        rigid = gameObject.GetComponent<Rigidbody2D>();
        spr = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        player_velo = rigid.velocity.x;

        //평소와 누웠을때의 오브젝트
        //idx -> 0 : head , 1 : body , 2 : leftprone, 3 : rightprone
        PlayerBody = transform.GetChild(1).gameObject;
        headBody = PlayerBody.transform.GetChild(0).gameObject;
        standBody = PlayerBody.transform.GetChild(1).gameObject;
        proneLeftBody = PlayerBody.transform.GetChild(2).gameObject;
        proneRightBody = PlayerBody.transform.GetChild(3).gameObject;

        headCol = headBody.GetComponent<BoxCollider2D>();
        standCol = standBody.GetComponent<BoxCollider2D>();
        proneLeftCol = proneLeftBody.GetComponent<BoxCollider2D>();
        proneRightCol = proneRightBody.GetComponent<BoxCollider2D>();

        //누워있는 것은 비활성화
        proneLeftCol.enabled = false;
        proneRightCol.enabled = false;

        onGround = false;
        state_prone = false;
        facedir = 1;
        muzok = false;
        alert = false;
        AlertSwitch = false;
        isknockback = false;
        outofControl = false;
        alert_timer = 0;

        jump_cnt = 1;

        pos = transform.position;
        maxSpeed = 1.6f;
        jumpMaxSpeed = 6.5f;
        jumpspeed = 6.5f;   //점프에 가해지는 힘
        maxGravity = -6.5f;   //낙하 최대 속도
        speedvalue = 0;
        airpower = 0.25f;    //점프 상태 움직이려는 힘
    }

    // Update is called once per frame
    void Update()
    {
        player_velo = rigid.velocity.x;
        gravity_num = rigid.velocity.y;
        if(rigid.velocity.y < maxGravity)
        {
            //최고 낙하속도 조절, 좌우 속도는 변하면 안됌
            rigid.velocity = new Vector2(rigid.velocity.x, maxGravity);
        }

        StateCheck();
        Move();
        Prone();
        Jump();
        //Adel_Divide();
    }

    void StateCheck()
    {
        //땅을 안밟고 있으면 항상 공중상태
        //입력 좌우키 입력이 꺼지면 Stand
        if (!onGround)
        {
            state = "onAir";
        }
        else if(Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        //else if(!Input.anyKey)
        {
            state = "Stand";
        }

        //방향 전환
        //동시에 눌렀을 때 한쪽만 적용시켜야함
        //둘 다 누르면 멈춰야됌 -> facedir = 0
        if (!outofControl && !Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
        {
            facedir = -1;
        }
        else if (!outofControl && !Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            facedir = 1;
        }
        else if(!outofControl && Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
        {
            facedir = 0;
        }

        //피격받거나 공격하면 5초의 타이머 시작
        if (AlertSwitch)
        {
            AlertSwitch = false;
            Debug.Log("Activate Timer");
            alert = true;
            alert_timer = 5.0f;
        }

        if (alert_timer > 0)
        {
            alert_timer -= Time.deltaTime;
        }
        else
        {
            alert = false;
        }

        //넉백맞음
        if (isknockback)
        {
            isknockback = false;
            StartCoroutine(co_damaged());
            StartCoroutine(knockback());
        }
    }

    void Prone()
    {
        //땅에있고 조작이 가능한 상태라면 아래키 눌렀으면 숙인다.
        //움직이는 도중엔 숙이지않는다.
        if(state != "Move" && onGround && !outofControl && Input.GetKey(KeyCode.DownArrow))
        {
            state = "Prone";
            state_prone = true;
            headCol.enabled = false;
            standCol.enabled = false;

            //보고있는 방향에따라 다른 콜라이더 활성화
            if (facedir == 1)
            {
                proneLeftCol.enabled = false;
                proneRightCol.enabled = true;
            }
            else
            {
                proneLeftCol.enabled = true;
                proneRightCol.enabled = false;
            }
        }
        else
        {
            //땅에있었으면 Stand로 되돌아간다.
            //숙이는 게아니면 머리, 몸만 활성화
            state_prone = false;
            headCol.enabled = true;
            standCol.enabled = true;
            proneLeftCol.enabled = false;
            proneRightCol.enabled = false;
        }
    }

    void Jump()
    {
        //점프 한번만, 누르고있어도 작동되야하므로 GetKey
        //숙이는 동안은 점프를 할 수 없음
        if (!outofControl && Input.GetKey(KeyCode.LeftAlt) && onGround && state != "Prone")
        {
            onGround = false;
            //rigid.velocity = Vector2.zero;
            Vector2 jumpVelocity = new Vector2(0, jumpspeed);

            //좌우점프는 보정
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                Debug.Log("LeftJump");
                rigid.AddForce(jumpVelocity, ForceMode2D.Impulse);
                rigid.velocity = new Vector2(-maxSpeed, rigid.velocity.y);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                Debug.Log("RightJump");
                rigid.AddForce(jumpVelocity, ForceMode2D.Impulse);
                rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
            }
            else if(state != "Prone")
            {
                Debug.Log("Jump");
                //제자리 점프 실행
                rigid.AddForce(jumpVelocity, ForceMode2D.Impulse);
            }
        }

        //2단점프
        if (state == "onAir" && !outofControl && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if(jump_cnt > 0)
            {
                jump_cnt--;
                Vector2 doublejump_power = new Vector2(facedir * 6.5f, 2.0f);

                rigid.AddForce(doublejump_power, ForceMode2D.Impulse);

                //점프 속도 과하면 조절
                if (rigid.velocity.x <= -jumpMaxSpeed)
                {
                    rigid.velocity = new Vector2(-jumpMaxSpeed, rigid.velocity.y);
                }
                else if (rigid.velocity.x >= jumpMaxSpeed)
                {
                    rigid.velocity = new Vector2(jumpMaxSpeed, rigid.velocity.y);
                }
            }
        }
    }

    void Move()
    {
        horizonInput = Input.GetAxis("Horizontal");
        force_num = horizonInput * 3.0f;

        //움직임엔 힘을 가함
        //플레이어가 움직이는 키를 떼는 순간부터는 힘을 가하면 안된다.
        if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && !outofControl && facedir != 0)
        {
            if ((state == "Stand" || state == "Move"))
            {
                //멈춰있는것 한해서 움직이면 보정받음
                if (state == "Stand" && rigid.velocity.x == 0)
                {
                    Debug.Log("Activate Move");
                    rigid.velocity = new Vector2(maxSpeed * facedir, rigid.velocity.y);
                }

                //속도가 넘어가면 힘을 주지않는다.
                if (-maxSpeed <= rigid.velocity.x && rigid.velocity.x <= maxSpeed)
                {
                    rigid.AddForce(Vector2.right * force_num);
                }
            }
            //공중에선 미약하게 움직일 수 있음.
            else if (state == "onAir")
            {
                rigid.AddForce(Vector2.right * force_num * 0.02f);
            }

            //위에서 처리 후 상태 변환
            if (state != "onAir" && facedir != 0)
            {
                state = "Move";
            }
        }

        /*///////////////
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //시작움직임 보정
            if (onGround && !outofControl)
            {
                rigid.AddForce(Vector2.right * 0.9f * -1, ForceMode2D.Impulse);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //시작움직임 보정
            if (onGround && !outofControl)
            {
                rigid.AddForce(Vector2.right * 0.9f, ForceMode2D.Impulse);
            }
        }

        //바라보는 방향바꾸는것.
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (state != "onAir") { state = "Move"; }
            facedir = false;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (state != "onAir") { state = "Move"; }
            facedir = true;
        }

        //키를 떼면 서있는 상태로 돌입
        if (state == "Move" && Input.GetKeyUp(KeyCode.LeftArrow))
        {
            state = "Stand";
        }
        else if(state == "Move" && Input.GetKeyUp(KeyCode.RightArrow))
        {
            state = "Stand";
        }

        //땅에서의 움직임 시도
        //통제불능이면 제한없음
        if ((state == "Stand" || state == "Move") && !outofControl)
        {
            rigid.AddForce(Vector2.right * force_num);
        }
        //공중에선 미약하게 움직일 수 있음.
        else if(state == "onAir" && !outofControl)
        {
            rigid.AddForce(Vector2.right * force_num * 0.1f);
        }

        *//////////////

            /*
            if (Input.GetKey(KeyCode.LeftArrow))    //땅에서 왼족으로 가면 점점 더 낮추고
            {
                facedir = false;
                //땅에서는 가속
                if (onGround)   
                {
                    speedvalue -= Time.deltaTime * gasok;
                    if (speedvalue < maxSpeed * -1) speedvalue = maxSpeed * -1;
                }
                //공중에서는 덜하게
                else
                {
                    speedvalue -= Time.deltaTime * gasok * airpower;
                    if (speedvalue < maxSpeed * -1) speedvalue = maxSpeed * -1;
                }
    ;        }
            else if (Input.GetKey(KeyCode.RightArrow))  //오른쪽으로 가면 점점 더 높인다
            {
                facedir = true;
                if (onGround)
                {
                    speedvalue += Time.deltaTime * gasok;
                    if (speedvalue > maxSpeed) speedvalue = maxSpeed;
                }
                else
                {
                    speedvalue += Time.deltaTime * gasok * airpower;
                    if (speedvalue > maxSpeed) speedvalue = maxSpeed;
                }
            }
            else
            {

            }

            if (onGround && speedvalue != 0)   //미끄러지고 있을 때
            {
                if (speedvalue > 0)  //0으로 수렴토록 진행
                {
                    speedvalue -= Time.deltaTime * friction;
                    if (speedvalue < 0) speedvalue = 0;
                }
                else if (speedvalue < 0)
                {
                    speedvalue += Time.deltaTime * friction;
                    if (speedvalue > 0) speedvalue = 0;
                }
            }

            rigid.velocity = new Vector2(speedvalue, rigid.velocity.y);
            //
            //transform.Translate(Vector3.right * horizonInput * Time.deltaTime * speed);
            //rigid.AddForce(Vector2.right * horizonInput * speed, ForceMode2D.Impulse);

            //오른쪽 최고속도, 왼쪽 최고속도
            /*
            if(rigid.velocity.x > maxSpeed)
            {
                rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
            }
            else if(rigid.velocity.x < (maxSpeed * -1))
            {
                rigid.velocity = new Vector2(maxSpeed * -1, rigid.velocity.y);
            }
            */
    }

    void Adel_Divide()
    {
        //A키눌렀을때 애니메이션이 재생되고 있지 않으면 시행
        if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Attacked");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jump_cnt = 1;
            onGround = true;
            state = "Stand";
        }
    }

    IEnumerator knockback()
    {
        yield return new WaitForSeconds(1.5f);
        outofControl = false;
    }

    //피해입으면 반짝거림
    IEnumerator co_damaged()
    {
        int countTime = 0;
        while(countTime < 20)
        {
            if(countTime % 2 == 0)
            {
                spr.color = new Color32(255, 255, 255, 90);
            }
            else
            {
                spr.color = new Color32(255, 255, 255, 180);
            }

            yield return new WaitForSeconds(0.05f);
            countTime++;
        }

        spr.color = new Color32(255, 255, 255, 255);
        yield return null;
    }
}
