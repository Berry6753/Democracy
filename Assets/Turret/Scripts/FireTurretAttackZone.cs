using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTurretAttackZone : MonoBehaviour
{
    [SerializeField]
    private GameObject parent;
    private Turret turret;
    private float checkTime;
    private void Awake()
    {
        turret =parent.GetComponent<Turret>();
    }

    private void Update()
    {
        if (turret.turretStateName != TurretStateName.ATTACK) 
        {
            gameObject.SetActive(false);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if ( other.gameObject.CompareTag("Monster")) 
        {
            checkTime += Time.deltaTime;
            if (checkTime >= 1 / turret.turretAttackSpeed) 
            {
                checkTime = 0;
                other.gameObject.GetComponent<Monster>().Hurt(turret.turretAttackDamge);
            }
            
        }
        else if (other.gameObject.CompareTag("Barrel"))
        {
            other.gameObject.GetComponent<Barrel>().Hurt();
        }


    }
}
