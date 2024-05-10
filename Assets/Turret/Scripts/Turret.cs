using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Turret : MonoBehaviour
{
    private float searchTime = 0.5f;
    private float checkSearchTime;

    protected Transform targetTransform;
    protected LayerMask monsterLayer = 6;

    protected float makingTime;
    protected float makingCost;
    protected float attackDamge;
    protected float attackSpeed;
    protected float attackRange;
    protected float hp;
    protected float upgradeCost;
    protected float upgradeTime;
    protected float repairTime;
    protected float repairCost;
    protected float attackRise;
    protected float hpRise;
    protected float attackSpeedRise;
    protected float upgradCostRise;
    protected float maxUpgradeCount;

    protected bool isUpgrade;
    protected bool isRepair;
    protected bool isTarget;

    protected abstract void Attack();

    //�ڷ�ƾ�� �������÷��Ͱ� ���� �Ҹ���
    //�޸𸮸� ���� �Դ´ٴ� ���̴�
    //��ž�� ������ �����̴� �ڷ�ƾ ���� �����غ���
    //������ �Լ��� ������Ʈ���� ȣ���� ���ϸ鼭 �ϴ°ͺ��� ����
    //������ �̺�Ʈ�� �̿��� ����� ����
    protected IEnumerator SearchEnemy()
    {
        while (true)
        {
            yield return new WaitUntil(() => targetTransform == null);
            yield return new WaitForSeconds(1);

            Collider[] enemyCollider = Physics.OverlapSphere(transform.position, attackRange, monsterLayer);//���̾� ����ũ ���� �߰�
            Transform nierTargetTransform = null;
            if (enemyCollider.Length > 0)
            {
                float nierTargetDistance = Mathf.Infinity;
                foreach (Collider collider in enemyCollider)
                {
                    float distance = Vector3.SqrMagnitude(transform.position - collider.transform.position);

                    if (distance < nierTargetDistance)
                    {
                        nierTargetDistance = distance;
                        nierTargetTransform = collider.transform;
                    }
                }
            }

            targetTransform = nierTargetTransform;

        }

    }


}
