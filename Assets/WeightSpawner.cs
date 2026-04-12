using UnityEngine;

public class WeightSpawner : MonoBehaviour
{
    public GameObject smallWeightPrefab;
    public GameObject busterWeightPrefab;
    public Transform spawnPoint;

    void Update()
    {
        // 按 1 生成小重量块
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnSmallWeight();
        }

        // 按 2 生成大重量块
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnBusterWeight();
        }
    }

    public void SpawnSmallWeight()
    {
        Instantiate(smallWeightPrefab, spawnPoint.position, Quaternion.identity);
    }

    public void SpawnBusterWeight()
    {
        Instantiate(busterWeightPrefab, spawnPoint.position, Quaternion.identity);
    }
}