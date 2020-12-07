// Copyright 2019 Varjo Technologies Oy. All rights reserved.

using System;
using System.Diagnostics;
using System.Collections;

using UnityEngine;

namespace VarjoExample
{
    /// <summary>
    /// Increase emission of the gameObject when VarjoGaze hits its collider.
    /// Decrease emission gradually in Update.
    /// Expects use of Unity standard shader.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(Collider))]

    //mic check 1 2, 1 2

    public class VarjoGazeTarget : MonoBehaviour
    {

         
        private float timeToClick = MenuHandler.DwellTimeDefault;
        private Stopwatch elapsedGazeTime;
        private int bFrames;

        public delegate void ClickAction(bool a, int errs);
        public static event ClickAction GazeClicked;

        public delegate void ClickAction2();
        public static event ClickAction2 MenuStart;
        public static event ClickAction2 GazeDwell;
        public static event ClickAction2 ResetTimer;

        public delegate void ClickAction3(bool a);
        public static event ClickAction3 MenuClick;

        
        private Renderer targetRenderer;
        private MaterialPropertyBlock materialPropertyBlock;
        private float emissionLevel = 0.0f;
        private Color emissionColor = Color.yellow;

        private GameObject self;
        private Zoom zoomScript;
        
        
        public static GameObject progressBar;

        private static Vector3 hidePos;
        private static bool pBarAlive;
        private static bool hit;
        private bool hasBeenCalled;
        private bool lookLeft;
        private float lastHit;
        private bool gazeStarted;
        private const float exitTime = 0.5f;

        void Start()
        {
            targetRenderer = GetComponent<Renderer>();
            targetRenderer.material.EnableKeyword("_EMISSION");
            materialPropertyBlock = new MaterialPropertyBlock();
            self = this.gameObject;
            if (!SceneHandler.testing)
            {
                zoomScript = GameObject.FindGameObjectWithTag("leftEye").GetComponent<Zoom>();
            }
            
            elapsedGazeTime = new System.Diagnostics.Stopwatch();
            bFrames = 0;
            hasBeenCalled = false;
            lookLeft = false;
            gazeStarted = false;

            //TODO fix PROGRESS BAR STUFF
            progressBar = GameObject.FindGameObjectWithTag("pBar");
            pBarAlive = false;
            hit = false;
            //hidePos = progressBar.transform.position;
        }

        IEnumerator flipTimer()
        {
            yield return new WaitForSeconds(2*MenuHandler.lookThreshold); //0.16seconds
            hasBeenCalled = false;
        }



        public void OnHit()
        {
            string gt_tag = self.tag;
            string clickedTarget = gt_tag[2].ToString();
            lastHit = Time.time;
            
            /*
            UnityEngine.Debug.Log("Debug GT | LookHappened - tag: " + gt_tag + " target #: " + clickedTarget
                + " | correctTarget: " + SceneHandler.correctTarget);
            */

            if ((gt_tag == "gt_1x" || gt_tag == "gt_2x" || gt_tag == "gt_2.5x" || gt_tag == "gt_3x") && elapsedGazeTime.Elapsed.TotalSeconds > MenuHandler.lookThreshold)
            {
                //while in the middle of a dwell, pause the revert timer
                //TODO  this could potentially get called a lot. may need bool for dwell in progress
               // GazeDwell();
            }

            //GazeEnter
           // UnityEngine.Debug.Log("Debug GT - before gazeEnter Gtimer: " + getGazeTimer() + " seconds, tag- " + self.tag);
            OnGazeEnter(gt_tag);
           // UnityEngine.Debug.Log("Debug GT - after gazeEnter Gtimer: " + getGazeTimer() + " seconds, tag- " + self.tag);
            //check if a click occured during this frame
            UnityEngine.Debug.Log("Debug GT - pre dwellClick Gtimer: " + getGazeTimer() + " seconds, tag- " + self.tag);
            dwellClick();
            UnityEngine.Debug.Log("Debug GT - post dwellClick Gtimer: " + getGazeTimer() + " seconds, tag- " + self.tag);
            //Falsely started dwell timers are considered as errors
            //if we are about to start increasing a misMatched gaze timer, we are makin an error
            if ( getGazeTimer() <= MenuHandler.lookThreshold && !clickedTarget.Equals(SceneHandler.correctTarget.ToString()) && SceneHandler.testing)
            {
                UnityEngine.Debug.Log("Debug GT | LookHappened | False activation! -> " 
                    + clickedTarget + " /\\ " + SceneHandler.correctTarget);
                if( !hasBeenCalled)
                {
                    SceneHandler.errorGazeActs += 1;
                    hasBeenCalled = true;
                }
                else
                {
                    //make hasBeenCalled false after 0.15
                    StartCoroutine(flipTimer());
                }
                
            }

        }

