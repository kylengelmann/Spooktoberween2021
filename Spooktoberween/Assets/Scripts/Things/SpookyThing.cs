using UnityEngine;

public class SpookyThing : MonoBehaviour
{
    public bool bIsVisible{get; private set;}

    public delegate void OnVisibleChanged(bool newVisible);
    public OnVisibleChanged onVisibleChanged;

    bool bIsInVisibleArea;
    bool bIsOnCamera;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SpookManager.spookManager.RegisterThing(this);
    }

    void UpdateVisible()
    {
        bool newVisible = bIsOnCamera && bIsInVisibleArea;
        if (bIsVisible != newVisible)
        {
            bIsVisible = newVisible;

            Debug.Log(gameObject.name + " visible changed to " + bIsVisible);

            if (onVisibleChanged != null)
            {
                onVisibleChanged(newVisible);
            }
        }
    }

    public void SetInVisibleArea(bool newInVisibleArea)
    {
        if(bIsInVisibleArea != newInVisibleArea)
        {
            bIsInVisibleArea = newInVisibleArea;

            spriteRenderer.enabled = newInVisibleArea;
        }
    }

    private void OnBecameVisible()
    {
        bIsOnCamera = true;
        UpdateVisible();
    }

    private void OnBecameInvisible()
    {
        bIsOnCamera = false;
        UpdateVisible();
    }
}
