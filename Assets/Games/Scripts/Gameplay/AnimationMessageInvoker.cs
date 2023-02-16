using AriUtomo.Helper;
using UnityEngine;

namespace AriUtomo.Gameplay
{
    public class AnimationMessageInvoker : MonoBehaviour
    {
        [SerializeField] private StringUnityEvent onInvoke;

        public void Invoke(string invoke_message) => onInvoke.Invoke(invoke_message);
    }
}