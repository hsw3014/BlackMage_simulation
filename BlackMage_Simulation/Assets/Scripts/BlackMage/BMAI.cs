using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMAI : MonoBehaviour
{
    public Transform target;
    BlackMage blackmage;
    Animator bm_anim;
    BM_Audio bm_audio;
    public float dir;
    public float distance;

    public bool knowPlayerPos;
    public bool skillReady;

    public bool AttractAct;

    //�ӽÿ�(�ܼ�)
    private PlayerMovement PlayerScript;

    public float attackDelay;

    // Start is called before the first frame update
    void Start()
    {
        //�ӽÿ�(�ܼ�)
        knowPlayerPos = true;
        skillReady = true;
        AttractAct = false;
        PlayerScript = GameObject.Find("Player").GetComponent<PlayerMovement>();
        blackmage = GetComponent<BlackMage>();
        bm_anim = blackmage.bm_anim;
        //BM�� 3��° �ε��� �ڽĿ� ������� �������
        bm_audio = transform.GetChild(3).gameObject.GetComponent<BM_Audio>();
    }

    // Update is called once per frame
    void Update()
    {
        //���� �� �ĵ�
        attackDelay -= Time.deltaTime;
        if (attackDelay < 0)
        {
            attackDelay = 0;
        }

        //�÷��̾�� �˸� �Ÿ�
        distance = Vector2.Distance(transform.position, target.position);

        //�����Ÿ��� ������ ���, ���� ����!
        if (distance <= blackmage.fieldVision && attackDelay == 0)
        {
            //����, �ֱ� 20��, ������ 1.8������,������ �� ���� �� ����
            if(!bm_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && skillReady)
            {
                SkillTarget();
            }
            //����, ��ų ���� �ƴϸ� ����ٲٸ鼭 ����
            else if (!bm_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !bm_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill"))
            {
                MoveToTarget();
            }
        }

        Skill_Attract();
    }

    void MoveToTarget()
    {
        //1�ʸ��� �÷��̾��� ��ġ�� �� �� �ֵ��� ��(���� ��������)
        if (knowPlayerPos)
        {
            knowPlayerPos = false;
            StartCoroutine(co_PlayerPos());
            dir = target.position.x - transform.position.x;
        }

        dir = dir < 0 ? -1 : 1; //�� �Ǵ� ��

        //���ʿ� �÷��̾� ����
        if (dir < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        transform.Translate(new Vector2(dir, 0) * blackmage.moveSpeed * Time.deltaTime);
        bm_anim.SetBool("isMove", true);
    }

    void Skill_Attract()
    {
        //����, ������������ ���������Ѵ�.
        //������ 200�ȼ�
        if (AttractAct && PlayerScript.onGround)
        {
            dir = target.position.x - transform.position.x; //���� �� ���
            if (dir < 0)
            {
                PlayerScript.rigid.AddForce(Vector2.right * 2.0f);
            }
            else if (dir > 0)
            {
                PlayerScript.rigid.AddForce(Vector2.right * -2.0f);
            }
        }
    }

    void SkillTarget()
    {
        //�ֱ� 20�� ����, ������ 1.8������
        Debug.Log("Skill Activate");
        AttractAct = true;
        skillReady = false;
        StartCoroutine(co_SkillReady());
        StartCoroutine(co_AttractPlayer());
        bm_anim.SetTrigger("Skill");    //��ų �ִϸ��̼� ����, isSkill�� false�̹Ƿ� �ڵ�����
        attackDelay = blackmage.skillSpeed;
    }

    void AttackTarget()
    {
        //���� �ִϸ��̼�, ���� ����
        bm_anim.SetTrigger("Attack");
        bm_audio.BM_PlaySound("Attack");
        //bm_anim.SetBool("isAttack", true);
        attackDelay = blackmage.atkSpeed;
    }

    //�����ϰ� �ִ� �� ��� ȣ��
    private void OnTriggerStay2D(Collider2D col)
    {
        //�а� ���� ���� �÷��̾� �ְ�, �ĵ��� ������ �����ؾ���
        //������ �켱�̴�.
        if (col.CompareTag("Player") && attackDelay == 0 && !skillReady)
        {
            AttackTarget();
        }

        //���� �ִϸ��̼� ���� Ʈ���� Ȱ��ȭ �Ǿ��� ��
        if (blackmage.atk_trig)
        {
            if (col.CompareTag("Player"))
            {
                PlayerScript.AlertSwitch = true;
                PlayerScript.outofControl = true;   //Ȱ��ȭ ��Ŵ���μ� �ӵ����� ����
                PlayerScript.isknockback = true;
                dir = target.position.x - transform.position.x; //���� �� ���
                dir = dir < 0 ? -1 : 1; //�� �Ǵ� ��
                //���߿� ���ؼ��� �ٸ��Զ���
                if (PlayerScript.onGround)
                {
                    PlayerScript.rigid.AddForce(Vector2.right * dir * blackmage.kickpower);
                }
                else
                {
                    PlayerScript.rigid.AddForce(Vector2.right * dir * blackmage.kickpower * 1.75f);
                }
                //������ �ٷ� Ʈ���� ��Ȱ��ȭ
                blackmage.atk_trig = false;
            }
        }
    }

    IEnumerator co_PlayerPos()
    {
        yield return new WaitForSeconds(1.0f);
        knowPlayerPos = true;
    }

    IEnumerator co_SkillReady()
    {
        yield return new WaitForSeconds(blackmage.skillCooldown + Random.Range(-2, 4));
        skillReady = true;
    }

    IEnumerator co_AttractPlayer()
    {
        yield return new WaitForSeconds(2.0f);
        AttractAct = false;
    }
}              
