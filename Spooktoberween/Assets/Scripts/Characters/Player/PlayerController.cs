using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public SpookyPlayer player {get; private set;}

    SpookyControls controls;
    bool bLastLookInputWasController;
    Vector2 mousePos = Vector2.zero;

    private void Awake()
    {
        controls = new SpookyControls();
        controls.Gameplay.Movement.performed += OnMovmentInput;
        controls.Gameplay.Movement.canceled += OnMovmentInput;

        controls.Gameplay.Look_Mouse.performed += OnMouseLookInput;
        controls.Gameplay.Look_Mouse.performed += OnMouseLookInput;

        controls.Gameplay.Look_Gamepad.performed += OnGamepadLookInput;
        controls.Gameplay.Look_Gamepad.performed += OnGamepadLookInput;

        if (isActiveAndEnabled)
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

    private void Update()
    {
        if(!bLastLookInputWasController)
        {
            Vector3 PlayerPos = Camera.main.WorldToScreenPoint(player.transform.position);
            Vector2 LookDir = mousePos - new Vector2(PlayerPos.x, PlayerPos.y);

            player.HandleLookInput(LookDir);
        }
    }

    void OnMovmentInput(InputAction.CallbackContext context)
    {
        if(player)
        {
            player.HandleMoveInput(context.ReadValue<Vector2>());
        }
    }

    void OnMouseLookInput(InputAction.CallbackContext context)
    {
        bLastLookInputWasController = false;

        if (player)
        {
            Vector3 PlayerPos = Camera.main.WorldToScreenPoint(player.transform.position);

            mousePos = context.ReadValue<Vector2>();

            Vector2 LookDir = mousePos - new Vector2(PlayerPos.x, PlayerPos.y);

            player.HandleLookInput(LookDir);
        }
    }

    void OnGamepadLookInput(InputAction.CallbackContext context)
    {
        bLastLookInputWasController = true;

        if (player)
        {
            player.HandleLookInput(context.ReadValue<Vector2>());
        }
    }
}
