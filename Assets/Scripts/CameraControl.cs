using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Diagnostics;
using UnityEngine.InputSystem.LowLevel;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Linq;
using Debug = UnityEngine.Debug;
using System;
using UnityEngine.UIElements;
using System.Runtime; // Where does all these unused packages come from? 

public class CameraControl : MonoBehaviour
{
    /// ===============================================================
    /// ========================== CONSTANTS ==========================  

    // A buffer time for the app to fully start to work 
    private const long SLEEP_TIMER = 1000;

    // 1000 milisecond in a second, used to convert second to milisecond 
    private const int MILISECOND_IN_SEC = 1000;

    // Whether or not to use the start animation 
    private const bool USE_START_ANIMATION = true;

    // Speed function is Gauss, this is the power
    private const float SPEED_FUNCTION_POW = 2;

    // Offset input of the speed function to approximate starting from 0 
    private const float SPEED_FUNCTION_OFFSET = 3;

    /// ===============================================================
    /// ==================== Serialized variables ===================== 

    [Space(15)]
    [Header("Object references")]
    [Space(5)]

    [SerializeField] private Camera thisCamera;


    /// ======================= Basic operation =======================   

    [Header("Basic operation")]
    [Space(5)]

    [Tooltip("Damping the panning action")]
    [Range(0, 10)]
    [SerializeField] private float cameraPanDamper = 1.0f;

    [Tooltip("Damping the zooming action")]
    [Range(0, 10)]
    [SerializeField] private float cameraZoomDamper = 1;

    [Tooltip("Damping the rotating action")]
    [Range(0, 50)]
    [SerializeField] private float cameraRotationDamper = 30f;

    [Tooltip("Whether or not interaction prodeuces momentum")]
    [SerializeField] private bool enableMomentum = true;

    [Tooltip("Movement bigger than this will trigger momentum")]
    [SerializeField] private Vector2 momentumThreshold = new Vector2 (1, 1);

    [Tooltip("The ratio of which the spped falls off")]
    [Range(0, 1)]
    [SerializeField] private float rotateFallOff = .9f; 

    [Tooltip("The defualt position if not locked on target")]
    [SerializeField] private Vector3 defaultPosition = Vector3.zero;

    [Tooltip("The default rotation, dicates the viewing angle")]
    [SerializeField] private Vector3 defaultRotation = Vector3.zero;

    /// =========================== Look-at ===========================  

    [Space(15)]
    [Header("Look at object")]
    [Space(5)]

    [Tooltip("Object to lock on")]
    [SerializeField] private GameObject targetObject; 

    [Tooltip("Enable this will make the camera rotate around am object or a point")]
    [SerializeField] private bool lockOnTarget = false;

    [Tooltip("Offset the camera by a certain height to bring better visibility")]
    [Range(-10f, 10f)]
    [SerializeField] private float targetHeightOffset = 2f; 

    [Tooltip("When locked on target, this indicates how far the camera is sway at the start")]
    [Range(0, 100)]
    [SerializeField] private float targetStartDistance = 20;

    [Tooltip("The minimum distance that can be zoomed in.")]
    [Range(0, 10)]
    [SerializeField] private float minimumDistance = 5;

    [Tooltip("The maximum distance that can be zoomed out.")]
    [Range(10, 100)]
    [SerializeField] private float maximumDistance = 50;

    [Tooltip("Making the rotate sensitivity change accroding to distance to the subject.")]
    [Range(-10, 10)]
    [SerializeField] private float distanceSensityCoef = .1f;


    /// ======================== Touch Control ========================  
    [Space(15)]
    [Header("Touch Control")]
    [Space(5)]

    [Tooltip("Touch control is less sensitive, magnification is thus needed")]
    [Range(0, 20)]
    [SerializeField] private float touchRotateMagnify = 1.5f; 

    [Tooltip("Touch control is less sensitive, magnification is thus needed")]
    [Range(0, 20)]
    [SerializeField] private float touchPinchMagnify= 20f;

