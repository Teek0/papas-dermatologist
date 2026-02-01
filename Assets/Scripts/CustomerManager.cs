using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class CustomerManager : MonoBehaviour
{
    public GameConstantsSO Constants;

    public Sprite[] BodyOptions;
    public Sprite[] HairOptions;
    public Sprite[] EyesOptions;

    public Sprite[] SkinConditionOptions_Forehead;
    public Sprite[] SkinConditionOptions_Cheeks;
    public Sprite[] SkinConditionOptions_Chin;

    public GameObject DialogueBox;
    public TextAsset dialogueOptions;
    private NPCDialogues dialogue;
    public static event Action<string> OnDialogueUpdate;

    public Customer currentCustomer;

    SpriteRenderer bodyRenderer;
    SpriteRenderer hairRenderer;
    SpriteRenderer eyesRenderer;
    SpriteRenderer foreheadRenderer;
    SpriteRenderer cheeksRenderer;
    SpriteRenderer chinRenderer;


    private float customerAlpha;

    private bool attending;
    private bool onHold;
    private bool customerSpawned;
    private bool dialogueVisible;

    private float timeToNextPatient;
    private float waitingTime;

    private bool isLoadingCamilla = false;
    private AsyncOperation asyncOperation;

    private string receptionSceneName;

    private void SpawnCustomer()
    {
        Sprite[][] skinConditions = { SkinConditionOptions_Forehead, SkinConditionOptions_Cheeks, SkinConditionOptions_Chin };
        currentCustomer = new Customer(Constants, BodyOptions, HairOptions, EyesOptions, skinConditions);

        // Get SpriteRenderers for customer parts
        bodyRenderer = (SpriteRenderer)transform.Find("Body").GetComponent(typeof(SpriteRenderer));
        hairRenderer = (SpriteRenderer)transform.Find("Hair").GetComponent(typeof(SpriteRenderer));
        eyesRenderer = (SpriteRenderer)transform.Find("Eyes").GetComponent(typeof(SpriteRenderer));
        
        Transform skinConditionsTransform = transform.Find("SkinConditions");
        foreheadRenderer = (SpriteRenderer)skinConditionsTransform.Find("Forehead").GetComponent(typeof(SpriteRenderer));
        cheeksRenderer = (SpriteRenderer)skinConditionsTransform.Find("Cheeks").GetComponent(typeof(SpriteRenderer));
        chinRenderer = (SpriteRenderer)skinConditionsTransform.Find("Chin").GetComponent(typeof(SpriteRenderer));

        // Sprite color modifier
        customerAlpha = 0f;
        Color opacityColor = new(1f, 1f, 1f, customerAlpha);

        // Make invisible before spawining
        bodyRenderer.color = opacityColor;
        hairRenderer.color = opacityColor;
        eyesRenderer.color = opacityColor;
        foreheadRenderer.color = opacityColor;
        cheeksRenderer.color = opacityColor;
        chinRenderer.color = opacityColor;

        // Assigns randomized sprites
        bodyRenderer.sprite = currentCustomer.Body;
        hairRenderer.sprite = currentCustomer.Hair;
        eyesRenderer.sprite = currentCustomer.Eyes;

        for (int i = 0; i < currentCustomer.Treatment.SkinConditions.Count; i++)
        {
            if (currentCustomer.Treatment.SkinConditions[i].AfflictedArea == "frente")
            {
                foreheadRenderer.sprite = currentCustomer.Treatment.SkinConditions[i].Sprite;
            } else if (currentCustomer.Treatment.SkinConditions[i].AfflictedArea == "cara")
            {
                cheeksRenderer.sprite = currentCustomer.Treatment.SkinConditions[i].Sprite;
            } else if (currentCustomer.Treatment.SkinConditions[i].AfflictedArea == "barbilla")
            {
                chinRenderer.sprite = currentCustomer.Treatment.SkinConditions[i].Sprite;
            }
        }
    }

    private void CustomerFadeIn()
    {
        if (customerAlpha < 1f)
        {
            customerAlpha += 0.005f;
            bodyRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            hairRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            eyesRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            foreheadRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            cheeksRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            chinRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
        }
        else
        {
            attending = true;
        }
    }

    private void CustomerFadeOut()
    {
        if (customerAlpha > 0f)
        {
            customerAlpha -= 0.005f;
            bodyRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            hairRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            eyesRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            foreheadRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            cheeksRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
            chinRenderer.color = new Color(1f, 1f, 1f, customerAlpha);
        } else
        {
            attending = false;
        }
    }

    public List<SkinCondition> GetSkinConditions()
    {
        if (attending)
        {
            return currentCustomer.Treatment.SkinConditions;
        } else
        {
            throw new System.Exception("Not attending a customer");
        }
    }

    public Customer CurrentCustomer => currentCustomer;

    private void SetText(string _message)
    {
        ((TMP_Text)DialogueBox.transform.Find("Text Box").transform.Find("NPC Text").GetComponent(typeof(TMP_Text))).text = _message;
        OnDialogueUpdate?.Invoke(_message);
    }

    private void SetRandomText()
    {
        string message, greeting, preamble, request, article, at, closing;

        greeting = dialogue.greetings[Random.Range(0, dialogue.greetings.Count)].line;
        preamble = dialogue.preambles[Random.Range(0, dialogue.preambles.Count)].line;
        request = dialogue.treatmentRequest[Random.Range(0, dialogue.treatmentRequest.Count)].line;
        at = dialogue.pointingAt[Random.Range(0, dialogue.pointingAt.Count)].line;
        closing = dialogue.closing[Random.Range(0, dialogue.closing.Count)].line;

        message = greeting + preamble + request;

        List<string> afflictions = new();
        List<string> areas = new();

        SkinCondition current;

        for (int i = 0; i < currentCustomer.Treatment.SkinConditions.Count; i++)
        {
            current = currentCustomer.Treatment.SkinConditions[i];
            if(!afflictions.Contains(current.Type))
            {
                afflictions.Add(current.Type);
            }
            
            if(!areas.Contains(current.AfflictedArea))
            {
                areas.Add(current.AfflictedArea);
            }
        }

        for (int i = 0; i < afflictions.Count; i++)
        { 
            if (i > 0)
            {
                if (i == afflictions.Count - 1)
                {
                    message += " y ";
                }
                else
                {
                    message += ", ";
                }
            }

            if (afflictions[i] == "acné")
            {
                article = "el ";
            } else
            {
                article = "las ";
            }

            message += article + afflictions[i];
        }

        bool statingTheObvious = !(Random.Range(0, 3) == 1);

        if (statingTheObvious)
        {
            message += at;

            for (int i = 0; i < areas.Count; i++)
            {
                if (i > 0)
                {
                    if (i == areas.Count - 1)
                    {
                        message += " y ";
                    }
                    else
                    {
                        message += ", ";
                    }
                }

                message += "mi " + areas[i];
            }
        }

        message += closing;
        SetText(message);
    }

    public void rejectCustomer()
    {
        customerSpawned = false;
        onHold = false;
    }

    public void acceptCustomer()
    {

        if (GameSession.I != null)
            GameSession.I.SetCustomer(currentCustomer);
        else
            Debug.LogWarning("No existe GameSession en la escena. No se pudo guardar el customer.");

        if (DialogueBox != null)
            DialogueBox.SetActive(false);

        if (isLoadingCamilla)
            return;

        if (asyncOperation == null)
        {
            Debug.LogWarning("CamillaScene aún no terminó de cargar. Intentando cargar ahora.");
            isLoadingCamilla = true;
            StartCoroutine(LoadCamillaAndSwitch());
            return;
        }

        isLoadingCamilla = true;
        StartCoroutine(SwitchToCamillaAndUnloadReception());
    }

    private IEnumerator LoadCamillaAndSwitch()
    {
        asyncOperation = SceneManager.LoadSceneAsync("CamillaScene", LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = false;

        while (asyncOperation.progress < 0.9f)
            yield return null;

        yield return SwitchToCamillaAndUnloadReception();
    }

    private IEnumerator SwitchToCamillaAndUnloadReception()
    {
        asyncOperation.allowSceneActivation = true;

        while (!asyncOperation.isDone)
            yield return null;

        Scene camilla = SceneManager.GetSceneByName("CamillaScene");
        if (camilla.IsValid())
            SceneManager.SetActiveScene(camilla);

        if (!string.IsNullOrEmpty(receptionSceneName))
            SceneManager.UnloadSceneAsync(receptionSceneName);

        isLoadingCamilla = false;
    }

    // Loads operating table scene
    IEnumerator loadScene(string _sceneName)
    {
        yield return null;
        asyncOperation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = false;

        // Debug.Log("Scene load progress: " + asyncOperation.progress);
        while (asyncOperation.progress <= 0.9f)
        {
            // Debug.Log("Loading scene. Progress: " + asyncOperation.progress * 100 + "%");
            yield return null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attending = false;
        customerSpawned = false;
        dialogueVisible = false;
        onHold = false;

        customerAlpha = 0f;

        timeToNextPatient = Random.Range(2, Constants.MaxWaitingTime);
        waitingTime = 0;

        DialogueBox.SetActive(false);
        dialogue = NPCDialogues.CreateFromJSON(dialogueOptions.text);

        receptionSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(loadScene("CamillaScene"));
    }

    // Update is called once per frame
    void Update()
    {
        if (onHold)
        {
            return;
        }
        if(!attending)
        {
            // Not attending a customer

            if(!customerSpawned)
            {
                // Waits for the next customer to arrive
                if (waitingTime >= timeToNextPatient)
                {
                    SpawnCustomer();
                    timeToNextPatient = Random.Range(3, Constants.MaxWaitingTime);
                    customerSpawned = true;
                    waitingTime = 0;
                }
                else
                {
                    waitingTime += Time.deltaTime;
                }
            } else
            {
                // Customer fade-in
                CustomerFadeIn();
            }
        } else
        {
            // Attending a customer

            if(customerSpawned)
            {
                if(!dialogueVisible)
                {
                    DialogueBox.SetActive(true);
                    // SetText("Hola, ¿me atiende?");
                    SetRandomText();
                    onHold = true;
                }
            } else
            {
                DialogueBox.SetActive(false);
                CustomerFadeOut();
            }
        }
    }
}
