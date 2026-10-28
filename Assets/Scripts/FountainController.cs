using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FountainController : MonoBehaviour
{
    [SerializeField] Material[] stages;
    [SerializeField] Material startingMaterial;
    int lastIndex, pillarsActive = 0;
    MeshRenderer meshRenderer;
    [SerializeField] AudioClip failSound;
    [SerializeField] AudioClip doorOpenSound;
    [SerializeField] AudioSource doorSource;
    [SerializeField] MeshRenderer doorRenderer;
    [SerializeField] Material doorStartingMaterial;
    [SerializeField] Material[] doorStages;
    Material[] doorMaterials;
    AudioSource mySource;
    public bool puzzleComplete = false;
    private void Start()
    {
        lastIndex = 0;
        meshRenderer = GetComponent<MeshRenderer>();
        mySource = GetComponent<AudioSource>();
        doorMaterials = doorRenderer.materials;
    }
    public void Check(int toCheck, AudioSource toPlayFrom, AudioClip toPlay)
    {
        if(toCheck == lastIndex + 1)
        {
            meshRenderer.material = stages[lastIndex];
            doorMaterials[1] = doorStages[lastIndex];
            doorRenderer.materials = doorMaterials;
            lastIndex++;
            if (SFXController.instance != null)
            {
                SFXController.instance.PlaySound(toPlayFrom, toPlay);
            }
            Debug.Log("Correctly Selected Pillar " + lastIndex);
            if (lastIndex == 4)
            {
                Debug.Log("Complete!");
                try
                {
                    GameObject.Find("Canvas").GetComponent<UIController>().Speak("", "You hear a door open.", true);
                }
                catch
                {
                    Debug.Log("Cannot speak");
                }
                doorRenderer.gameObject.tag = "TeleportPoint";
                puzzleComplete = true;
                if (SFXController.instance != null)
                {
                    SFXController.instance.PlaySound(doorSource, doorOpenSound);
                }
            }
        } else if(lastIndex != 4)
        {
            Debug.Log("Failure!");
            meshRenderer.material = startingMaterial;
            doorMaterials[1] = doorStartingMaterial;
            doorRenderer.materials = doorMaterials;
            lastIndex = 0;
        }
    }

    public void OnPillarActivated()
    {
        pillarsActive++;
        if(pillarsActive >= 4)
        {
            meshRenderer.material = startingMaterial;
        }
    }
}
