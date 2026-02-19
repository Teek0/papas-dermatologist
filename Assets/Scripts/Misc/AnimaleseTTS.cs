using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;

public class AnimaleseTTS : MonoBehaviour
{

    public AnimaleseSpeaker speaker;
    public TMP_Text dialogueText;

    [Header("Settings")]
    public float charactersPerSecond = 25f;
    public int textCap = 25;

    private void OnEnable()
    {
        CustomerManager.OnDialogueUpdate += HandleNewTextReceived;
    }

    private void OnDisable()
    {
        CustomerManager.OnDialogueUpdate -= HandleNewTextReceived;
    }

    private void HandleNewTextReceived(string dialogueText)
    {
        dialogueText = dialogueText.Length > textCap ? dialogueText[..textCap] : dialogueText;
        StopAllCoroutines();
        StartCoroutine(PlaySentence(dialogueText));
    }
    
    /*
    public void ReadText()
    {
        StopAllCoroutines();
        StartCoroutine(PlaySentence(dialogueText.text));
    }
    */

    IEnumerator PlaySentence(string sentence)
    {
        foreach (char letter in sentence)
        {
            if (letter == ' ')
            {
                yield return new WaitForSeconds(1f / charactersPerSecond);
                continue;
            }

            speaker.SpeakKey(letter.ToString().ToLower());
            yield return new WaitForSeconds(1f / charactersPerSecond);
        }
    }
}
