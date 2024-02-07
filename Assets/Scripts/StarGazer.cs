using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarGazer : MonoBehaviour
{

    /// ===============================================================
    /// ==================== Serialized variables ===================== 
    [SerializeField] Camera meinCamera; // Das war ein befehl!

    [Tooltip("Objects that can interact with the star gazer")]
    [SerializeField] List<GameObject> gazableObjects = new List<GameObject>();

    /// ===============================================================
    /// ====================== Private variables ====================== 

    private Dictionary<GameObject, float> gazeConfidence = new Dictionary<GameObject, float>();

    private float cameraDiagonalFoV = 41; 

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < gazableObjects.Count; i++)
        {
            gazeConfidence.Add(gazableObjects[i], 0);
        }

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(gazeConfidence);
        IterateConfidenceLevel();
        PrintCondidenceLevel();
    }



    /// ===============================================================
    /// ======================= Private Methods ======================= 
    /// =============================================================== 


    /// <summary>
    /// Calculate the level of confidence given an angle 
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private float ConfidenceFunction(float angle)
    {
        return 1 - (angle / cameraDiagonalFoV);
    }

    /// <summary>
    /// Iterate through the listed objects and update their gaze level of confidence.
    /// If necessary, also change their color sheen (need to have the material). 
    /// </summary>
    private void IterateConfidenceLevel()
    {
        // Update the camera diagonal field of view
        // in case the camera setting changes during the game 
        UpdateDiagonalFoV(); 

        // Iterate through the listed objects 
        for (int i = 0; i < gazableObjects.Count; i++)
        {
            // Calculate the angle between object and camera view vector 
            Vector3 line = gazableObjects[i].transform.position - meinCamera.transform.position;
            float angle = Vector3.Angle(line, meinCamera.transform.forward);

            // Covert the angle into LoC, also clamp the value to avoid possibility overflow 
            float LoC = ConfidenceFunction(angle); 
            LoC = Mathf.Clamp(LoC, 0, 1); 

            // Calculate the color and opacity 
            gazeConfidence[gazableObjects[i]] = LoC;

            gazableObjects[i].GetComponent<ObjectBehavior>().GazeOperation(LoC);
        }
    }
    
    

    /// <summary>
    /// Calculate the camera diagonal field of view and update it. 
    /// </summary>
    private void UpdateDiagonalFoV()
    {
        float vertFoV = meinCamera.fieldOfView;
        float aspectRatio = meinCamera.pixelWidth / meinCamera.pixelHeight;
        float horiFoV = vertFoV * aspectRatio;

        cameraDiagonalFoV = Mathf.Sqrt(Mathf.Pow(vertFoV, 2) + Mathf.Pow(horiFoV, 2)); 
    }

    /// <summary>
    /// Show the object and their gaze level of confident in console. 
    /// </summary>
    private void PrintCondidenceLevel()
    {
        foreach(KeyValuePair<GameObject, float> kvp in gazeConfidence)
        {
            Debug.Log(" " + kvp.Key.name + " \t with confidence " + kvp.Value);
        }
    }

    /// <summary>
    /// An estimate of where the eyes are looking at. 
    /// </summary>
    /// <returns>Vector of estimated direction</returns>
    private Vector2 EstimatedGazeDirection()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }

    /// <summary>
    /// Shot a ray and try to get the name of the object
    /// </summary>
    /// <returns>Name of the object if hit</returns>
    private string GazerRaytrace()
    {
        string nameOfTapped = null;

        RaycastHit hit;
        Ray ray = meinCamera.ScreenPointToRay(EstimatedGazeDirection());

        if (Physics.Raycast(ray, out hit))
        {
            // If the ray hit something
            nameOfTapped = hit.transform.name;
        }
        else
        {
            // If the ray hit nothing 
            nameOfTapped = null;
        }

        return nameOfTapped;
    }



}
