using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointsManager : MonoBehaviour
{
    public static CheckpointsManager Instance;

    private int _checkpointsNumber;

    public int CheckpointsNumber
    {
        get
        {
            return _checkpointsNumber;
        }
        set
        {
            if (value >= maxCheckpoints)
            {
                Debug.Log("Bunny wins!");
            }
                
            _checkpointsNumber = Mathf.Clamp(value, 0, maxCheckpoints);
            UpdateUI();
        }
    }

    [SerializeField] private int maxCheckpoints;

    [Header("UI")]
    [SerializeField] private Image[] fills;
    [SerializeField] private TextMeshProUGUI checkpointsText;

    private Checkpoint[] _checkpoints;

    private void Awake()
    {
        if (!Instance)
            Instance = this;

        _checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        maxCheckpoints = _checkpoints.Length;
        UpdateUI();
    }

    public void OnCheckpoint()
    {
        CheckpointsNumber++;
    }

    private void UpdateUI()
    {
        checkpointsText.text = $"Checkpoints {CheckpointsNumber}/{maxCheckpoints}";

        foreach (Image img in fills)
        {
            img.fillAmount = Mathf.InverseLerp(0, maxCheckpoints, CheckpointsNumber);
        }
    }
}
