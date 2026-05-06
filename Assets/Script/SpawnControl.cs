using System.Collections;
using UnityEngine;

public class SpawnControl : MonoBehaviour
{


    [Header("Ground")]
    [SerializeField] public Transform[] SpawnPoint;
    [SerializeField] public GameObject[] EnemyPrefab;
    public float SpawnTime = 3f;
    private void Start()
    {
        StartCoroutine(SpawnObstacleRoutine());
    }
    IEnumerator SpawnObstacleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(SpawnTime);
            GroundSpawnRandomObstacleOne();
        }
        void GroundSpawnRandomObstacleOne()
        {
            int groundRandomObsIndex;
            int groundRandomSpawnIndex;
            groundRandomObsIndex = Random.Range(0, EnemyPrefab.Length);
            groundRandomSpawnIndex = Random.Range(0, SpawnPoint.Length);
            Instantiate(EnemyPrefab[groundRandomObsIndex], SpawnPoint[groundRandomSpawnIndex].position, Quaternion.identity);
        }
    }
}
