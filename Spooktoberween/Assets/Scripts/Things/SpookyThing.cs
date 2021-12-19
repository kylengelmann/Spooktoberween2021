using UnityEngine;

public class SpookyThing : MonoBehaviour
{
    public bool bIsVisible{get; private set;}

    public delegate void OnVisibleChanged(bool newVisible);
    public OnVisibleChanged onVisibleChanged;

    bool bIsInVisibleArea;

    public SpriteRenderer spriteRenderer {get; private set;}

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        //bIsOnCamera = spriteRenderer.isVisible;
    }

    private void Start()
    {
        SpookManager.spookManager.RegisterThing(this);
    }

    void UpdateVisible()
    {
        bool newVisible = bIsInVisibleArea;
        if (bIsVisible != newVisible)
        {
            bIsVisible = newVisible;

            spriteRenderer.enabled = newVisible;

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

            UpdateVisible();
        }
    }

    //private void OnBecameVisible()
    //{
    //    bIsOnCamera = true;
    //    UpdateVisible();
    //}

    //private void OnBecameInvisible()
    //{
    //    bIsOnCamera = false;
    //    UpdateVisible();
    //}
}

class WaitForVisibilityChange : CustomYieldInstruction
{
    bool bVisibilityChanged = false;

    public override bool keepWaiting
    {
        get
        {
            return !bVisibilityChanged;
        }
    }

    public WaitForVisibilityChange(SpookyThing thing)
    {
        thing.onVisibleChanged += (bool bIsVisible) => { bVisibilityChanged = true; };
    }
}