    [Tooltip("Touch control is less sensitive, magnification is thus needed")]
    [Range(0, 20)]
    [SerializeField] private float touchPanMagnify = 10f;

    [Tooltip("Finger will always move a bit on screen, this variable is used " +
        "as an activation threshold, bigger than which will be treated as pinching")]
    [Range(0, 20)]
    [SerializeField] private float pinchActivationValue = 1f;

    /// =========================== Animation ========================= 
    [Space(15)]
    [Header("Self Animation")]
    [Space(5)]

    [Tooltip("The amount of time for the starting animation")]
    [SerializeField] private float startAnimationTime = 2f;

    [Tooltip("The amount of time without input after which the camera enters idle mode")]
    [Range(0f, 10f)]
    [SerializeField] private float idleStartPeriod = 5f;

    [Tooltip("Whether or not the idle camera rotates clockwise")]
    [SerializeField] private bool rotateClockWise = true;

    [Tooltip("The speed of which the rotate animation goes")]
    [Range(0f, 100f)]
    [SerializeField] private float rotateSpeed = 20;

    [Tooltip("The speed of which the zoom animation goes")]
    [Range(0f, 100f)]
    [SerializeField] private float zoomSpeed = 20;

    [Tooltip("The maxmium speed of which the self animation goes")]
    [Range(0f, 400f)]
    [SerializeField] private float selfAnimationMaxSpeed = 200f;

    [Tooltip("The accleration of self animation")]
    [Range(0f, 20f)]
    [SerializeField] private float selfAnimationAcclerate = 5f;


    /// ===================== UI precautionary ======================== 
    [Space(15)]
    [Header("UI Precautionary")]
    [Space(5)]

    [Help("Since UI will take space on the screen, camera touch control " +
        "will interfere with the UI. The values below will mark certain " + 
        "areas on screen no longer responsive to touch once UI shows up")]
    [Space(5)]

    [Tooltip("Part of top and bottom screen becomes unresponsive when UIs are active")]
    [SerializeField] Vector2 topBottomNullArea = Vector2.zero;

    [Tooltip("Part of left and right screen becomes unresponsive when UIs are active")]
    [SerializeField] Vector2 leftRightNullArea = Vector2.zero;

    [Tooltip("When using the slider in UI, it is possible to drastically rotate the mdoel " +
        "when finger goes outside of the UI area. Enable this option to nullify rotate when " +
        "operating the slider or the color picker")]
    [SerializeField] bool decoupleSliderOverdraft = true; 

    /// ===============================================================
    /// ============================= Inputs ========================== 
    private InputAction mouseLMB;
    private InputAction mouseRMB;
    private InputAction mousePosition;
    private InputAction mouseDelta;
    private InputAction mouseScroll;

    private InputAction touchPrimary;
    private InputAction touchSecondary;


    /// ===============================================================
    /// ======================== Stat variables =======================

    // This variable is hidden from inspector and is designed to be
    // only accesible via the touch sensitivity slider in UI setting
    private float globalSensitivityControl = 0; 

    private Vector3 lookAtPosition = Vector3.zero;  // Can be swapped for the subject
    private Vector3 lookAtPosOffset = Vector3.zero; // Offset of the look at position height  

    private Vector2 lastDelta = new Vector2();
    private Vector2 deltaMinValue = new Vector2(.02f, .02f); // Lower than this value stops the momentum 

    // Current set distance between the camera and the target object 
    private float targetDistance;

    private Vector2 lastPositionF1;
    private Vector2 lastPositionF2;

    // For 3 finger pinch or pan
    private float lastFingerDist = 0;
    private Vector2 lastFingerAverage = new Vector2();

    private bool touchPinchFlag = false; // Flag this true if pinch is detected 
    private bool touchPanFlag = false;   // Flag this true if pan is detected 
     

    // Monitoring the double click/tap event 
    private Stopwatch doubleClickSW = new Stopwatch();
    private int clickCount = 0;
    private int doubleClickIntervalStart = 25; // Milisecond of effective double click 
    private int doubleClickIntervalEnd = 300; // Milisecond of effective double click 
    private long lastClick = 0;

