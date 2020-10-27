using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VarjoExample {
    public class Zoom : MonoBehaviour
    {
        public float MinScale = 0.1f;
        public float MaxScale = 0.875f;

        public float ScaleIncrement = 0.01f;
        private const float defaultSwpTime = 3.0f; //the time in seconds
        private float swapTimer = 0.0f;
        private float swapCheck = 0.0f;
        private bool swapInit = false;
        private bool primarySwp;

        private Rect _currentScale;
        private Vector2 lockedCenter;

        public RawImage view;
        private GameObject initZoom;
        private GameObject gazeTargets;

        private TextMesh noti;


        private void Awake()
        {
            initZoom = GameObject.FindGameObjectWithTag("gt_init");
            gazeTargets = GameObject.FindGameObjectWithTag("gazetarget");
            initZoom.SetActive(true);
            gazeTargets.SetActive(false);
        }

        private void Start()
        {
            view = GameObject.FindGameObjectWithTag("leftEye").GetComponent<RawImage>();
            noti = GameObject.FindGameObjectWithTag("zoomLvL").GetComponent<TextMesh>();

            _currentScale = view.uvRect;
            lockedCenter = view.uvRect.center;

            primarySwp = false;
            setZoom(ZoomLevel.NONE);            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                setZoom(ZoomLevel.NONE);
                swapImages(ZoomLevel.NONE);
                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            } 
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                swapImages(ZoomLevel.TWO_X);
                setZoom(ZoomLevel.TWO_X);
                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                swapImages(ZoomLevel.TWOP5_X);
                setZoom(ZoomLevel.TWOP5_X);
                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                swapImages(ZoomLevel.THREE_X);
                setZoom(ZoomLevel.THREE_X);
                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                primarySwp = !primarySwp;
                flipGUI(primarySwp);

                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            }

            //TODO replace this with one of those timed functions, a CoRoutine
            if (swapInit)
            {
                swapCheck += Time.deltaTime;
                UnityEngine.Debug.Log("Swap debug, timer: " + swapTimer.ToString() + "s, swapInit: " + swapInit.ToString());
                if (swapCheck >= swapTimer) 
                {
                    initZoom.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture2D>("graphics/new_zoom");
                    swapCheck -= swapTimer;
                    swapInit = false;
                    UnityEngine.Debug.Log("Swap Debug: InitZoom texture loaded!");
                }
            }
        }

        IEnumerator zoomLvlNotification()
        {
            yield return new WaitForSeconds(defaultSwpTime);
        }

        private void AdjustScale(float scaleIncrement)
        {
            var scaleAdjustment = _currentScale.width + scaleIncrement;
            UnityEngine.Debug.Log("Scale Adjustment: " + scaleAdjustment);
            //If we are out of bounds, do nothing and return;
            if (scaleAdjustment <= MinScale || scaleAdjustment >= MaxScale)
            {
                UnityEngine.Debug.Log("wHAT THE ACTUAL FUCK BRUV");
                UnityEngine.Debug.Log("Changed UV: " + _currentScale.ToString());
                return;
            }

            _currentScale.width = scaleAdjustment;
            _currentScale.height = scaleAdjustment;
            _currentScale.center = lockedCenter;

            view.uvRect = _currentScale;
        }

        //TODO: Figure out the actual zoom levels based on the width/height ratio
        // solve the following to find corresponding zoom  w/h val = 
        // 
        public void setZoom(ZoomLevel level)
        {
            switch (level)
            {
                case ZoomLevel.NONE:
                    setZoom(1.0f);
                    break;
                case ZoomLevel.TWO_X:
                    setZoom(0.5f);
                    break;
                case ZoomLevel.TWOP5_X:
                    setZoom(0.4f);
                    break;
                case ZoomLevel.THREE_X:
                    setZoom(0.33333f);
                    break;
            }
        }

        public void setZoom(float wh_value)
        {
            if(wh_value > MaxScale)
            {
                wh_value = MaxScale;
            }

            if (wh_value < MinScale)
            {
                wh_value = MinScale;
            }

            _currentScale.center = lockedCenter;
            _currentScale.width = wh_value;
            _currentScale.height = _currentScale.width;
            _currentScale.x = (float) (0.5 - (wh_value / 2));
            _currentScale.y = (float) (0.5 - (wh_value / 2));
            view.uvRect = _currentScale;

            double copy;
            if(wh_value == 1)
            {
                noti.text = "1x";
                copy = 1;
            }
            else
            {
                copy = Math.Round(-8 * (wh_value - 1), 1);
                noti.text = copy.ToString() + "x";
            }
            UnityEngine.Debug.Log("Check Noti | wh_value, noti.text = " + wh_value.ToString() + " " + copy.ToString());
        }

        public float getWH()
        {
            return _currentScale.width;
        }

        public void swapImages(ZoomLevel lvl, float swpTime = defaultSwpTime) //TODO turn this into a corourtine 
        {
            swapTimer = swpTime;
            swapCheck = 0.0f;
            swapInit = true;
            initZoom = GameObject.FindGameObjectWithTag("gt_init");
            switch (lvl)
            {
                case ZoomLevel.NONE:
                    initZoom.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture2D>("graphics/1x_mag");
                    UnityEngine.Debug.Log("Inside InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name + " but should be " + lvl.ToString());
                    break;
                case ZoomLevel.TWO_X:
                    initZoom.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture2D>("graphics/2x_mag");
                    UnityEngine.Debug.Log("Inside InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name + " but should be " + lvl.ToString());
                    break;
                case ZoomLevel.TWOP5_X:
                    initZoom.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture2D>("graphics/2.5X_mag");
                    UnityEngine.Debug.Log("Inside InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name + " but should be " + lvl.ToString());
                    break;
                case ZoomLevel.THREE_X:
                    initZoom.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture2D>("graphics/3x_mag");
                    UnityEngine.Debug.Log("Inside InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name + " but should be " + lvl.ToString());
                    break;
                default:
                    break; 
            }
        }

        //Shows the primary button and hides the secondary buttons or vice-versa based on flipper
        private void flipGUI(bool flipper)
        {
            initZoom.SetActive(!flipper);
            gazeTargets.SetActive(flipper);

        }

        public GameObject getGTs()
        {
            return gazeTargets;
        }

        public GameObject getZoomButton()
        {
            return initZoom;
        }
    }



}