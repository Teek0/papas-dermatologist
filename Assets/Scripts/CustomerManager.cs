using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Random = UnityEngine.Random;

public class CustomerManager : MonoBehaviour
{
    public GameConstantsSO Constants;
    public Customer currentCustomer;

    [Header("Character Generation Options")]
    public Sprite[] BodyOptions;
    public Sprite[] HairOptions;
    public Sprite[] EyesOptions;

    [Header("Skin Condition Options")]
    public Sprite[] SkinConditionOptions_Forehead;
    public Sprite[] SkinConditionOptions_Cheeks;
    public Sprite[] SkinConditionOptions_Chin;

    [Header("Character Renderers")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer hairRenderer;
    [SerializeField] private SpriteRenderer eyesRenderer;
    [SerializeField] private SpriteRenderer foreheadRenderer;
    [SerializeField] private SpriteRenderer cheeksRenderer;
    [SerializeField] private SpriteRenderer chinRenderer;

    [Header("Dialogue GameObjects")]
    public GameObject DialogueBox;
    [SerializeField] private TMP_Text npcText;
    public TextAsset dialogueOptions;
    private NPCDialogues dialogue;
    public static event Action<string> OnDialogueUpdate;

    [Header("Scene Transition")]
    [SerializeField] private string camillaSceneName = SceneNames.Camilla;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool validateSkinConditionIndexContract = true;

    private float customerAlpha;

    private bool attending;
    private bool onHold;
    private bool customerSpawned;
    private bool dialogueVisible;

    private float timeToNextPatient;
    private float waitingTime;

    private void SpawnCustomer()
    {
        Sprite[][] skinConditions = { SkinConditionOptions_Forehead, SkinConditionOptions_Cheeks, SkinConditionOptions_Chin };
        currentCustomer = new Customer(Constants, BodyOptions, HairOptions, EyesOptions, skinConditions);

        if (!ResolveCustomerRenderers())
        {
            Debug.LogError("CustomerManager: faltan renderers del cliente. No se puede mostrar el paciente.");
            currentCustomer = null;
            return;
        }

        foreheadRenderer.sprite = null;
        cheeksRenderer.sprite = null;
        chinRenderer.sprite = null;

        customerAlpha = 0f;
        Color opacityColor = new(1f, 1f, 1f, customerAlpha);

        SetCustomerRenderersColor(opacityColor);

        bodyRenderer.sprite = currentCustomer.Body;
        hairRenderer.sprite = currentCustomer.Hair;
        eyesRenderer.sprite = currentCustomer.Eyes;

        if (currentCustomer?.Treatment?.SkinConditions != null)
        {
            for (int i = 0; i < currentCustomer.Treatment.SkinConditions.Count; i++)
            {
                var sc = currentCustomer.Treatment.SkinConditions[i];
                if (sc == null) continue;

                if (sc.AfflictedArea == "frente")
                    foreheadRenderer.sprite = sc.Sprite;
                else if (sc.AfflictedArea == "mejillas")
                    cheeksRenderer.sprite = sc.Sprite;
                else if (sc.AfflictedArea == "barbilla")
                    chinRenderer.sprite = sc.Sprite;
            }
        }

        dialogueVisible = false;
        if (DialogueBox != null) DialogueBox.SetActive(false);
    }

    private bool ResolveCustomerRenderers()
    {
        bodyRenderer = ResolveRenderer(bodyRenderer, "Body");
        hairRenderer = ResolveRenderer(hairRenderer, "Hair");
        eyesRenderer = ResolveRenderer(eyesRenderer, "Eyes");
        foreheadRenderer = ResolveRenderer(foreheadRenderer, "SkinConditions/Forehead");
        cheeksRenderer = ResolveRenderer(cheeksRenderer, "SkinConditions/Cheeks");
        chinRenderer = ResolveRenderer(chinRenderer, "SkinConditions/Chin");

        return bodyRenderer != null &&
               hairRenderer != null &&
               eyesRenderer != null &&
               foreheadRenderer != null &&
               cheeksRenderer != null &&
               chinRenderer != null;
    }

    private SpriteRenderer ResolveRenderer(SpriteRenderer current, string childPath)
    {
        if (current != null)
            return current;

        Transform child = transform.Find(childPath);
        return child != null ? child.GetComponent<SpriteRenderer>() : null;
    }

    private void SetCustomerRenderersColor(Color color)
    {
        if (!ResolveCustomerRenderers())
            return;

        bodyRenderer.color = color;
        hairRenderer.color = color;
        eyesRenderer.color = color;
        foreheadRenderer.color = color;
        cheeksRenderer.color = color;
        chinRenderer.color = color;
    }

    private void CustomerFadeIn()
    {
        if (!ResolveCustomerRenderers())
            return;

        if (customerAlpha < 1f)
        {
            customerAlpha += 0.005f;
            SetCustomerRenderersColor(new Color(1f, 1f, 1f, customerAlpha));
        }
        else
        {
            attending = true;
        }
    }

    private void CustomerFadeOut()
    {
        if (!ResolveCustomerRenderers())
            return;

        if (customerAlpha > 0f)
        {
            customerAlpha -= 0.005f;
            SetCustomerRenderersColor(new Color(1f, 1f, 1f, customerAlpha));
        }
        else
        {
            attending = false;
            foreheadRenderer.sprite = null;
            cheeksRenderer.sprite = null;
            chinRenderer.sprite = null;

            dialogueVisible = false;
            if (DialogueBox != null) DialogueBox.SetActive(false);
        }
    }

    public List<SkinCondition> GetSkinConditions()
    {
        if (attending) return currentCustomer.Treatment.SkinConditions;
        throw new Exception("Not attending a customer");
    }

    public Customer CurrentCustomer => currentCustomer;

    private void SetText(string _message)
    {
        if (DialogueBox == null)
        {
            Debug.LogWarning("DialogueBox is null. Cannot set NPC dialogue text.");
            return;
        }

        if (npcText == null)
        {
            var textTf = DialogueBox.transform.Find("Text Box")?.Find("NPC Text");
            if (textTf != null)
                npcText = textTf.GetComponent<TMP_Text>();
        }

        if (npcText == null)
        {
            Debug.LogWarning("NPC Text missing. Assign npcText or keep DialogueBox/Text Box/NPC Text.");
            return;
        }

        npcText.text = _message;
        OnDialogueUpdate?.Invoke(_message);
    }


    private static string ExtractLine(object item)
    {
        if (item == null) return null;

        if (item is string s) return s;

        Type t = item.GetType();

        PropertyInfo p = t.GetProperty("line", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (p != null && p.PropertyType == typeof(string))
            return (string)p.GetValue(item);

        FieldInfo f = t.GetField("line", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (f != null && f.FieldType == typeof(string))
            return (string)f.GetValue(item);

        return null;
    }

    private string PickLineOrFallback(IList list, string fallback)
    {
        if (list == null || list.Count == 0) return fallback;

        int idx = Random.Range(0, list.Count);
        string line = ExtractLine(list[idx]);

        if (string.IsNullOrWhiteSpace(line)) return fallback;
        return line;
    }

    private void SetRandomText()
    {
        string greeting = "Hola. ";
        string preamble = "";
        string request = "¿Me puede ayudar con esto? ";
        string at = " Me molesta en ";
        string closing = ".";

        if (dialogue != null)
        {
            greeting = PickLineOrFallback(dialogue.greetings as IList, greeting);
            preamble = PickLineOrFallback(dialogue.preambles as IList, preamble);
            request = PickLineOrFallback(dialogue.treatmentRequest as IList, request);
            at = PickLineOrFallback(dialogue.pointingAt as IList, at);
            closing = PickLineOrFallback(dialogue.closing as IList, closing);
        }

        string message = greeting + preamble + request;

        List<string> afflictions = new();
        List<string> areas = new();

        if (currentCustomer?.Treatment?.SkinConditions != null)
        {
            for (int i = 0; i < currentCustomer.Treatment.SkinConditions.Count; i++)
            {
                var current = currentCustomer.Treatment.SkinConditions[i];
                if (current == null) continue;

                if (!string.IsNullOrEmpty(current.Type) && !afflictions.Contains(current.Type))
                    afflictions.Add(current.Type);

                if (!string.IsNullOrEmpty(current.AfflictedArea) && !areas.Contains(current.AfflictedArea))
                    areas.Add(current.AfflictedArea);
            }
        }

        for (int i = 0; i < afflictions.Count; i++)
        {
            if (i > 0)
            {
                if (i == afflictions.Count - 1) message += " y ";
                else message += ", ";
            }

            string article = (afflictions[i] == "acné") ? "el " : "las ";
            message += article + afflictions[i];
        }

        bool statingTheObvious = !(Random.Range(0, 3) == 1);

        if (statingTheObvious && areas.Count > 0)
        {
            message += at;

            for (int i = 0; i < areas.Count; i++)
            {
                if (i > 0)
                {
                    if (i == areas.Count - 1) message += " y ";
                    else message += ", ";
                }

                if (areas[i] == "mejillas") message += "mis " + areas[i];
                else message += "mi " + areas[i];
            }
        }

        message += closing;
        SetText(message);
    }

    public void rejectCustomer()
    {
        customerSpawned = false;
        onHold = false;

        dialogueVisible = false;
        if (DialogueBox != null) DialogueBox.SetActive(false);
    }

    public void acceptCustomer()
    {
        if (GameSession.I != null)
            GameSession.I.SetCustomer(currentCustomer);
        else
            Debug.LogWarning("No existe GameSession en la escena. No se pudo guardar el customer.");

        dialogueVisible = false;
        if (DialogueBox != null) DialogueBox.SetActive(false);

        Time.timeScale = 1f;
        StartCoroutine(TransitionToCamilla());
    }

    private IEnumerator TransitionToCamilla()
    {
        yield return StartCoroutine(SceneTransitionService.FadeOutAndLoadScene(
            camillaSceneName,
            fadeCanvasGroup,
            1f,
            true,
            mainMixer,
            fadeOutDuration));
    }

    void Start()
    {
        if (Constants == null)
        {
            Debug.LogError("CustomerManager: Constants no esta asignado.");
            enabled = false;
            return;
        }

        attending = false;
        customerSpawned = false;
        dialogueVisible = false;
        onHold = false;

        customerAlpha = 0f;

        timeToNextPatient = Random.Range(2, Constants.MaxWaitingTime);
        waitingTime = 0;

        if (DialogueBox != null) DialogueBox.SetActive(false);

        if (dialogueOptions != null && !string.IsNullOrEmpty(dialogueOptions.text))
            dialogue = NPCDialogues.CreateFromJSON(dialogueOptions.text);
        else
        {
            Debug.LogWarning("dialogueOptions is null/empty. Dialogue will use fallbacks.");
            dialogue = null;
        }


        if (validateSkinConditionIndexContract)
            ValidateSkinConditionIndexContract();
    }

    void Update()
    {
        if (onHold) return;

        if (!attending)
        {
            if (!customerSpawned)
            {
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
            }
            else
            {
                CustomerFadeIn();
            }
        }
        else
        {
            if (customerSpawned)
            {
                if (!dialogueVisible)
                {
                    if (DialogueBox != null) DialogueBox.SetActive(true);
                    SetRandomText();

                    dialogueVisible = true;
                    onHold = true;
                }
            }
            else
            {
                if (DialogueBox != null) DialogueBox.SetActive(false);
                dialogueVisible = false;
                CustomerFadeOut();
            }
        }
    }

    private void ValidateSkinConditionIndexContract()
{
    Sprite[][] opts = { SkinConditionOptions_Forehead, SkinConditionOptions_Cheeks, SkinConditionOptions_Chin };
    string[] areaName = { "frente", "mejillas", "mentón" };
    string[] expectedByIndex = { "acné", "arrugas", "cicatrices" };

    for (int a = 0; a < opts.Length; a++)
    {
        var row = opts[a];
        if (row == null)
        {
            Debug.LogError($"[IndexContract] {areaName[a]}: lista null");
            continue;
        }

        if (row.Length < expectedByIndex.Length)
        {
            Debug.LogWarning($"[IndexContract] {areaName[a]}: length={row.Length}. " +
                             $"Contrato incompleto (se esperan 3 índices: 0..2).");
        }

        int max = Mathf.Min(row.Length, expectedByIndex.Length);
        for (int i = 0; i < max; i++)
        {
            var sp = row[i];
            string expected = expectedByIndex[i];

            if (sp == null)
            {
                Debug.LogWarning($"[IndexContract] {areaName[a]} idx={i} (esperado {expected}): sprite null");
                continue;
            }

            string inferred = InferConditionTypeFromSpriteName(sp.name);

            // Si no se puede inferir, no acusamos mismatch, solo avisamos.
            if (inferred == "desconocido")
            {
                Debug.LogWarning($"[IndexContract] {areaName[a]} idx={i} (esperado {expected}): " +
                                 $"no se pudo inferir tipo desde sprite='{sp.name}'.");
                continue;
            }

            if (inferred != expected)
            {
                Debug.LogError($"[IndexContract] MISMATCH en {areaName[a]} idx={i}. " +
                               $"Se esperaba '{expected}' pero sprite='{sp.name}' parece '{inferred}'. " +
                               $"(Probable arrastre/orden incorrecto en Inspector).");
            }
        }
    }
}

private string InferConditionTypeFromSpriteName(string spriteName)
{
    if (string.IsNullOrWhiteSpace(spriteName))
        return "desconocido";

    string n = spriteName.ToLowerInvariant();

    // acné
    if (n.Contains("acne") || n.Contains("espinilla") || n.Contains("espinillas"))
        return "acné";

    // arrugas
    if (n.Contains("arruga") || n.Contains("arrugas") || n.Contains("wrinkle"))
        return "arrugas";

    // cicatrices
    if (n.Contains("cicatriz") || n.Contains("cicatrices") || n.Contains("scar"))
        return "cicatrices";

    return "desconocido";
}

}