    // Monitoring the state of the camera 
    private Stopwatch cameraStateSW = new Stopwatch();
    private Globals.CameraState cameraState;

    // Self-animation
    private float currentSelfAnimeSpeed = 0;
    private bool disableSelfIdleAnim = false;

    // UI interference 
    // Top, bottom, left, right, as defined in Globals
    List<bool> areaProtected = new List<bool>() { false, false, false, false };

    // If the current touch started in an protected area, used to decide
    // whether or not to rotate the camera. 
    private bool touchStartedInProtected = false;

    /// ===============================================================
    /// =========================== Methods =========================== 
    /// ===============================================================

    public void Initialize(InputAction LMB, InputAction RMB, InputAction MP, InputAction MD, InputAction MS, 
        InputAction TP, InputAction TS)
    {
        mouseLMB = LMB;
        mouseRMB = RMB;
        mousePosition = MP;
        mouseDelta = MD;
        mouseScroll = MS;

        touchPrimary = TP;
        touchSecondary = TS;

        lastPositionF1 = new Vector2();

        cameraState = Globals.CameraState.Buffer;

        doubleClickSW.Start();
        cameraStateSW.Start();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        lookAtPosOffset = new Vector3(0, targetHeightOffset, 0);

        if (USE_START_ANIMATION)
        {
            targetDistance = minimumDistance;
        }
        else
        {
            targetDistance = targetStartDistance;
        }

        // Keep the camera looking at the subject/location 
        thisCamera.transform.LookAt(lookAtPosition + lookAtPosOffset, Vector3.up);

        RegulateDistance();

    }

    // Update is called once per frame
    void Update()
    {
        if (StartupBuffer() && cameraState == Globals.CameraState.Buffer)
            return;

        if(cameraState == Globals.CameraState.Start) 
            StartAnimation();
        else
        {
            UpdateTouchControl();

            UpdateState();
            UpdateSelfAnimation();

            if (lockOnTarget)
            {
                UpdateTargetLock();
            }
            else
            {
                UpdateFreeMovement();
            }

            CheckDoubleClick();
            CheckDoubleTap();
        }

    }

    /// <summary>
    /// Enable or disable camera self rotate animation
    /// </summary>
    /// <param name="value">Bool flag of the state</param>
    public void ToggleAutoRotate(bool value)
    {
        disableSelfIdleAnim = value;
    }

    /// <summary>
    /// Disable right area's touch control response
    /// </summary>
    public void ProtectRightArea()
    {
        areaProtected[(int)Globals.Side.Right] = true;
    }

    /// <summary>
    /// Release right area for touch control 
    /// </summary>
    public void FreeRightArea()
    {
        areaProtected[(int)Globals.Side.Right] = false;
    }

    /// <summary>
    /// Update the touch sensitivity for all touch controls
    /// </summary>
    /// <param name="input">Taget sensitivity</param>
    public void SetTouchSensitivity(float input)
    {
        globalSensitivityControl = input;
    }

    /// <summary>
    /// Provide the current camera state for other classes to reference. 
    /// </summary>
    /// <returns>Current cameraState</returns>
    public Globals.CameraState GetCameraState()
    {
        return cameraState; 
    }

    /// ===============================================================
    /// ======================= Private Methods ======================= 
    /// ===============================================================


