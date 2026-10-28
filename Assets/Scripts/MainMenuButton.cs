using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class MainMenuButton : MonoBehaviour
{
    public UnityEvent unityEvent = new UnityEvent();
    public GameObject button;
    public GameObject light;
    [SerializeField] AudioSource breakSource;
    [SerializeField] AudioClip breakClip;
    UIController uI;
    // Start is called before the first frame update
    void Start()
    {
        light.SetActive(false);
        button = this.gameObject;
        uI = GameObject.Find("Canvas").GetComponent<UIController>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            if (!uI.menuActive)
            {
                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                {
                    unityEvent.Invoke();
                    if (gameObject.CompareTag("MMPlayButton"))
                    {
                        if (gameObject.TryGetComponent(out ShatterScript shatterScript))
                        {
                            shatterScript.Explode();
                            GameObject.Find("VFXController").GetComponent<VFXController>().SpawnParticle(gameObject);
                            if (SFXController.instance != null && breakClip != null && breakSource != null)
                            {
                                SFXController.instance.PlaySound(breakSource, breakClip);
                            }
                        }
                    }
                }
            }
        }
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject && (gameObject.CompareTag("MMButton") || gameObject.CompareTag("MMPlayButton")))
        {
            light.SetActive(true);
        }
        else
        {
            light.SetActive(false);
        }
    }
}
