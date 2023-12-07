using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarGazer : MonoBehaviour
{

    /// ===============================================================
    /// ==================== Serialized variables ===================== 
    [SerializeField] Camera meinCamera; // Das war ein befehl!

    [Tooltip("The angle at which shall be regarded as inrelevant")]
    [SerializeField] float angleBound = 90;

    [Tooltip("Rescale the level of confidence between these 2 values")]
    [SerializeField] Vector2 clampValue = new Vector2 (.5f, 1);

    [Tooltip("Possibility of gaze is represented by color")]
    [SerializeField] private bool enableColor;

    [ShowIf(ActionOnConditionFail.DisableOnly, ConditionOperator.And, nameof(enableColor))]
    [Tooltip("Color when the object is believed to be gazed upon")]
    [SerializeField] private Color focusColor = new Color(1, 0, 0);

    [ShowIf(ActionOnConditionFail.DisableOnly, ConditionOperator.And, nameof(enableColor))]
    [Tooltip("Color when the object is believed to be inrelevant")]
    [SerializeField] private Color neglectColor = new Color (0, 0, 1);

    [Tooltip("Objects that can interact with the star gazer")]
    [SerializeField] List<GameObject> gazableObjects = new List<GameObject>();

    /// ===============================================================
    /// ====================== Private variables ====================== 

    private Dictionary<GameObject, float> gazeConfidence = new Dictionary<GameObject, float>();

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


    private float ConfidenceFunction(float angle)
    {

        return 1 - (angle / angleBound);
    }


    private void IterateConfidenceLevel()
    {
        for (int i = 0; i < gazableObjects.Count; i++)
        {
            // Calculate the angle between object and camera view vector 
            Vector3 line = gazableObjects[i].transform.position - meinCamera.transform.position;
            float angle = Vector3.Angle(line, meinCamera.transform.forward);

            // Covert the angle into 
            float LoC = ConfidenceFunction(angle); 
            LoC = (LoC - clampValue.x) / (clampValue.y - clampValue.x);
            LoC = Mathf.Clamp(LoC, 0, 1); 

            gazeConfidence[gazableObjects[i]] = LoC;

            if (enableColor)
            {
                Color sheen = InterpolateFocusColor(LoC);
                gazableObjects[i].GetComponent<Renderer>().material.SetColor("_Sheen", sheen);
                gazableObjects[i].GetComponent<Renderer>().material.SetFloat("_Opacity", LoC);
            }
        }

    }
    

    private Color InterpolateFocusColor(float LoC)
    {
        Color color = new Color();

        color = LoC * focusColor + (1 - LoC) * neglectColor; 

        return color; 
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
