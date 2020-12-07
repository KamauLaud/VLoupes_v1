using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VarjoExample;

public class MenuHandler : MonoBehaviour
{
    //TODO eventually we want to move all HUD/MENU/INTERFACE functionality here!!
    private GameObject gazeInit;
    private GameObject secMenu;
    private GameObject gt1;
    private GameObject gt2;
    private GameObject gt3;
    private GameObject gt4;
    private GameObject gazeWatcher;
    private GameObject topMenu;

    // revertTimer > secondaryClickTime > primaryClickTime >> look threshold
    private const float revertTime = 1.2f;
    private System.Diagnostics.Stopwatch revertTimer;
    public const float lookThreshold = 0.08f;
    public const int nFrames = 8; // able to flick away for 15 frames or 0.5 seconds assuming 
    private const float secondaryClickTime = 0.9f;
    private const float primaryClickTime = 0.25f;
    public static float DwellTimeDefault = 1.0f;
    public const bool twoStepEnable = false;
    


    public static bool discreteEnable;
    private bool continousEnable;
    private bool dwellInProgress;
    public static bool rTimerStarted;
    public static string lastGazedAt;

    

    // Start is called before the first frame update
    void Start()
    {
        //secure reference to all menu associated gaze targets
        gazeInit = GameObject.FindGameObjectWithTag("gt_init");
        gazeWatcher = GameObject.FindGameObjectWithTag("continousController");
        topMenu = GameObject.FindGameObjectWithTag("topMenu");
        discreteEnable = true;
        rTimerStarted = false;
        dwellInProgress = false;
        continousEnable = false;

        topMenu.SetActive(discreteEnable);
        gazeWatcher.SetActive(continousEnable);
        revertTimer = new System.Diagnostics.Stopwatch();
        lastGazedAt = "nah, b";
    }

    // Update is called once per frame
    void Update()
    {
        //if T is pressed, toggle interface capabaility 
        if (Input.GetKeyDown(KeyCode.T))
        {
            discreteEnable = !discreteEnable;
            continousEnable = !continousEnable;

            topMenu.SetActive(discreteEnable);
            gazeWatcher.SetActive(continousEnable);

            UnityEngine.Debug.Log("Menu Debug | interface toggled");
        }


        if (twoStepEnable && (float)revertTimer.Elapsed.TotalSeconds >= revertTime  )
        {
            revertMenu();
        }
        
        if(dwellInProgress && twoStepEnable)
        {
            pauseTimer();
        }

    }

    void OnEnable()
    {
        VarjoGazeTarget.MenuStart += startMenu;
        VarjoGazeTarget.MenuClick += resetTimer;
        VarjoGazeTarget.GazeDwell += pauseTimer;
        VarjoGazeTarget.ResetTimer += resetGazeTimers;

    }

    void OnDisable()
    {
        VarjoGazeTarget.MenuStart -= startMenu;
        VarjoGazeTarget.MenuClick -= resetTimer;       
        VarjoGazeTarget.GazeDwell -= pauseTimer;
        VarjoGazeTarget.ResetTimer -= resetGazeTimers;
    }

    public void resetGazeTimers()
    {
        UnityEngine.Debug.Log("MenuHandlerDebug - Gaze timer reset started.);");
        setupGazeRef("reset");
        UnityEngine.Debug.Log("MenuHandlerDebug - Gaze timer reset finished.);");
    }

