using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpookyPlayer : Character
{
    public PlayerMovementComponent movementComponent {get; private set;}

    [SerializeField] GameObject VisibilityLightPrefab;
    [SerializeField] GameObject Sprite;

    VisibilityLight visibilityLight;
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
        }
    }

    public void HandleMoveInput(Vector2 input)
    {
        movementComponent.SetMovmentInput(input);
    }
}
