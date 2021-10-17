// GENERATED AUTOMATICALLY FROM 'Assets/DataAssets/Input/SpookyControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @SpookyControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @SpookyControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""SpookyControls"",
    ""maps"": [
        {
            ""name"": ""Gameplay"",
            ""id"": ""83f5b3f2-cc66-4a3e-94a9-5bde26040eec"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""41cc140e-8f4f-4d39-91bf-d011335723be"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Look_Mouse"",
                    ""type"": ""Value"",
                    ""id"": ""734396fb-4064-412f-91be-0ddfe497b445"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Look_Gamepad"",
                    ""type"": ""Value"",
                    ""id"": ""ecdde42e-a74a-4783-b781-7107ddf9ff89"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""21a8d075-dff0-4437-9cf0-2e27d850aff1"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""69b075c3-001b-4722-8a54-fd7b0d4736b3"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""85b9f064-6bfc-4ad9-9c89-6a59792d772a"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""3cd9a3ae-3063-4d84-85ce-abe82f0174d8"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""c052d058-d219-496e-b2a9-55b8b41e6b89"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ee69a354-0809-47b7-b5fa-15b92f8bbca2"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""a475828c-696b-4638-a5e4-6ca5394316c4"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look_Mouse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cd411f54-079a-4e1c-9860-be221adbb525"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look_Gamepad"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Gameplay
        m_Gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
        m_Gameplay_Movement = m_Gameplay.FindAction("Movement", throwIfNotFound: true);
        m_Gameplay_Look_Mouse = m_Gameplay.FindAction("Look_Mouse", throwIfNotFound: true);
        m_Gameplay_Look_Gamepad = m_Gameplay.FindAction("Look_Gamepad", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Gameplay
    private readonly InputActionMap m_Gameplay;
    private IGameplayActions m_GameplayActionsCallbackInterface;
    private readonly InputAction m_Gameplay_Movement;
    private readonly InputAction m_Gameplay_Look_Mouse;
    private readonly InputAction m_Gameplay_Look_Gamepad;
    public struct GameplayActions
    {
        private @SpookyControls m_Wrapper;
        public GameplayActions(@SpookyControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_Gameplay_Movement;
        public InputAction @Look_Mouse => m_Wrapper.m_Gameplay_Look_Mouse;
        public InputAction @Look_Gamepad => m_Wrapper.m_Gameplay_Look_Gamepad;
        public InputActionMap Get() { return m_Wrapper.m_Gameplay; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameplayActions set) { return set.Get(); }
        public void SetCallbacks(IGameplayActions instance)
        {
            if (m_Wrapper.m_GameplayActionsCallbackInterface != null)
            {
                @Movement.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMovement;
                @Look_Mouse.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLook_Mouse;
                @Look_Mouse.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLook_Mouse;
                @Look_Mouse.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLook_Mouse;
                @Look_Gamepad.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLook_Gamepad;
                @Look_Gamepad.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLook_Gamepad;
                @Look_Gamepad.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLook_Gamepad;
            }
            m_Wrapper.m_GameplayActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
                @Look_Mouse.started += instance.OnLook_Mouse;
                @Look_Mouse.performed += instance.OnLook_Mouse;
                @Look_Mouse.canceled += instance.OnLook_Mouse;
                @Look_Gamepad.started += instance.OnLook_Gamepad;
                @Look_Gamepad.performed += instance.OnLook_Gamepad;
                @Look_Gamepad.canceled += instance.OnLook_Gamepad;
            }
        }
    }
    public GameplayActions @Gameplay => new GameplayActions(this);
    public interface IGameplayActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnLook_Mouse(InputAction.CallbackContext context);
        void OnLook_Gamepad(InputAction.CallbackContext context);
    }
}
