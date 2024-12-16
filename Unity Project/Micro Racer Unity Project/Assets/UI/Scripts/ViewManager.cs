using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

    [SerializeField] private bool autoInitialize;
    [SerializeField] private View[] views;
    [SerializeField] private View defaultView;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    private void Start()
    {
        if (autoInitialize)
            Initialize();
    }

    public void Initialize()
    {
        foreach (var view in views)
        {
            view.Initialize();
            view.Hide();
        }

        if (defaultView != null)
            defaultView.Show();
    }

    public void Show<TView>(object args = null) where TView : View
    {
        foreach (var view in views)
        {
            if (view is TView)
            {
                view.Show();
            }
            else
            {
                view.Hide();
            }
        }
    }
} 