    /// <summary>
    /// Update method when camera is not looking at an object, it pans and tilts freely.
    /// </summary>
    private void UpdateFreeMovement()
    {
        // Mouse delta is used for both position and rotation 
        Vector2 mouseD = mouseDelta.ReadValue<Vector2>();

        // Scoll is used to zoom in and out 
        float mouseS = mouseScroll.ReadValue<Vector2>().y;


        // Right mouse button move the camera
        if (mouseRMB.ReadValue<float>() == 1f)
        {
            thisCamera.transform.Translate(
                new Vector3(-mouseD.x, -mouseD.y, 0) * Time.deltaTime * cameraPanDamper,
                Space.Self);
        }
        // Move the camera position on its Z axis (zoom)
        thisCamera.transform.Translate(
            new Vector3(0, 0, mouseS) * Time.deltaTime * cameraZoomDamper,
            Space.Self);

        // Left mouse button rotate the camera 
        if (mouseLMB.ReadValue<float>() == 1f)
        {
            // Following the mouse, the camera rotates to change the viewing angle
            thisCamera.transform.Rotate(
                new Vector3(mouseD.y, -mouseD.x, 0) * Time.deltaTime * cameraRotationDamper,
                Space.Self);

            // Rotation hierarchy always introduces unwanted gimbal movement,
            // this is to cancel the Z axis offset and level the camera. 
            Vector3 curentRotation = thisCamera.transform.eulerAngles;
            Vector3 updateRotation = new Vector3(curentRotation.x, curentRotation.y, 0);
            thisCamera.transform.rotation = Quaternion.Euler(updateRotation);
        }

    }

    /// <summary>
    /// Update method when the camera has a designated location to look at.
    /// </summary>
    private void UpdateTargetLock()
    {
        // Mouse delta is used for both position and rotation 
        Vector2 mouseD = mouseDelta.ReadValue<Vector2>();

        

        // Scroll change the distance between the camera and the target 
        float mouseS = mouseScroll.ReadValue<Vector2>().y;
        targetDistance -= mouseS * Time.deltaTime * cameraZoomDamper;
        // Regulate the distance above the minimum 
        if (targetDistance < minimumDistance) targetDistance = minimumDistance;

        // Right mouse button moves both the camera and the look-at position 
        if (mouseRMB.ReadValue<float>() == 1f)
        {
            Vector3 camUp = thisCamera.transform.up;
            Vector3 camRight = Vector3.Cross(thisCamera.transform.forward, camUp).normalized;

            // Detla is a composite of camera up and camera right 
            Vector3 deltaMovement = (camUp * -mouseD.y + camRight * mouseD.x)
                * Time.deltaTime * cameraPanDamper;

            thisCamera.transform.Translate(deltaMovement, Space.World);
            lookAtPosition += deltaMovement;
        }

        // Left mouse button move the camera only, but achieve similar effect as rotate 
        if (mouseLMB.ReadValue<float>() == 1f)
        {
            thisCamera.transform.Translate(
                new Vector3(-mouseD.x, -mouseD.y, 0) * Time.deltaTime * cameraRotationDamper,
                Space.Self);
        }

        // Keep the camera looking at the subject/location 
        thisCamera.transform.LookAt(lookAtPosition + lookAtPosOffset, Vector3.up);

        RegulateDistance();

    }

