using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.UIElements;
using UnityEngine.Windows;


public class TapHandler : MonoBehaviour
{
    /// ===============================================================
    /// ========================== CONSTANTS ========================== 
    private const string NONE = "none";

    private const string ERR_SELECT = "No description for obnject selected";

    /// ===============================================================
    /// ==================== Serialized variables ===================== 
    [SerializeField] Camera meinCamera; // Das war ein befehl!

    [SerializeField] float rayLength;

    [Tooltip("Objects in the list below can be tapped and highlighted")]
    [SerializeField] List<GameObject> canBeTapped = new List<GameObject>();

    [Tooltip("List of voiceovers, corresponds to each mesh")]
    [SerializeField] List<AudioClip> voiceOvers = new List<AudioClip>();

    [Tooltip("List of text that will be displayed as the descrption")]
    [SerializeField] List<string> descriptions = new List<string>();

    [Tooltip("Default Shaders for the Can Be Tapped objects")]
    [SerializeField] List<Shader> defaultShaders = new List<Shader>();

    [Tooltip("Highlight effect shaders for the Can Be Tapped objects when selected")]
    [SerializeField] List<Shader> highlightShaders = new List<Shader>();

    [Space(15)]
    [Header("Opacity control")]
    [Space(5)]

    [Tooltip("Opacity of the object(s) that have been selected.")]
    [Range(0f, 1f)]
    [SerializeField] public float selectedOpacity = 1.0f;

    [Tooltip("Opacity of the objects that are not selected.")]
    [Range(0f, 1f)]
    [SerializeField] public float unselectedOpacity = 1.0f;


    /// ===================== UI precautionary ======================== 
    [Space(15)]
    [Header("UI Precautionary")]
    [Space(5)]

    [Help("Since UI will take space on the screen, tap select/unselect " +
        "will interfere with the UI. This section is to deal with the effect " +
        "UI has on camera and its touch control.")]
    [Space(5)]

    [Tooltip("Part of top and bottom screen becomes unresponsive when UIs are active")]
    [SerializeField] Vector2 topBottomNullArea = Vector2.zero;

    [Tooltip("Part of left and right screen becomes unresponsive when UIs are active")]
    [SerializeField] Vector2 leftRightNullArea = Vector2.zero;

    /// ===============================================================
    /// ======================= Input actions =========================
    private InputAction touchPrimary;

    /// ===============================================================
    /// ======================== Stat variables =======================  

    // Restrict the update in checking tap for certain amount of miliseconds 
    private int tapProtection = 150; 

    // This is used to ensure accidental touch will not reset previous tap selection
    private int lastTapCount = 0; 
    
    // Mark each object's select status, true if this object is currently selected/highlighted 
    private List<bool> selected = new List<bool>();

    // For counting time between taps
    private Stopwatch tapStopwatch = new Stopwatch();

    // Mark this to true whenever the user makes a new selection 
    public bool newlySelected = false;

    // Name of the last selected object 
    private string lastSelected = ""; 

    // UI interference 
    // Top, bottom, left, right, as defined in Globals
    List<bool> areaProtected = new List<bool>() { false, false, false, false };

