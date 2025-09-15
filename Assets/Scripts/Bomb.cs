using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject BombPrefab;
    public List<Transform>SpawnPosition;

    [Header("oŒ»Ý’è")]
    public float SpawnTime=5f;
    public int Maxinstantiate=20;
    public int InstantiateBomb=0;
    private void OnEnable()
    {
        StartCoroutine("BombSpawn");
    }

    IEnumerator BombSpawn()
    {
     while(Maxinstantiate>InstantiateBomb)
        {
            var p = SpawnPosition[Random.Range(0, SpawnPosition.Count)];
            Instantiate(BombPrefab, p.position, Quaternion.identity);
            InstantiateBomb++;
            yield return new WaitForSeconds(SpawnTime);
        }

    }
}
