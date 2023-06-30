using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Pretty much hand animating the entire thing by giving hard positional data. 
public class UI_HelpFunction : MonoBehaviour
{
    /// ===============================================================
    /// ========================== CONSTANTS ========================== 

    private const float PINCH_ICON_POS = 50;

    private const int INSTRUCTION_GB_TOP = 260;

    private const int INSTRUCTION_GB_HEIGHT = 1024;

    /// ===============================================================
    /// ==================== Serialized variables =====================


    [SerializeField] CameraControl meinCamera;

    [SerializeField] TapHandler tapHandler;

    [SerializeField] UI_Operations UIoperation; 

    [SerializeField] private string rotateInstruction;

    [SerializeField] private string panInstruction;

    [SerializeField] private string zoomInstruction;

    [SerializeField] private Sprite mouseInstruction;

    [SerializeField] private Sprite touchInstruction;


    [Help("Hard coding the position of the animations. The following defines " +
        "the parameters of the initial animation. After that, the user can " +
        "still access this by tapping the question mark on top-left, but " +
        "that would be still images without animtion.")]
    [Header("Manual animation")]

    [Tooltip("When enabled, a help animation plays automatically if the player makes no" +
        "input for 5 scconds after the app started.")]
    [SerializeField] private bool enableStartAnimation = false;

    [Tooltip("When enabled, the user can touch and end the help animation while ")]
    [SerializeField] bool allowTouchInterruption = false;

    [Tooltip("The maxmium opacity for all elements in this UI document")]
    [Range(0f, 1f)]
    [SerializeField] float maxOpacity = .9f;


    [Tooltip("For developers only. This threshold defines the value that will be " +
        "neglected if 2 opacity values' difference is below this.")]
    [Range(0f, .1f)]
    [SerializeField] private float opacityThreshold = .02f;

    [Tooltip("The unified fade in/out animation time.")]
    [Range(0f, 5f)]
    [SerializeField] float fadeTime = .5f;

    [Tooltip("The help instruction may not start immediately, this indicates how long " +
        "of a delay should be after the application starts.")]
    [Range(0f, 10f)]
    [SerializeField] float initialDelay = 1f;

    /// ===============================================================
    /// ======================= Input actions =========================
    private InputAction touchPrimary;

    /// ===============================================================
    /// ======================= Stat variables ========================

    private VisualElement root;

    private Button helpInstructionButton;
    //private Button rand;

    private GroupBox helpInstructionsGB;
    private GroupBox instructionSprite; 
    private Label rotText;
    private Label panText;
    private Label zoomText;

    private bool helpButtonSelected = false;

    // Each animation stage is marked true once complete
    private List<bool> animeStage = new List<bool>() { false, false, false };

    private bool initialFadeInStarted = false;
    private bool initialFadeInFinished = false;

    private bool helpPanelTransitioning = false;

    private List<float> textPos = new List<float>() { 360, 960, 1560 };

    private int lastTapCount = 0; 
    private Stopwatch tapProtectionSW = new Stopwatch();
    private int tapProtectionPeriod = 1200;    // Time in milisecond that ignores tapping 

    private Stopwatch progressionSW = new Stopwatch();

    private Globals.CameraState lastCameraState; 

    private Color defaultTintColor = Color.white;

    private bool initialUpdated = false;


    private bool lastLightModeStat = false;
    private Color UITintColor = new Color();
    private Color txtTintColor = new Color();

    private int breathingCycle = 2000; // In milisecond 
    private Stopwatch breathingSW = new Stopwatch();    


    private void OnEnable() 
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        helpInstructionButton = root.Q<Button>("HelpButton");

        helpInstructionsGB = root.Q<GroupBox>("HelpInstructions");
        instructionSprite = helpInstructionsGB.Q<GroupBox>("ImageSprite");
        rotText  = helpInstructionsGB.Q<Label>("RotText");
        panText  = helpInstructionsGB.Q<Label>("PanText");
        zoomText = helpInstructionsGB.Q<Label>("ZoomText");

