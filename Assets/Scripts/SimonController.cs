using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class SimonController : MonoBehaviour
{
    int[] combination;
    int index;
    bool glowing;
    [SerializeField] Animator doorAnim;
    bool opened;
    [SerializeField] MeshRenderer[] glowObjects;
    Material[] defaultMaterials;
    [SerializeField] Material[] glowMaterials;
    [SerializeField] AudioClip failSound;
    AudioSource mySource;
    [SerializeField] AudioClip doorOpenSound;
    [SerializeField] AudioSource doorSource;
    [SerializeField] AudioSource[] simonSources;
    [SerializeField] AudioClip[] bellSounds;
    [SerializeField] Animator buttonAnim;
    [SerializeField] AudioClip buttonSound;
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material activeMaterial;
    [SerializeField] MeshRenderer buttonRenderer;
    [SerializeField] Animator[] simonAnimators;
    void Start()
    {
        mySource = GetComponent<AudioSource>();
        combination = new int[4];
        index = 0;
        for(int i = 0; i < combination.Length; i++)
        {
            combination[i] = Random.Range(1, 4);
        }
        Debug.Log("The combination is " + combination[0] + combination[1] + combination[2] + combination[3]);
    }
    public void Check(int toCheck)
    {
        if(!opened && !glowing)
        {
            if (combination[index] == toCheck && !glowing)
            {
                if (SFXController.instance != null)
                {
                    Debug.Log("Played bell sound");
                    StartCoroutine(GlowSingle(toCheck - 1));
                }
                if (index != 3)
                {
                    index++;
                }
                else if (!opened)
                {
                    doorAnim.Play("DoorOpen");
                    try
                    {
                        GameObject.Find("Canvas").GetComponent<UIController>().Speak("", "You hear a door open.", true);
                    }
                    catch
                    {
                        Debug.Log("Cannot speak");
                    }
                    doorAnim.gameObject.tag = "Untagged";
                    opened = true;
                }
            }
            else
            {
                if (SFXController.instance != null)
                {
                    SFXController.instance.PlaySound(simonSources[toCheck - 1], failSound);
                }
                index = 0;
                StartCoroutine("Glow");
            }
        }
    }
    public void GlowSequence()
    {
        if(!glowing && !opened)
        {
            Press();
            StartCoroutine("Glow");
        }
    }
    IEnumerator GlowSingle(int index)
    {
        glowing = true;
        Debug.Log("Glowing index: " + index);
        if (simonAnimators[index] != null)
        {
            simonAnimators[index].Play("Bell");
            SFXController.instance.PlaySound(simonSources[index], bellSounds[index]);
        }
        yield return new WaitForSeconds(1.5f);
        glowing = false;
    }
    public void Press()
    {
        if(!glowing)
        {
            if (buttonAnim != null)
            {
                buttonAnim.Play("Button");
            }
            if (SFXController.instance != null)
            {
                SFXController.instance.PlaySound(mySource, buttonSound);
            }
            StartCoroutine("Glow");
        }
    }
    IEnumerator Glow()
    {
        glowing = true;
        buttonRenderer.material = activeMaterial;
        for(int i = 0; i < combination.Length; i++)
        {
            if(simonAnimators[combination[i] - 1] != null)
            {
                simonAnimators[combination[i] - 1].Play("Bell");
                SFXController.instance.PlaySound(simonSources[combination[i] - 1], bellSounds[combination[i] - 1]);
            }
            yield return new WaitForSeconds(1.5f);
        }
        buttonRenderer.material = defaultMaterial;
        glowing = false;
    }
}
