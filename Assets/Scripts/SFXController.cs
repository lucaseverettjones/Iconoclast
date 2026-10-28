using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SFXController : MonoBehaviour
{
    public static SFXController instance;
    [SerializeField] AudioClip[] music;
    AudioSource musicPlayer;
    UIController uIController;
    //Singleton
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        musicPlayer = GetComponent<AudioSource>();
        uIController = GameObject.Find("Canvas").GetComponent<UIController>();
        musicPlayer.loop = false;
        StartCoroutine("UpdateMusic");
        StartCoroutine("UpdateVolume");
    }
    public void PlaySound(AudioSource source, AudioClip clip)
    {
        if (uIController == null)
        {
            uIController = GameObject.Find("Canvas").GetComponent<UIController>();
        }
        source.volume = uIController.sfxVolume;
        source.PlayOneShot(clip);
    }
    IEnumerator UpdateMusic()
    {
        while(true)
        {
            if(SceneManager.GetActiveScene().name == "Level1")
            {
                musicPlayer.clip = music[1];
            }
            else if (SceneManager.GetActiveScene().name == "Level2")
            {
                musicPlayer.clip = music[2];
            }
            else if (SceneManager.GetActiveScene().name == "Level3")
            {
                musicPlayer.clip = music[3];
            }
            else
            {
                musicPlayer.clip = music[0];
            }
            musicPlayer.Play();
            yield return new WaitUntil(() => musicPlayer.isPlaying == false);
        }
    }
    IEnumerator UpdateVolume()
    {
        while (true)
        {
            musicPlayer.volume = uIController.musicVolume;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
