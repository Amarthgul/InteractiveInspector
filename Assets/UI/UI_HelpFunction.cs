using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor.Experimental;
using UnityEngine.XR;
using static UnityEngine.Rendering.DebugUI.MessageBox;

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

    [SerializeField] Color iconTintColor = Color.white;

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

    [Space(15)]
    [Header("Rotate animation")]
    [Space(10)]

    [Range(0f, 10f)]
    [SerializeField] float rotAnimTime = 3f;

    [SerializeField] string rotDescription = "haha";

    [SerializeField] Sprite rotSprite;

    [Tooltip("Only X is needed as the style.left distance")]
    [SerializeField] Vector2 rotAnimStartPos = Vector2.zero;

    [Tooltip("Only X is needed as the style.left distance")]
    [SerializeField] Vector2 rotAnimEndPos = Vector2.zero;

    [Space(15)]
    [Header("Pan animation")]
    [Space(10)]

    [Range(0f, 10f)]
    [SerializeField] float panAnimTime = 3f;

    [SerializeField] string panDescription = "hahahaha";

    [SerializeField] Sprite panSprite;

    [Tooltip("Only X is needed as the style.left distance")]
    [SerializeField] Vector2 panAnimStartPos = Vector2.zero;

    [Tooltip("Only X is needed as the style.left distance")]
    [SerializeField] Vector2 panAnimEndPos = Vector2.zero;


    [Space(15)]
    [Header("Zoom animation")]
    [Space(10)]

    [Range(0f, 10f)]
    [SerializeField] float zoomAnimTime = 3f;

    [SerializeField] string zoomDescription = "hahahahahaha";

    [SerializeField] Sprite zoomSprite;

    [SerializeField] Color arrowTintColor;

    [SerializeField] Vector2 zoomArrow1StartPos = Vector2.zero;
    [SerializeField] Vector2 zoomArrow1EndPos = Vector2.zero;

    [SerializeField] Vector2 zoomArrow2StartPos = Vector2.zero;
    [SerializeField] Vector2 zoomArrow2EndPos = Vector2.zero;

    /// ===============================================================
    /// ======================= Input actions =========================
    private InputAction touchPrimary;

    /// ===============================================================
    /// ======================= Stat variables ========================

    private VisualElement root;

    private Button helpInstructionButton;
    //private Button rand;

    private GroupBox animeContainer;
    private Button animeSprite;
    private Button animeText;

    private IMGUIContainer arrowTopSprite;
    private IMGUIContainer arrowBottomSprite;

    private GroupBox helpInstructionsGB;
    private Label rotText;
    private Label panText;
    private Label zoomText;

    private bool helpButtonSelected = false;

    // Each animation stage is marked true once complete
    private List<bool> animeStage = new List<bool>() { false, false, false };

    private bool initialFadeInStarted = false;
    private bool initialFadeInFinished = false;

    private bool helpPanelTransitioning = false;

    // True location after factor in the screen resolution difference 
    private Vector2 TrotAnimStartPos;
    private Vector2 TrotAnimEndPos;
    private Vector2 TpanAnimStartPos;
    private Vector2 TpanAnimEndPos;
    private Vector2 TzoomArrow1StartPos;
    private Vector2 TzoomArrow1EndPos;
    private Vector2 TzoomArrow2StartPos;
    private Vector2 TzoomArrow2EndPos;

    private List<float> textPos = new List<float>() { 360, 960, 1560 };

    private int lastTapCount = 0; 
    private Stopwatch tapProtectionSW = new Stopwatch();
    private int tapProtectionPeriod = 1200;    // Time in milisecond that ignores tapping 

    private Stopwatch progressionSW = new Stopwatch();

    private Globals.CameraState lastCameraState; 

    private Color defaultTintColor = Color.white;

    private bool initialUpdated = false;


    private void OnEnable() 
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        helpInstructionButton = root.Q<Button>("HelpButton");
        animeContainer = root.Q<GroupBox>("ContainerBox");
        animeSprite = animeContainer.Q<Button>("AnimationSprite");
        animeText = animeContainer.Q<Button>("AnimationText");

        arrowTopSprite = animeContainer.Q<IMGUIContainer>("ArrowTop");
        arrowBottomSprite = animeContainer.Q<IMGUIContainer>("ArrowBottom");

        helpInstructionsGB = root.Q<GroupBox>("HelpInstructions");
        rotText  = helpInstructionsGB.Q<Label>("RotText");
        panText  = helpInstructionsGB.Q<Label>("PanText");
        zoomText = helpInstructionsGB.Q<Label>("ZoomText");

        helpInstructionButton.clicked += () => ToggleHelpInstructionDisplay();
    }

    // Start is called before the first frame update
    void Start()
    {
        root.style.opacity = maxOpacity;
        lastCameraState = meinCamera.GetCameraState();

        animeSprite.style.backgroundImage = new StyleBackground(rotSprite);
        animeSprite.style.left = TrotAnimStartPos.x;
        animeText.text = rotDescription;

        animeSprite.style.opacity = 0;
        animeText.style.opacity = 0;

        arrowBottomSprite.style.opacity = 0;
        arrowTopSprite.style.opacity = 0;

        helpInstructionsGB.style.opacity = 0;
        rotText.text = rotDescription;
        panText.text = panDescription;
        zoomText.text = zoomDescription;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialUpdated)
            CalculateLocations();

        if (enableStartAnimation)
        {
            CheckWarmUp();
            UpdateWarmUp();

            // As long as a stage is not complete
            // update the initial animation 
            if (animeStage.Contains(false))
            {
                UpdateAnimation();
            }
            // Ensure all animated sprites are dimmed out
            else
            {
                UpdateDimmedOutSprites();
            }
            UpdateTouchInterruption();
        }

        UpdateHelpInstructions();
    }

    public void Initialize(InputAction PT)
    {
        touchPrimary = PT;
    }

    /// ===============================================================
    /// ======================= Private Methods =======================
    /// ===============================================================


    /// <summary>
    /// Given the current screen's resolution, calculate the location 
    /// of the UI elements and their animtion locations. 
    /// </summary>
    private void CalculateLocations()
    {
        int screenWidth = Screen.currentResolution.width;
        int screenHeight = Screen.currentResolution.height;

        // Scale ratio between iPad Air 4th gen and current device 
        float ratio = screenWidth / Globals.iPadAir4thGen.y;

        TrotAnimStartPos = rotAnimStartPos * ratio;
        TrotAnimEndPos = rotAnimEndPos * ratio;
        TpanAnimStartPos = panAnimStartPos * ratio;
        TpanAnimEndPos = panAnimEndPos * ratio;
        animeSprite.style.left = TpanAnimStartPos.x; 

        TzoomArrow1StartPos = zoomArrow1StartPos * ratio;
        TzoomArrow1EndPos = zoomArrow1EndPos * ratio;
        TzoomArrow2StartPos = zoomArrow2StartPos * ratio;
        TzoomArrow2EndPos = zoomArrow2EndPos * ratio;

        // The first several updates, for some reason, returns NaN for size (shouldn't Start and OnEnable be called first?).
        // So only after the values become positive will the initial update be marked as complete. 
        if (helpInstructionsGB.resolvedStyle.height > 0)
            initialUpdated = true;
    }

    /// <summary>
    /// Helper animation will not start count down before the camera is ready
    /// </summary>
    private void CheckWarmUp()
    {
        if (initialFadeInFinished) return; 

        // If the camera has passed warmup and has started 
        if (lastCameraState != Globals.CameraState.Start
            && meinCamera.GetCameraState() == Globals.CameraState.Start)
        {
            progressionSW.Start();
        }

        lastCameraState = meinCamera.GetCameraState();
    }

    /// <summary>
    /// Delay a certain amount of time and then mark the start of the help animation. 
    /// </summary>
    private void UpdateWarmUp()
    {
        // Do nothing if this has alreday been done once 
        if (initialFadeInFinished) return;

        // Do nothing if the countdown is still on-going 
        if (progressionSW.ElapsedMilliseconds < initialDelay * Globals.MILISECOND_IN_SEC && !initialFadeInStarted) 
            return; 

        // Mark the start of animation 
        else if(!initialFadeInStarted)
        {
            initialFadeInStarted = true;
            progressionSW.Restart();
        }

        // Fade in the elements if the user has not touxhed the screen
        if (initialFadeInStarted && animeStage.Contains(false))
        {
            float fadeProrgession = progressionSW.ElapsedMilliseconds / (fadeTime * Globals.MILISECOND_IN_SEC);
            float opacity = Sigmoid(fadeProrgession) * maxOpacity;
             
            animeSprite.style.opacity = opacity;
            animeText.style.opacity = opacity;

            if (progressionSW.ElapsedMilliseconds > (fadeTime * Globals.MILISECOND_IN_SEC)) 
            {
                animeSprite.style.opacity = maxOpacity;
                animeText.style.opacity = maxOpacity;
                initialFadeInFinished = true;

                progressionSW.Restart();
            }
        }
    }

    /// <summary>
    /// The automatic animation of the help instruction at the start
    /// </summary>
    private void UpdateAnimation()
    {
        // There are appreantly tons of duplicates in this method,
        // it was built up gradually as the demand shifted from the original. 
        // Try simplfy these if you wish. 

        // Before initial fade in finishes, do nothing 
        // If user already touched the screen, do nothing 
        if (!initialFadeInFinished || !animeStage.Contains(false)) return;

        float prorgession;
        float opacity;
        float LeftPos;

        // =======================================================================
        // =============== Stage 1, camera rotate help instruction ===============
        if (!animeStage[0])
        {
             
            if(progressionSW.ElapsedMilliseconds < (rotAnimTime * Globals.MILISECOND_IN_SEC))
            {
                // Animate the sprite 
                prorgession = (float)progressionSW.ElapsedMilliseconds /
                (rotAnimTime * Globals.MILISECOND_IN_SEC);
                LeftPos = TrotAnimEndPos.x * Sigmoid(prorgession) +
                Sigmoid(1 - prorgession) * TrotAnimStartPos.x;

                animeSprite.style.left = LeftPos;
            }
            else
            {
                // Fade out the sprite and text 
                prorgession = (float)(progressionSW.ElapsedMilliseconds - 
                    (rotAnimTime * Globals.MILISECOND_IN_SEC)) /
                (fadeTime * Globals.MILISECOND_IN_SEC);
                opacity = Sigmoid(1 - prorgession) * maxOpacity;

                animeSprite.style.opacity = opacity;
                animeText.style.opacity = opacity;

                // Faded out, mark stage 1 as complete
                if(progressionSW.ElapsedMilliseconds > 
                    (rotAnimTime + fadeTime) * Globals.MILISECOND_IN_SEC)
                {
                    animeStage[0] = true;
                    progressionSW.Restart();

                    animeSprite.style.backgroundImage = new StyleBackground(panSprite);
                    animeSprite.style.left = TpanAnimStartPos.x;
                    animeText.text = panDescription;
                }
            }
        }
        // =======================================================================
        // ================= Stage 2, camera pan help instruction ================
        else if (!animeStage[1])
        {
            // Fade in animation
            if (progressionSW.ElapsedMilliseconds < (fadeTime * Globals.MILISECOND_IN_SEC))
            {
                prorgession = progressionSW.ElapsedMilliseconds / 
                    (fadeTime * Globals.MILISECOND_IN_SEC);
                
                opacity = Sigmoid(prorgession) * maxOpacity;

                animeSprite.style.opacity = opacity;
                animeText.style.opacity = opacity;
            }
            // Moving animation
            else if(progressionSW.ElapsedMilliseconds < (fadeTime + panAnimTime) * Globals.MILISECOND_IN_SEC)
            {
                prorgession = (float)(progressionSW.ElapsedMilliseconds - fadeTime * Globals.MILISECOND_IN_SEC)/
                    (panAnimTime * Globals.MILISECOND_IN_SEC);

                LeftPos = panAnimEndPos.x * Sigmoid(prorgession) +
                    Sigmoid(1 - prorgession) * panAnimStartPos.x;
                animeSprite.style.left = LeftPos;
            }
            // Fade out animation
            else if (progressionSW.ElapsedMilliseconds < ((fadeTime*2 + panAnimTime) * Globals.MILISECOND_IN_SEC))
            {
                prorgession = (float)(progressionSW.ElapsedMilliseconds - (fadeTime + panAnimTime) * Globals.MILISECOND_IN_SEC) /
                    (fadeTime * Globals.MILISECOND_IN_SEC);

                opacity = Sigmoid(1 - prorgession) * maxOpacity;

                animeSprite.style.opacity = opacity;
                animeText.style.opacity = opacity;
            }
            // All finished, mark stage 2 as complete
            else
            {
                animeStage[1] = true;
                progressionSW.Restart();

                animeSprite.style.backgroundImage = new StyleBackground(zoomSprite);
                animeText.text = zoomDescription;

                animeSprite.style.left = PINCH_ICON_POS;

                arrowBottomSprite.style.left = TzoomArrow2StartPos.x;
                arrowBottomSprite.style.top = TzoomArrow2StartPos.y;

                arrowTopSprite.style.left = TzoomArrow1StartPos.x;
                arrowTopSprite.style.top = TzoomArrow1StartPos.y;
            }
        }
        // =======================================================================
        // ================ Stage 3, camera zoom help instruction ================
        else if (!animeStage[2])
        {
            // Fade in animation
            if (progressionSW.ElapsedMilliseconds < (fadeTime * Globals.MILISECOND_IN_SEC))
            {
                prorgession = progressionSW.ElapsedMilliseconds /
                    (fadeTime * Globals.MILISECOND_IN_SEC);

                opacity = Sigmoid(prorgession) * maxOpacity;

                animeSprite.style.opacity = opacity;
                animeText.style.opacity = opacity;
                arrowBottomSprite.style.opacity = opacity;
                arrowTopSprite.style.opacity = opacity;

            }
            // Arrow move animation
            else if (progressionSW.ElapsedMilliseconds < (fadeTime + zoomAnimTime) * Globals.MILISECOND_IN_SEC)
            {
                prorgession = (float)(progressionSW.ElapsedMilliseconds - fadeTime * Globals.MILISECOND_IN_SEC) /
                    (zoomAnimTime * Globals.MILISECOND_IN_SEC);
                float SigProgression = Sigmoid(prorgession); 
                float InvSigProgression = 1 - SigProgression;

                float LeftPos1 = TzoomArrow1EndPos.x * SigProgression +
                    InvSigProgression * TzoomArrow1StartPos.x;
                float LeftPos2 = TzoomArrow2EndPos.x * SigProgression +
                    InvSigProgression * TzoomArrow2StartPos.x;
                float TopPos1 = TzoomArrow1EndPos.y * SigProgression +
                    InvSigProgression * TzoomArrow1StartPos.y;
                float TopPos2 = TzoomArrow2EndPos.y * SigProgression +
                    InvSigProgression * TzoomArrow2StartPos.y;

                arrowBottomSprite.style.left = LeftPos2;
                arrowBottomSprite.style.top = TopPos2;

                arrowTopSprite.style.left = LeftPos1;
                arrowTopSprite.style.top = TopPos1;

                // Tint the arrow color
                arrowTopSprite.style.unityBackgroundImageTintColor = Color.white * InvSigProgression +
                    arrowTintColor * SigProgression;
                arrowBottomSprite.style.unityBackgroundImageTintColor = Color.white * InvSigProgression +
                    arrowTintColor * SigProgression;
            }
            // Fade out animation 
            else if (progressionSW.ElapsedMilliseconds < ((fadeTime * 2 + zoomAnimTime) * Globals.MILISECOND_IN_SEC))
            {
                prorgession = (float)(progressionSW.ElapsedMilliseconds - (fadeTime + zoomAnimTime) * Globals.MILISECOND_IN_SEC) /
                    (fadeTime * Globals.MILISECOND_IN_SEC);

                opacity = Sigmoid(1 - prorgession) * maxOpacity;

                animeSprite.style.opacity = opacity;
                animeText.style.opacity = opacity;
                arrowBottomSprite.style.opacity = opacity;
                arrowTopSprite.style.opacity = opacity;
            }
            else
            {
                animeStage[2] = true;
                progressionSW.Stop();

                animeSprite.style.opacity = 0;
                animeText.style.opacity = 0;
                arrowBottomSprite.style.opacity = 0;
                arrowTopSprite.style.opacity = 0;
            }
        }

        // UpdateTouchInterruption();
    }

    /// <summary>
    /// If allow interruption, user touch will end the help animation.
    /// </summary>
    private void UpdateTouchInterruption() 
    {
        // If touch interruption is disabled, skip this method 
        if (!allowTouchInterruption) return;

        if (Globals.activateStates.Contains(touchPrimary.ReadValue<TouchState>().phase))
        {
            // Manually override the stages 
            animeStage = new List<bool> { true, true, true };
            progressionSW.Restart();
        }

    }

    /// <summary>
    /// Ensure all animation spites and UIs are dimmed out
    /// </summary>
    private void UpdateDimmedOutSprites()
    {
        // If it's dimmed out, do nothing 
        if (animeSprite.style.opacity.value <= 0) return;

        float prorgession = progressionSW.ElapsedMilliseconds /
                    (fadeTime * Globals.MILISECOND_IN_SEC);

        float opacity = Sigmoid(1 - prorgession) * maxOpacity;

        animeSprite.style.opacity = opacity;
        animeText.style.opacity = opacity;
        arrowBottomSprite.style.opacity = opacity;
        arrowTopSprite.style.opacity = opacity;

        if(opacity <= 0) 
        {
            progressionSW.Stop();
        }
    }

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

            helpInstructionButton.style.unityBackgroundImageTintColor = defaultTintColor;
        }
        else
        {
            tapHandler.UnselectAll();
            helpButtonSelected = true;
            helpPanelTransitioning = true;
            progressionSW.Restart();

            helpInstructionButton.style.unityBackgroundImageTintColor = iconTintColor;
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

        // Check for additional touch and toggle off the help instruction if
        // the tap is outside of the help instruction display area 
        if (helpButtonSelected && !helpPanelTransitioning 
            && tapProtectionSW.ElapsedMilliseconds > tapProtectionPeriod)
        {
            TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();

            if(currentTouchF1.tapCount != lastTapCount)
            {
                // Check for tap position 
                if(currentTouchF1.position.y > INSTRUCTION_GB_TOP + INSTRUCTION_GB_HEIGHT ||
                    currentTouchF1.position.y < INSTRUCTION_GB_TOP)
                {
                    ToggleHelpInstructionDisplay();
                }
            }

            lastTapCount = currentTouchF1.tapCount;
        }

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
}
