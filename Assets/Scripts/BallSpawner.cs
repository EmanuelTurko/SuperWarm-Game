using UnityEngine;

namespace Assets.Scripts
{
    public class FallingBallSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private int minBallsPerSpawn = 1;
        [SerializeField] private int maxBallsPerSpawn = 2;
        [SerializeField] private Vector3 spawnArea = new Vector3(3f, 0f, 3f);
        [SerializeField] private float ballLifetime = 6f;

        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnBalls();
            }
        }

        private void SpawnBalls()
        {
            if (ballPrefab == null)
                return;

            int count = Random.Range(minBallsPerSpawn, maxBallsPerSpawn + 1);

            for (int i = 0; i < count; i++)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
                    0f,
                    Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
                );

                Vector3 spawnPos = transform.position + randomOffset;

                GameObject ball = Instantiate(ballPrefab, spawnPos, Random.rotation);
                Destroy(ball, ballLifetime);
            }
        }
    }
}