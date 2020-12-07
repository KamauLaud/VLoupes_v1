using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varjo;


namespace VarjoExample
{
    public class GazeStatus : MonoBehaviour
    {
        private VarjoPlugin.GazeEyeStatus leftEyeStatus;
        private VarjoPlugin.GazeEyeStatus rightEyeStatus;
        private VarjoPlugin.GazeData gStatus;
        private float lastLeftWink;
        private float lastRightWink;
        private readonly float winkEngage = 0.5f;
        private Zoom zoomScript;
        private readonly float contZoomAdjust = 0.00025f;
        public GameObject gazeTargets;
        public GameObject initZoom;


        // Start is called before the first frame update
        void Start()
        {
            gStatus = VarjoPlugin.GetGaze();
            leftEyeStatus = gStatus.leftStatus;
            rightEyeStatus = gStatus.rightStatus;
            if (!SceneHandler.testing)
            {
                zoomScript = GameObject.FindGameObjectWithTag("leftEye").GetComponent<Zoom>();
            }
            

        }

        // Update is called once per frame
        void Update()
        {
            gStatus = VarjoPlugin.GetGaze();
            leftEyeStatus = gStatus.leftStatus;
            rightEyeStatus = gStatus.rightStatus;
            
            UnityEngine.Debug.Log("Gaze Status - LEFT/RIGHT: " + leftEyeStatus.ToString() + " " + rightEyeStatus.ToString());

            //
            if ( isLeftWink(leftEyeStatus, rightEyeStatus))
            {
                lastLeftWink += Time.deltaTime;
                if (lastLeftWink > winkEngage)
                {
                    //zoom start
                    UnityEngine.Debug.Log("Gaze Status - Left eye continous zooming OUT, baby!");
                    // perform left action - zoom out
                    float curWH = zoomScript.getWH();
                    zoomScript.setZoom(curWH + contZoomAdjust);
                }
            }
            else
            {
                //lastLeftWink -= Time.deltaTime;
                lastLeftWink = 0;
            }



            if (isRightWink(leftEyeStatus, rightEyeStatus))
            {
                lastRightWink += Time.deltaTime;
                if (lastRightWink > winkEngage)
                {
                    UnityEngine.Debug.Log("Gaze Status - Right eye continous zooming IN, baby!");
                    // perform right action - zoom in
                    float curWH = zoomScript.getWH();
                    zoomScript.setZoom(curWH - contZoomAdjust);
                    
                }
            }
            else
            {
                lastRightWink = 0;
            }


            switch (leftEyeStatus)
            {
                case VarjoPlugin.GazeEyeStatus.EYE_COMPENSATED:
                    UnityEngine.Debug.Log("Gaze Status_L - Eye compensated.");
                    break;
                case VarjoPlugin.GazeEyeStatus.EYE_INVALID:
                    UnityEngine.Debug.Log("Gaze Status_L - Eye invalid AKA not seen.");
                    break;
                case VarjoPlugin.GazeEyeStatus.EYE_TRACKED:
                    UnityEngine.Debug.Log("Gaze Status_L - Eye tracked.");
                    break;
                case VarjoPlugin.GazeEyeStatus.EYE_VISIBLE:
                    UnityEngine.Debug.Log("Gaze Status_L - Eye visible.");
                    break;
                default:
                    break;
            }

            switch (rightEyeStatus)
            {
                case VarjoPlugin.GazeEyeStatus.EYE_COMPENSATED:
                    UnityEngine.Debug.Log("Gaze Status_R - Eye compensated.");
                    break;
                case VarjoPlugin.GazeEyeStatus.EYE_INVALID:
                    UnityEngine.Debug.Log("Gaze Status_R - Eye invalid aka not seen.");
                    break;
                case VarjoPlugin.GazeEyeStatus.EYE_TRACKED:
                    UnityEngine.Debug.Log("Gaze Status_R - Eye tracked.");
                    break;
                case VarjoPlugin.GazeEyeStatus.EYE_VISIBLE:
                    UnityEngine.Debug.Log("Gaze Status_R - Eye visible.");
                    break;
                default:
                    break;
            }
        }

        // The person is doing this ->   ↼‿ಠ
        // reminder Wink is the eye that is CLOSED aka INVALID
        bool isLeftWink(VarjoPlugin.GazeEyeStatus left, VarjoPlugin.GazeEyeStatus right)
        {
            if (left == VarjoPlugin.GazeEyeStatus.EYE_INVALID && 
                (right == VarjoPlugin.GazeEyeStatus.EYE_TRACKED || right == VarjoPlugin.GazeEyeStatus.EYE_COMPENSATED))
            { 
                UnityEngine.Debug.Log("Gaze Status - Left eye blinked!");
                return true;
            }
            else
            {
                return false;
            }
        }

        // The person is doing this ->  ಠ‿↼ 
        bool isRightWink(VarjoPlugin.GazeEyeStatus left, VarjoPlugin.GazeEyeStatus right)
        {
            if (right == VarjoPlugin.GazeEyeStatus.EYE_INVALID &&
                (left == VarjoPlugin.GazeEyeStatus.EYE_TRACKED || left == VarjoPlugin.GazeEyeStatus.EYE_COMPENSATED))
            {
                UnityEngine.Debug.Log("Gaze Status - Right eye blinked!");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
