using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine;


/// <summary>
/// Gradually fade out the image and show the underlaying model. 
/// </summary>
public class StartScreenHandler : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image startUpImage;

    [Tooltip("Time in milisecond of which the start screen remains constant")]
    [SerializeField] private long startUpImageConst = 500;

    [Tooltip("Time in milisecond of whcih the start screen fades away")]
    [SerializeField] private long startUpImageFade = 1000;

    // Startup image
    private Stopwatch startUpImageSW = new Stopwatch();
    private bool startUpImageDone = false;
    
    // Start is called before the first frame update
    void Start()
    {
        startUpImageSW.Start();
    }

    // Update is called once per frame
    void Update()
    {
        StartUpImage();
    }

    private void StartUpImage()
    {
        if (startUpImageDone) return;

        if (startUpImageSW.ElapsedMilliseconds > startUpImageConst)
        {
            // Start to fade away
            long elapsedAfterConst = startUpImageSW.ElapsedMilliseconds - startUpImageConst;

            float progression = (float)elapsedAfterConst / startUpImageFade;

            Color c = startUpImage.color;
            c = new Color(c.r, c.g, c.b, 1 - progression);
            startUpImage.color = c;

            if (startUpImageSW.ElapsedMilliseconds > (startUpImageConst + startUpImageFade))
            {
                startUpImageDone = true;
                startUpImageSW.Stop();
            }
        }

    }
}
