using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_BuildSystem : MonoBehaviour
{
    [SerializeField] private float SelectBuildTurretIndex;
    [SerializeField] private LayerMask mask;

    private Player_Aiming aiming;
    private Player_Info info;

    private List<string> poolDicTag = new List<string>();

    public float BuildModeOn {  get; private set; }

    GameObject build;
    GameObject deleteBuild;

    private Vector3 buildPos;
    private Ray ray;

    private Vector2 screenCenterPoint;

    private void Awake()
    {
        aiming = GetComponent<Player_Aiming>();
        info = GetComponent<Player_Info>();
        BuildModeOn = -1f;
        SelectBuildTurretIndex = 0;
    }

    private Vector3 GetMouseWorldPosition()
    {
        screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, 8f, mask))
        {
            buildPos = hit.point;
        }
        else
        {
            buildPos = Camera.main.transform.position + new Vector3(transform.forward.x, 0, transform.forward.z) * 11.5f;
        }

        return buildPos;        
    }

    public void OnSelectTurret(InputAction.CallbackContext context)
    {
        if (aiming.isGameStop > 0) return;
        if (context.performed)
        {
            if (context.ReadValue<float>() > 0.5f)
            {
                SelectBuildTurretIndex++;

                if (SelectBuildTurretIndex >= MultiObjectPool.inst.poolDictionary.Count)
                {
                    SelectBuildTurretIndex = 0;
                }
            }
            else if (context.ReadValue<float>() < 0.5f)
            {
                SelectBuildTurretIndex--;

                if (SelectBuildTurretIndex < 0)
                {
                    SelectBuildTurretIndex = MultiObjectPool.inst.poolDictionary.Count - 1;
                }
            }

            if (build != null)
            {
                build.SetActive(false);
                build = MultiObjectPool.SpawnFromPool(poolDicTag[(int)SelectBuildTurretIndex], GetMouseWorldPosition());
            }
        }
        
    }

    public void OnChangeBuildMode(InputAction.CallbackContext callback)
    {
        if (aiming.isGameStop > 0) return;

        BuildModeOn *= -1;

        if(BuildModeOn > 0f)
        {
            foreach (var item in MultiObjectPool.inst.poolDictionary)
            {
                if (!poolDicTag.Contains(item.Key))
                {
                    poolDicTag.Add(item.Key);
                }
            }
        }
    }

    private void Update()
    {
        CreateBuilding();
        //BuildTurret();
    }

    private void CreateBuilding()
    {
        if (BuildModeOn > 0f)
        {
            screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit hit, 8f, mask))
            {
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) <= 0.5f) 
                {
                    DeleteBuild();
                    return;
                }

                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Turret"))
                {
                    Debug.Log("��ġ�� �ͷ� ã��");
                    deleteBuild = hit.transform.gameObject;

                    DeleteBuild();
                }
                else if (build != null)
                {
                    deleteBuild = null;
                    build.transform.position = GetMouseWorldPosition();
                    build.transform.rotation = transform.rotation;
                }
                else
                {
                    deleteBuild = null;
                    build = MultiObjectPool.SpawnFromPool(poolDicTag[(int)SelectBuildTurretIndex], GetMouseWorldPosition());
                }
            }
            else
            {
                //if (build != null)
                //{
                //    build.transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z) + new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * 8f;
                //    build.transform.rotation = transform.rotation;
                //}
                //else
                //{
                //    build = MultiObjectPool.SpawnFromPool(poolDicTag[(int)SelectBuildTurretIndex], GetMouseWorldPosition());
                //}
                DeleteBuild();
            }        
        }
        else
        {
            DeleteBuild();
        }
    }

    private void DeleteBuild()
    {
        if (build != null)
        {
            build.SetActive(false);
            build = null;
        }
    }

    public void BuildTurret()
    {
        if (BuildModeOn < 0f) return;

        if (GetComponent<Player_Aiming>().isFire)
        {          
            if(deleteBuild != null)
            {
                float destroyTurretGearCount = 0f;

                if(deleteBuild.GetComponent<Turret>() != null)
                {
                    destroyTurretGearCount = deleteBuild.GetComponent<Turret>().turretMakingCost;
                }
                else if (deleteBuild.GetComponent<Barrel>() != null)
                {
                    destroyTurretGearCount = deleteBuild.GetComponent<Barrel>().makingCost;
                }
                

                //���õ� �ͷ��� ���¸� destory�� ����

                //����� ����
                deleteBuild.SetActive(false);
                deleteBuild = null;

                //�ش� ��ž�� ���� ����� 50% ȸ��
                info.AddGearCount((int)Mathf.Round(destroyTurretGearCount * 0.5f));
            }
            else
            {
                if (build == null) return;
                if(build.GetComponent<Turret>() != null)
                {
                    if (build.GetComponent<Turret>().isMake)
                    {
                        CreateTurretPrecondition();
                    }
                }
                else if(build.transform.GetChild(0).GetComponent<Barrel>() != null)
                {
                    if (build.transform.GetChild(0).GetComponent<Barrel>().isMake)
                    {
                        CreateTurretPrecondition();
                    }
                }
               
            }
        }        
    }

    private void CreateTurretPrecondition()
    {
        int buildTurretGearCount;

        if (build.GetComponent<Turret>() != null)
        {
            buildTurretGearCount = (int)build.GetComponent<Turret>().turretMakingCost;
        }
        else
        {
            buildTurretGearCount = build.transform.GetChild(0).GetComponent<Barrel>().makingCost;
        }

        if (info.GearCount < buildTurretGearCount)
        {
            Debug.Log("����� ���� �����մϴ�.");
            return;
        }

        //��� �Ҹ�
        info.UseGear(buildTurretGearCount);
        //�ͷ� ����
        GameObject BuilingTurret = MultiObjectPool.SpawnFromPool(poolDicTag[(int)SelectBuildTurretIndex], build.transform.position, transform.rotation);

        //�ͷ��� ���¸� Build or Making�̶�� ����
        if (BuilingTurret.GetComponent<Turret>() != null)
        {
            BuilingTurret.GetComponent<Turret>().TurretMake();
        }
        else
        {
            BuilingTurret.transform.GetChild(0).GetComponent<Barrel>().Making();
        }

        //����� ����
        //������ �ͷ� �±׿� ���̾� ����
        //var childs = BuilingTurret.GetComponentsInChildren<Collider>();
        //foreach (var turretCollider in childs)
        //{
        //    turretCollider.gameObject.layer = LayerMask.NameToLayer("Turret");
        //    turretCollider.tag = "Turret";
        //    turretCollider.isTrigger = false;
        //}

        //BuilingTurret.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }
}
