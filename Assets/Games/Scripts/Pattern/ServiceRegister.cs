using UnityEngine;

namespace AriUtomo.Pattern
{
    public class ServiceRegister : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour service;

        private void Awake()
        {
            ServiceLocator.Provide(service);
        }
    }
}