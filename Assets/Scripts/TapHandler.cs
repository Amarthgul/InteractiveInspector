using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.UIElements;


public class TapHandler : MonoBehaviour
{
    /// ===============================================================
    /// ========================== CONSTANTS ========================== 
    private const string NONE = "none";

    /// ===============================================================
    /// ==================== Serialized variables ===================== 
    [SerializeField] Camera meinCamera; // Das war ein befehl!

    [SerializeField] float rayLength;

    [Tooltip("Objects in the list below can be tapped and highlighted")]
    [SerializeField] List<GameObject> canBeTapped = new List<GameObject>();

    [Tooltip("Default Shaders for the Can Be Tapped objects")]
    [SerializeField] List<Shader> defaultShaders = new List<Shader>();

    [Tooltip("Highlight effect shaders for the Can Be Tapped objects when selected")]
    [SerializeField] List<Shader> highlightShaders = new List<Shader>();


    [SerializeField] List<AudioClip> voiceOvers = new List<AudioClip>();

    [Space(15)]
    [Header("Opacity control")]
    [Space(5)]

    [Tooltip("Opacity of the object(s) that have been selected.")]
    [Range(0f, 1f)]
    [SerializeField] public float selectedOpacity = 1.0f;

    [Tooltip("Opacity of the objects that are not selected.")]
    [Range(0f, 1f)]
    [SerializeField] public float unselectedOpacity = 1.0f;

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

        string nameOfTapped = TapResponse(); 

        if (nameOfTapped != null)
        {
            // If a valid onject is selected, alter its (or others') shader(s)
            AlterTapped(nameOfTapped); 
        }
        if(nameOfTapped == NONE)
        {
            // If tapped outside of objects, resets all effects 
            ResetAllShaders();
        }

        UpdateOpacity();

    }

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
        // Iterate throught the objects that can be tap selected 
        for(int i = 0; i < canBeTapped.Count; i++)
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


    private void PlayAudio()
    {

    }

}
