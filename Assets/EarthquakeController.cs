using UnityEngine;

public class EarthquakeController : MonoBehaviour
{
    public Rigidbody platformRb;

    public float amplitude = 0.08f;   // 左右振幅
    public float frequency = 2.5f;    // 频率
    public float duration = 4f;       // 地震持续时间

    private Vector3 startPos;
    private float timer = 0f;
    private bool isShaking = false;

    void Start()
    {
        if (platformRb != null)
        {
            startPos = platformRb.position;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isShaking)
        {
            StartEarthquake();
        }
    }

    void FixedUpdate()
    {
        if (!isShaking || platformRb == null) return;

        timer += Time.fixedDeltaTime;

        float offsetX = Mathf.Sin(timer * frequency * Mathf.PI * 2f) * amplitude;
        Vector3 targetPos = startPos + new Vector3(offsetX, 0f, 0f);

        platformRb.MovePosition(targetPos);

        if (timer >= duration)
        {
            StopEarthquake();
        }
    }

    public void StartEarthquake()
    {
        isShaking = true;
        timer = 0f;
        startPos = platformRb.position;
    }

    public void StopEarthquake()
    {
        isShaking = false;
        platformRb.MovePosition(startPos);
    }
}
// ......//