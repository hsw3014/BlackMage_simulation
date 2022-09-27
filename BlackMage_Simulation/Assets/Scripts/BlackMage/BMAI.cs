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

    //임시용(콘솔)
    private PlayerMovement PlayerScript;

    public float attackDelay;

    // Start is called before the first frame update
    void Start()
    {
        //임시용(콘솔)
        knowPlayerPos = true;
        skillReady = true;
        AttractAct = false;
        PlayerScript = GameObject.Find("Player").GetComponent<PlayerMovement>();
        blackmage = GetComponent<BlackMage>();
        bm_anim = blackmage.bm_anim;
        //BM의 3번째 인덱스 자식에 오디오가 담겨있음
        bm_audio = transform.GetChild(3).gameObject.GetComponent<BM_Audio>();
    }

    // Update is called once per frame
    void Update()
    {
        //패턴 별 후딜
        attackDelay -= Time.deltaTime;
        if (attackDelay < 0)
        {
            attackDelay = 0;
        }

        //플레이어와 검마 거리
        distance = Vector2.Distance(transform.position, target.position);

        //감지거리에 존재할 경우, 공격 가능!
        if (distance <= blackmage.fieldVision && attackDelay == 0)
        {
            //끌격, 주기 20초, 딜레이 1.8초정도,공격할 수 있을 때 진행
            if(!bm_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && skillReady)
            {
                SkillTarget();
            }
            //공격, 스킬 중이 아니면 방향바꾸면서 추적
            else if (!bm_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !bm_anim.GetCurrentAnimatorStateInfo(0).IsName("Skill"))
            {
                MoveToTarget();
            }
        }

        Skill_Attract();
    }

    void MoveToTarget()
    {
        //1초마다 플레이어의 위치를 알 수 있도록 함(이후 서버적용)
        if (knowPlayerPos)
        {
            knowPlayerPos = false;
            StartCoroutine(co_PlayerPos());
            dir = target.position.x - transform.position.x;
        }

        dir = dir < 0 ? -1 : 1; //좌 또는 우

        //왼쪽에 플레이어 존재
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
        //끌격, 지상에있을때만 끌려가야한다.
        //판정은 200픽셀
        if (AttractAct && PlayerScript.onGround)
        {
            dir = target.position.x - transform.position.x; //방향 재 계산
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
        //주기 20초 내외, 딜레이 1.8초정도
        Debug.Log("Skill Activate");
        AttractAct = true;
        skillReady = false;
        StartCoroutine(co_SkillReady());
        StartCoroutine(co_AttractPlayer());
        bm_anim.SetTrigger("Skill");    //스킬 애니메이션 실행, isSkill이 false이므로 자동복귀
        attackDelay = blackmage.skillSpeed;
    }

    void AttackTarget()
    {
        //공격 애니메이션, 사운드 실행
        bm_anim.SetTrigger("Attack");
        bm_audio.BM_PlaySound("Attack");
        //bm_anim.SetBool("isAttack", true);
        attackDelay = blackmage.atkSpeed;
    }

    //접촉하고 있는 한 계속 호출
    private void OnTriggerStay2D(Collider2D col)
    {
        //밀격 범위 내에 플레이어 있고, 후딜이 끝나면 공격해야함
        //끌격이 우선이다.
        if (col.CompareTag("Player") && attackDelay == 0 && !skillReady)
        {
            AttackTarget();
        }

        //공격 애니메이션 순간 트리거 활성화 되었을 때
        if (blackmage.atk_trig)
        {
            if (col.CompareTag("Player"))
            {
                PlayerScript.AlertSwitch = true;
                PlayerScript.outofControl = true;   //활성화 시킴으로서 속도제한 제거
                PlayerScript.isknockback = true;
                dir = target.position.x - transform.position.x; //방향 재 계산
                dir = dir < 0 ? -1 : 1; //좌 또는 우
                //공중에 한해서만 다르게때림
                if (PlayerScript.onGround)
                {
                    PlayerScript.rigid.AddForce(Vector2.right * dir * blackmage.kickpower);
                }
                else
                {
                    PlayerScript.rigid.AddForce(Vector2.right * dir * blackmage.kickpower * 1.75f);
                }
                //맞으면 바로 트리거 비활성화
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
