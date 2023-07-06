using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly:CheatSystem.CheatClass(typeof(SpookyPlayer))]
public class SpookyPlayer : Character, IHittableInterface
{

    [SerializeField]
    private int _maxHP = 3;
    public int maxHP { get { return _maxHP; } private set {_maxHP = value; } }

    public int currentHP { get; protected set;}

    public PlayerMovementComponent movementComponent { get; private set; }

    public float maxTurnRate = 360;
    public float turnDampingTime = .1f;

    public Vector2 lookDir {get; private set;}

    [SerializeField] GameObject VisibilityLightPrefab;
    [SerializeField] GameObject Sprite;
    [SerializeField] GameObject FlashlightPrefab;

    public GameObject visibleArea;

    VisibilityLight visibilityLight;
    GameObject Flashlight;

    Quaternion CurrentLookRotation = Quaternion.identity;
    Quaternion TargetLookRotation = Quaternion.identity;

    float CurrentLookSpeed = 0f;

    float CurrentLookAngle = 0f;
    float TargetLookAngle = 0f;
    float FloorAngle = 60f;
    Quaternion FloorRotation;

    public float huntTotalBlinkTime = .5f;
    public float huntFastBlinkTime = .05f;
    public float huntFastBlinkProbability = .667f;
    public float huntSlowBlinkTime = .1f;
    public float huntBlinkOffTime = .05f;
    Coroutine huntBlinkCoroutine;

    SpookyThing thingFocused;

    [System.Serializable]
    public struct FlashlightFocusData
    {
        public float FocusTime;
        public float UnfocusTime;

        public float FocusSpotAngle;
        public float FocusInnerSpotAngle;
        public float FocusIntensity;
        public float FocusRange;
        public float FocusVisibilityAngle;

        public float UnfocusSpotAngle;
        public float UnfocusInnerSpotAngle;
        public float UnfocusIntensity;
        public float UnfocusRange;
        public float UnfocusVisibilityAngle;

        public float FocusDistanceCheck;
        public float FocusRangeDistanceAddition;

        [System.NonSerialized]
        public float TimeFocusInput;

        [System.NonSerialized]
        public float FocusPercentAtInput;

        [System.NonSerialized]
        public float CurrentFocusPercent;

        [System.NonSerialized]
        public bool bIsFocused;

        [System.NonSerialized]
        public bool bIsTransitioning;
    }

    [SerializeField]
    FlashlightFocusData flashlightFocusData = new FlashlightFocusData() { FocusTime = .5f, UnfocusTime = .5f, CurrentFocusPercent = 0f, TimeFocusInput = -1f};

    public delegate void OnHPChanged(int newHP, int oldHP);

    public OnHPChanged onHPChanged;

    private void Awake()
    {
        movementComponent = GetComponent<PlayerMovementComponent>();

        currentFaceDirection = EFaceDirection.South;
    }

    Vector3 FloorNormal = Vector3.up;

    private void Start()
    {
        FloorRotation = CurrentLookRotation = TargetLookRotation = Quaternion.Euler(FloorAngle, 0f, 0f);
        FloorNormal = FloorRotation * Vector3.up;

        Debug.Log(FloorNormal);

        if(VisibilityLightPrefab)
        {
            GameObject visibilityObject = Instantiate(VisibilityLightPrefab, Sprite.transform, false);
            visibilityLight = visibilityObject.GetComponent<VisibilityLight>();
            visibilityLight.transform.rotation = Quaternion.identity;

            VisibleArea.visLight = visibilityLight;
        }
        if(FlashlightPrefab)
        {
            Flashlight = Instantiate(FlashlightPrefab, Sprite.transform, false);
            Flashlight.transform.rotation = CurrentLookRotation;
        }

        SpookManager spookManager = SpookManager.spookManager;
        if(spookManager)
        {
            spookManager.onHuntProgressed += OnHuntProgressed;
        }

        Heal();
    }

    private void Update()
    {
        CurrentLookAngle = Mathf.SmoothDampAngle(CurrentLookAngle, TargetLookAngle, ref CurrentLookSpeed, turnDampingTime, maxTurnRate, Time.deltaTime);

        CurrentLookRotation = FloorRotation * Quaternion.Euler(0f, CurrentLookAngle, 0f);
        Vector3 CurrentLookDir = CurrentLookRotation * Vector3.right;
        visibilityLight.ViewDir = new Vector2(CurrentLookDir.x, CurrentLookDir.z);
        Flashlight.transform.rotation = CurrentLookRotation;

        visibleArea.transform.LookAt(transform.position + new Vector3(CurrentLookDir.x, 0f, CurrentLookDir.z), Vector3.up);
        SetLookDir(new Vector2(CurrentLookDir.x, CurrentLookDir.z));

        UpdateFlashlightFocus();
    }