    /// <summary>
    /// All camera control by touch updates in thie method, including rotate, pan, and zoom. 
    /// </summary>
    private void UpdateTouchControl()
    {
        float toTargetDistance = targetDistance;
        Vector2 singleFingerDelta = new Vector2();

        TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();
        TouchState currentTouchF2 = touchSecondary.ReadValue<TouchState>();

        Vector2 touchPosF1 = new Vector2(currentTouchF1.position.x, currentTouchF1.position.y);
        Vector2 touchPosF2 = new Vector2(currentTouchF2.position.x, currentTouchF2.position.y);

        Vector2 fingerAverage = (touchPosF1 + touchPosF2) / 2;
        float fingerDist = Vector2.Distance(touchPosF1, touchPosF2);

        float midDistance = (maximumDistance + minimumDistance) / 2;
        float overshot = targetDistance - midDistance;
        float sensitivityMod = (overshot / (maximumDistance - minimumDistance)) * distanceSensityCoef;

        // =====================================================================================
        // ================================= Double finger operation ===========================
        if (currentTouchF1.phase == UnityEngine.InputSystem.TouchPhase.Moved &&
            currentTouchF2.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            float fingerDistDelta = fingerDist - lastFingerDist;

            // Prioritize pinch
            if (Mathf.Abs(fingerDistDelta) > pinchActivationValue)
                touchPinchFlag = true;
            // If not a pinch, treat it as a pan
            if (!touchPanFlag && !touchPinchFlag)
                touchPanFlag = true;
            
            
            if (touchPanFlag)
            {
                // Set up camera direction vectors 
                Vector2 fingerAverageDelta = fingerAverage - lastFingerAverage;
                Vector3 camUp = thisCamera.transform.up;
                Vector3 camRight = Vector3.Cross(thisCamera.transform.forward, camUp).normalized;

                // Detla is a composite of camera up and camera right 
                Vector3 deltaMovement = (camUp * -fingerAverageDelta.y + camRight * fingerAverageDelta.x)
                    * Time.deltaTime * cameraPanDamper * touchPanMagnify * (1 + sensitivityMod)
                    * (1 + globalSensitivityControl);

                // Move both camera and lookAtPosition
                thisCamera.transform.Translate(deltaMovement, Space.World);
                lookAtPosition += deltaMovement;
            }
            if (touchPinchFlag)
            {
                // 2 finger zoom 
                targetDistance -= fingerDistDelta * Time.deltaTime * cameraZoomDamper * touchPinchMagnify
                    * (1 + sensitivityMod) * (1 + globalSensitivityControl);
            }

        }
        else
        {
            touchPanFlag = false;
            touchPinchFlag = false;
        }

        // =====================================================================================
        // ================================= Single finger operation ===========================
        if (currentTouchF1.phase == UnityEngine.InputSystem.TouchPhase.Began) 
        {
            // The start of a rotation clears previsou data 
            lastPositionF1 = touchPosF1;

            // See if this touch started in the UI area 
            touchStartedInProtected = currentTouchF1.position.x > leftRightNullArea.y 
                && areaProtected[(int)Globals.Side.Right]; 
        }
        else if (currentTouchF1.phase == UnityEngine.InputSystem.TouchPhase.Moved &&
            Globals.deactivatedStats.Contains(currentTouchF2.phase))
        {
            // If the touch is within the protected UI area or that the touch started
            // in the slider/picker area, then do not rotate the camera, quit directly.
            if (!ValidInput() || touchStartedInProtected) return;

            // During a camera rotation operation, first calculates delta 
            singleFingerDelta = lastPositionF1 - touchPosF1;

            // Apply the delta 
            thisCamera.transform.Translate(
                new Vector3(singleFingerDelta.x, singleFingerDelta.y, 0) * Time.deltaTime * 
                cameraRotationDamper * touchRotateMagnify * (1 + sensitivityMod)
                * (1 + globalSensitivityControl),
                Space.Self);

            // Keep the camera looking at the subject/location 
            thisCamera.transform.LookAt(lookAtPosition + lookAtPosOffset, Vector3.up);

            // Records the most recent non-zero single finger rotateion delta 
            if (Vector2.SqrMagnitude(singleFingerDelta) > 0)
                lastDelta = singleFingerDelta;
        }
        else
        {
            if( enableMomentum) // Let the model keep rotating a bit 
            {
                // Decrease delta 
                lastDelta *= rotateFallOff; 

                // Only delta high enough can trigger momentum effect 
                if (Vector2.SqrMagnitude(lastDelta) > Vector2.SqrMagnitude(momentumThreshold))
                {
                    thisCamera.transform.Translate(
                    new Vector3(lastDelta.x, lastDelta.y, 0) * Time.deltaTime *
                    cameraRotationDamper * touchRotateMagnify * (1 + sensitivityMod),
                    Space.Self);
                }

                // Stop the movement when it's below certain value 
                if (Vector2.SqrMagnitude(lastDelta) < Vector2.SqrMagnitude(deltaMinValue))
                    lastDelta = Vector2.zero; 
            }
        }

        // Update all the last stats to current stats 
        lastPositionF1 = touchPosF1;
        lastFingerDist = fingerDist;
        lastFingerAverage = fingerAverage; 

        RegulateDistance();
    }

