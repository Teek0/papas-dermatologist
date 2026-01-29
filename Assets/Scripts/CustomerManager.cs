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
        // Assigns randomized sprites
        ((SpriteRenderer)transform.Find("Body").GetComponent(typeof(SpriteRenderer))).sprite = currentCustomer.GetBody();
        ((SpriteRenderer)transform.Find("Mouth").GetComponent(typeof(SpriteRenderer))).sprite = currentCustomer.GetMouth();
        ((SpriteRenderer)transform.Find("Eyes").GetComponent(typeof(SpriteRenderer))).sprite = currentCustomer.GetEyes();

        // ToDo: Save as Prefab to export to second Scene
    }

    private bool attending;
    private float timeToNextPatient;
    private float waitingTime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attending = false;
        timeToNextPatient = Random.Range(3, Constants.MaxWaitingTime);
        waitingTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(!attending)
        {
            waitingTime += Time.deltaTime;
            if(waitingTime >= timeToNextPatient)
            {
                SpawnCustomer();
                waitingTime = 0;
                attending = true;
            }
        } else
        {
            waitingTime += Time.deltaTime;
            if(waitingTime >= timeToNextPatient)
            {
                waitingTime = 0;
                attending = false;
            }
        }
    }
}
