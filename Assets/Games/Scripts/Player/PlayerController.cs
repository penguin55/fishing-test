using AriUtomo.Manager;
using AriUtomo.Pattern;
using DG.Tweening;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AriUtomo.Player
{
    [RequireComponent(typeof(PlayerVisual))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float gravity;
        [SerializeField] private float groundOffset;
        [SerializeField] private float groundRadius;
        [SerializeField] private LayerMask groundLayer;

        private StateMachine<State> state;
        private FishingManager fishingManager;
        private CharacterController controller;
        private PlayerVisual visual;
        private PlayerInput input;
        [SerializeField] private Status status;

        float rotationVelocity = 0;
        float targetRotation = 0;
        float rotationSmoothTime = 0.03f;

        public enum State { Normal, FishingCast, FishingIdle, FishingCatch}

        #region Unity Function
        protected void Awake()
        {
            state = StateMachine<State>.Initialize(this);
            input = new PlayerInput();
            status = new Status();
            controller = GetComponent<CharacterController>();
            visual = GetComponent<PlayerVisual>();
            fishingManager = ServiceLocator.GetService<FishingManager>();
        }

        private void Start()
        {
            state.Changed += st =>
            {
                status.lastState = status.currentState;
                status.currentState = st;
            };

            state.ChangeState(State.Normal);
        }
        #endregion

        #region Finite States Machine
        private void Normal_Update()
        {
            GroundedCheck();

            //Movement logic
            var input_direction = new Vector3(input.Walk.x, 0f, input.Walk.y).normalized;
            float calculate_speed = 0f;
            if (input.Walk != Vector2.zero)
            {
                calculate_speed = walkSpeed;
                targetRotation = Mathf.Atan2(input_direction.x, input_direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            if (input.Fishing)
            {
                if (status.currentState.Equals(State.Normal))
                {
                    visual.UpdateAnimation(status, input);
                    state.ChangeState(State.FishingCast);
                    return;
                }
            }

            status.velocity = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward * calculate_speed * Time.deltaTime;
            Gravity();
            controller.Move(status.velocity);

            visual.UpdateAnimation(status, input);
        }

        private void Normal_Exit()
        {
            status.velocity = Vector3.zero;
        }

        private void FishingCast_Enter()
        {
            status.onFishing = true;
            fishingManager.StartFishing();
            visual.UpdateAnimation(status, input);
        }

        private void FishingCast_Update()
        {
            GroundedCheck();

            if (input.CastRod)
            {
                //Hold casting when player hold the input
                fishingManager.HoldCast();
            }
            else
            {
                state.ChangeState(State.FishingIdle);
                return;
            }

            Gravity();
            controller.Move(status.velocity);

            visual.UpdateAnimation(status, input);
        }

        private IEnumerator FishingIdle_Enter()
        {
            visual.UpdateAnimation(status, input);
            //Checking the fishing casting animation already sent a CAST_FISHING_LINE message from the animation or not
            yield return new WaitUntil(() => visual.CheckAnimationMessage("CAST_FISHING_LINE"));
            //Throwing cast animation with DOTween
            bool throw_success = fishingManager.TryThrowCast(out Tween throwing_tween, transform);
            yield return throwing_tween?.WaitForCompletion();

            //Force back to Normal state when throwing bait to ground
            if (!throw_success)
            {
                fishingManager.EndFishing();
                yield return new WaitForSeconds(0.3f);
                status.onFishing = false;
                visual.UpdateAnimation(status, input);
                state.ChangeState(State.Normal);
            } 
        }

        private void FishingIdle_Update()
        {
            GroundedCheck();

            if (input.Fishing)
            {
                //Going to catch state
                status.onPullingFishingLine = true;
                state.ChangeState(State.FishingCatch);

                return;
            }

            Gravity();
            controller.Move(status.velocity);

            visual.UpdateAnimation(status, input);
        }

        private void FishingIdle_Exit()
        {
            status.onFishing = false;
            visual.UpdateAnimation(status, input);
        }

        private IEnumerator FishingCatch_Enter()
        {
            //Trying to catch fish on water
            fishingManager.TryCatchFish();
            visual.UpdateAnimation(status, input);

            //Checking if the catching animation has sent a CATCH_FISH message from the animation
            yield return new WaitUntil(() => visual.CheckAnimationMessage("CATCH_FISH"));
            //Do pull bait animation with DOTween
            fishingManager.PullingBait(out Tween pulling_bait_tween);
            yield return pulling_bait_tween.WaitForCompletion();

            fishingManager.EndFishing();
            visual.UpdateAnimation(status, input);
            status.onPullingFishingLine = false;
            state.ChangeState(State.Normal);
        }
        #endregion

        #region Internal Function
        private void Gravity()
        {
            if (status.onGround)
            {
                if (status.velocity.y < 0.0f)
                {
                    status.velocity.y = 0f;
                }
            }
            else
            {
                if (status.velocity.y > gravity)
                {
                    status.velocity.y += gravity * Time.deltaTime;
                }
            }
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundOffset,
                transform.position.z);
            status.onGround = Physics.CheckSphere(spherePosition, groundRadius, groundLayer,
                QueryTriggerInteraction.Ignore);
        }
        #endregion

        [System.Serializable]
        public class Status
        {
            //State Info
            public State lastState;
            public State currentState;

            //Status
            public Vector3 velocity;
            public bool onGround;
            public bool onFishing;
            public bool onPullingFishingLine;
        }
    }
}