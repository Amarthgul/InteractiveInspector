using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarGazer : MonoBehaviour
{

    /// ===============================================================
    /// ==================== Serialized variables ===================== 
    [SerializeField] Camera meinCamera; // Das war ein befehl!

    // Objects that can interact with the star gazer
    [SerializeField] List<GameObject> gazableObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(GazerRaytrace());
    }


    private Vector2 EstimatedGazeDirection()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }

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
