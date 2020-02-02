// GENERATED AUTOMATICALLY FROM 'Assets/Common/Settings/TutorialControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @TutorialControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @TutorialControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""TutorialControls"",
    ""maps"": [
        {
            ""name"": ""DefaultMapping"",
            ""id"": ""0bcf1ae4-d4a8-4a89-af0f-2fc99c062bc4"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""0b87067b-513e-4be1-b906-637b3f915ddd"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": ""RepeatedPress(delay=0.2)""
                },
                {
                    ""name"": ""GenerateMap"",
                    ""type"": ""Button"",
                    ""id"": ""265618f5-c436-4ae7-9d31-f88042c1122f"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""RepeatedPress""
                },
                {
                    ""name"": ""ResizeMap"",
                    ""type"": ""Value"",
                    ""id"": ""fd074da2-8877-4d0b-9885-2388a909c4d0"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": ""RepeatedPress(delay=0.2,rate=0.01)""
                },
                {
                    ""name"": ""QuitGame"",
                    ""type"": ""Button"",
                    ""id"": ""bed7f492-4a3b-4b7a-911e-deaaf13f9117"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""RepeatedPress(delay=0.2,rate=0.01)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""fcc22626-bae5-4eb6-8f58-2d668aa71a50"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""35363870-0971-4da9-8f1e-05d98ceddd20"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""6467042c-84dd-4f1a-934c-9020d5e42063"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2882d01e-f9cb-4571-9acc-d41449c9e6fa"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""83bcb719-9ea6-48b9-87b9-1b661ff3b523"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""e6d58367-ab70-4f1e-a6a9-8c59c5aeae21"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Numpad"",
                    ""id"": ""6898ec67-4196-402b-a995-91ee0e03d9ea"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""eb4a6870-dabf-4a86-b9d1-ab5acba6d6fb"",
                    ""path"": ""<Keyboard>/numpad8"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""821a4d05-b27b-444f-a44d-40150a3c7a82"",
                    ""path"": ""<Keyboard>/numpad2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""8f28d01a-8bb3-4713-89ec-8fbb9ff9cebe"",
                    ""path"": ""<Keyboard>/numpad4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""9ec2842e-0e84-4669-8b43-1378e22a4800"",
                    ""path"": ""<Keyboard>/numpad6"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c1e84ef1-66d6-44b0-8874-2fe1656f1e0f"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GenerateMap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""84a35741-4aa2-46ac-86ff-ef6ef3afd03f"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ResizeMap"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""848f3b74-ba46-4de6-9502-6348fa7aa635"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ResizeMap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""0445c42f-f663-461d-9120-d8d9e1259a1d"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ResizeMap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""6e4e578b-7839-4673-bee5-1d1ef8a536a2"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ResizeMap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""85f29695-d3a1-4808-a927-141fc9d9dd8d"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ResizeMap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""cdf5231c-43a1-4164-b7b3-b4f15ed10bed"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""QuitGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // DefaultMapping
        m_DefaultMapping = asset.FindActionMap("DefaultMapping", throwIfNotFound: true);
        m_DefaultMapping_Move = m_DefaultMapping.FindAction("Move", throwIfNotFound: true);
        m_DefaultMapping_GenerateMap = m_DefaultMapping.FindAction("GenerateMap", throwIfNotFound: true);
        m_DefaultMapping_ResizeMap = m_DefaultMapping.FindAction("ResizeMap", throwIfNotFound: true);
        m_DefaultMapping_QuitGame = m_DefaultMapping.FindAction("QuitGame", throwIfNotFound: true);
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

    // DefaultMapping
    private readonly InputActionMap m_DefaultMapping;
    private IDefaultMappingActions m_DefaultMappingActionsCallbackInterface;
    private readonly InputAction m_DefaultMapping_Move;
    private readonly InputAction m_DefaultMapping_GenerateMap;
    private readonly InputAction m_DefaultMapping_ResizeMap;
    private readonly InputAction m_DefaultMapping_QuitGame;
    public struct DefaultMappingActions
    {
        private @TutorialControls m_Wrapper;
        public DefaultMappingActions(@TutorialControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_DefaultMapping_Move;
        public InputAction @GenerateMap => m_Wrapper.m_DefaultMapping_GenerateMap;
        public InputAction @ResizeMap => m_Wrapper.m_DefaultMapping_ResizeMap;
        public InputAction @QuitGame => m_Wrapper.m_DefaultMapping_QuitGame;
        public InputActionMap Get() { return m_Wrapper.m_DefaultMapping; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DefaultMappingActions set) { return set.Get(); }
        public void SetCallbacks(IDefaultMappingActions instance)
        {
            if (m_Wrapper.m_DefaultMappingActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnMove;
                @GenerateMap.started -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnGenerateMap;
                @GenerateMap.performed -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnGenerateMap;
                @GenerateMap.canceled -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnGenerateMap;
                @ResizeMap.started -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnResizeMap;
                @ResizeMap.performed -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnResizeMap;
                @ResizeMap.canceled -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnResizeMap;
                @QuitGame.started -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnQuitGame;
                @QuitGame.performed -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnQuitGame;
                @QuitGame.canceled -= m_Wrapper.m_DefaultMappingActionsCallbackInterface.OnQuitGame;
            }
            m_Wrapper.m_DefaultMappingActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @GenerateMap.started += instance.OnGenerateMap;
                @GenerateMap.performed += instance.OnGenerateMap;
                @GenerateMap.canceled += instance.OnGenerateMap;
                @ResizeMap.started += instance.OnResizeMap;
                @ResizeMap.performed += instance.OnResizeMap;
                @ResizeMap.canceled += instance.OnResizeMap;
                @QuitGame.started += instance.OnQuitGame;
                @QuitGame.performed += instance.OnQuitGame;
                @QuitGame.canceled += instance.OnQuitGame;
            }
        }
    }
    public DefaultMappingActions @DefaultMapping => new DefaultMappingActions(this);
    public interface IDefaultMappingActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnGenerateMap(InputAction.CallbackContext context);
        void OnResizeMap(InputAction.CallbackContext context);
        void OnQuitGame(InputAction.CallbackContext context);
    }
}
