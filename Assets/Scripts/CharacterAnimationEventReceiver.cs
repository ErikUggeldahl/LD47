using System;
using UnityEngine;

public class CharacterAnimationEventReceiver : MonoBehaviour
{
    public enum WhichDagger
    {
        Left = 0,
        Right = 1,
        Both = 2,
    }

    public event Action<WhichDagger> GrabDaggers;
    public event Action<WhichDagger> ReleaseDaggers;
    public event Action ThrowComplete;
    public event Action Footstep;

    public void GrabDaggerAnimEvent(int which)
    {
        GrabDaggers((WhichDagger)which);
    }

    public void ReleaseDaggerAnimEvent(int which)
    {
        ReleaseDaggers((WhichDagger)which);
    }

    public void ThrowCompleteAnimEvent()
    {
        ThrowComplete();
    }

    public void FootstepAnimEvent()
    {
        Footstep();
    }
}