    /// <summary>
    /// Check for right mouse button double click
    /// </summary>
    private void CheckDoubleClick()
    {
        // Start a potential double click listener (double tap has been moved into another method)
        if (mouseRMB.ReadValue<float>() == 1f ) // ||
            //(activateStates.Contains(touchPrimary.ReadValue<TouchState>().phase) &&
            //activateStates.Contains(touchSecondary.ReadValue<TouchState>().phase)))
        {
            clickCount += 1;
            if (clickCount == 2 &&
                (doubleClickSW.ElapsedMilliseconds - lastClick) > doubleClickIntervalStart &&
                (doubleClickSW.ElapsedMilliseconds - lastClick) < doubleClickIntervalEnd)
            {
                RestSettings();

                doubleClickSW.Restart();
            }

            lastClick = doubleClickSW.ElapsedMilliseconds;

            // More click is 1 click 
            if (clickCount > 2) clickCount = 1;
        }
    }

    /// <summary>
    /// Dedicated method for detecting double tap touch control 
    /// </summary>
    private void CheckDoubleTap()
    {
        TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();
        TouchState currentTouchF2 = touchSecondary.ReadValue<TouchState>();

        if (currentTouchF1.tapCount == 2 && currentTouchF2.tapCount == 2) 
        {
            RestSettings();

            doubleClickSW.Restart();
        }
    }

    /// <summary>
    /// Relocate the camera so the distance between camera and object stays the same. 
    /// </summary>
    private void RegulateDistance()
    {
        // Regulate the distance 
        if (targetDistance < minimumDistance) targetDistance = minimumDistance;
        if (targetDistance > maximumDistance) targetDistance = maximumDistance;

        // Relocate the camera so that the distance stays the same 
        float currentDistance = Vector3.Distance(thisCamera.transform.position, lookAtPosition + lookAtPosOffset);
        float difference = currentDistance - targetDistance;
        Vector3 offset = thisCamera.transform.forward.normalized * difference;
        thisCamera.transform.Translate(offset, Space.World);
    }

    /// <summary>
    /// Rest the camera positions  
    /// </summary>
    private void RestSettings()
    {
        if (lockOnTarget)
        {
            if (targetObject != null)
            {
                lookAtPosition = targetObject.transform.position;
            }
            else
            {
                lookAtPosition = Vector3.zero;
            }
            targetDistance = targetStartDistance;
            RegulateDistance();
        }
        else
        {
            thisCamera.transform.position = defaultPosition;
            thisCamera.transform.rotation = Quaternion.Euler(defaultRotation);
        }

    }

    /// <summary>
    /// update the camera state by listening to user input. 
    /// If there are user input detected, then the state shall be kept at "Controlled",
    /// if the user remains silent for a while, switch state to "idle"
    /// </summary>
    /// <returns>True if the user made any input</returns>
    private bool UpdateState()
    {
        bool interaction = false;

        TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();
        TouchState currentTouchF2 = touchSecondary.ReadValue<TouchState>();

        if (Globals.activateStates.Contains(currentTouchF1.phase) ||
            Globals.activateStates.Contains(currentTouchF2.phase))
        {
            interaction = true;
            cameraStateSW.Restart();
            cameraState = Globals.CameraState.Controlled; 
        }
        
        if(cameraStateSW.ElapsedMilliseconds > idleStartPeriod * MILISECOND_IN_SEC 
            && cameraState != Globals.CameraState.Idle)
        {
            cameraState = Globals.CameraState.Idle;
            currentSelfAnimeSpeed = 0;
        }

        return interaction; 
    }