        //when gaze enters a gt, start incrementing the counter
        public void OnGazeEnter(string currentGT)
        {
            UnityEngine.Debug.Log("Debug GT - cur & prev before OnGazeEnter | current gt: " + self.tag + " last gt: " + MenuHandler.lastGazedAt);
            string currentGazeTarget = currentGT;
            //if  this target hasn't been gazed at, start the timer
            // else skip
            UnityEngine.Debug.Log("Debug GT | entered target " + getTag());
            if (!gazeStarted)
            {
                getGazeWatch().Start();
                gazeStarted = true;
            }
            else
            { //gazeStarted == true which means another frame has started while gazing at the same target
                if (!lookLeft) // gazeStarted true and flick hasn't occured 
                {
                    // this would probably be where we update the progress bar
                    // or OnGazeStay()
                }
                else
                { // gazeStarted == true and flick has occured (lookLeft == True)
                  //   this means that you looked away for too long and now the dwell time needs to be repeated fully
                    if (getBFrames() > MenuHandler.nFrames)
                    {
                        lookLeft = false;
                        gazeStarted = false;
                        resetBFrames();
                        getGazeWatch().Reset(); //zero timer and  if flick lasted longer than nFrames
                    }
                    else if (MenuHandler.lastGazedAt == currentGazeTarget)
                    {
                        //if you are within the frame limit, it was just a short error, continue timing
                        UnityEngine.Debug.Log("Debug GT | entered target " + getFirstEnter() + " | timer started");
                        
                        getGazeWatch().Start();
                        UnityEngine.Debug.Log("Debug GT - inside OnGazeEnter Gtimer: " + getGazeTimer() + " seconds, tag-> " + self.tag);
                    }
                }

                //for the 2 step approach, when the primary button is clicked, 
                //      start a timer to revert back if no selection is made within revert time
                if (!MenuHandler.rTimerStarted && MenuHandler.discreteEnable && MenuHandler.twoStepEnable)
                {
                    MenuClick(true);
                }

            }
        }

        //when gaze leaves a gt, reset the timer (this is what should happen in a perfect world)
        // in actuality, when gaze leaves target for a small number of frames, reset
        //                        if it comes back in small number of frames, continue timer
        public void OnGazeExit()
        {
            UnityEngine.Debug.Log("Debug GT | exited target | lookLeft = " + getFirstEnter() + " | timer stopped.");
            //start frameCounter
            lookLeft = true;
            getGazeWatch().Stop();
            MenuHandler.lastGazedAt = self.tag;
        }

        void dwellClick()
        {
            UnityEngine.Debug.Log("Debug GT - cur & prev before click | current gt: " + self.tag + " last gt: " + MenuHandler.lastGazedAt);
            
            //OnClick()
            if (getGazeTimer() >= timeToClick) //a click has been completed@
            {
                UnityEngine.Debug.Log("Debug GT - ClickHappened");
                switch (self.tag)
                {
                    case "gt_1x":
                        
                        zoomScript.swapImages(ZoomLevel.NONE);
                        zoomScript.setZoom(ZoomLevel.NONE);
                        MenuClick(false); //this will also reset all other active gazeTimers
                        ResetTimer();

                        setDefaultState();
                        flipGUI(false);

                        UnityEngine.Debug.Log("Debug GT - ClickZoom1x");
                        break;
                    case "gt_2x":
                        
                        zoomScript.swapImages(ZoomLevel.TWO_X);
                        zoomScript.setZoom(ZoomLevel.TWO_X);

                        MenuClick(false); //this will also reset all other active gazeTimers

                        ResetTimer();
                        setDefaultState();
                        flipGUI(false);
                        UnityEngine.Debug.Log("Debug GT - ClickZoom2");
                        break;
                    case "gt_2.5x":
                        
                        zoomScript.swapImages(ZoomLevel.TWOP5_X);
                        zoomScript.setZoom(ZoomLevel.TWOP5_X);

                        MenuClick(false); //this will also reset all other active gazeTimers
                        ResetTimer();

                        setDefaultState();
                        flipGUI(false);

                        UnityEngine.Debug.Log("Debug GT - ClickZoom2.5");
                        break;
                    case "gt_3x":

                        zoomScript.swapImages(ZoomLevel.THREE_X);
                        zoomScript.setZoom(ZoomLevel.THREE_X);

                        MenuClick(false); //this will also reset all other active gazeTimers
                        ResetTimer();

                        setDefaultState();
                        flipGUI(false);

                        UnityEngine.Debug.Log("Debug GT - ClickZoom3");
                        break;
                    case "gt_cancel":
                        
                        ResetTimer(); //this will also reset all other active gazeTimers
                        setDefaultState();

                        flipGUI(false);


                        UnityEngine.Debug.Log("Debug GT - ClickCancel");
                        break;
                    case "gt_init":
                        
                        ResetTimer();
                        setDefaultState();
                        if (MenuHandler.twoStepEnable)
                        {
                            MenuStart(); //when using the 2step process, start it
                        }
                        flipGUI(true);
                        UnityEngine.Debug.Log("Debug GT - ClickZoomINIT");
                        break;
                    default:
                        break;
                }

                if (SceneHandler.testing)
                {
                    String clickedTarget = self.tag[2].ToString();
                    GazeClicked(!clickedTarget.Equals(SceneHandler.correctTarget.ToString()), SceneHandler.errorGazeActs);
                }
            }

            //reset bFrames every dwell click
            resetBFrames();
        }

