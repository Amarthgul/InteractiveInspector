using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{

    /// <summary>
    /// Generally, the app try to accommodate both touch and mouse/keyboard control.
    /// But at times, there may be cases where only 1 type of contorl can be allowed,
    /// this toggle is for the decision of which one takes priority. 
    /// Remember to toggle this when building for different platforms. 
    /// </summary>
    public static bool webMode = true;

    /// <summary>
    /// A gloabl flag to save the effort of rewritting all the log message into the build
    /// for testing the disgusting difference between Game window and actual brower/app. 
    /// </summary>
    public static bool DEBUGGING = true;

    /// <summary>
    /// The ratio between logical resolution and physical/screen resoultion 
    /// </summary>
    public static Vector2 _scalingRatio = new Vector2(); 

    /// <summary>
    /// Light mode changes the UIs to white and font/icons to black. 
    /// </summary>
    public static bool lightModeOn = false;

    public static Color lightModeUITint = Color.white;
    public static Color lightModeTxtTint = new Color(.1f, .1f, .1f);

    public static Color darkModeUITint = new Color(.1f, .1f, .1f);
    public static Color darkModeTxtTint = new Color(.9f, .9f, .9f);


    // 1000 milisecond in a second, used to convert second to milisecond 
    public const int MILISECOND_IN_SEC = 1000;


    /// <summary>
    /// Resolution of the iPad Air 4th gen.
    /// Since the app was originally developed and tested on this device,
    /// much of the dimensions was specified for this resolution. As such,
    /// some later code uses this dimension as a reference to scale the UI.
    /// </summary>
    public static Vector2 iPadAir4thGen = new Vector2(1640, 2360);

    
    /// <summary>
    /// The list of states for a touch session that would be treated as inactive,
    /// that is, the user is not making active interaction with the application.
    /// </summary>
    public static List<UnityEngine.InputSystem.TouchPhase> deactivatedStats = 
        new List<UnityEngine.InputSystem.TouchPhase>()
    {
        UnityEngine.InputSystem.TouchPhase.Canceled,
        UnityEngine.InputSystem.TouchPhase.Ended,
        UnityEngine.InputSystem.TouchPhase.None
    };

    /// <summary>
    /// The list of states for a touch session that would be treated as active,
    /// that is, the user is actively making interactions with the application.
    /// </summary>
    public static List<UnityEngine.InputSystem.TouchPhase> activateStates = 
        new List<UnityEngine.InputSystem.TouchPhase>() 
    {
        UnityEngine.InputSystem.TouchPhase.Began,
        UnityEngine.InputSystem.TouchPhase.Moved,
        UnityEngine.InputSystem.TouchPhase.Stationary
    };


    /// <summary>
    /// 4 states of the camera. 
    /// The Buffer state extends the time before which the camera actually starts to work,
    /// in order to avoid the program's starting up jamming the initial animation. 
    /// The start state is when the camera would zoom out to reveal the model.
    /// Once the user is actively interacting, the state goes to Controlled.
    /// If the user does not control the camera for a while, then it goes to Idle. 
    /// </summary>
    public enum CameraState {Buffer, Start, Controlled, Idle}

    /// <summary>
    /// Options: the option bar at the bottom.
    /// Settings: the setting group box at the left.
    /// Text: the text description box, with voice over options.
    /// Appearance: the appearance control box at the right. 
    /// </summary>
    public enum UIElements { Options, Settings, Text, Appearance };


    /// <summary>
    /// The 4 sides of the screen. 
    /// Used to indicate if an UI element is currently occupying this area
    /// and thus may interfere with touch control or object selection. 
    /// </summary>
    public enum Side { Top, Bottom, Left, Right }


    /// <summary>
    /// Random number generator for all classes to use. 
    /// Try use this for all RNG, since creating RNG everytime it's needed may cause
    /// same or similar numbers to be generated due to the seed issue. 
    /// </summary>
    public static System.Random RNG = new System.Random();

    /// <summary>
    /// Color from the OSU Brand Guide > Visual Design > Color > Scarlet
    /// </summary>
    public static Color buckeyeHighlight = new Color(.7294f, .047f, .1843f, 1);

}
