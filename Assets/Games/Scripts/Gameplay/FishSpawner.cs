using System.Collections;
using UnityEngine;

namespace AriUtomo.Gameplay
{
    public class FishSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] fishes;
        [SerializeField] private float minSpawnTime;
        [SerializeField] private float maxSpawnTime;

        private IEnumerator spawnCoroutine;

        //Abort the spawning task when the bait is withdrawn earlier before the fish bites the bait
        public void DeactiveLastSpawning()
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        }

        public void RandomSpawnFish(Vector3 spawn_point)
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = DoSpawnFish(spawn_point);
            StartCoroutine(spawnCoroutine);
        }

        private IEnumerator DoSpawnFish(Vector3 spawn_point)
        {
            var random_spawn_time = Random.Range(minSpawnTime, maxSpawnTime);
            var random_pick_index = Random.Range(0, fishes.Length);
            var random_angle = Random.Range(-180f, 180f);

            yield return new WaitForSeconds(random_spawn_time);
            Instantiate(fishes[random_pick_index], spawn_point, Quaternion.Euler(0f, random_angle, 0f));
        }
    }
}