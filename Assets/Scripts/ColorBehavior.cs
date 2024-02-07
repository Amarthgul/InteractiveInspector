using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBehavior : MonoBehaviour, ObjectBehavior
{
    [Tooltip("Possibility of gaze is represented by color")]
    [SerializeField] private bool enableColor = true;

    [ShowIf(ActionOnConditionFail.DisableOnly, ConditionOperator.And, nameof(enableColor))]
    [Tooltip("Color when the object is believed to be gazed upon")]
    [SerializeField] private Color focusColor = new Color(1, 0, 0);

    [ShowIf(ActionOnConditionFail.DisableOnly, ConditionOperator.And, nameof(enableColor))]
    [Tooltip("Color when the object is believed to be inrelevant")]
    [SerializeField] private Color neglectColor = new Color(0, 0, 1);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GazeOperation(float LoC)
    {
        Color sheen = InterpolateFocusColor(LoC);

        /// =============================================================== 
        if (enableColor) // Assign the color and opacity if desired 
        {
            RecursionChildColor(this.gameObject, sheen, LoC);
        }
    }

    /// <summary>
    /// Recursively enter each object and change its sheen color/opacity. 
    /// </summary>
    /// <param name="thisObject">Object to check</param>
    /// <param name="sheenColor">Goal sheen color</param>
    /// <param name="LoC">Level of Confidence as opacity</param>
    private void RecursionChildColor(GameObject thisObject, Color sheenColor, float LoC)
    {
        // Keep doing recusion if this child also has child objects
        if (thisObject.transform.childCount > 0)
        {
            for (int i = 0; i < thisObject.transform.childCount; i++)
                RecursionChildColor(thisObject.transform.GetChild(i).gameObject, sheenColor, LoC);
        }
        // Change the color and effect opacity if this is the parent object 
        else if (thisObject.GetComponent<Renderer>() != null) // If this is the parent object 
        {
            thisObject.GetComponent<Renderer>().material.SetColor("_Sheen", sheenColor);
            thisObject.GetComponent<Renderer>().material.SetFloat("_Opacity", LoC);
        }
    }

    /// <summary>
    /// Interpolate between 2 colors. 
    /// </summary>
    /// <param name="LoC">Level of confidence</param>
    /// <returns>Color interpolated between high and low representations</returns>
    private Color InterpolateFocusColor(float LoC)
    {
        Color color = new Color();

        color = LoC * focusColor + (1 - LoC) * neglectColor;

        return color;
    }
}
