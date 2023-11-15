using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Unity.VisualScripting;
using static UnityEngine.UI.CanvasScaler;

public class UI_Operations : MonoBehaviour
{
    /// ===============================================================
    /// ========================== CONSTANTS ========================== 

    private const float INVISIBLE = 0f;

    private const int COLOR_PICKER_OFFSET = 512;

    private const int CHECKER_CHART_SIZE = 384;  // Width and height of the checker chart

    private const int CHECKER_CHART_MARGIN = 64; // Margin of the checker chart 

    // The dropper is an image whose origin is not the center.
    // This is used to correct its location so that the center follows touch control. 
    private const int DROPPER_OFFSET = 32;

    // This is purely a magical number obtained by observation.
    // The y position of the dropper circle when translated from world
    // to local has a bias of roughly 90 pixels.
    // I assume this has something to do with 32 + 64, despite this
    // combination makes no sense, it works, so here it is. 
    private const int DROPPER_TRANSFORM_OFFSET = 96;

    // This space is to move the UI down to make what's underneath this one selectable.
    // Otherwise, this UI will cover everthing and render other layers useless. 
    private const int TOP_OFFSET = 140; 

    /// ===============================================================
    /// ==================== Serialized variables =====================  

    [SerializeField] TapHandler tapHandler;

    [SerializeField] CameraControl meinCamera; // Das war ein befehl!

    [Space(15)]
    [Header("Animations")]
    [Space(10)]