    void SetLookDir(in Vector2 newLookDir)
    {
        lookDir = newLookDir;
        EFaceDirection newFaceDir = SpookyUtilities.VectorToFaceDirection(newLookDir);
        if(newFaceDir != EFaceDirection.None) currentFaceDirection = newFaceDir;
    }

    public void HandleMoveInput(Vector2 input)
    {
        movementComponent.SetMovmentInput(input);
    }

    public void HandleLookInput(Vector2 input)
    {
        Vector3 NewLookDir = new Vector3(input.x, Mathf.Tan(FloorAngle * Mathf.Deg2Rad) * input.y, input.y).normalized;

        float cos = Vector3.Dot(NewLookDir, Vector3.right);
        float sin = Vector3.Dot(Vector3.Cross(Vector3.right, NewLookDir), FloorNormal);

        TargetLookAngle = Mathf.Acos(cos) * Mathf.Rad2Deg * (sin < 0f ? 1f : -1f);

        TargetLookRotation = FloorRotation * Quaternion.Euler(0f, TargetLookAngle, 0f);
    }

    public override Vector2 GetVelocity()
    {
        if(movementComponent)
        {
            return movementComponent.velocity;
        }

        return Vector2.zero;
    }

    [CheatSystem.Cheat("PlayerOnHuntProgressed")]
    void OnHuntProgressed()
    {
        if (huntBlinkCoroutine == null)
        {
            huntBlinkCoroutine = StartCoroutine(BlinkFlashlight());
        }
    }

    IEnumerator BlinkFlashlight()
    {
        if(!Flashlight) yield break;
        Light flashLightLight = Flashlight.GetComponentInChildren<Light>();
        
        if(!flashLightLight) yield break;

        WaitForSeconds fastBlink = new WaitForSeconds(huntFastBlinkTime);
        WaitForSeconds slowBlink = new WaitForSeconds(huntSlowBlinkTime);
        WaitForSeconds offTime = new WaitForSeconds(huntBlinkOffTime);
        float totalTime = 0f;
        while(totalTime < huntTotalBlinkTime)
        {
            flashLightLight.enabled = false;

            yield return offTime;
            totalTime += huntBlinkOffTime;

            flashLightLight.enabled = true;

            if (Random.value < huntFastBlinkProbability)
            {
                yield return fastBlink;
                totalTime += huntFastBlinkTime;
            }
            else
            {
                yield return slowBlink;
                totalTime += huntSlowBlinkTime;
            }
        }

        huntBlinkCoroutine = null;
    }

    public void SetLightFocus(bool bNewFocus)
    {
        if(flashlightFocusData.bIsFocused != bNewFocus)
        {
            flashlightFocusData.bIsFocused = bNewFocus;

            if ((flashlightFocusData.bIsFocused && flashlightFocusData.CurrentFocusPercent >= 1f) || (!flashlightFocusData.bIsFocused && flashlightFocusData.CurrentFocusPercent <= 0f))
            {
                flashlightFocusData.CurrentFocusPercent = flashlightFocusData.bIsFocused ? 1f : 0f;
            }
            else
            {
                flashlightFocusData.TimeFocusInput = Time.time;
                flashlightFocusData.FocusPercentAtInput = flashlightFocusData.CurrentFocusPercent;
                flashlightFocusData.bIsTransitioning = true;
            }
            
        }
    }

