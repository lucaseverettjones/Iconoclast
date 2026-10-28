using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] string[] interactionText;
    [SerializeField] string[] ambientText;
    [SerializeField] string aggroText;
    [SerializeField] bool isHostile;
    [SerializeField] bool isDrunk;
    private GameObject player;
    UIController uIController;
    int playerHearingDistance;
    public PlayerController myPlayerController;
    [SerializeField] string characterName;
    [SerializeField] AudioClip[] speechClips;
    [SerializeField] AudioClip[] aggroClips;
    [SerializeField] AudioClip[] drunkClips;
    AudioSource mySource;
    bool moves;
    private Animator animator;
    private int interactIndex = 0, aggroIndex = 0, ambientIndex = 0;
    public void Start()
    {
        if(TryGetComponent<NPCPatrol>(out NPCPatrol nPCPatrol))
        {
            moves = true;
        }
        playerHearingDistance = 5;
        mySource = GetComponent<AudioSource>();
        player = GameObject.Find("Player");
        uIController = GameObject.Find("Canvas").GetComponent<UIController>();
        animator = GetComponent<Animator>();
        if (ambientText.Length > 0)
        {
            StartCoroutine("SpeakAmbiently");
        }
    }
    IEnumerator SpeakAmbiently()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(15,60));
            if (uIController.allowAmbientDialogue && !player.GetComponent<PlayerController>().freeze)
            {
                Vector3 distanceToPlayer = transform.position - player.transform.position;
                if (distanceToPlayer.magnitude < playerHearingDistance && uIController.TalkCoroutinesRunning == 0 && ambientText.Length > 0)
                {
                    //int speakIndex = Random.Range(0, ambientText.Length);
                    uIController.Speak(characterName, ambientText[ambientIndex], true);
                    ambientIndex++;
                    if (ambientIndex >= ambientText.Length)
                    {
                        ambientIndex = 0;
                    }
                    if (SFXController.instance != null)
                    {
                        if (isDrunk)
                        {
                            int soundIndex = Random.Range(0, drunkClips.Length);
                            SFXController.instance.PlaySound(mySource, speechClips[soundIndex]);
                        }
                        else
                        {
                            int soundIndex = Random.Range(0, speechClips.Length);
                            SFXController.instance.PlaySound(mySource, speechClips[soundIndex]);
                        }
                    }
                }
            }
        }
    }
    public void AggroSpeech()
    {
        
        
            uIController.Speak(characterName, aggroText, true);

            int soundIndex = Random.Range(0, aggroClips.Length);
            SFXController.instance.PlaySound(mySource, aggroClips[soundIndex]);
            
        

    }
    public void Interact()
    {
        //Debug.Log("Interaction");
        if (SFXController.instance != null)
        {
            if(isHostile && aggroClips.Length > 0)
            {
                int soundIndex = Random.Range(0, aggroClips.Length);
                SFXController.instance.PlaySound(mySource, aggroClips[soundIndex]);
            }
            else if(isDrunk && drunkClips.Length > 0)
            {
                int soundIndex = Random.Range(0, drunkClips.Length);
                SFXController.instance.PlaySound(mySource, speechClips[soundIndex]);
            }
            else if(speechClips.Length > 0)
            {
                int soundIndex = Random.Range(0, drunkClips.Length);
                SFXController.instance.PlaySound(mySource, speechClips[soundIndex]);
            }
        }
        foreach (string toSpeak in interactionText)
        {
            if (!isHostile)
            {
                uIController.Speak(characterName, toSpeak, false, gameObject);
            }
            else
            {
                uIController.Speak(characterName, toSpeak, false);
                StartTalking();
            }
        }
        
        if (isHostile)
        {
            myPlayerController.freeze = true;
            myPlayerController.StopAllCoroutines();
            StartTalking();
            StartCoroutine("WaitForTalkingToFinish");
        }
    }

    public void StartTalking()
    {
        if (animator != null)
        {
            animator.SetBool("isTalking", true);
        }
    }

    public void StopTalking()
    {
        if (animator != null)
        {
            animator.SetBool("isTalking", false);
        }
    }

    private IEnumerator WaitForTalkingToFinish()
    {
        WaitForSeconds delay = new WaitForSeconds(0.5f);
        while(uIController.TalkCoroutinesRunning > 0)
        {
            
            yield return delay;
        }
        StartCoroutine("Fade");
    }

    IEnumerator Fade()
    {
        player.GetComponent<PlayerController>().freeze = true;
        yield return new WaitForSeconds(1.2f);
        if (player.GetComponent<PlayerController>().lastCheckpoint == null)
        {
            uIController.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            player.GetComponent<PlayerController>().GoToCheckpoint(animator);
        }
    }
}
