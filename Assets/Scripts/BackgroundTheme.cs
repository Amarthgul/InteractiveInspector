using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BackgroundTheme : MonoBehaviour
{

    [SerializeField] Image thisImage;

    [SerializeField] private Sprite darkModeBackground;

    [SerializeField] private Sprite lightModeBackground;

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
            
        }

        lastLightModeStat = Globals.lightModeOn;
    }


}
