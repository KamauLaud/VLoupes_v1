using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VarjoExample {
    public class Zoom : MonoBehaviour
    {
        public float MinScale = 0.25f; //4x zoom
        public float MaxScale = 1.0f; //1x

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
        private GameObject menuHandler;

        private TextMesh noti;

        private void Start()
        {
            initZoom = GameObject.FindGameObjectWithTag("gt_init");
            gazeTargets = GameObject.FindGameObjectWithTag("gazetarget");
            menuHandler = GameObject.FindGameObjectWithTag("menu_control");
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
               //swapImages(ZoomLevel.NONE);
                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            } 
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //swapImages(ZoomLevel.TWO_X);
                setZoom(ZoomLevel.TWO_X);
                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                //swapImages(ZoomLevel.TWOP5_X);
                setZoom(ZoomLevel.THREEP5_X);
                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                //swapImages(ZoomLevel.THREE_X);
                setZoom(ZoomLevel.THREE_X);
                UnityEngine.Debug.Log("InitZoom texture -> " + initZoom.GetComponent<Renderer>().material.name);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                //originally false
                primarySwp = !primarySwp;
                flipGUI(primarySwp, !primarySwp);

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
                UnityEngine.Debug.Log("...........");
                UnityEngine.Debug.Log("Changed UV: " + _currentScale.ToString());
                return;
            }

            _currentScale.width = scaleAdjustment;
            _currentScale.height = scaleAdjustment;
            _currentScale.center = lockedCenter;

            view.uvRect = _currentScale;
        }

        // zoom level = 1/wh_val
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
                case ZoomLevel.THREEP5_X:
                    //3.5x
                    setZoom(0.2857f);
                    break;
                case ZoomLevel.THREE_X:
                    setZoom(0.3333333f);
                    break;
                case ZoomLevel.TWOP5_X:
                    setZoom(0.4f);
                    break;
                default:
                    setZoom(1.0f);
                    break;
            }
        }

        public void setZoom(float wh_ratio)
        {

            _currentScale.center = lockedCenter;
            _currentScale.width = wh_ratio;
            _currentScale.height = wh_ratio;
            _currentScale.x = (float) (0.5 - (wh_ratio / 2));
            _currentScale.y = (float) (0.5 - (wh_ratio / 2));
            view.uvRect = _currentScale;

            double copy;
            if(wh_ratio == 1)
            {
                noti.text = "1x";
                copy = 1;
            }
            else
            {
                copy = 1 / wh_ratio;
                noti.text = copy.ToString("#.##") + "x";
            }
            UnityEngine.Debug.Log("Check Noti | wh_value, noti.text = " + wh_ratio.ToString("#.###") + " " + copy.ToString());
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
        public void flipGUI(bool primaryHide, bool secondaryHide)
        {
            //if primaryHide == true, hide button
            menuHandler.GetComponent<MenuHandler>().hideMenuObject(primaryHide,
                initZoom.tag, new Vector3(3.3139f,-1.286f,0));

            menuHandler.GetComponent<MenuHandler>().hideMenuObject(secondaryHide, 
                gazeTargets.tag, new Vector3(1.84f, -1.84f, 0));
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