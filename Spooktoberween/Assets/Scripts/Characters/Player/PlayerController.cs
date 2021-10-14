using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public SpookyPlayer player {get; private set;}

    SpookyControls controls;

    private void Awake()
    {
        controls = new SpookyControls();
        controls.Gameplay.Movement.performed += OnMovmentInput;
        controls.Gameplay.Movement.canceled += OnMovmentInput;

        if(isActiveAndEnabled)
        {
            controls.Enable();
        }

        player = GetComponent<SpookyPlayer>();
    }

    private void OnEnable()
    {
        if(controls != null)
        {
            controls.Enable();
        }
    }

    private void OnDisable()
    {
        if(controls != null)
        {
            controls.Disable();
        }
    }

    void OnMovmentInput(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>());
        if(player)
        {
            player.HandleMoveInput(context.ReadValue<Vector2>());
        }
    }
}
