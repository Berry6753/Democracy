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

    private void Update()
    {
        if(rigidbody.velocity.y < 0f)
        {
            rigidbody.AddForce(Vector3.down *  GravityAddForce * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //����Ʈ ����
        Instantiate(skillEffect);
        gameObject.SetActive(false);
    }
}