        void Update()
        {

            emissionLevel = (getGazeTimer() / timeToClick) + 0.0001f;
            emissionLevel = Mathf.Clamp01(emissionLevel); //[0.0001,1]
            materialPropertyBlock.SetColor("_EmissionColor", emissionColor * emissionLevel * 1.2f);
            this.targetRenderer.SetPropertyBlock(materialPropertyBlock);
            //handleProgressBar(emissionLevel);

            if (lookLeft)
            {
                bFrames += 1;
            }

            VarjoGazeTarget loc = null;
            try
            {
                loc = VarjoGazeRay.gazeRayHit.collider.gameObject.GetComponent<VarjoGazeTarget>();
            }
            catch(Exception e)
            {
                //this exception is thrown when the user isn't gazing at a Gaze target. 
                //UnityEngine.Debug.Log("Debug GT | exception | " + e);
            }
            //exitTime how long can we look away
            //if ( loc == null && gazeStarted )// (Time.time - lastHit > ???)
            if (loc == null && (Time.time - lastHit) > exitTime)
            {
                UnityEngine.Debug.Log("Debug GT - User's gaze left target for too long. Reset timer");
                //can only exit AFTER an ENTER has occured
                //this should be done when VarjoGazeRay.gazeRayHit == null
                OnGazeExit();
                
            }

            //TODO this is a patchwork fix 
            if(getGazeTimer() > MenuHandler.DwellTimeDefault)
            {
                //shave time off timer
                // ......but you can't do this with a Stopwatch
            }
        }




        //Shows the primary button and hides the secondary buttons or vice-versa based on flipper
        private void flipGUI(bool flipper)
        {
            if (!SceneHandler.testing)
            {
                zoomScript.getZoomButton().SetActive(!flipper);
                zoomScript.getGTs().SetActive(flipper);
            }
                     
        }

        public float getGazeTimer()
        {
            TimeSpan time;
            try
            {
                time = elapsedGazeTime.Elapsed;
                   
            }
            catch( Exception b)
            {
                UnityEngine.Debug.Log("Debug GT | exception | " + b);
            }
            return (float)time.TotalSeconds;
        }

        public void setClickTimer(float time)
        {
            this.timeToClick = time;
        }

        public void resetGazeTimer()
        {
            elapsedGazeTime.Reset();
        }

        private void setDefaultState()
        {
            lookLeft = false;
            gazeStarted = false;
            resetBFrames();
            //getGazeWatch().Reset();
        }

        public String getTag()
        {
            return self.tag;
        }

        public bool getFirstEnter()
        {
            return lookLeft;
        }

        public void setFirstEnter(bool val)
        {
            lookLeft = val;
        }

        public Stopwatch getGazeWatch()
        {
            return elapsedGazeTime;
        }
        public int getBFrames()
        {
            return bFrames;
        }

        public void resetBFrames()
        {
            bFrames = 0;
        }


        private void OnGUI()
        {
            //handleProgressBar(emissionLevel);
        }

        private void spawnProgressBar(Vector3 loc)
        {
            if (self.tag.Contains("gt_"))
            {
                progressBar.transform.position = loc;
                pBarAlive = true;
                RadialProgress progScript = progressBar.GetComponentInChildren<RadialProgress>();
                //progScript.progress = 0;
                UnityEngine.Debug.Log("Debug pBar -> created progessBar");
            }
        }

        private void updateProgressBar(float progress)
        {
            try
            {
                RadialProgress progScript = progressBar.GetComponentInChildren<RadialProgress>();
                progScript.progress = progress;
                UnityEngine.Debug.Log("Debug pBar -> Updating progessBar...");
            }
            catch
            {
                UnityEngine.Debug.Log("There is no progress bar. How is pBarAlive and there's no script?");
            }
        }

        private void handleProgressBar(float prog)
        {
            UnityEngine.Debug.Log("Debug pBar -> hit: " + hit.ToString() + " pBarAlive: " + pBarAlive.ToString());
            if (!hit && pBarAlive) //deSpawn pBar
            {
                progressBar.transform.position = hidePos;
                UnityEngine.Debug.Log("Debug pBar -> destroyed progressBar");
                pBarAlive = false;
            }
            else if (hit && !pBarAlive) //spawnPBar
            {
                RaycastHit rch = VarjoGazeRay.gazeRayHit;
                UnityEngine.Debug.Log("raycast hit loc: " + rch.point.ToString() +
                    " | Debug pBar - spawned here " + (rch.point + new Vector3(0, 0, -0.01f)).ToString());

                spawnProgressBar(rch.point + new Vector3(0, 0, -0.01f));
            }
            else if (hit && pBarAlive) //update
            {
                //update progressBar
                updateProgressBar(prog);
            }
        }
    }

}