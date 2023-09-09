using UnityEngine;

[System.Flags]
public enum EThingType
{
    None = 0,

    Dish = 1,
    HallDecor = 1 << 1,
    Furniture = 1 << 2,
}

public class SpookyThing : MonoBehaviour
{
    public EThingType ThingType;

    public bool bIsVisible{get; private set;}

    public delegate void OnFlagChanged(bool newFlag);
    public OnFlagChanged onVisibleChanged;

    public OnFlagChanged onFocusedChanged;

    bool bIsInVisibleArea;

    public bool bIsFocused {get; private set;}

    public SpriteRenderer spriteRenderer {get; private set;}

    int StencilRefID = Shader.PropertyToID("_StencilRef");
    int EmissiveTexID = Shader.PropertyToID("_Emissive");
    int FocusedTimeID = Shader.PropertyToID("_TimeFocused");
    int UnfocusedTimeID = Shader.PropertyToID("_TimeUnfocused");
    int PossessedTimeID = Shader.PropertyToID("_TimePossessed");

    int StencilRefDefault;

    bool bIsDisplayingPossess = false;

    public delegate void spookyThingEvent(SpookyThing thing);
    public spookyThingEvent OnTeleported;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        StencilRefDefault = spriteRenderer.material.GetInteger(StencilRefID);

        spriteRenderer.material.SetTexture(EmissiveTexID, spriteRenderer.sprite.texture);
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

    public void SetFocused(bool bNewFocused)
    {
        if(bNewFocused == bIsFocused)
        {
            return;
        }

        bIsFocused = bNewFocused;

        int StencilValue = bIsFocused ? StencilRefDefault | 128 : StencilRefDefault & ~128;

        spriteRenderer.material.SetInteger(StencilRefID, StencilValue);
        spriteRenderer.material.SetFloat(bIsFocused ? FocusedTimeID : UnfocusedTimeID, Time.timeSinceLevelLoad);

        if(onFocusedChanged != null)
        {
            onFocusedChanged(bIsFocused);
        }
    }

    public void SetDisplayPossession(bool bDisplay)
    {
        if(bDisplay != bIsDisplayingPossess)
        {
            bIsDisplayingPossess = bDisplay;

            spriteRenderer.material.SetFloat(PossessedTimeID, bDisplay ? Time.timeSinceLevelLoad : -1f);
        }
    }

    public void NotifyTeleport()
    {
        if(OnTeleported != null)
        {
            OnTeleported(this);
        }
    }
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