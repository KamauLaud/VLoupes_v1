using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcam : MonoBehaviour
{
    
    WebCamTexture webcamTexture;
    // Start is called before the first frame update
    void Start()
    {
        WebCamDevice[] cam_devices = WebCamTexture.devices;
        for (int i = 0; i < cam_devices.Length; i++)
        {
            Debug.Log("Webcam #" + i.ToString()+ " available: " + cam_devices[i].name);
        }
        //rawimage = GetComponent<RawImage>();


        Renderer quad = GetComponent<Renderer>();
        
        webcamTexture = new WebCamTexture(cam_devices[1].name);
        quad.material.mainTexture = webcamTexture;
        //quad.material.mainTexture.height = 500;
        //quad.material.mainTexture.width = 500;

        if (webcamTexture != null)
        {
            Debug.Log("");
            //rawimage.texture = webcamTexture;
            webcamTexture.Play();
        }
        
       

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