        helpInstructionButton.clicked += () => ToggleHelpInstructionDisplay();
    }

    // Start is called before the first frame update
    void Start()
    {
        UITintColor = Globals.darkModeUITint;
        txtTintColor = Globals.darkModeTxtTint;
        UpdateAllModeTintColors();

        root.style.opacity = maxOpacity;
        helpInstructionsGB.style.opacity = 0;
        lastCameraState = meinCamera.GetCameraState();

        rotText.text = rotateInstruction;
        panText.text = panInstruction;
        zoomText.text = zoomInstruction;

        
    }

    // Update is called once per frame
    void Update()
    {

        UpdateHelpInstructions();

        UpdateTheme();
    }

    public void Initialize(InputAction PT)
    {
        touchPrimary = PT;
    }

    /// ===============================================================
    /// ======================= Private Methods =======================
    /// ===============================================================

    /// <summary>
    /// Toggle help instruction page display when user tapped on the help icon. 
    /// </summary>
    private void ToggleHelpInstructionDisplay()
    {

        if (helpButtonSelected)
        {
            helpButtonSelected = false;
            helpPanelTransitioning = true;
            progressionSW.Restart();
            breathingSW.Restart();

            helpInstructionButton.style.unityBackgroundImageTintColor = defaultTintColor;
        }
        else
        {
            tapHandler.UnselectAll();
            UIoperation.TurnOffAllPanels();
            helpButtonSelected = true;
            helpPanelTransitioning = true;
            progressionSW.Restart();

            helpInstructionButton.style.unityBackgroundImageTintColor = Globals.buckeyeHighlight;
        }
    }

    /// <summary>
    /// Display or hide the help instruction panel. 
    /// </summary>
    private void UpdateHelpInstructions()
    {

        if(helpPanelTransitioning)
        {
            // Fade in the help instruction panel
            if (helpButtonSelected)
            {
                float progression = progressionSW.ElapsedMilliseconds / (fadeTime * Globals.MILISECOND_IN_SEC);
                float opacity = Sigmoid(progression) * maxOpacity;

                helpInstructionsGB.style.opacity = opacity;

                if (opacity >= (maxOpacity - opacityThreshold))
                {
                    helpPanelTransitioning = false;
                    helpInstructionsGB.style.opacity = maxOpacity;
                    tapProtectionSW.Restart(); 
                }
            }
            // Fade out the help instruction panel 
            else
            {
                float progression = progressionSW.ElapsedMilliseconds / (fadeTime * Globals.MILISECOND_IN_SEC);
                float opacity = (1 - Sigmoid(progression)) * maxOpacity;

                helpInstructionsGB.style.opacity = opacity;

                if (opacity <= (0 + opacityThreshold))
                {
                    helpPanelTransitioning = false;
                    helpInstructionsGB.style.opacity = 0;
                }
            }
        }

        // Check for additional touch and toggle off the help instruction
        // if the tap is outside of the help instruction display area.
        // Also 
        if (helpButtonSelected && !helpPanelTransitioning 
            && tapProtectionSW.ElapsedMilliseconds > tapProtectionPeriod)
        {
            TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();

            Vector2 position = new Vector2(); 
            bool newTap = false;

            if(!Globals.webMode && currentTouchF1.tapCount != lastTapCount)
            {
                newTap = true; 
                position = currentTouchF1.position; 
            }
            if (Globals.webMode)
            {

            }



            if (newTap && (position.y > INSTRUCTION_GB_TOP + INSTRUCTION_GB_HEIGHT ||
                    position.y < INSTRUCTION_GB_TOP)) 
            {
                ToggleHelpInstructionDisplay();
            }

            lastTapCount = currentTouchF1.tapCount;

            UpdateBreathingAnimation(); 
        }

    }

    /// <summary>
    /// When in webmode, update the key highlight effect with breathing animation. 
    /// </summary>
    private void UpdateBreathingAnimation()
    {

    }

    /// <summary>
    /// Try to see if the mode has switched, and if so, update the color palette 
    /// </summary>
    private void UpdateTheme()
    {

        if (lastLightModeStat != Globals.lightModeOn)
        {
            if (Globals.lightModeOn)
            {
                UITintColor = Globals.lightModeUITint;
                txtTintColor = Globals.lightModeTxtTint;
            }
            else
            {
                UITintColor = Globals.darkModeUITint;
                txtTintColor = Globals.darkModeTxtTint;
            }
            UpdateAllModeTintColors();
        }

        lastLightModeStat = Globals.lightModeOn;
    }

    /// <summary>
    /// Update all UI elements in this UI doc to the current color scheme 
    /// </summary>
    private void UpdateAllModeTintColors()
    {
        helpInstructionButton.style.unityBackgroundImageTintColor = txtTintColor;
        helpInstructionsGB.style.backgroundColor = UITintColor;
        helpInstructionsGB.Q<GroupBox>("ImageSprite").style.unityBackgroundImageTintColor = txtTintColor;

        rotText.style.color = txtTintColor;
        panText.style.color = txtTintColor;
        zoomText.style.color = txtTintColor;

        if (Globals.webMode)
            instructionSprite.style.backgroundImage = new StyleBackground(mouseInstruction);
        else
            instructionSprite.style.backgroundImage = new StyleBackground(touchInstruction);
    }

    /// ===============================================================
    /// ======================= Utility Methods =======================
    /// ===============================================================

    /// <summary>
    /// A sigmoid function with offset, roughly map input [0, 1] to output (0, 1)
    /// to simulate the slow-in slow-out effect. 
    /// </summary>
    /// <param name="valueX">Input X value, ideally in [0, 1]</param>
    /// <returns>A value in [0, 1]</returns>
    private float Sigmoid(float valueX)
    {
        return 1 / (1 + Mathf.Exp(-10 * (valueX - 0.5f)));
    }

    /// <summary>
    /// Calculated the opacity for the breathing animation effect. 
    /// </summary>
    /// <param name="valueX">Input</param>
    /// <param name="offset">Peak offset</param>
    /// <returns></returns>
    private float Breath(float valueX, float offset)
    {
        return Mathf.Clamp( Mathf.Sin((valueX + offset) * (Mathf.PI * 2) / breathingCycle), 
            0f, 1f);
    }
}
