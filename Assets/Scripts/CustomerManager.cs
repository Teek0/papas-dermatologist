using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    public GameConstantsSO Constants;

    public Sprite[] BodyOptions;
    public Sprite[] HairOptions;
    public Sprite[] EyesOptions;

    public Sprite[] SkinConditionOptions_Forehead;
    public Sprite[] SkinConditionOptions_Cheeks;
    public Sprite[] SkinConditionOptions_Chin;

    // public TextAsset ...
    public GameObject DialogueBox;

    protected Customer currentCustomer;

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

    private AsyncOperation asyncOperation;

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
        Color opacityColor = new Color(1f, 1f, 1f, customerAlpha);

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

        // Assing randomized (TBD) text
        // SetText("Hola, ¿me atiende?");
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

    private void SetText(string _message)
    {
        ((TMP_Text)DialogueBox.transform.Find("Text Box").transform.Find("NPC Text").GetComponent(typeof(TMP_Text))).text = _message;
    }

    public void rejectCustomer()
    {
        customerSpawned = false;
        onHold = false;
    }

    public void acceptCustomer()
    {
        StartCoroutine(closeReception());
    }

    IEnumerator closeReception()
    {

        Debug.Log("Activating CamillaScene");
        asyncOperation.allowSceneActivation = true;

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        Scene sceneToLoad = SceneManager.GetSceneByName("CamillaScene");

        if (sceneToLoad.IsValid())
        {
            Debug.Log("Scene is valid");
            SceneManager.MoveGameObjectToScene(gameObject, sceneToLoad);
            Debug.Log("Moved actor to scene");
        }

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync("ReceptionScene");

        while (!asyncUnload.isDone)
        {
            if (asyncUnload.progress <= 0.9f)
            {
                Debug.Log("Unloading Reception. Progress: " + asyncUnload.progress * 100 + "%");
                break;
            }
            yield return null;
        }

        SceneManager.SetActiveScene(sceneToLoad);
        Debug.Log("Scene activated");
    }

    // Loads operating table scene
    IEnumerator loadScene(string _sceneName)
    {
        yield return null;
        asyncOperation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = false;

        Debug.Log("Scene load progress: " + asyncOperation.progress);
        while (asyncOperation.progress <= 0.9f)
        {
            Debug.Log("Loading scene. Progress: " + asyncOperation.progress * 100 + "%");
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
        timeToNextPatient = Random.Range(3, Constants.MaxWaitingTime);
        waitingTime = 0;
        DialogueBox.SetActive(false);
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
                    customerSpawned = true;
                    waitingTime = 0;
                }
                else
                {
                    waitingTime += Time.deltaTime;
                    timeToNextPatient = Random.Range(3, Constants.MaxWaitingTime);
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
                    SetText("Hola, ¿me atiende?");
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
