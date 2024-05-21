using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Command : MonoBehaviour
{
    [Header("Player LevelUp Cost �ʱⰪ")]
    [SerializeField]
    private float LevelUpCost;

    [Header("LevelUp Cost ���� ��ġ(���� : n�� )")]
    [SerializeField]
    private float upCostValue;

    private Player_Info info;
    public bool isCommand { get; private set; }
    public bool isCore;

    public Queue<int> commandQueue;

    private bool isPush;

    private void Awake()
    {
        info = GetComponent<Player_Info>();
        commandQueue = new Queue<int>();
    }

    public void OnCommand(InputAction.CallbackContext context)
    {
        if (context.started) return;
        if (context.performed)
        {
            if (isCore && !isCommand)
            {
                isCommand = true;

                Debug.Log("Ŀ��� ON");

                //�ൿ ����
                StartCoroutine(InputSelectAction());
                //������ Ŀ��� �Է�
                //StartCoroutine(PushCommand());
            }
        }
    }

    IEnumerator InputSelectAction()
    {
        while (true)
        {
            commandQueue.Clear();

            isPush = false;

            yield return new WaitUntil(() => Input.anyKeyDown);
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Ŀ��� ���");
                commandQueue.Enqueue(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Player Upgrade");
                commandQueue.Enqueue(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("Core Upgrade");
                commandQueue.Enqueue(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log("ž ����");
                commandQueue.Enqueue(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Debug.Log("X");
                commandQueue.Enqueue(0);
            }

            isPush = true;
            yield return new WaitUntil(() => isPush);
            
            if(commandQueue.Count > 0)
            {
                SelectAction();                             
                yield break;
            }
        }
    }

    private void SelectAction()
    {
        switch(commandQueue.Dequeue())
        {
            case 1:
                StartCoroutine(PushCommand());
                break;
            case 2:
                StartCoroutine(UpgradePlayer());
                break;
            case 3:
                CoreUpgrade();
                commandQueue.Clear();
                isCommand = false;
                break;
            case 4:
                Core.instance.Reloading();
                commandQueue.Clear();
                isCommand = false;
                break;
            default:
                Debug.Log("Ŀ��Ʈ ����");
                commandQueue.Clear();
                isCommand = false;
                break;
        }
    }

    private void CoreUpgrade()
    {
        if (info.GearCount < Core.instance.upgradeCost)
            Core.instance.isUpgrading = true;
    }

    IEnumerator UpgradePlayer()
    {
        while (true)
        {
            commandQueue.Clear();

            isPush = false;

            yield return new WaitUntil(() => Input.anyKeyDown);
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("���׷��̵�");
                commandQueue.Enqueue(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("X");
                commandQueue.Enqueue(0);
            }

            isPush = true;
            yield return new WaitUntil(() => isPush);

            if (commandQueue.Count <= 0) continue;

            switch (commandQueue.Dequeue())
            {
                case 1:
                    //�ڽ�Ʈ�� �����ϸ� continue;
                    if (info.GearCount < LevelUpCost)
                    {
                        Debug.Log("��� ����");
                        continue;
                    }
                    //�ڽ�Ʈ �Һ�
                    info.UseGear((int)Mathf.Round(LevelUpCost));
                    //Player ���� ����
                    info.UpgradePlayer();
                    //�ʿ� �ڽ�Ʈ ����
                    LevelUpCost *= upCostValue;

                    commandQueue.Clear();

                    Debug.Log("�������� ������ ��� ���ӵ�");
                    yield return true;
                    break;
                default:
                    //�ݱ�
                    Debug.Log("Ŀ��Ʈ ����");
                    commandQueue.Clear();
                    //isCommand = false;
                    yield break;
            }
        }
    }

    IEnumerator PushCommand()
    {
        commandQueue.Clear();

        while (true)
        {
            isPush = false;

            yield return new WaitUntil(() => Input.anyKeyDown);
            if (Input.GetKeyDown(KeyCode.W))
            {
                commandQueue.Enqueue(1);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                commandQueue.Enqueue(2);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                commandQueue.Enqueue(3);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                commandQueue.Enqueue(4);
            }
            else
            {
                Debug.Log("X");
                commandQueue.Enqueue(0);
            }

            isPush = true;
            yield return new WaitUntil(() => isPush);
        
            if (commandQueue.Count == 4)
            {
                if (Core.instance.CheckeCommand())
                {
                    commandQueue.Clear();
                    //Ŀ��� ��� OFF
                    isCommand = false;
                    yield break;
                }
                else
                {
                    commandQueue.Clear();
                }
                
                //StopCoroutine(PushCommand());
            }

        }
    }
}
