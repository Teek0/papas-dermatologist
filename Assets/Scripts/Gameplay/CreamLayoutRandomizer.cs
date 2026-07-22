using UnityEngine;

public class CreamLayoutRandomizer : MonoBehaviour
{
    [Header("Cream Roots")]
    [SerializeField] private Transform[] creamRoots;

    [Header("Behaviour")]
    [SerializeField] private bool randomizeOnlyInHardMode = true;
    [SerializeField] private bool includeRotation;
    [SerializeField] private bool includeScale;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private Vector3[] initialScales;

    private void Awake()
    {
        CaptureInitialLayout();
    }

    private void Start()
    {
        ApplyCurrentDifficultyLayout();
    }

    public void ApplyCurrentDifficultyLayout()
    {
        bool isHardMode = GameSession.I != null
            ? GameSession.I.IsHardMode
            : RunConfigurationStore.Current.IsHardMode;

        if (randomizeOnlyInHardMode && !isHardMode)
        {
            RestoreInitialLayout();
            return;
        }

        RandomizeLayout();
    }

    public void RandomizeLayout()
    {
        if (creamRoots == null || creamRoots.Length < 2)
            return;

        EnsureInitialLayout();

        int[] slotOrder = BuildShuffledSlotOrder(creamRoots.Length);

        for (int i = 0; i < creamRoots.Length; i++)
        {
            Transform creamRoot = creamRoots[i];
            if (creamRoot == null)
                continue;

            int slotIndex = slotOrder[i];
            creamRoot.position = initialPositions[slotIndex];

            if (includeRotation)
                creamRoot.rotation = initialRotations[slotIndex];

            if (includeScale)
                creamRoot.localScale = initialScales[slotIndex];
        }
    }

    public void RestoreInitialLayout()
    {
        if (creamRoots == null || creamRoots.Length == 0)
            return;

        EnsureInitialLayout();

        for (int i = 0; i < creamRoots.Length; i++)
        {
            Transform creamRoot = creamRoots[i];
            if (creamRoot == null)
                continue;

            creamRoot.position = initialPositions[i];
            creamRoot.rotation = initialRotations[i];
            creamRoot.localScale = initialScales[i];
        }
    }

    private void CaptureInitialLayout()
    {
        if (creamRoots == null)
            return;

        initialPositions = new Vector3[creamRoots.Length];
        initialRotations = new Quaternion[creamRoots.Length];
        initialScales = new Vector3[creamRoots.Length];

        for (int i = 0; i < creamRoots.Length; i++)
        {
            Transform creamRoot = creamRoots[i];

            if (creamRoot == null)
                continue;

            initialPositions[i] = creamRoot.position;
            initialRotations[i] = creamRoot.rotation;
            initialScales[i] = creamRoot.localScale;
        }
    }

    private void EnsureInitialLayout()
    {
        if (initialPositions == null || initialPositions.Length != creamRoots.Length)
            CaptureInitialLayout();
    }

    private int[] BuildShuffledSlotOrder(int count)
    {
        int[] order = new int[count];

        for (int i = 0; i < count; i++)
            order[i] = i;

        for (int i = count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (order[i], order[swapIndex]) = (order[swapIndex], order[i]);
        }

        return order;
    }
}