    [Tooltip("The speed of which the elements fade in and out")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeSpeed = 1.0f;

    [Tooltip("The maxmium opacity of elements. 1 is completely opaque.")]
    [Range(0f, 1f)]
    [SerializeField] private float maxOpacity = .9f;

    [Tooltip("For developers only. This threshold defines the value that will be "+
        "neglected if 2 opacity values' difference is below this.")]
    [Range(0f, .1f)]
    [SerializeField] private float opacityThreshold = .02f;

    [Tooltip("Total time of the fly-in animation")]
    [SerializeField] private float flyInTime = 1f;

    [Tooltip("The start and end position of the setting panel flying from left")]
    [SerializeField] private Vector2 settingFlyInDistance = Vector2.zero;

    [Tooltip("The start and end position of the text description panel flying from left")]
    [SerializeField] private Vector2 textFlyInDistace = Vector2.zero;

    [Tooltip("The start and end position of panels flying from right")]
    [SerializeField] private Vector2 rightFlyInDistance = Vector2.zero;

    [Space(15)]
    [Header("Display and sprites")]
    [Space(10)]

    [Tooltip("Icons tint to this color when selected")]
    [SerializeField] private Color highlightColor = new Color();

    [Tooltip("The default checkbox sprite")]
    [SerializeField] private Sprite checkBoxDefault;

    [Tooltip("Checkbox sprite when the option is checked")]
    [SerializeField] private Sprite checkBoxChecked;

    [Tooltip("Color picker panel for selecting color")]
    [SerializeField] private Sprite colorPcikerTexture;

    [Tooltip("Background panel image for when fill light is selected")]
    [SerializeField] private Sprite fillSelectedBG;

    [Tooltip("Background panel image for when rim light is selected")]
    [SerializeField] private Sprite rimSelectedBG;

    [SerializeField] private Color defaultFontColor;

    [SerializeField] private Color dimFontColor; 

    [Tooltip("When enabled, the dropper moves continuously with touch")]
    [SerializeField] private bool allowContinuousPick = true; 

    /// ===============================================================
    /// ======================= Input actions =========================
    private InputAction touchPrimary;

    /// ===============================================================
    /// ======================== Stat variables =======================

    private VisualElement root;

    private GroupBox optionsGB; 
    private Button settingsButton;
    private Button textButton;
    private Button appearanceButton;

    private GroupBox settingsGB;
    private GroupBox appearanceGB;
    private GroupBox TextDescriptionGB;

    private Button disableRotateButton;
    private Button disableVoiceoverButton;
    private Slider touchSensSlider;
    private Button resetButton;

    private Slider selectedOpaSlider;
    private Slider unselectedOpaSlider;
    private Button colorPickerButton;
    private Button selectFillButton;
    private Button selectRimButton;
    private IMGUIContainer pickerIcon;

    private Label objectTitle;
    private Label objectDescription;
    private Button playAudioButton;

    private AudioClip currentAudio; // Audio for the currently selected object, if there are any

    private bool disableAutoRotate = false;
    private bool disableVoiceover = false;

    private float currentOpacity;  // Opacity for the base visual element 

    // Left and right position during transitional animtions
    private float leftPos;    
    private float rightPos;

    // Bool flags for transitional animtions
    private bool settingTransitioning = false;
    private bool textTransitioning = false;
    private bool rightTransitioning = false;

    // Stopwatches for each panel's transitional animtions
    private Stopwatch settingTransSW = new Stopwatch();
    private Stopwatch textTransSW = new Stopwatch();
    private Stopwatch rightTransSW = new Stopwatch();

    private bool playSoundPressed = false; // Indicates a session of tint animation 
    private int playSoundTintTime = 2000;  // Play sound iconis highlighted and slowly change back
    private Stopwatch playSoundTintSW = new Stopwatch();

    private bool resetButtonPressed = false;
    private int resetButtonTintTime = 1000;
    private Stopwatch resetButtonTintSW = new Stopwatch();

    private bool isInFillSelect = true; 

    // Position of the rim color dropper in percentage location of the color chart 
    private Vector2 rimColorPosition = new Vector2(.5f, .5f);

    // Position of the fill color dropper in percentage location of the color chart 
    private Vector2 fillColorPosition = new Vector2(0f, 0f);

    // Dict recording the stats of the UIs, whether or not they are active. Initilized in `Start()`
    private Dictionary<Globals.UIElements, bool> activeUI = new Dictionary<Globals.UIElements, bool>();

    private bool initialUpdated = false;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        optionsGB = root.Q<GroupBox>("OptionPanel"); 
        settingsButton = root.Q<Button>("Settings");
        textButton = root.Q<Button>("Text");
        appearanceButton = root.Q<Button>("Appearance");

        settingsGB = root.Q<GroupBox>("Settings");
        appearanceGB = root.Q<GroupBox>("VisualSettings");
        TextDescriptionGB = root.Q<GroupBox>("TextDescription");

        disableRotateButton = settingsGB.Q<Button>("DisAutoRotToggle");
        disableVoiceoverButton = settingsGB.Q<Button>("DisVoiceToggle");
        touchSensSlider = settingsGB.Q<Slider>("TouchSensSlider");
        resetButton = settingsGB.Q<Button>("ResetButton");

        resetButton.clicked += () => ResetClicked(); 

        objectTitle = TextDescriptionGB.Q<Label>("Title");
        objectDescription = TextDescriptionGB.Q<Label>("Description");
        playAudioButton = TextDescriptionGB.Q<Button>("PlaySoundButton"); 

        settingsButton.clicked += () => ToggleSettingsGB();
        textButton.clicked += () => ToggleTextGB();
        appearanceButton.clicked += () => ToggleAppearanceGB();

        disableRotateButton.clicked += () => ToggleAutoRotate();
        disableVoiceoverButton.clicked += () => ToggleVoiceOver();

        playAudioButton.clicked += () => ManuallyPlayAudiop(); 

        selectedOpaSlider = appearanceGB.Q<Slider>("SelectedOpacitySlider");
        unselectedOpaSlider = appearanceGB.Q<Slider>("UnselectedOpacitySlider");
        colorPickerButton = appearanceGB.Q<Button>("ColorPicker");
        pickerIcon = appearanceGB.Q<IMGUIContainer>("PickerIcon");
        selectFillButton = appearanceGB.Q<IMGUIContainer>("RimFillSelector").Q<Button>("SelectFill");
        selectRimButton = appearanceGB.Q<IMGUIContainer>("RimFillSelector").Q<Button>("SelectRim");

        colorPickerButton.clicked += () => ColorPicking();
        selectFillButton.clicked += () => SelectFillColor();
        selectRimButton.clicked += () => SelectRimColor();

        
    }

    public void Initialize(InputAction PT)
    {
        touchPrimary = PT;
    }