    //will revert menu to initial state 
    public void revertMenu()
    {
        //after the primary is pressed, this will be called, 
        // if none of the gaze timers are > 0, revert menu back to primary stage
       
        UnityEngine.Debug.Log("MenuHandlerDebug - timers: " + gt1.GetComponent<VarjoGazeTarget>().getGazeTimer() + " "  + gt2.GetComponent<VarjoGazeTarget>().getGazeTimer()
            + " " + gt3.GetComponent<VarjoGazeTarget>().getGazeTimer() + " " + gt4.GetComponent<VarjoGazeTarget>().getGazeTimer());

        float t1 = gt1.GetComponent<VarjoGazeTarget>().getGazeTimer();
        float t2 = gt2.GetComponent<VarjoGazeTarget>().getGazeTimer();
        float t3 = gt3.GetComponent<VarjoGazeTarget>().getGazeTimer();
        float t4 = gt4.GetComponent<VarjoGazeTarget>().getGazeTimer();
        if ( t1 < lookThreshold && t2 < lookThreshold && t3 < lookThreshold && t4 < lookThreshold)
        {
            //revert Menu
            secMenu.SetActive(false);
            gazeInit.SetActive(true);
            UnityEngine.Debug.Log("MenuHandlerDebug - revertHappened");
        }
    }

    private void startMenu()
    {
        setupGazeRef("2-step");
        UnityEngine.Debug.Log("MenuHandlerDebug - revertMenuStarted");
        resetTimer(true);
    }
    private void resetTimer(bool start)
    {
        if (twoStepEnable)
        {
            revertTimer.Reset();
            if (start)
            {
                revertTimer.Start();
                UnityEngine.Debug.Log("MenuHandlerDebug - timer is started.");
                rTimerStarted = true;
            }
        }
        else
        {
            setupGazeRef("reset");
        }
    }

    private void pauseTimer()
    {
        revertTimer.Stop();
        UnityEngine.Debug.Log("MenuHandlerDebug - timer is paused.");
        rTimerStarted = false;
    }

    private void setupGazeRef(string mode)
    {
        gazeInit = GameObject.FindGameObjectWithTag("gt_init");
        secMenu = GameObject.FindGameObjectWithTag("gazetarget");
        gt1 = GameObject.FindGameObjectWithTag("gt_1x");
        gt2 = GameObject.FindGameObjectWithTag("gt_2x");
        gt3 = GameObject.FindGameObjectWithTag("gt_2.5x");
        gt4 = GameObject.FindGameObjectWithTag("gt_3x");

        //container.Add(secMenu);
       // container.Add(gt1);
        //container.Add(gt2);
        //container.Add(gt3);
       // container.Add(gt4);

        switch (mode)
        {
            case "2-step":
                setClickTimers("2-step");
                break;
            case "reset":
                setClickTimers("reset");
                break;
            default:
                break;
        }

        
    }

    private void setClickTimers(string mode)
    {
        // remember in order to reset something...said object must be active
        switch(mode){

            case "2-step":
                //set indiviual click timers for all buttons
                try
                {
                    gazeInit.GetComponent<VarjoGazeTarget>().setClickTimer(primaryClickTime);
                    gt1.GetComponent<VarjoGazeTarget>().setClickTimer(secondaryClickTime);
                    gt2.GetComponent<VarjoGazeTarget>().setClickTimer(secondaryClickTime);
                    gt3.GetComponent<VarjoGazeTarget>().setClickTimer(secondaryClickTime);
                    gt4.GetComponent<VarjoGazeTarget>().setClickTimer(secondaryClickTime);
                }
                catch(Exception e)
                {
                     UnityEngine.Debug.Log("MenuHandlerDebug | caught exception | " + e);
                }
                break;
            case "reset":

                try
                {
                    gazeInit.GetComponent<VarjoGazeTarget>().resetGazeTimer();
                    gt1.GetComponent<VarjoGazeTarget>().resetGazeTimer();
                    gt2.GetComponent<VarjoGazeTarget>().resetGazeTimer();
                    gt3.GetComponent<VarjoGazeTarget>().resetGazeTimer();
                    gt4.GetComponent<VarjoGazeTarget>().resetGazeTimer();

                    GameObject gt_cancel = GameObject.FindGameObjectWithTag("gt_cancel");
                    gt_cancel.GetComponent<VarjoGazeTarget>().resetGazeTimer();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("MenuHandlerDebug | caught exception | " + e);
                }

                break;
            default:
                break;
        }
        

    }
}
