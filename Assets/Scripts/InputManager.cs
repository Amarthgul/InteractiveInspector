using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    /// <summary>
    /// ======================= Input actions =========================
    /// </summary>
    private UserInput inputScheme;


    /// <summary>
    /// ==================== Serialized variables ===================== 
    /// </summary>

    [SerializeField] private CameraControl cameraControl;

    [SerializeField] private TapHandler tapDetector;

    [Tooltip("UI_HelpFunction class UI toolkit")]
    [SerializeField] private UI_HelpFunction UIHF;

    [Tooltip("UI_Operations class UI toolkit")]
    [SerializeField] private UI_Operations UIO; 

    private void Awake()
    {
        inputScheme = new UserInput();

        cameraControl.Initialize(inputScheme.Desktop.LeftClick,
            inputScheme.Desktop.RightClick,
            inputScheme.Desktop.MousePosition,
            inputScheme.Desktop.MouseDelta,
            inputScheme.Desktop.MouseScroll, 
            inputScheme.iOS.Primary, 
            inputScheme.iOS.Secondary);

        tapDetector.Initialize(inputScheme.iOS.Primary);

        UIHF.Initialize(inputScheme.iOS.Primary);

        UIO.Initialize(inputScheme.iOS.Primary);
    }

    private void OnEnable()
    {
        inputScheme.Enable();

        var _q = new QuitHandler(inputScheme.Desktop.Quit);

    }


}
