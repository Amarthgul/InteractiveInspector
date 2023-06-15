using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerMM : MonoBehaviour
{
    /// <summary>
    /// ======================= Input actions =========================
    /// </summary>
    private UserInput inputScheme;


    /// <summary>
    /// ==================== Serialized variables ===================== 
    /// </summary>



    private void Awake()
    {
        inputScheme = new UserInput();

    }

    private void OnEnable()
    {
        inputScheme.Enable();

        var _q = new QuitHandler(inputScheme.Desktop.Quit);

    }


}
