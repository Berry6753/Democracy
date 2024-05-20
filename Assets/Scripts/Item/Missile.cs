using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{

    [Header("���� ����Ʈ")]
    [SerializeField]
    private GameObject skillEffect;

    [Header("�߷� ���ӵ�")]
    [SerializeField]
    private float GravityAddForce;

    private Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(rigidbody.velocity.y < 0f)
        {
            rigidbody.AddForce(Vector3.down *  GravityAddForce, ForceMode.VelocityChange);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //����Ʈ ����
        ItemObjectPool.SpawnFromPool(skillEffect.name, transform.position, transform.rotation);

        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        ItemObjectPool.ReturnToPool(gameObject);
    }
}
