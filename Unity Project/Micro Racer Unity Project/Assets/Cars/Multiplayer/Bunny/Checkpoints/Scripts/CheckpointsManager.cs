using UnityEngine;

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
                // DO SMTH
            }
                
            _checkpointsNumber = Mathf.Clamp(value, 0, maxCheckpoints);
        }
    }

    [SerializeField] private int maxCheckpoints;

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
    }

    public void OnCheckpoint()
    {
        CheckpointsNumber++;
    }
}