    /// <summary>
    /// Check if camera is idle, and if so, update the self-animation
    /// </summary>
    private void UpdateSelfAnimation()
    {
        // If self idle rotation animation is disabled, then skip this 
        if (disableSelfIdleAnim) return;

        if (cameraState == Globals.CameraState.Idle)
        {
            currentSelfAnimeSpeed += Time.deltaTime * selfAnimationAcclerate;
            if (currentSelfAnimeSpeed > selfAnimationMaxSpeed * Time.deltaTime)
                currentSelfAnimeSpeed = selfAnimationMaxSpeed * Time.deltaTime; 

            float currentRotateStep = (rotateClockWise ? -1 : 1) * Time.deltaTime * currentSelfAnimeSpeed;

            thisCamera.transform.Translate(new Vector3(currentRotateStep, 0, 0), Space.Self);

            // Keep the camera looking at the subject/location 
            thisCamera.transform.LookAt(lookAtPosition, Vector3.up);

            RegulateDistance();
        }
    }

    /// <summary>
    /// Animation that smooth zoom out and show the whole model
    /// </summary>
    private void StartAnimation()
    {
        if (cameraState != Globals.CameraState.Start) return;

        // If time tiked out or user made input, then stop the animation  
        if(cameraStateSW.ElapsedMilliseconds > (long)(startAnimationTime * MILISECOND_IN_SEC) || UpdateState())
        {
            cameraState = Globals.CameraState.Controlled; 
        }
        else
        {
            float currentProgress = (float)cameraStateSW.ElapsedMilliseconds / (startAnimationTime * MILISECOND_IN_SEC);
            float xProgress = currentProgress * SPEED_FUNCTION_OFFSET * 2;
            float sigProgress = SpeedFunction(xProgress - SPEED_FUNCTION_OFFSET);

            float currentRotateStep = (rotateClockWise ? -1 : 1) * Time.deltaTime * rotateSpeed * sigProgress;

            targetDistance += sigProgress * Time.deltaTime * zoomSpeed;
            thisCamera.transform.Translate(new Vector3(currentRotateStep, 0, 0), Space.Self);

            // Keep the camera looking at the subject/location 
            thisCamera.transform.LookAt(lookAtPosition + lookAtPosOffset, Vector3.up);

            RegulateDistance();
        }


    }

    /// <summary>
    /// Buffer time for the app to warm up.
    /// </summary>
    /// <returns>True if the buffer time is not ended</returns>
    private bool StartupBuffer()
    {
        if (cameraState != Globals.CameraState.Buffer) return false;

        bool inBuffer = true;

        if (cameraStateSW.ElapsedMilliseconds > SLEEP_TIMER)
        {
            inBuffer = false;
            cameraState = Globals.CameraState.Start;
            cameraStateSW.Restart();
        }

        return inBuffer;
    }

    /// <summary>
    /// Given an input position, check with active areas to see if its position is valid.
    /// </summary>
    /// <param name="position">Vec2 position in screen space</param>
    /// <returns>True if the position is in active area</returns>
    private bool ValidInput()
    {
        // Assume the interval between the caller function and this funtion is so small
        // that the touch position does not change (or small enough to neglect)
        TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();
        Vector2 position = new Vector2(currentTouchF1.position.x, currentTouchF1.position.y);

        // If there is no deactivated area, return true
        if (!areaProtected.Contains(false)) return true;

        return (areaProtected[(int)Globals.Side.Right] ? position.x < leftRightNullArea.y : true) // Beyond right side 
            && (areaProtected[(int)Globals.Side.Left] ? position.x > leftRightNullArea.x : true) // Beyond left side
            && (areaProtected[(int)Globals.Side.Bottom] ? position.y > topBottomNullArea.y : true) // Beyond bottom side
            && (areaProtected[(int)Globals.Side.Top] ? position.y < topBottomNullArea.x : true);// Beyond top side
        ;
    }

    /// ===============================================================
    /// ======================= Utility Methods =======================
    /// ===============================================================

    /// <summary>
    /// Guass function to calculate the speed at a given point.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Y = e^(-x^2)</returns>
    private float SpeedFunction(float input)
    {
        return Mathf.Exp(-Mathf.Pow(input, SPEED_FUNCTION_POW));
    }

    
}

