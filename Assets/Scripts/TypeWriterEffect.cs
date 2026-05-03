using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private float timeBetweenChars = 0.05f;
    
    private string fullText;
    [HideInInspector] public bool isFinished = false;

    void Awake()
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        
        // Store the original text written in the Inspector
        fullText = textComponent.text;
        
        // Hide text initially
        textComponent.maxVisibleCharacters = 0;
    }

    // This allows us to trigger the typewriter manually from other scripts
    public void StartTyping()
    {
        StopAllCoroutines();
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isFinished = false;
        
        textComponent.text = fullText;
        textComponent.ForceMeshUpdate();
        
        int totalVisibleCharacters = textComponent.textInfo.characterCount;
        textComponent.maxVisibleCharacters = 0; 

        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            textComponent.maxVisibleCharacters = counter;
            counter++;

            yield return new WaitForSeconds(timeBetweenChars);
        }

        isFinished = true;
    }
}