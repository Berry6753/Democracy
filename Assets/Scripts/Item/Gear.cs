using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody body;

    [Header("회전 속도")]
    [SerializeField]
    private float rotationSpeed;

    private float GearCount;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        int GearRandom = Random.Range(0, 100);
        if (GearRandom < 60) GearCount = 10f;
        else if (GearRandom < 80) GearCount = 20f;
        else if (GearRandom < 92) GearCount = 30f;
        else if (GearRandom < 97) GearCount = 40f;
        else GearCount = 50f;

        body.useGravity = true;
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            body.useGravity = false;
            body.velocity = Vector3.zero;
        }
        
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player_Info>().AddGearCount((int)GearCount);
            gameObject.SetActive(false);            
        }
    }

    private void OnDisable()
    {
        QueueObjectPool.instance.Relese(gameObject);
    }
}
