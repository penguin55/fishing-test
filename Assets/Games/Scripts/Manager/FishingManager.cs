using AriUtomo.Helper;
using AriUtomo.Pattern;
using AriUtomo.UI;
using DG.Tweening;
using UnityEngine;

namespace AriUtomo.Manager
{
    public class FishingManager : MonoBehaviour
    {
        [Header("Fishing Properties")]
        [SerializeField] private float speedCastPowerBar;
        [SerializeField] private float nearestCastRod;
        [SerializeField] private float farestCastRod;
        [SerializeField] private LayerMask fishingLayer;
        [SerializeField] private LayerMask objectLayer;

        [Header("Fishing References")]
        [SerializeField] private Transform pivotRod;
        [SerializeField] private Transform bait;

        private HUD hud;
        private SwarmFishManager swarmFishManager;

        private float castPower;
        private int powerIncrementDirection = 1;
        private float baseThrowSpeed = 0.3f;

        protected void Awake()
        {
            hud = ServiceLocator.GetService<HUD>();
            swarmFishManager = ServiceLocator.GetService<SwarmFishManager>();
        }

        public void StartFishing()
        {
            castPower = 0f;
            hud.EnableCastingBar(true);
        }

        public void EndFishing()
        {
            foreach (Transform child in bait)
            {
                Destroy(child.gameObject);
            }

            bait.gameObject.SetActive(false);
        }

        //Holding the cast determines the value of the cast power, the greater the value of the cast power, the farther the bait will be thrown
        public void HoldCast()
        {
            castPower += powerIncrementDirection * speedCastPowerBar * Time.deltaTime;
            castPower = Mathf.Clamp01(castPower);

            if (castPower == 0) powerIncrementDirection = 1;
            else if (castPower == 1) powerIncrementDirection = -1;

            hud.UpdateCastingBar(castPower);
        }

        public bool TryThrowCast(out Tween throwing_tween, Transform cast_transform)
        {
            var throw_status = false;
            throwing_tween = null;
            hud.EnableCastingBar(false);

            var throw_distance = castPower.Remap(0, 1, nearestCastRod, farestCastRod);
            // Get origin cast with add the position of player with height offset = 1.5 and throw_distance according the cast power of the player
            var origin_cast = cast_transform.position + (Vector3.up * 1.5f) + (cast_transform.forward * throw_distance);
            var target_thrown = origin_cast;

            if (Physics.SphereCast(origin_cast, 0.3f, Vector3.down, out RaycastHit hit, 20f, fishingLayer))
            {
                bait.position = pivotRod.position;
                bait.gameObject.SetActive(true);

                //Checking the player is throwing to water or ground
                if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Water")))
                {
                    target_thrown = hit.point + (Vector3.down * 1.1f);
                    throw_status = true;
                    throwing_tween = bait.DOPath(new Vector3[] { pivotRod.position, (pivotRod.position + target_thrown) / 2f + Vector3.up * 2f, target_thrown }, baseThrowSpeed + (baseThrowSpeed * castPower), PathType.CatmullRom).SetEase(Ease.Linear);
                    throwing_tween.OnComplete(() => swarmFishManager.BaitFish(bait.position));
                } else
                {
                    target_thrown = hit.point;
                    throwing_tween = bait.DOPath(new Vector3[] { pivotRod.position, (pivotRod.position + target_thrown) / 2f + Vector3.up * 2f, target_thrown }, baseThrowSpeed + (baseThrowSpeed * castPower), PathType.CatmullRom).SetEase(Ease.Linear);
                }
            }

            return throw_status;
        }

        //Checking is there a fish when pulling bait
        public bool TryCatchFish()
        {
            var catch_status = swarmFishManager.TryFetchFish(out GameObject fetched_fish);
            if (catch_status) fetched_fish.transform.parent = bait;

            return catch_status;
        }


        //Pull Bait animation with dotween to make curve movement
        public void PullingBait(out Tween pulling_bait_tween)
        {
            var control_pos = (bait.position + pivotRod.position) / 2f + (Vector3.up * 1.5f);
            pulling_bait_tween = bait.DOPath(new Vector3[] { bait.position, control_pos, pivotRod.position }, (baseThrowSpeed / 1.4f) + (baseThrowSpeed / 1.4f * castPower), PathType.CatmullRom).SetEase(Ease.Linear);
        }
    }
}