    // Start is called before the first frame update
    void Start()
    {
        

        activeUI = new Dictionary<Globals.UIElements, bool>() {
            { Globals.UIElements.Options, false },
            { Globals.UIElements.Settings, false },
            { Globals.UIElements.Text, false },
            { Globals.UIElements.Appearance, false }
        };

        currentOpacity = 0;
        root.style.opacity = currentOpacity;

        settingsGB.style.opacity = 0;
        appearanceGB.style.opacity = 0;
        TextDescriptionGB.style.opacity = 0;
        TextDescriptionGB.style.left = textFlyInDistace.x; 

        leftPos = settingFlyInDistance.x; 
        rightPos = rightFlyInDistance.x;

        colorPickerButton.style.top = COLOR_PICKER_OFFSET;
        colorPickerButton.style.width = CHECKER_CHART_SIZE;
        colorPickerButton.style.height = CHECKER_CHART_SIZE;
        colorPickerButton.style.marginBottom = CHECKER_CHART_MARGIN;
        colorPickerButton.style.marginTop = CHECKER_CHART_MARGIN;
        colorPickerButton.style.marginLeft = CHECKER_CHART_MARGIN;
        colorPickerButton.style.marginRight = CHECKER_CHART_MARGIN;
        colorPickerButton.style.backgroundImage = new StyleBackground(colorPcikerTexture);

        SelectFillColor();
        SelectRimColor();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!initialUpdated)
        CalculateLocations();
        

        UpdateVisbility();
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

        root.style.width = screenWidth;
        root.style.height = screenHeight;
        root.style.top = TOP_OFFSET;

        optionsGB.style.left = screenWidth / 2 - (int)optionsGB.resolvedStyle.width / 2;
        optionsGB.style.top = screenHeight - (int)optionsGB.resolvedStyle.height * 2 - TOP_OFFSET;

        int leftPos = (screenHeight - (int)settingsGB.resolvedStyle.height) / 2 - TOP_OFFSET * 2;
        settingsGB.style.top = leftPos;
        int rightPos = (screenHeight - (int)appearanceGB.resolvedStyle.height) / 2 - TOP_OFFSET * 2;
        appearanceGB.style.top = rightPos;
        int txtPos = (screenHeight - (int)TextDescriptionGB.resolvedStyle.height) / 2 - TOP_OFFSET * 2;
        TextDescriptionGB.style.top = txtPos > 0 ? txtPos : 0;

        meinCamera.SetLeftRightProtectArea((int)settingsGB.resolvedStyle.width, 
            screenWidth - (int)appearanceGB.resolvedStyle.width);

