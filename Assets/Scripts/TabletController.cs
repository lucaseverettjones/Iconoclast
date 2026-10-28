using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletController : MonoBehaviour
{
    [SerializeField] string type;
    [SerializeField] Transform forceOriginPoint;
    RuleContainer ruleContainer;
    UIController uIController;
    ShatterScript shatterScript;
    Rigidbody rb;
    Transform player;
    bool broken;
    [SerializeField] AudioClip breakSound;
    AudioSource mySource;
    bool interacted;
    VFXController vFXController;
    private void Start()
    {
        shatterScript = GetComponent<ShatterScript>();
        ruleContainer = GameObject.Find("RuleContainer").GetComponent<RuleContainer>();
        uIController= GameObject.Find("Canvas").GetComponent<UIController>();
        mySource = GetComponent<AudioSource>();
    }
    public void FallOver()
    {
        if(!interacted)
        {
            StartCoroutine("FallSequence");
            interacted = true;
        }
    }
    IEnumerator FallSequence()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player").GetComponent<Transform>();
        Vector3 direction = Vector3.Normalize(transform.position- forceOriginPoint.position);
        rb.AddForce(direction, ForceMode.Impulse);
        yield return new WaitForSeconds(0.8f);
        broken = true;
    }
    IEnumerator DestroySequence()
    {
        shatterScript.Explode();
        //gameObject.GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        SFXController.instance.PlaySound(mySource, breakSound);
        vFXController = GameObject.Find("VFXController").GetComponent<VFXController>();
        vFXController.SpawnParticle(gameObject);
        //vFXController.Pieces("Tablet", gameObject);
        yield return new WaitForSeconds(breakSound.length);
        //Destroy(gameObject);
    }
    private void OnCollisionStay(Collision other)
    {
        if(broken && other.gameObject.CompareTag("TabletDestroyer"))
        {
            if (type.ToLower() == "crouch")
            {
                ruleContainer.canCrouch = true;
                uIController.Speak("", "You can now press C to crouch.", true, gameObject);
                uIController.Speak("", "Dash while crouching to slide.", true, gameObject);
            }
            else if (type.ToLower() == "dash")
            {
                ruleContainer.canDash = true;
                uIController.Speak("", "You can now left click to dash.", true, gameObject);
                uIController.Speak("", "Dash into cracked walls to break them.", true, gameObject);
            }
            else
            {
                ruleContainer.canJump = true;
                uIController.Speak("", "You can now press space to jump.", true, gameObject);
            }
            StartCoroutine(DestroySequence());
            broken = false;
        }
    }
}
