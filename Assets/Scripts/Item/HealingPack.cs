using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealingPack : MonoBehaviour
{
    [Header("��ü ȸ����")]
    [SerializeField]
    private float HealingValue;

    [Header("ȸ�� ���� �ð�")]
    [SerializeField]
    private float HealingTime;

    [Header("ȸ�� ������")]
    [SerializeField]
    private float HealingDelaySpeed;

    private GameObject player;

    private float healValue;
    private float totalTimer;
    private float timer;

    private void Awake()
    {
        healValue = (HealingValue / HealingTime);
    }

    private void OnEnable()
    {
        totalTimer = HealingTime;
        timer = 0;
        StartCoroutine(startHealing());
    }

    private void Update()
    {        
        if (player == null) return;
        Healing();
    }

    private IEnumerator startHealing()
    {
        while(totalTimer > 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            totalTimer -= Time.deltaTime;
        }
        transform.parent.gameObject.SetActive(false);
        yield break;
    }

    private void Healing()
    {
        timer += Time.deltaTime;
        if(timer >= HealingDelaySpeed)
        {
            //ȸ�� ȿ��
            player.GetComponent<Player_Info>().Heal(healValue * HealingDelaySpeed);
            //ȸ�� ������ Ÿ�̸� �ʱ�ȭ
            timer = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;  
        }
    }

    private void OnDisable()
    {
        ItemObjectPool.ReturnToPool(transform.parent.gameObject);
    }
}
