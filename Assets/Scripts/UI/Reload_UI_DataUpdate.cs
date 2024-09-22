using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Reload_UI_DataUpdate : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField]
    private TMP_Text Bullet_Text;

    [Header("Magazine")]
    [SerializeField]
    private TMP_Text Magazine_Text;

    private Player_Info player;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer.GetComponent<Player_Info>();
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            Bullet_Text.text = $"{player.equipedBulletCount} / {player.maxEquipedBulletCount}";
            Magazine_Text.text = $"{player.magazineCount} / {player.maxMagazineCount}";
        }
    }
}
