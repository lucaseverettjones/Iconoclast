using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [SerializeField] int number;
    [SerializeField] GameObject pillar;
    PillarController pillarController;
    bool interacted;
    [SerializeField] Material activeMaterial;
    AudioSource mySource;
    [SerializeField] AudioClip buttonSound;
    private void Start()
    {
        mySource = GetComponent<AudioSource>();
    }
    public void Interact()
    {
        if(!interacted)
        {
            if(SFXController.instance != null)
            {
                SFXController.instance.PlaySound(mySource, buttonSound);
            }
            interacted = true;
            pillarController = pillar.GetComponent<PillarController>();
            if (number == 1)
            {
                pillarController.button1 = true;
                pillarController.Check();
            }
            else if (number == 2)
            {
                pillarController.button2 = true;
                pillarController.Check();
            }
            else
            {
                pillarController.button3 = true;
                pillarController.Check();
            }
            transform.GetChild(0).gameObject.GetComponent<Animator>().Play("Button");
            transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = activeMaterial;
            gameObject.tag = "Untagged";
        }
    }
}
