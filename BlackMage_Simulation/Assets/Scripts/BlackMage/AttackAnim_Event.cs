using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnim_Event : MonoBehaviour
{
    BlackMage blackmage;
    // Start is called before the first frame update
    void Start()
    {
        //�θ� ������Ʈ ������
        blackmage = transform.parent.gameObject.GetComponent<BlackMage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AttackTrigger()
    {
        Debug.Log("Attk!");
        //ª�� �ð����� Ȱ��ȭ
        blackmage.atk_trig = true;
        StartCoroutine(off_atk_trig());
    }

    IEnumerator off_atk_trig()
    {
        yield return new WaitForSeconds(0.1f);
        blackmage.atk_trig = false;
    }
}
