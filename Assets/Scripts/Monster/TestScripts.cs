using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestScripts : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] private float hp = 100;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            Debug.Log("���ع���");
        }
    }

    private void Hurt(float damage)
    { 
        hp -= damage;
    }
}
