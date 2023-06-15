using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BackgroundTheme : MonoBehaviour
{

    [SerializeField] Image thisImage;

    [Tooltip("Image to be used as background for dark mode")]
    [SerializeField] private Sprite darkModeBackground;

    [Tooltip("Image to be used as background for light mode")]
    [SerializeField] private Sprite lightModeBackground;

    // Last recorded light mode on/off status
    private bool lastLightModeStat = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        UpdateTheme();


    }

    /// <summary>
    /// Check if the theme changes and change background accrodingly.
    /// </summary>
    private void UpdateTheme()
    {

        if (lastLightModeStat != Globals.lightModeOn)
        {
            if (Globals.lightModeOn)
            {
                thisImage.sprite = lightModeBackground;
            }
            else
            {
                thisImage.sprite = darkModeBackground;
            }
            lastLightModeStat = Globals.lightModeOn;
        }

        
    }


}
