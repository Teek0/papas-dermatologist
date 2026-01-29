using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public Sprite[] BodyOptions;
    public Sprite[] MouthOptions;
    public Sprite[] EyesOptions;
    public Sprite[] SkinConditionOptions;
    public GameConstantsSO Constants;

    protected Customer currentCustomer;

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

        // ToDo: Save as Prefab to export to second Scene
    }

    private void FadeIn()
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

    private void FadeOut()
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

    private float customerAlpha;

    private bool attending;
    private bool customerSpawned;

    private float timeToNextPatient;
    private float waitingTime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attending = false;
        customerSpawned = false;
        customerAlpha = 0f;
        timeToNextPatient = Random.Range(3, Constants.MaxWaitingTime);
        waitingTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
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
                FadeIn();
            }
        } else
        {
            // Attending a customer

            // This is placeholder to test fade-in/fade-out and customer randomization
            if(customerSpawned)
            {
                waitingTime += Time.deltaTime;
                Debug.Log("Waiting Time: " + waitingTime + "; Max Time to Wait: " + timeToNextPatient);
                Debug.Log("alpha = " + customerAlpha);
                if (waitingTime >= timeToNextPatient)
                {
                    customerSpawned = false;
                    waitingTime = 0;
                }
            } else
            {
                FadeOut();
            }
        }
    }
}
