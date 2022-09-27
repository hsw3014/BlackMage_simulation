using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnim_Event : MonoBehaviour
{
    BlackMage blackmage;
    // Start is called before the first frame update
    void Start()
    {
        //부모 오브젝트 가져옴
        blackmage = transform.parent.gameObject.GetComponent<BlackMage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AttackTrigger()
    {
        Debug.Log("Attk!");
        //짧은 시간동안 활성화
        blackmage.atk_trig = true;
        StartCoroutine(off_atk_trig());
    }

    IEnumerator off_atk_trig()
    {
        yield return new WaitForSeconds(0.1f);
        blackmage.atk_trig = false;
    }
}
