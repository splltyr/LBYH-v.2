using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField] MenuButtonController menuButtonController;
    [SerializeField] Animator animator;
    [SerializeField] AnimatorFunctions animatorFunctions;
    [SerializeField] int thisIndex;

    [Header("Hover Settings")]
    public float shakeSpeed = 5f;
    public float shakeAmount = 0.2f;

    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if(menuButtonController.index == thisIndex)
        {
            animator.SetBool ("selected", true);
            
            // This handles the controlled hover movement
            float newY = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            transform.localPosition = new Vector3(originalPos.x, originalPos.y + newY, originalPos.z);

            if(Input.GetAxis ("Submit") == 1)
            {
                animator.SetBool ("pressed", true);
                HandleAction();
            }
            else if (animator.GetBool ("pressed"))
            {
                animator.SetBool ("pressed", false);
                animatorFunctions.disableOnce = true;
            }
        }
        else
        {
            animator.SetBool ("selected", false);
            transform.localPosition = originalPos;
        }
    }

    void HandleAction()
    {
        if (thisIndex == 0) // Start Button
        {
            SceneManager.LoadScene("Prologue Damon 2");
        }
        else if (thisIndex == 2) // Exit Button
        {
            Debug.Log("Quit Game");
            Application.Quit();
        }
    }
}