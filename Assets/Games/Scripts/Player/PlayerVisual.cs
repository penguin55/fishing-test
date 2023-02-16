using UnityEngine;

namespace AriUtomo.Player
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private string animationCurrentMessage = "";

        public void UpdateAnimation(PlayerController.Status status, PlayerInput input)
        {
            if (status.onFishing && input.Fishing)
            {
                animator.SetTrigger("Cast Rod-Trigger");
            }

            animator.SetFloat("Move Magnitude", input.Walk.magnitude);
            if (status.currentState.Equals(PlayerController.State.FishingCast)) animator.SetBool("Cast Rod-Bool", input.CastRod);
            animator.SetBool("Fishing", status.onFishing);
            animator.SetBool("Pulling Fishing Line", status.onPullingFishingLine);
        }
        public void UpdateAnimationMessage(string animation_message) => animationCurrentMessage = animation_message;
        public bool CheckAnimationMessage(string animation_message) => animationCurrentMessage.Equals(animation_message.ToUpper());
    }
}