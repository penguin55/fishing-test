using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace AriUtomo.Manager
{
    public class SwarmFishManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] fishPrefabs;
        [SerializeField] private GameObject swarmParent;
        [SerializeField] private LayerMask fishLayer;

        private Vector3 fetchedPosition;
        private Tween t_baitFish;
        private GameObject fishBitesBait;
        private bool bittenBait;

        private const float REFERENCE_DISTANCE = 5f;
        private const float REFERENCE_TIME = 1f;
        private const float MINIMUM_TIME_TO_BAIT = 3f;

        public void BaitFish(Vector3 bait_position)
        {
            bittenBait = false;
            var colls = Physics.OverlapSphere(bait_position, 5f, fishLayer);

            if (colls.Length > 0)
            {
                fishBitesBait = colls[0].gameObject;
                fetchedPosition = fishBitesBait.transform.position;

                //Tween animation fish come to bait
                var delta_time_move = ((fishBitesBait.transform.position - bait_position).magnitude / REFERENCE_DISTANCE) * REFERENCE_TIME;
                var sequence = DOTween.Sequence();

                if (delta_time_move < MINIMUM_TIME_TO_BAIT)
                {
                    var remaining_time = MINIMUM_TIME_TO_BAIT - delta_time_move;
                    sequence.AppendInterval(remaining_time);
                }

                sequence.AppendCallback(()=> fishBitesBait.transform.LookAt(bait_position, Vector3.up));
                sequence.Append(fishBitesBait.transform.DOMove(bait_position, delta_time_move).SetEase(Ease.Linear));
                sequence.OnComplete(()=>
                {
                    bittenBait = true;
                    // Do animation jump a little to fish for feedback impact
                    fishBitesBait.transform.DOPunchPosition(Vector3.up * 0.2f, 0.2f).SetEase(Ease.Linear).OnKill(()=> fishBitesBait.transform.position = bait_position);
                });
                t_baitFish = sequence;
            }
        }

        public bool TryFetchFish(out GameObject fetched_fish)
        {
            var fetch_status = false;
            fetched_fish = null;
            t_baitFish.Kill();

            if (bittenBait)
            {
                fetched_fish = fishBitesBait;
                bittenBait = false;
                StartCoroutine(DoRespawnFish());
                fetch_status = true;
            } else
            {
                //Fish run back to original position
                var fish = fishBitesBait;
                var delta_time_move = ((fishBitesBait.transform.position - fetchedPosition).magnitude / REFERENCE_DISTANCE) * REFERENCE_TIME;

                var sequence = DOTween.Sequence();
                sequence.AppendInterval(0.3f);
                sequence.AppendCallback(() => fishBitesBait.transform.LookAt(fetchedPosition, Vector3.up));
                sequence.Append(fish.transform.DOMove(fetchedPosition, delta_time_move).SetEase(Ease.Linear));
            }

            return fetch_status;
        }

        //Spawn fish after last fish fetched by player
        private IEnumerator DoRespawnFish()
        {
            var random_spawn_time = Random.Range(3f, 6f);
            var random_index = Random.Range(0, fishPrefabs.Length);
            var random_angle = Random.Range(-180f, 180f);

            yield return new WaitForSeconds(random_spawn_time);
            Instantiate(fishPrefabs[random_index], fetchedPosition, Quaternion.Euler(Vector3.up * random_angle), swarmParent.transform);
        }
    }
}