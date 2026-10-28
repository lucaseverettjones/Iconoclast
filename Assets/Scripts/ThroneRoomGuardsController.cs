using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThroneRoomGuardsController : MonoBehaviour
{
    [SerializeField] private NPCInteraction speakingGuard;

    public void Interact()
    {
        speakingGuard.Interact();
    }
}
