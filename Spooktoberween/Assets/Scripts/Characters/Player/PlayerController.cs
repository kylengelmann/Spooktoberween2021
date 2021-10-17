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

        controls.Gameplay.Look_Mouse.performed += OnLookInput;
        controls.Gameplay.Look_Mouse.performed += OnLookInput;

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
        if(player)
        {
            player.HandleMoveInput(context.ReadValue<Vector2>());
        }
    }

    void OnLookInput(InputAction.CallbackContext context)
    {
        if (player)
        {
            Vector3 PlayerPos = Camera.main.WorldToScreenPoint(player.transform.position);

            Vector2 LookInput = context.ReadValue<Vector2>();
            Vector2 LookDir = LookInput - new Vector2(PlayerPos.x, PlayerPos.y);

            player.HandleLookInput(LookDir);
        }
    }
}
