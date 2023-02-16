using UnityEngine;
using AriUtomo.InputSystem;

namespace AriUtomo.Player
{
    public class PlayerInput
    {
        public Vector2 Walk 
        { 
            get { return new Vector2(Input.GetAxis(InputData.WALK_HORIZONTAL), Input.GetAxis(InputData.WALK_VERTICAL)); } 
        }

        public bool Fishing 
        {
            get { return Input.GetButtonDown(InputData.FISHING); }
        }

        public bool CastRod
        {
            get { return Input.GetButton(InputData.FISHING); }
        }
    }
}