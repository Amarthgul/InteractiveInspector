using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using static UnityEditor.Experimental.GraphView.Port;

public class MainMenu : MonoBehaviour
{

    /// ===============================================================
    /// ========================== CONSTANTS ========================== 
    private const int NUM_OF_BOX = 18;

    private const int TITLE_BOX_INDEX = 0;

    // The index of the boxes that are directly next to the title box
    private static List<int> firstLayer = new List<int>() {
        2, 3, 5, 6, 7, 8, 9, 10
        };

    /// ===============================================================
    /// ==================== Serialized variables ===================== 

    [Tooltip("Amount of time in seconds to play the initial fade in animation")]
    [SerializeField] private float fadeInTime = 2f;

    [Tooltip("Title box appear first, followed by other boxes. " +
        "This decides the amount of time after which the rest of the boxes " +
        "took started to emerge")]
    [SerializeField] private float followUpRatio = .4f;

    [Tooltip("At maximum, the random fade in delay can be this long")]
    [SerializeField]  private float maxRandomMiliSec = .5f;

    [Tooltip("The maximum opacity that the panel boxes can have")]
    [SerializeField] private float maxOpacity = 1f;

    /// ===============================================================
    /// ========================= UI Elements =========================
    private VisualElement root;

    private List<GroupBox> optionGroup = new List<GroupBox>();



    /// ===============================================================
    /// ======================= Stat variables ========================

    // Each panel box (except the title box) has a slight delay in fade in
    // animation, generated randomly, to avoid them all appearing at once. 
    private List<int> fadeInDelay = new List<int>();

    // Prorgession of the fade in animation for each box in [0, 1]
    private List<float> fadeInProgression = new List<float>();

    // Bool flag to mark those the finishing of fade in animations of eac box
    private List<bool> fadeInFinished = new List<bool>();

    private Stopwatch intialFadeInSW = new Stopwatch();


    // Since this is the menu, when first starting up, the program may take some
    // time to load. Thus a warm up is introduced to delay the execution of codes
    private Stopwatch warmUpSW = new Stopwatch();
    private float warmUpTime = 1.5f;
    private bool warmUpFinished = false; 

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        for (int i = 0; i < NUM_OF_BOX; i++)
        {
            optionGroup.Add(root.Q<GroupBox>("Box" + (1+i)));
        }

        for (int i = 0; i < NUM_OF_BOX; i++)
        {
            // Hide all the boxes 
            optionGroup[i].style.opacity = 0;

            // Generate a random delay for them 
            fadeInDelay.Add((int)(Globals.RNG.NextDouble() * maxRandomMiliSec * Globals.MILISECOND_IN_SEC));
            // Mark the box fade in as unfinished 
            fadeInFinished.Add(false);
            // Make all progressions 0 as haven't begin yet 
            fadeInProgression.Add(0f);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        AssignPanels();
        SyncTheme();

        warmUpSW.Start();

    }


    // Update is called once per frame
    void Update()
    {

        if (warmUpFinished)
        {
            UpdateInitialAnimation();
        }
        else
        {
            WarmUpDelay();
        }

        

    }


    /// <summary>
    /// Assign each panel box to a functionality. 
    /// </summary>
    private void AssignPanels()
    {
        optionGroup[1].Q<Label>("LabelTxt").text = "This is a test"; 
    }


    /// <summary>
    /// Initial fade in animation
    /// </summary>
    private void UpdateInitialAnimation()
    {

        float titleProgression = intialFadeInSW.ElapsedMilliseconds / (fadeInTime * Globals.MILISECOND_IN_SEC);

        // Title box 
        if (titleProgression < 1f && !fadeInFinished[TITLE_BOX_INDEX]) 
        {
            float opacity = Sigmoid(titleProgression);
            optionGroup[TITLE_BOX_INDEX].style.opacity = opacity;
        }
        else
        {
            optionGroup[TITLE_BOX_INDEX].style.opacity = maxOpacity;
            fadeInFinished[TITLE_BOX_INDEX] = true;
        }

        // Peripheral boxes' fade in aniamtion 
        if (intialFadeInSW.ElapsedMilliseconds > (followUpRatio * Globals.MILISECOND_IN_SEC))
        {
            for (int i = 0; i < optionGroup.Count; i++)
            {
                // Ignore the title box and those who have finished 
                if (i == TITLE_BOX_INDEX || fadeInFinished[i]) continue;

                float currentElapsed = intialFadeInSW.ElapsedMilliseconds - followUpRatio * Globals.MILISECOND_IN_SEC - fadeInDelay[i];
                if (currentElapsed > 0f)
                {
                    fadeInProgression[i] = currentElapsed / (fadeInTime * Globals.MILISECOND_IN_SEC);
                    optionGroup[i].style.opacity = fadeInProgression[i];
                }
                if (fadeInProgression[i] > 1f)
                {
                    fadeInFinished[i] = true;
                    optionGroup[i].style.opacity = maxOpacity;
                }
                
            }

        }

    }


    /// <summary>
    /// Dealy the execution of codes by counting a timer, giving time for the entire program
    /// to warm up and have every parts properly registered into the memory. 
    /// </summary>
    private void WarmUpDelay()
    {
        if(warmUpSW.ElapsedMilliseconds > warmUpTime * Globals.MILISECOND_IN_SEC) 
        {
            warmUpSW.Stop();
            warmUpFinished = true;
            intialFadeInSW.Start();
        }
    }

    /// <summary>
    /// Read the current setting of application theme (dark or light theme),
    /// and change the UI elements to the corrresponding theme. 
    /// </summary>
    private void SyncTheme()
    {
        if (Globals.lightModeOn)
        {
            for (int i = 0; i < optionGroup.Count; i++)
            {
                optionGroup[i].style.unityBackgroundImageTintColor = Globals.lightModeUITint;
            }
        }
        else
        {
            for (int i = 0; i < optionGroup.Count; i++)
            {
                optionGroup[i].style.unityBackgroundImageTintColor = Globals.darkModeUITint; 
            }

        }


    }

    /// <summary>
    /// A sigmoid function with offset, roughly map input [0, 1] to output (0, 1)
    /// to simulate the slow-in slow-out animation curve effect. 
    /// </summary>
    /// <param name="valueX">Input X value, ideally in [0, 1]</param>
    /// <returns>A value in [0, 1]</returns>
    private float Sigmoid(float valueX)
    {
        return 1 / (1 + Mathf.Exp(-10 * (valueX - 0.5f)));
    }
}
