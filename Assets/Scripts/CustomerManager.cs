using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;

public class CustomerManager : MonoBehaviour
{
    public GameConstantsSO Constants;

    public Sprite[] BodyOptions;
    public Sprite[] MouthOptions;
    public Sprite[] EyesOptions;
    public Sprite[] SkinConditionOptions;

    // public TextAsset ...
    public GameObject DialogueBox;

    protected Customer currentCustomer;

    private float customerAlpha;

    private bool attending;
    private bool onHold;
    private bool customerSpawned;
    private bool dialogueVisible;

    private float timeToNextPatient;
    private float waitingTime;

    private void SpawnCustomer()
    {
        currentCustomer = new Customer(Constants, BodyOptions, MouthOptions, EyesOptions);

        // Get SpriteRenderers for customer parts
        SpriteRenderer bodyRenderer = (SpriteRenderer)transform.Find("Body").GetComponent(typeof(SpriteRenderer));
        SpriteRenderer mouthRenderer = (SpriteRenderer)transform.Find("Mouth").GetComponent(typeof(SpriteRenderer));
        SpriteRenderer eyesRenderer = (SpriteRenderer)transform.Find("Eyes").GetComponent(typeof(SpriteRenderer));

        // Sprite color modifier
        customerAlpha = 0f;
        Color opacityColor = new Color(1f, 1f, 1f, customerAlpha);

        // Make invisible before spawining
        bodyRenderer.color = opacityColor;
        mouthRenderer.color = opacityColor;
        eyesRenderer.color = opacityColor;

        // Assigns randomized sprites
        bodyRenderer.sprite = currentCustomer.GetBody();
        mouthRenderer.sprite = currentCustomer.GetMouth();
        eyesRenderer.sprite = currentCustomer.GetEyes();

        // Assing randomized (TBD) text
        // SetText("Hola, ¿me atiende?");
    }

    private void CustomerFadeIn()
    {
        if (customerAlpha < 1f)
        {
            customerAlpha += 0.005f;
            ((SpriteRenderer)transform.Find("Body").GetComponent(typeof(SpriteRenderer))).color = new Color(1f, 1f, 1f, customerAlpha);
            ((SpriteRenderer)transform.Find("Mouth").GetComponent(typeof(SpriteRenderer))).color = new Color(1f, 1f, 1f, customerAlpha);
            ((SpriteRenderer)transform.Find("Eyes").GetComponent(typeof(SpriteRenderer))).color = new Color(1f, 1f, 1f, customerAlpha);
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
            ((SpriteRenderer)transform.Find("Body").GetComponent(typeof(SpriteRenderer))).color = new Color(1f, 1f, 1f, customerAlpha);
            ((SpriteRenderer)transform.Find("Mouth").GetComponent(typeof(SpriteRenderer))).color = new Color(1f, 1f, 1f, customerAlpha);
            ((SpriteRenderer)transform.Find("Eyes").GetComponent(typeof(SpriteRenderer))).color = new Color(1f, 1f, 1f, customerAlpha);
        } else
        {
            attending = false;
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
        bool success;
        PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/Prefabs/" + gameObject.name + ".prefab", out success);
        if (success)
        {
            SceneManager.LoadScene("CamillaScene", LoadSceneMode.Single);
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
