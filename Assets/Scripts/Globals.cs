using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals 
{
    
    /// <summary>
    /// 3 states of the camera. 
    /// The start state is when the app just started before the user make any input.
    /// Once the user is actively interacting, the state goes to Controlled.
    /// If the user does not control the camera for a while, then it goes to Idle. 
    /// </summary>
    public enum CameraState {Buffer, Start, Controlled, Idle}



}
