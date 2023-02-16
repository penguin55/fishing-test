using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AriUtomo.UI
{
    public class HUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image castingRodBar;
        [SerializeField] private Image castingRodBar_Fill;

        public void EnableCastingBar(bool active)
        {
            castingRodBar.gameObject.SetActive(active);
        }

        public void UpdateCastingBar(float value)
        {
            castingRodBar_Fill.fillAmount = value;
        }
    }
}