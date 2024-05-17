using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    [SerializeField] private Wave[] waves;               //���� ���̺� ����
    [SerializeField] private MonsterSpawner monsterSpawner;
    private int currentWaveIndex = 0;

    public void StartWave()
    {
        if (monsterSpawner.MonsterList.Count == 0 && currentWaveIndex < waves.Length)
        {
            currentWaveIndex++;
            monsterSpawner.StartWave(waves[currentWaveIndex]);
        }
    }
}

[System.Serializable]
public struct Wave
{
    public float spawnTime;             //���� ���̺� �� ���� �ֱ�
    public int[] maxMonsterCount;           //���� ���̺� �� ���� ����
    public GameObject[] mosnterPrefab;    //���� ������

}