using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals 
{

    // 1000 milisecond in a second, used to convert second to milisecond 
    public const int MILISECOND_IN_SEC = 1000;

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

}