        // The first several updates, for some reason, returns NaN (shouldn't Start and OnEnable be called first?).
        // So only after the values become positive will the initial update be marked as complete. 
        if (optionsGB.resolvedStyle.height > 0)
            initialUpdated = true;
    }

    /// <summary>
    /// Update the opacity of the entire visual element, including
    /// the setting, the text, and the appearance panel. 
    /// If nothing is selected, then none of these UI will show up. 
    /// </summary>
    private void UpdateVisbility()
    {
        if (tapHandler.HasSelected())
        {
            // When something is selected, fade in the UIs
            currentOpacity += fadeSpeed * Time.deltaTime;
            if (currentOpacity >= maxOpacity - opacityThreshold)
            {
                currentOpacity = maxOpacity;
                tapHandler.ProtectBottomArea();
            }
        }
        else
        {
            // If nothing is slected, fade out the UIs 
            currentOpacity -= fadeSpeed * Time.deltaTime;
            if (currentOpacity <= INVISIBLE + opacityThreshold)
            {
                currentOpacity = INVISIBLE;
                tapHandler.FreeBottomArea();
            }
        }

        root.style.opacity = currentOpacity;

        UpdateSettingsGB();
        UpdateAppearanceGB();
        UpdateTextGB();

        
    }

    /// <summary>
    /// Update the animation and display of the setting panel 
    /// </summary>
    private void UpdateSettingsGB()
    {
        // Fly in animation and fade in 
        if (activeUI[Globals.UIElements.Settings])
        {
            // if animation period is over, mark its finish
            if (settingTransSW.ElapsedMilliseconds >= flyInTime * Globals.MILISECOND_IN_SEC 
                && settingTransitioning)
            {
                settingsGB.style.opacity = maxOpacity;
                settingsGB.style.left = (int)settingFlyInDistance.y;
                tapHandler.ProtectLeftArea();
                meinCamera.ProtectLeftArea();

                settingTransitioning = false;
            }
            // During the animation period, update opacity and position
            if (settingTransSW.ElapsedMilliseconds < flyInTime * Globals.MILISECOND_IN_SEC 
                && settingTransitioning)
            {
                float progression = Sigmoid(settingTransSW.ElapsedMilliseconds / (flyInTime * Globals.MILISECOND_IN_SEC));

                leftPos = settingFlyInDistance.x + (settingFlyInDistance.y - settingFlyInDistance.x) * progression;

                settingsGB.style.opacity = progression * maxOpacity;
                settingsGB.style.left = (int)leftPos;
            }
        }

        // Fly out animation and fade out 
        if (!activeUI[Globals.UIElements.Settings])
        {
            // if animation period is over, mark its finish
            if (settingTransSW.ElapsedMilliseconds >= flyInTime * Globals.MILISECOND_IN_SEC 
                && settingTransitioning)
            {
                settingsGB.style.opacity = 0;
                settingsGB.style.left = (int)settingFlyInDistance.x;

                // If no other elements are active, free the area 
                if (!activeUI[Globals.UIElements.Text])
                {
                    tapHandler.FreeLeftArea();
                    meinCamera.FreeLeftArea();
                }
                    

                settingTransitioning = false;
            }
            // During the animation period, update opacity and position
            if (settingTransSW.ElapsedMilliseconds < flyInTime * Globals.MILISECOND_IN_SEC 
                && settingTransitioning)
            {
                float progression = Sigmoid(settingTransSW.ElapsedMilliseconds / (flyInTime * Globals.MILISECOND_IN_SEC));

                leftPos = settingFlyInDistance.y + (settingFlyInDistance.x - settingFlyInDistance.y) * progression;

                settingsGB.style.opacity = maxOpacity - progression * maxOpacity;
                settingsGB.style.left = (int)leftPos;
            }
        }

        // This slider only shows up with setting panel
        // So update is only performed within 
        UpdateTouchSensitivity();

        UpdateRersetButton();
    }

    /// <summary>
    /// Turn on or off the setting group box. 
    /// Starts the fly in/out and fade animation. 
    /// </summary>
    private void ToggleSettingsGB()
    {
        // Turn off setting panel 
        if (activeUI[Globals.UIElements.Settings])
        {
            activeUI[Globals.UIElements.Settings] = false;
            if (!settingTransitioning)
            {
                settingsButton.style.unityBackgroundImageTintColor = Color.white;
                settingTransitioning = true;
                settingTransSW.Restart();
            }
        }
        // Turn on setting panel 
        else
        {
            // Text and setting occupy the same space and are thus exclusive.
            // Turn off one when the other is active 
            if (activeUI[Globals.UIElements.Text])
                ToggleTextGB();

            activeUI[Globals.UIElements.Settings] = true;
            if (!settingTransitioning)
            {
                settingsButton.style.unityBackgroundImageTintColor = highlightColor;
                settingTransitioning = true;
                settingTransSW.Restart();
            }
        }
    }


    /// <summary>
    /// Update the animation and display of the appearance control panel 
    /// </summary>
    private void UpdateAppearanceGB()
    {

        // Fly in animation and fade in 
        if (activeUI[Globals.UIElements.Appearance])
        {
            // if animation period is over, mark its finish
            if (rightTransSW.ElapsedMilliseconds >= flyInTime * Globals.MILISECOND_IN_SEC && rightTransitioning)
            {
                appearanceGB.style.opacity = maxOpacity;
                appearanceGB.style.right = (int)rightFlyInDistance.y;
                tapHandler.ProtectRightArea();
                meinCamera.ProtectRightArea();

                rightTransitioning = false;
            }
            // During the animation period, update opacity and position
            if (rightTransSW.ElapsedMilliseconds < flyInTime * Globals.MILISECOND_IN_SEC && rightTransitioning)
            {
                float progression = Sigmoid((float)rightTransSW.ElapsedMilliseconds / (flyInTime * Globals.MILISECOND_IN_SEC));

                rightPos = rightFlyInDistance.x + (rightFlyInDistance.y - rightFlyInDistance.x) * progression;

                appearanceGB.style.opacity = progression * maxOpacity;
                appearanceGB.style.right = (int)rightPos;
            }
        }

        // Fly out animation and fade out 
        if (!activeUI[Globals.UIElements.Appearance])
        {
            // if animation period is over, mark its finish
            if (rightTransSW.ElapsedMilliseconds >= flyInTime * Globals.MILISECOND_IN_SEC && rightTransitioning)
            {
                appearanceGB.style.opacity = 0;
                appearanceGB.style.right = (int)rightFlyInDistance.x;
                tapHandler.FreeRightArea();
                meinCamera.FreeRightArea();

                rightTransitioning = false;
            }
            // During the animation period, update opacity and position
            if (rightTransSW.ElapsedMilliseconds < flyInTime * Globals.MILISECOND_IN_SEC && rightTransitioning)
            {
                float progression = Sigmoid((float)rightTransSW.ElapsedMilliseconds / (flyInTime * Globals.MILISECOND_IN_SEC));

                rightPos = rightFlyInDistance.y + (rightFlyInDistance.x - rightFlyInDistance.y) * progression;

                appearanceGB.style.opacity = maxOpacity - progression * maxOpacity;
                appearanceGB.style.right = (int)rightPos;
            }
        }

        tapHandler.SetSelectedOpacity(selectedOpaSlider.value);
        tapHandler.SetUnselectedOpacity(unselectedOpaSlider.value);

        UpdateContinuousPick();
    }

    /// <summary>
    /// Turn on or off the appearance group box. 
    /// Starts the fly in/out and fade animation. 
    /// </summary>
    private void ToggleAppearanceGB()
    {
        // Turn off setting panel 
        if (activeUI[Globals.UIElements.Appearance])
        {
            activeUI[Globals.UIElements.Appearance] = false;
            if (!rightTransitioning)
            {
                appearanceButton.style.unityBackgroundImageTintColor = Color.white;
                rightTransitioning = true;
                rightTransSW.Restart();
            }
        }
        // Turn on setting panel 
        else
        {
            activeUI[Globals.UIElements.Appearance] = true;
            if (!rightTransitioning)
            {
                appearanceButton.style.unityBackgroundImageTintColor = highlightColor;
                //settingsButton.style.backgroundImage = new StyleBackground(background);
                rightTransitioning = true;
                rightTransSW.Restart();
            }
        }
    }

    /// <summary>
    /// Enable or disable auto-rotate animation of the camera.
    /// </summary>
    private void ToggleAutoRotate()
    {
        disableAutoRotate = !disableAutoRotate;

        meinCamera.ToggleAutoRotate(disableAutoRotate);

        if(disableAutoRotate )
        {
            disableRotateButton.style.backgroundImage = new StyleBackground(checkBoxChecked);

        }
        else
        {
            disableRotateButton.style.backgroundImage = new StyleBackground(checkBoxDefault);
        }
    }

    /// <summary>
    /// Enable or disable the automatic voiceover when clicking on text description 
    /// </summary>
    private void ToggleVoiceOver()
    {
        disableVoiceover = !disableVoiceover;

        if (disableVoiceover)
        {
            disableVoiceoverButton.style.backgroundImage = new StyleBackground(checkBoxChecked);

        }
        else
        {
            disableVoiceoverButton.style.backgroundImage = new StyleBackground(checkBoxDefault);
        }
    }

    /// <summary>
    /// Read the touch sensitivity value and send it to the camera
    /// </summary>
    private void UpdateTouchSensitivity()
    {
        meinCamera.SetTouchSensitivity(touchSensSlider.value);
    }

    /// <summary>
    /// Reset the settings 
    /// </summary>
    private void ResetClicked()
    {
        if (disableAutoRotate)
            ToggleAutoRotate();

        if (disableVoiceover)
            ToggleVoiceOver();

        meinCamera.ExternalReset();
        resetButtonPressed = true;
        resetButtonTintSW.Restart();
    }

    /// <summary>
    /// Updating the color of the reset button after it is pressed. 
    /// </summary>
    private void UpdateRersetButton()
    {
        // Update the tint color of the play icon 
        if (resetButtonPressed && resetButtonTintSW.ElapsedMilliseconds < resetButtonTintTime)
        {
            // After stopwatch counts beyond resetButtonTintTime, calculation stops and thus
            // does not need to judge whether or not the color goes beyond 1
            float progression = Sigmoid((float)resetButtonTintSW.ElapsedMilliseconds / resetButtonTintTime);
            Color currentColor = progression * Color.white + (1 - progression) * highlightColor;

            resetButton.style.unityBackgroundImageTintColor = currentColor;
        }
    }

    /// <summary>
    /// Turn on or off the text description panel. 
    /// </summary>
    private void ToggleTextGB()
    {
        // Turn off text panel 
        if (activeUI[Globals.UIElements.Text])
        {
            activeUI[Globals.UIElements.Text] = false;
            if (!textTransitioning)
            {
                textButton.style.unityBackgroundImageTintColor = Color.white;
                textTransitioning = true;
                textTransSW.Restart();
            }
        }
        // Turn on text panel 
        else
        {
            // Text and setting occupy the same space and are thus exclusive.
            // Turn off one when the other is active 
            if (activeUI[Globals.UIElements.Settings])
                ToggleSettingsGB();

            RefreshTexts();

            // If auto-play is not disabled, then play the audio once the text panel shows up
            if (!disableVoiceover) 
            {
                ManuallyPlayAudiop(); 
            }

            activeUI[Globals.UIElements.Text] = true;
            if (!textTransitioning)
            {
                textButton.style.unityBackgroundImageTintColor = highlightColor;
                textTransitioning = true;
                textTransSW.Restart();
            }
        }
    }

    /// <summary>
    /// Text panel animation and options. 
    /// </summary>
    private void UpdateTextGB()
    {
        // Fly in animation and fade in 
        if (activeUI[Globals.UIElements.Text])
        {
            // if animation period is over, mark its finish
            if (textTransSW.ElapsedMilliseconds >= flyInTime * Globals.MILISECOND_IN_SEC 
                && textTransitioning)
            {
                TextDescriptionGB.style.opacity = maxOpacity;
                TextDescriptionGB.style.left = (int)textFlyInDistace.y;
                tapHandler.ProtectLeftArea();

                textTransitioning = false;
            }
            // During the animation period, update opacity and position
            if (textTransSW.ElapsedMilliseconds < flyInTime * Globals.MILISECOND_IN_SEC 
                && textTransitioning)
            {
                float progression = Sigmoid(textTransSW.ElapsedMilliseconds / (flyInTime * Globals.MILISECOND_IN_SEC));

                leftPos = textFlyInDistace.x + (textFlyInDistace.y - textFlyInDistace.x) * progression;

                TextDescriptionGB.style.opacity = progression * maxOpacity;
                TextDescriptionGB.style.left = (int)leftPos;
            }
        }

        // Fly out animation and fade out 
        if (!activeUI[Globals.UIElements.Text])
        {
            // if animation period is over, mark its finish
            if (textTransSW.ElapsedMilliseconds >= flyInTime * Globals.MILISECOND_IN_SEC 
                && textTransitioning)
            {
                TextDescriptionGB.style.opacity = 0;
                TextDescriptionGB.style.left = (int)textFlyInDistace.x;

                // If no other elements are active, free the area 
                if (!activeUI[Globals.UIElements.Settings]) 
                    tapHandler.FreeLeftArea();

                textTransitioning = false;
            }
            // During the animation period, update opacity and position
            if (textTransSW.ElapsedMilliseconds < flyInTime * Globals.MILISECOND_IN_SEC 
                && textTransitioning)
            {
                float progression = Sigmoid(textTransSW.ElapsedMilliseconds / (flyInTime * Globals.MILISECOND_IN_SEC));

                leftPos = textFlyInDistace.y + (textFlyInDistace.x - textFlyInDistace.y) * progression;

                TextDescriptionGB.style.opacity = maxOpacity - progression * maxOpacity;
                TextDescriptionGB.style.left = (int)leftPos;
            }
        }

        // Update the tint color of the play icon 
        if (playSoundPressed && playSoundTintSW.ElapsedMilliseconds < playSoundTintTime)
        {
            // After stopwatch counts beyond playSoundTintTime, calculation stops and thus
            // does not need to judge whether or not the color goes beyond 1
            float progression = Sigmoid((float)playSoundTintSW.ElapsedMilliseconds / playSoundTintTime);
            Color currentColor = progression * Color.white + (1 - progression) * highlightColor;

            playAudioButton.style.unityBackgroundImageTintColor = currentColor;
        }

        if (tapHandler.newlySelected)
        {
            RefreshTexts();
        }
    }

    /// <summary>
    /// Before text panel shows up, refresh the text based on what is selected.
    /// Also sync the audio clips to be played. 
    /// </summary>
    private void RefreshTexts()
    {
        string title = tapHandler.GetTitle();
        string description = tapHandler.GetTextDescription();

        objectTitle.text = title;
        objectDescription.text = description;

        currentAudio = tapHandler.GetVoiceOver();
    }

    /// <summary>
    /// If auto play audio is disabled, this button will allow the user
    /// to play audio by manually clicking the button. 
    /// </summary>
    private void ManuallyPlayAudiop()
    {
        if (currentAudio != null)
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = currentAudio;
            audio.Play();

            // Start a tint color animation session 
            playSoundPressed  = true;
            playSoundTintSW.Restart();

            // Change the tint color of the icon to highlight press event 
            playAudioButton.style.unityBackgroundImageTintColor = highlightColor;
        }
    }

    /// <summary>
    /// Toggle the fake radio button options to fill color
    /// </summary>
    private void SelectFillColor()
    {
        appearanceGB.style.backgroundImage = new StyleBackground(fillSelectedBG);
        selectFillButton.style.color = defaultFontColor;
        selectRimButton.style.color = dimFontColor; 

        isInFillSelect = true;

        pickerIcon.style.left = fillColorPosition.x;
        pickerIcon.style.top = fillColorPosition.y;

        AlterColor(fillColorPosition);
    }

    /// <summary>
    /// Toggle the fake radio button options to rim color
    /// </summary>
    private void SelectRimColor()
    {
        appearanceGB.style.backgroundImage = new StyleBackground(rimSelectedBG);
        selectFillButton.style.color = dimFontColor;
        selectRimButton.style.color = defaultFontColor;

        isInFillSelect = false;

        pickerIcon.style.left = rimColorPosition.x;
        pickerIcon.style.top = rimColorPosition.y;

        AlterColor(rimColorPosition);
    }

    /// <summary>
    /// Button function for color picking.
    /// This is either called only once every time the user touches the picker chart,
    /// or called continuously while the user is dragging around in the picker chart. 
    /// </summary>
    private void ColorPicking()
    {
        TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();

        // Convert from world space to the button's local space 
        Vector2 localPos = colorPickerButton.WorldToLocal(
            new Vector2(currentTouchF1.position.x, currentTouchF1.position.y));

        // Local space's y is inverted and is biased due to the button margin. 
        // This invert the sign and offset the effect of the margin.
        Vector2 translated = new Vector2(localPos.x, -localPos.y - DROPPER_TRANSFORM_OFFSET);

        AlterColor(translated / CHECKER_CHART_SIZE);
    }

    /// <summary>
    /// Check is continuous picking is allowed and perform related tasks
    /// </summary>
    private void UpdateContinuousPick()
    {
        // If continuous picking is disabled or the appearance panel is not active
        // quit directly 
        if (!allowContinuousPick || !activeUI[Globals.UIElements.Appearance]) return;

        TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();
        Vector2 localPos = colorPickerButton.WorldToLocal(
            new Vector2(currentTouchF1.position.x, currentTouchF1.position.y));

        // If it's in the picker chart, then update the color
        if(localPos.x > 0 && localPos.x < CHECKER_CHART_SIZE &&
            localPos.y < -DROPPER_TRANSFORM_OFFSET && localPos.y > -(DROPPER_TRANSFORM_OFFSET + CHECKER_CHART_SIZE))
        {
            ColorPicking();
        }
    }

    /// <summary>
    /// Alter the color of the selected parts and update dropper position. 
    /// </summary>
    /// <param name="precentLocation"></param>
    private void AlterColor(Vector2 precentLocation)
    {
        Texture2D bgImage = textureFromSprite(colorPcikerTexture);

        int pixelX = (int)(precentLocation.x * bgImage.width);
        int pixelY = (int)(precentLocation.y * bgImage.height);


        Color selectedColor = bgImage.GetPixel(pixelX, bgImage.height - pixelY) ;

        Vector2 updatedPos = new Vector2(
            precentLocation.x * CHECKER_CHART_SIZE
            - DROPPER_OFFSET + CHECKER_CHART_MARGIN, 
            precentLocation.y * CHECKER_CHART_SIZE
            - DROPPER_OFFSET + COLOR_PICKER_OFFSET + CHECKER_CHART_MARGIN);

        // Move the icon to the selected location 
        pickerIcon.style.left = updatedPos.x;
        pickerIcon.style.top = updatedPos.y;

        pickerIcon.style.unityBackgroundImageTintColor = selectedColor;

        // Save the icon location and change the corresponding shader color 
        if(isInFillSelect)
        {
            fillColorPosition = precentLocation;
            tapHandler.SetFillColor(selectedColor);
        }
        else
        {
            rimColorPosition = precentLocation;
            tapHandler.SetRimColor(selectedColor);
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

    /// <summary>
    /// Convert a sprite to texture 2D for color lookup. 
    /// </summary>
    /// <param name="sprite">Source sprite</param>
    /// <returns>Generate texture2D object</returns>
    private static Texture2D textureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
            return sprite.texture;
    }



}
