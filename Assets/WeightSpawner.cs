using UnityEngine;

public class WeightSpawner : MonoBehaviour
{
    public GameObject smallWeightPrefab;
    public GameObject busterWeightPrefab;
    public Transform spawnPoint;

    void Update()
    {
        // 按 7 生成小重量块
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SpawnSmallWeight();
        }

        // 按 8 生成大重量块
        if (Input.GetKeyDown(KeyCode.Alpha8))
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