    public void Initialize(InputAction PT)
    {
        touchPrimary = PT;

        tapStopwatch.Start();
    }


    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < canBeTapped.Count; i++)
        {
            selected.Add(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Skip this function for current version. 
        return; 

        string nameOfTapped = TapResponse();
        
        if (nameOfTapped != null)
        {
            

            // If a valid object is selected, alter its (or others') shader(s)
            AlterTapped(nameOfTapped);

            if (nameOfTapped != lastSelected && IsSelected(nameOfTapped))
            {
                newlySelected = true;
                lastSelected = nameOfTapped;
            }
            else
            {
                newlySelected = false;
            }

        }
        if (nameOfTapped == NONE)
        {
            if (!ValidInput()) return;
            // If tapped outside of objects, resets all effects 
            ResetAllShaders();
        }

        UpdateOpacity();

    }

    /// ===============================================================
    /// ======================== Public Methods =======================
    /// ===============================================================

    /// <summary>
    /// Check is there are any items currently being selected.
    /// </summary>
    /// <returns>True if something is selected</returns>
    public bool HasSelected()
    {
        return selected.Contains(true);
    }


    /// <summary>
    /// Disable top area's touch control
    /// </summary>
    public void ProtectTopArea()
    {
        areaProtected[(int)Globals.Side.Top] = true;
    }

    /// <summary>
    /// Enable top area's touch control
    /// </summary>
    public void FreeTopArea()
    {
        areaProtected[(int)Globals.Side.Top] = false;
    }

    /// <summary>
    /// Disable bottom area's touch control
    /// </summary>
    public void ProtectBottomArea()
    {
        areaProtected[(int)Globals.Side.Bottom] = true;
    }

    /// <summary>
    /// Enable bottom area's touch control
    /// </summary>
    public void FreeBottomArea()
    {
        areaProtected[(int)Globals.Side.Bottom] = false;
    }

    /// <summary>
    /// Disable left area's touch control
    /// </summary>
    public void ProtectLeftArea()
    {
        areaProtected[(int)Globals.Side.Left] = true;
    }

    /// <summary>
    /// Enable left area's touch control
    /// </summary>
    public void FreeLeftArea()
    {
        areaProtected[(int)Globals.Side.Left] = false;
    }

    /// <summary>
    /// Disable right area's touch control
    /// </summary>
    public void ProtectRightArea()
    {
        areaProtected[(int)Globals.Side.Right] = true;
    }

    /// <summary>
    /// Enable right area's touch control
    /// </summary>
    public void FreeRightArea()
    {
        areaProtected[(int)Globals.Side.Right] = false;
    }

    /// <summary>
    /// Unselect all objects and thus void the selection. 
    /// </summary>
    public void UnselectAll()
    {
        for (int i = 0; i < canBeTapped.Count; i++)
        {
            selected[i] = false;
        }
    }

    /// <summary>
    /// Alter the opacity of the selected objects.
    /// </summary>
    /// <param name="opacity">Target opacity, value between 0 to 1</param>
    public void SetSelectedOpacity(float opacity)
    {
        selectedOpacity = opacity; 
    }

    /// <summary>
    /// Alter the opacity of objects that are not selected
    /// </summary>
    /// <param name="opacity">Target opacity, value between 0 to 1</param>
    public void SetUnselectedOpacity(float opacity)
    {
        unselectedOpacity = opacity;
    }

    /// <summary>
    /// Change the rim color of the selected highlight effect.
    /// </summary>
    /// <param name="input">Target color</param>
    public void SetRimColor(Color input)
    {
        for(int i = 0; i < canBeTapped.Count; i++)
        {
            if (selected[i])
                canBeTapped[i].GetComponent<Renderer>().material.SetColor("_RimLight", input);
        }
    }

    /// <summary>
    /// Change the fill color of the selected highlight effect.
    /// </summary>
    /// <param name="input">Target color</param>
    public void SetFillColor(Color input)
    {
        for (int i = 0; i < canBeTapped.Count; i++)
        {
            if (selected[i])
                canBeTapped[i].GetComponent<Renderer>().material.SetColor("_FillLight", input);
        }
    }

    /// <summary>
    /// Get the text description of the selected object.
    /// If multiple objects are selected, reutrn the first one on the list. 
    /// If selected object has no description, return error message.
    /// </summary>
    /// <returns>Text description of the selected</returns>
    public string GetTextDescription()
    {

        for (int i = 0; i < selected.Count; i++)
        {
            //if (selected[i] && descriptions[i].Length > 0)
                //return descriptions[i];
            if (canBeTapped[i].name == lastSelected)
                return descriptions[i];
        }
        
        return ERR_SELECT;
    }

    /// <summary>
    /// Get the name of the selected object.
    /// When multiple objects are selected, return the first one on the list.
    /// </summary>
    /// <returns>Title name of the selected</returns>
    public string GetTitle()
    {
        for (int i = 0; i < selected.Count; i++)
        {
            //if (selected[i])
                //return canBeTapped[i].name;
            if (canBeTapped[i].name == lastSelected)
                return canBeTapped[i].name;
        }

        return ERR_SELECT;
    }

    /// <summary>
    /// Get the voiceover audio of the selected object.
    /// When multiple objects are selected, return the first one on the list.
    /// </summary>
    /// <returns>Voiceover audio clip of the selected</returns>
    public AudioClip GetVoiceOver()
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (canBeTapped[i].name == lastSelected)
                return voiceOvers[i];
        }

        return null;
    }

    /// ===============================================================
    /// ======================= Private Methods =======================
    /// ===============================================================


    /// <summary>
    /// Try to find which object is selected when detecting a tap. 
    /// </summary>
    private string TapResponse()
    {

        TouchState currentTouchF1 = touchPrimary.ReadValue<TouchState>();
        string nameOfTapped = null;

        if(currentTouchF1.tapCount >= 1 && currentTouchF1.tapCount != lastTapCount 
            && tapStopwatch.ElapsedMilliseconds > tapProtection)
        {
            tapStopwatch.Restart();

            // Only cast ray when there is a tap 

            RaycastHit hit; 
            Ray ray = meinCamera.ScreenPointToRay(currentTouchF1.position);

            if (Physics.Raycast(ray, out hit))
            {
                // If the ray hit something
                nameOfTapped = hit.transform.name;

                // Record the count index of this effective touch 
                lastTapCount = currentTouchF1.tapCount; 
            }
            else
            {
                // If the ray hit nothing 
                nameOfTapped = NONE;
            }
        }

        // tapCount automatically reset to 0 after a few seconds without tap action. 
        // lastTapCount thus needs resetting to accommodate the change in tapCount. 
        if (currentTouchF1.tapCount == 0)
            lastTapCount = 0;

        return nameOfTapped; 
    }

    /// <summary>
    /// Change the display of the tapped (or all others not tapped)
    /// </summary>
    /// <param name="nameOfTapped">String name of the tapped obj</param>
    private void AlterTapped(string nameOfTapped)
    {
        if (!ValidInput()) return;

        
        // Iterate throught the objects that can be tap selected 
        for (int i = 0; i < canBeTapped.Count; i++)
        {
            MeshFilter mesh = canBeTapped[i].GetComponent<MeshFilter>();

            // Find the one that matches the tap result 
            if (mesh.name == nameOfTapped)
            {
                Renderer rend = canBeTapped[i].GetComponent<Renderer>();

                if (selected[i]) 
                {
                    // If this object is already selected, revoke the selection
                    SetShader(rend, defaultShaders, i);
                    selected[i] = false;
                }
                else
                {
                    // If this object is not selected, select and highlight it 
                    SetShader(rend, highlightShaders, i);
                    selected[i] = true;
                }        
            }
        }
    }

    /// <summary>
    /// Clear all the highlight effects by reseting all the shaders
    /// </summary>
    private void ResetAllShaders()
    {
        if (!ValidInput()) return;

        for (int i = 0; i < canBeTapped.Count; i++)
        {
            Renderer rend = canBeTapped[i].GetComponent<Renderer>();

            SetShader(rend, defaultShaders, i);
            selected[i] = false;
        }
    }

    /// <summary>
    /// Try to set the Rend shader to the i-th shader in Shaders list.
    /// </summary>
    /// <param name="Rend">The renderer to be altered</param>
    /// <param name="Shaders">List of shaders for changing the renderer</param>
    /// <param name="Index">Index of ideal shader in the shader list</param>
    private void SetShader(Renderer Rend, List<Shader> Shaders, int Index)
    {
        if (Index <= (Shaders.Count - 1)) // Index start with 0, offset the count is needed 
        {
            // When there are enough shaders, assign shader accroding to the index
            Rend.material.shader = Shaders[Index];
        }
        else
        {
            // If there is not enough shaders, assign the cloest one
            Rend.material.shader = Shaders[Shaders.Count - 1];
        }
    }

    /// <summary>
    /// Update the opacity of selected or unselected objects.
    /// </summary>
    private void UpdateOpacity()
    {
        // If nothing is selected, then skip this update
        if (!selected.Contains(true))
            return;


        for (int i = 0; i < selected.Count; i++)
        {
            Shader shader = canBeTapped[i].GetComponent<Renderer>().material.shader;

            if (selected[i])
            {
                canBeTapped[i].GetComponent<Renderer>().material.SetFloat("_Alpha", selectedOpacity);
            }
            else
            {
                canBeTapped[i].GetComponent<Renderer>().material.SetFloat("_Alpha", unselectedOpacity);
            }

        }

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
        
    }

    private bool IsSelected(string name)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (canBeTapped[i].name == name && selected[i])
                return true;
        }
        return false;
    }

}
