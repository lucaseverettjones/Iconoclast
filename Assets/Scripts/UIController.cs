using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIController : MonoBehaviour
{
    public bool menuActive;
    Animator transition;
    Slider[] sliders;
    Toggle toggle;
    TextMeshProUGUI[] textBoxes;
    bool speaking;
    bool waitingToClear;
    GameObject menu;
    public string currentScene;
    GameObject interactText;
    GameObject textBackground;
    AudioSource fountainSource;
    GameObject abilities;
    GameObject[] abilitiesText;
    RuleContainer ruleContainer;
    PlayerController player;
    private int talkCoroutinesRunning = 0;
    public int TalkCoroutinesRunning { get { return talkCoroutinesRunning; } }
    private Queue<string> queuedDialogue, queuedNames;
    [SerializeField] GameObject[] mainMenu;
    public bool allowAmbientDialogue = true;
    private void Start()
    {
        queuedDialogue = new Queue<string>();
        queuedNames = new Queue<string>();
        sliders = new Slider[3];
        textBoxes = new TextMeshProUGUI[3];
        abilitiesText= new GameObject[4];
        currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Logo")
        {
            StartCoroutine("LoadMenu");
        }
        if (currentScene == "Level1" || currentScene == "Level2" || currentScene == "Level3" || currentScene == "Menu")
        {
            menu = GameObject.Find("Menu");
            Debug.Log("Found Menu");
            toggle = GameObject.Find("Toggle").GetComponent<Toggle>();
            Debug.Log("Found Toggle");
            sliders[0] = GameObject.Find("Music").GetComponent<Slider>();
            Debug.Log("Found Music");
            sliders[0].value = musicVolume;
            sliders[1] = GameObject.Find("SFX").GetComponent<Slider>();
            Debug.Log("Found SFX");
            if (currentScene == "Level3")
            {
                fountainSource = GameObject.Find("Fountain").GetComponent<AudioSource>();
            }
            sliders[1].value = sfxVolume;
            sliders[2] = GameObject.Find("Mouse").GetComponent<Slider>();
            Debug.Log("Found Mouse");
            sliders[2].value = mouseSensitivity;
            if (headBob == 1)
            {
                toggle.isOn = true;
            }
            else
            {
                toggle.isOn = false;
            }
            menu.SetActive(false);
        }
        if(currentScene == "Level1" || currentScene == "Level2" || currentScene == "Level3")
        {
            ruleContainer = GameObject.Find("RuleContainer").GetComponent<RuleContainer>();
            player = GameObject.Find("Player").GetComponent<PlayerController>();
            textBoxes[0] = GameObject.Find("Text3").GetComponent<TextMeshProUGUI>();
            textBoxes[1] = GameObject.Find("Text2").GetComponent<TextMeshProUGUI>();
            textBoxes[2] = GameObject.Find("Text1").GetComponent<TextMeshProUGUI>();
            textBackground = GameObject.Find("TextBackground");
            interactText = GameObject.Find("InteractText");
            interactText.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (textBackground != null)
        {
            textBackground.SetActive(false);
        }
        if (currentScene == "Level1")
        {
            Speak("", "Use WASD or arrow keys to move.", true, gameObject);
            Speak("", "Use E to interact with objects and NPCs.", true, gameObject);
            Speak("", "You can also skip dialogue with E.", true, gameObject);
            Speak("", "You can press Esc to open the pause menu.", true);
        }
        abilities = GameObject.Find("Abilities");
        try
        {
            abilitiesText[0] = GameObject.Find("MoveText");
            abilitiesText[1] = GameObject.Find("JumpText");
            abilitiesText[2] = GameObject.Find("DashText");
            abilitiesText[3] = GameObject.Find("CrouchText");
        }
        catch
        {
            Debug.Log("Where's the abilities text??");
        }
        if (menu != null)
        {
            menu.SetActive(false);
        }
        if (abilities != null)
        {
            abilities.SetActive(false);
        }
    }
    public float musicVolume
    {
        get
        {
            if(PlayerPrefs.HasKey("musicVolume") )
            {
                return PlayerPrefs.GetFloat("musicVolume");
            } else
            {
                return 0.5f;
            }
        }
        set
        {
            PlayerPrefs.SetFloat("musicVolume", value);
        }
    }
    public float sfxVolume
    {
        get
        {
            if(PlayerPrefs.HasKey("sfxVolume"))
            {
                return PlayerPrefs.GetFloat("sfxVolume");
            }
            else
            {
                return 0.5f;
            }
            
        }
        set
        {
            PlayerPrefs.SetFloat("sfxVolume", value);
        }
    }
    public float mouseSensitivity
    {
        get
        {
            if(PlayerPrefs.HasKey("mouseSensitivity"))
            {
                return PlayerPrefs.GetFloat("mouseSensitivity");
            }
            else
            {
                return 0.5f;
            }
        }
        set
        {
            PlayerPrefs.SetFloat("mouseSensitivity", value);
        }
    }

    public int headBob
    {
        get
        {
            if(PlayerPrefs.HasKey("headBob"))
            {
                return PlayerPrefs.GetInt("headBob");
            }
            else
            {
                return 0;
            }
        }
        set
        {
            if(toggle.isOn)
            {
                PlayerPrefs.SetInt("headBob", 1);
            } else
            {
                PlayerPrefs.SetInt("headBob", 0);
            }
        }
    }

    public void ToggleInteract()
    {
        if(interactText.activeInHierarchy)
        {
            interactText.SetActive(false);
        }
        else
        {
            interactText.SetActive(true);
        }
    }
    public void ToggleMenu()
    {
        if(menu.activeInHierarchy)
        {
            menu.SetActive(false);
            menuActive = false;
            if (currentScene == "Menu")
            {
                for (int i = 0; i < mainMenu.Length; i++)
                {
                    mainMenu[i].SetActive(true);
                }
            }
            if (abilities != null) 
            {
                abilities.SetActive(false);
            }
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3")
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().freeze = false;
            }
            //Time.timeScale = 1;
        }
        else if(abilities != null)
        {
            if(abilities.activeInHierarchy) 
            {
                menu.SetActive(false);
                abilities.SetActive(false);
                string sceneName = SceneManager.GetActiveScene().name;
                if (sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3")
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().freeze = false;
                }
            }
            else
            {
                menu.SetActive(true);
                if (currentScene == "Menu")
                {
                    for (int i = 0; i < mainMenu.Length; i++)
                    {
                        mainMenu[i].SetActive(false);
                    }
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                //Time.timeScale = 0;
            }
        }
        else
        {
            menu.SetActive(true);
            menuActive = true;
            if (currentScene == "Menu")
            {
                for (int i = 0; i < mainMenu.Length; i++)
                {
                    mainMenu[i].SetActive(false);
                }
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            //Time.timeScale = 0;
        }
    }

    private IEnumerator LoadMenu()
    {
        yield return new WaitForSeconds(3);
        LoadScene("Menu");
    }

    //Starts loading a scene
    public void LoadScene(string scene)
    {
        Time.timeScale = 1;
        if(scene == "")
        {
            scene = currentScene;
        }
        else if(scene == "Next")
        {
            if(currentScene == "Level1")
            {
                StartCoroutine("SceneTransition", "Level2");
            }
            else if (currentScene == "Level2")
            {
                StartCoroutine("SceneTransition", "Level3");
            }
            else
            {
                StartCoroutine("SceneTransition", "Ending");
            }
        }
        else
        {
            StartCoroutine("SceneTransition", scene);
        }
    }
    public void UpdatePreference(string toUpdate)
    {
        if(toUpdate.ToLower() == "music")
        {
            musicVolume = sliders[0].value;
        }
        else if (toUpdate.ToLower() == "sfx")
        {
            sfxVolume = sliders[1].value;
            if(currentScene == "Level3")
            {
                fountainSource.volume = sfxVolume;
            }
        }
        else if (toUpdate.ToLower() == "headbob")
        {
            if(toggle.isOn)
            {
                headBob = 1;
            }
            else
            {
                headBob = 0;
            }
            try
            {
                GameObject.Find("Player").GetComponent<PlayerController>().UpdateShouldHeadBob();
            }
            catch
            {
                Debug.Log("Player Not In Scene");
            }
        }
        else
        {
            mouseSensitivity = sliders[2].value;
            Debug.Log(mouseSensitivity);
            try
            {
                GameObject.Find("Player").GetComponent<PlayerController>().UpdateMouseSensitivity();
            }
            catch
            {
                Debug.Log("Player Not In Scene");
            }
        }
    }
    //Closes the game
    public void Quit()
    {
        Application.Quit();
    }
    IEnumerator SceneTransition(string scene)
    {
        transition = GameObject.Find("Transition").GetComponent<Animator>();
        transition.Play("TransitionEnd");
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(scene);
    }
    public void Speak(string name, string toSpeak, bool speakingAmbiently, GameObject sender = null)
    {
        if (toSpeak != "")
        {
            allowAmbientDialogue = false;
            if (!speakingAmbiently)
            {
                player.freeze = true;
            }
            if (talkCoroutinesRunning <= 0)
            {
                //Debug.Log(sender + " started a talk coroutine");
                talkCoroutinesRunning++;
                if (sender != null)
                {
                    if (sender.TryGetComponent(out NPCInteraction npc))
                    {
                        StartCoroutine(Talk(name, toSpeak, npc));
                    }
                    else
                    {
                        StartCoroutine(Talk(name, toSpeak));
                    }
                }
                else
                {
                    StartCoroutine(Talk(name, toSpeak));
                }
            }
            else
            {
                queuedDialogue.Enqueue(toSpeak);
                queuedNames.Enqueue(name);
            }
        }
    }
    IEnumerator Talk(string myName, string toSpeak, NPCInteraction npc = null)
    {
        if(npc != null) 
        {
            npc.StartTalking();
        }
        if(!textBackground.activeInHierarchy && toSpeak != "")
        {
            textBackground.SetActive(true);
        }    
        speaking = true;
        if (textBoxes[1].text == "")
        {
            textBoxes[1].text = textBoxes[0].text;
        }
        else
        {
            textBoxes[2].text = textBoxes[1].text;
            textBoxes[1].text = textBoxes[0].text;
        }
        if(myName != "")
        {
            textBoxes[0].text = myName + ": ";
        }
        else
        {
            textBoxes[0].text = "";
        }
        bool skip = false;
        yield return new WaitForSeconds(0.1f);
        foreach (char letter in toSpeak)
        {
            if(!skip)
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    skip = true;
                    yield return null;
                    textBoxes[0].text += letter;
                }
                else
                {
                    textBoxes[0].text += letter;
                    yield return new WaitForSeconds(0.03f);
                }
            }
            else
            {
                textBoxes[0].text += letter;
            }
        } 
        allowAmbientDialogue = true;
        if(queuedDialogue.Count > 0)
        {
            string nextString = queuedDialogue.Peek();
            string nextName = queuedNames.Peek();
            queuedDialogue.Dequeue();
            queuedNames.Dequeue();
            yield return new WaitForSeconds(0.5f);
            if (npc != null)
            {
                StartCoroutine(Talk(nextName, nextString, npc));
            }
            else
            {
                StartCoroutine(Talk(nextName, nextString));
            }
        }
        else
        {
            player.freeze = false;
            speaking = false;
            talkCoroutinesRunning--;
            if (npc != null)
            {
                npc.StopTalking();
            }
            if (!waitingToClear)
            {
                StartCoroutine("ClearText");
            }
        }
    }
    IEnumerator ClearText()
    {
        waitingToClear = true;
        while(true)
        {
            yield return new WaitForSeconds(5);
            if(!speaking)
            {
                break;
            }
        }
        while (waitingToClear)
        {
            if (queuedDialogue.Count == 0)
            {
                if (textBoxes[2].text != "")
                {
                    textBoxes[2].text = "";
                }
                else if (textBoxes[1].text != "")
                {
                    textBoxes[1].text = "";
                }
                else if (textBoxes[0].text != "")
                {
                    textBoxes[0].text = "";
                    if (talkCoroutinesRunning == 0)
                    {
                        textBackground.SetActive(false);
                    }
                }
                else
                {
                    waitingToClear = false;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
    public void OnAbilitiesButtonClick()
    {
        abilities.SetActive(true);
        menu.SetActive(false);
        if(ruleContainer.canJump)
        {
            abilitiesText[1].SetActive(true);
        }
        else
        {
            abilitiesText[1].SetActive(false);
        }
        if(ruleContainer.canDash) 
        {
            abilitiesText[2].SetActive(true);
        }
        else
        {
            abilitiesText[2].SetActive(false);
        }
        if(ruleContainer.canCrouch)
        {
            abilitiesText[3].SetActive(true);
        }
        else
        {
            abilitiesText[3].SetActive(false);
        }
    }

    public void OnBackButtonClick()
    {
        abilities.SetActive(false);
        menu.SetActive(true);
    }
}
