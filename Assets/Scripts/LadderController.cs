using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderController : MonoBehaviour
{
    [SerializeField] GameObject[] locations;
    private void OnCollisionEnter(Collision collision)
    {
        if(!collision.gameObject.CompareTag("Breakable") && locations.Length == 1)
        {
            locations[0].transform.parent = transform;
            locations[0].transform.SetSiblingIndex(0);
            tag = "Ladder";
        }
    }
}
