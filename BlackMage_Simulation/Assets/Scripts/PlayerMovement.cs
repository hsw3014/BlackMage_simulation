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

    PlayerAnim PlayerAnimScript;    //�ڽ��� �ִϸ��̼� ������Ʈ
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
    public float facedir;    // <- ����, -> ���

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

        //��ҿ� ���������� ������Ʈ
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

        //�����ִ� ���� ��Ȱ��ȭ
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
        jumpspeed = 6.5f;   //������ �������� ��
        maxGravity = -6.5f;   //���� �ִ� �ӵ�
        speedvalue = 0;
        airpower = 0.25f;    //���� ���� �����̷��� ��
    }

    // Update is called once per frame
    void Update()
    {
        player_velo = rigid.velocity.x;
        gravity_num = rigid.velocity.y;
        if(rigid.velocity.y < maxGravity)
        {
            //�ְ� ���ϼӵ� ����, �¿� �ӵ��� ���ϸ� �ȉ�
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
        //���� �ȹ�� ������ �׻� ���߻���
        //�Է� �¿�Ű �Է��� ������ Stand
        if (!onGround)
        {
            state = "onAir";
        }
        else if(Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        //else if(!Input.anyKey)
        {
            state = "Stand";
        }

        //���� ��ȯ
        //���ÿ� ������ �� ���ʸ� ������Ѿ���
        //�� �� ������ ����߉� -> facedir = 0
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

        //�ǰݹްų� �����ϸ� 5���� Ÿ�̸� ����
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

        //�˹����
        if (isknockback)
        {
            isknockback = false;
            StartCoroutine(co_damaged());
            StartCoroutine(knockback());
        }
    }

    void Prone()
    {
        //�����ְ� ������ ������ ���¶�� �Ʒ�Ű �������� ���δ�.
        //�����̴� ���߿� �������ʴ´�.
        if(state != "Move" && onGround && !outofControl && Input.GetKey(KeyCode.DownArrow))
        {
            state = "Prone";
            state_prone = true;
            headCol.enabled = false;
            standCol.enabled = false;

            //�����ִ� ���⿡���� �ٸ� �ݶ��̴� Ȱ��ȭ
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
            //�����־����� Stand�� �ǵ��ư���.
            //���̴� �Ծƴϸ� �Ӹ�, ���� Ȱ��ȭ
            state_prone = false;
            headCol.enabled = true;
            standCol.enabled = true;
            proneLeftCol.enabled = false;
            proneRightCol.enabled = false;
        }
    }

    void Jump()
    {
        //���� �ѹ���, �������־ �۵��Ǿ��ϹǷ� GetKey
        //���̴� ������ ������ �� �� ����
        if (!outofControl && Input.GetKey(KeyCode.LeftAlt) && onGround && state != "Prone")
        {
            onGround = false;
            //rigid.velocity = Vector2.zero;
            Vector2 jumpVelocity = new Vector2(0, jumpspeed);

            //�¿������� ����
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
                //���ڸ� ���� ����
                rigid.AddForce(jumpVelocity, ForceMode2D.Impulse);
            }
        }

        //2������
        if (state == "onAir" && !outofControl && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if(jump_cnt > 0)
            {
                jump_cnt--;
                Vector2 doublejump_power = new Vector2(facedir * 6.5f, 2.0f);

                rigid.AddForce(doublejump_power, ForceMode2D.Impulse);

                //���� �ӵ� ���ϸ� ����
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

        //�����ӿ� ���� ����
        //�÷��̾ �����̴� Ű�� ���� �������ʹ� ���� ���ϸ� �ȵȴ�.
        if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && !outofControl && facedir != 0)
        {
            if ((state == "Stand" || state == "Move"))
            {
                //�����ִ°� ���ؼ� �����̸� ��������
                if (state == "Stand" && rigid.velocity.x == 0)
                {
                    Debug.Log("Activate Move");
                    rigid.velocity = new Vector2(maxSpeed * facedir, rigid.velocity.y);
                }

                //�ӵ��� �Ѿ�� ���� �����ʴ´�.
                if (-maxSpeed <= rigid.velocity.x && rigid.velocity.x <= maxSpeed)
                {
                    rigid.AddForce(Vector2.right * force_num);
                }
            }
            //���߿��� �̾��ϰ� ������ �� ����.
            else if (state == "onAir")
            {
                rigid.AddForce(Vector2.right * force_num * 0.02f);
            }

            //������ ó�� �� ���� ��ȯ
            if (state != "onAir" && facedir != 0)
            {
                state = "Move";
            }
        }

        /*///////////////
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //���ۿ����� ����
            if (onGround && !outofControl)
            {
                rigid.AddForce(Vector2.right * 0.9f * -1, ForceMode2D.Impulse);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //���ۿ����� ����
            if (onGround && !outofControl)
            {
                rigid.AddForce(Vector2.right * 0.9f, ForceMode2D.Impulse);
            }
        }

        //�ٶ󺸴� ����ٲٴ°�.
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

        //Ű�� ���� ���ִ� ���·� ����
        if (state == "Move" && Input.GetKeyUp(KeyCode.LeftArrow))
        {
            state = "Stand";
        }
        else if(state == "Move" && Input.GetKeyUp(KeyCode.RightArrow))
        {
            state = "Stand";
        }

        //�������� ������ �õ�
        //�����Ҵ��̸� ���Ѿ���
        if ((state == "Stand" || state == "Move") && !outofControl)
        {
            rigid.AddForce(Vector2.right * force_num);
        }
        //���߿��� �̾��ϰ� ������ �� ����.
        else if(state == "onAir" && !outofControl)
        {
            rigid.AddForce(Vector2.right * force_num * 0.1f);
        }

        *//////////////

            /*
            if (Input.GetKey(KeyCode.LeftArrow))    //������ �������� ���� ���� �� ���߰�
            {
                facedir = false;
                //�������� ����
                if (onGround)   
                {
                    speedvalue -= Time.deltaTime * gasok;
                    if (speedvalue < maxSpeed * -1) speedvalue = maxSpeed * -1;
                }
                //���߿����� ���ϰ�
                else
                {
                    speedvalue -= Time.deltaTime * gasok * airpower;
                    if (speedvalue < maxSpeed * -1) speedvalue = maxSpeed * -1;
                }
    ;        }
            else if (Input.GetKey(KeyCode.RightArrow))  //���������� ���� ���� �� ���δ�
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

            if (onGround && speedvalue != 0)   //�̲������� ���� ��
            {
                if (speedvalue > 0)  //0���� ������� ����
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

            //������ �ְ�ӵ�, ���� �ְ�ӵ�
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
        //AŰ�������� �ִϸ��̼��� ����ǰ� ���� ������ ����
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

    //���������� ��¦�Ÿ�
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
