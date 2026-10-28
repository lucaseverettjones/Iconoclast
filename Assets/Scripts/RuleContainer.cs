using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RuleContainer : MonoBehaviour
{
    public bool canJump;
    public bool canDash;
    public bool canCrouch;
    string scene;
    private void Start()
    {
        scene = SceneManager.GetActiveScene().name;
        if(scene == "Level1")
        {
            canCrouch = false;
            canJump = false;
            canDash = false;
        }
        else if (scene == "Level2")
        {
            canCrouch = false;
            canJump = true;
            canDash = true;
        }
        else
        {
            canCrouch = true;
            canJump = true;
            canDash = true;
        }
    }
}
