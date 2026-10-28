using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarController : MonoBehaviour
{
    [SerializeField] int number;
    public MeshRenderer meshRenderer;
    public Material defaultMaterial;
    [SerializeField] Material glowMaterial;
    bool active;
    [SerializeField] GameObject fountain;
    FountainController fountainController;
    UIController uIController;
    public bool button1;
    public bool button2;
    public bool button3;
    AudioSource mySource;
    [SerializeField] AudioClip mySound;
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterial = meshRenderer.material;
        fountainController = fountain.GetComponent<FountainController>();
        uIController = GameObject.Find("Canvas").GetComponent<UIController>();
        try
        {
            mySource = GetComponent<AudioSource>();
        }
        catch
        {
            Debug.Log("Pillar has no audio source");
        }
    }
    public void Check()
    {
        if(button1 && button2 && button3)
        {
            active = true;
            meshRenderer.material = glowMaterial;
            fountainController.OnPillarActivated();
        }
    }
    public void Interact()
    {
        if(active)
        {
            if (fountainController == null)
            {
                fountainController = GameObject.Find("Fountain").GetComponent<FountainController>();
            }
            fountainController.Check(number, mySource, mySound);
        }
        else
        {
            uIController.Speak("", "This pillar is inactive. You'll have to find and press its activation buttons.", false, gameObject);
        }
    }
}
