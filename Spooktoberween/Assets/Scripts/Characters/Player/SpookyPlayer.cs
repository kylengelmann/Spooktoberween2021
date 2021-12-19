using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly:CheatSystem.CheatClass(typeof(SpookyPlayer))]
public class SpookyPlayer : Character
{
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
    Coroutine huntBlinkCoroutine;

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

    public Vector2 GetVelocity()
    {
        return movementComponent.velocity;
    }

    void OnHuntProgressed()
    {
        if (huntBlinkCoroutine == null)
        {
            huntBlinkCoroutine = StartCoroutine(BlinkFlashlight());
        }
    }

    IEnumerator BlinkFlashlight()
    {
        WaitForSeconds fastBlink = new WaitForSeconds(.05f);
        WaitForSeconds slowBlink = new WaitForSeconds(.1f);
        float totalTime = 0f;
        while(totalTime < huntTotalBlinkTime)
        {
            if(Random.value < huntFastBlinkProbability)
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