    void UpdateFlashlightFocus()
    {
        if(flashlightFocusData.bIsTransitioning)
        {
            if(flashlightFocusData.bIsFocused)
            {
                flashlightFocusData.CurrentFocusPercent = flashlightFocusData.FocusPercentAtInput + (Time.time - flashlightFocusData.TimeFocusInput) / flashlightFocusData.FocusTime;
                if(flashlightFocusData.CurrentFocusPercent >= 1f)
                {
                    flashlightFocusData.CurrentFocusPercent = 1f;
                    flashlightFocusData.bIsTransitioning = false;
                }
            }
            else
            {
                flashlightFocusData.CurrentFocusPercent = flashlightFocusData.FocusPercentAtInput - (Time.time - flashlightFocusData.TimeFocusInput) / flashlightFocusData.UnfocusTime;
                if(flashlightFocusData.CurrentFocusPercent <= 0f)
                {
                    flashlightFocusData.CurrentFocusPercent = 0f;
                    flashlightFocusData.bIsTransitioning = false;
                }
            }

            Light flashLightComp = Flashlight.GetComponentInChildren<Light>();
            if(flashLightComp)
            {
                flashLightComp.spotAngle = Mathf.Lerp(flashlightFocusData.UnfocusSpotAngle, flashlightFocusData.FocusSpotAngle, flashlightFocusData.CurrentFocusPercent);
                flashLightComp.innerSpotAngle = Mathf.Lerp(flashlightFocusData.UnfocusInnerSpotAngle, flashlightFocusData.FocusInnerSpotAngle, flashlightFocusData.CurrentFocusPercent);
                flashLightComp.intensity = Mathf.Lerp(flashlightFocusData.UnfocusIntensity, flashlightFocusData.FocusIntensity, flashlightFocusData.CurrentFocusPercent);
                flashLightComp.range = Mathf.Lerp(flashlightFocusData.UnfocusRange, flashlightFocusData.FocusRange, flashlightFocusData.CurrentFocusPercent);
            }

            if(visibilityLight)
            {
                visibilityLight.VisibilityBoundsAngle = Mathf.Lerp(flashlightFocusData.UnfocusVisibilityAngle, flashlightFocusData.FocusVisibilityAngle, flashlightFocusData.CurrentFocusPercent);
            }
        }

        if(flashlightFocusData.bIsFocused && !flashlightFocusData.bIsTransitioning)
        {
            RaycastHit hit;
            Vector3 traceStart = visibleArea.transform.position;
            traceStart.y = SpookyCollider.CollisionYValue + 2.5f;
            if(Physics.Raycast(traceStart, visibleArea.transform.forward, out hit, flashlightFocusData.FocusDistanceCheck, 1 << SpookyLayers.SpookyThingLayer))
            {
                Debug.DrawLine(traceStart, traceStart + hit.distance * visibleArea.transform.forward, Color.green);

                Light visLightComp = visibilityLight.light;

                visLightComp.range = Mathf.Min(hit.distance * 2f + flashlightFocusData.FocusRangeDistanceAddition * 2f, flashlightFocusData.FocusRange * 2f);


                SpookyThing currentFocus = hit.collider.GetComponent<SpookyThing>();
                if(currentFocus)
                {
                    if(thingFocused && thingFocused != currentFocus )
                    {
                        thingFocused.SetFocused(false);
                    }

                    thingFocused = currentFocus;
                    thingFocused.SetFocused(true);
                }
            }
            else
            {
                Debug.DrawLine(traceStart, traceStart + 3f * visibleArea.transform.forward, Color.red);

                Light visLightComp = visibilityLight.light;
                visLightComp.range = 7f;

                if(thingFocused)
                {
                    thingFocused.SetFocused(false);
                }
            }
        }
        else
        {
            Light visLightComp = visibilityLight.light;
            visLightComp.range = 7f;

            if(thingFocused)
            {
                thingFocused.SetFocused(false);
            }
        }
    }

    public void OnHit(MonoBehaviour objectHitting)
    {
        Damage();
    }

    public void Damage(int amount = 1)
    {
        int oldHP = currentHP;

        if(amount > 0)
        {
            currentHP = Mathf.Max(currentHP - amount, 0);
        }
        else
        {
            currentHP = 0;
        }

        if(onHPChanged != null)
        {
            onHPChanged(currentHP, oldHP);
        }

        if(currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount = -1)
    {
        int oldHP = currentHP;

        if(amount > 0)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
        }
        else
        {
            currentHP = maxHP;
        }

        if (onHPChanged != null)
        {
            onHPChanged(currentHP, oldHP);
        }
    }

    void Die()
    {

    }

    [CheatSystem.Cheat(), System.Diagnostics.Conditional("USING_CHEAT_SYSTEM")]
    void ToggleVisibilityLightEnabled()
    {
        if(visibilityLight)
        {
            visibilityLight.enabled = !visibilityLight.enabled;
        }
    }
}
