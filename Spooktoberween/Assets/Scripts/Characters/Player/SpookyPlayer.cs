using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpookyPlayer : Character
{
    public PlayerMovementComponent movementComponent {get; private set;}

    public float maxTurnRate = 360;

    [SerializeField] GameObject VisibilityLightPrefab;
    [SerializeField] GameObject Sprite;
    [SerializeField] GameObject FlashlightPrefab;

    VisibilityLight visibilityLight;
    GameObject Flashlight;

    Quaternion CurrentLookRotation = Quaternion.identity;
    Quaternion TargetLookRotation = Quaternion.identity;
    float FloorAngle = -60f;

    Quaternion StartingFlashlightRotation = Quaternion.identity;
    private void Awake()
    {
        movementComponent = GetComponent<PlayerMovementComponent>();
    }

    private void Start()
    {
        if(VisibilityLightPrefab)
        {
            GameObject visibilityObject = Instantiate(VisibilityLightPrefab, Sprite.transform, false);
            visibilityLight = visibilityObject.GetComponent<VisibilityLight>();
            visibilityLight.transform.rotation = Quaternion.identity;

            Flashlight = Instantiate(FlashlightPrefab, Sprite.transform, false);
            StartingFlashlightRotation = Flashlight.transform.rotation = Quaternion.Inverse(Sprite.transform.rotation) * Flashlight.transform.rotation;
        }
    }

    private void Update()
    {
        CurrentLookRotation = Quaternion.RotateTowards(CurrentLookRotation, TargetLookRotation, maxTurnRate * Time.deltaTime);
        Vector3 CurrentLookDir = CurrentLookRotation * Vector3.right;
        visibilityLight.ViewDir = new Vector2(CurrentLookDir.x, CurrentLookDir.z);
        //Flashlight.transform.rotation = CurrentLookRotation * StartingFlashlightRotation;
    }

    public void HandleMoveInput(Vector2 input)
    {
        movementComponent.SetMovmentInput(input);
    }

    public void HandleLookInput(Vector2 input)
    {
        Vector3 NewLookDir = new Vector3(input.x, Mathf.Tan(FloorAngle * Mathf.Deg2Rad) * input.y, input.y);

        Debug.Log(NewLookDir);

        TargetLookRotation = Quaternion.FromToRotation(Vector3.right, NewLookDir);
    }
}
