using UnityEngine;
using System.Collections;
using TMPro;

// This script shall function as an interface between AnimaleseSpeaker and the text to emulate speech from.
// Input: dialogueText (this should be changed to receive text from the client at reception.)
// Which feeds into PlaySentence(string text)

// NO SÉ COMO SERÁ LA INTERFAZ ENTRE ESTO. 
// ¿PUEDE SER CREAR UN OBJETO QUE CONTENGA EL DIÁLOGO A MOSTRAR EN PANTALLA
// Y QUE TAMBIEN ENTREGUE ESE TEXTO A ESTA FUNCIÓN?

public class AnimaleseTTS : MonoBehaviour
{

    public AnimaleseSpeaker speaker;
    public TMP_Text dialogueText;

    [Header("Settings")]
    public float charactersPerSecond = 12f; // Speed of talking

    public void ReadText()
    {
        // Stop any current talking before starting new text
        StopAllCoroutines();
        StartCoroutine(PlaySentence(dialogueText.text));
    }

    // Main function.
    IEnumerator PlaySentence(string sentence)
    {
        foreach (char letter in sentence)
        {
            // If letter is a space, still take time as if sound is being played.
            if (letter == ' ')
            {
                yield return new WaitForSeconds(1f / charactersPerSecond);
                continue;
            }

            speaker.SpeakKey(letter.ToString());
            yield return new WaitForSeconds(1f / charactersPerSecond);
        }
    }
}
