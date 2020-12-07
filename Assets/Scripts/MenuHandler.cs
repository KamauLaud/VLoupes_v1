using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VarjoExample;

public class MenuHandler : MonoBehaviour
{
    //TODO eventually we want to move all HUD/MENU/INTERFACE functionality here!!
    private GameObject secMenu;
    private GameObject[] gaze_targets;
    private GameObject gazeWatcher;
    private GameObject topMenu;
    private GameObject zoomMenu;
    private GameObject mainCamera;

    // revertTimer > secondaryClickTime > primaryClickTime >> look threshold
    private const float revertTime = 1.2f;
    private System.Diagnostics.Stopwatch revertTimer;
    public const float lookThreshold = 0.08f;
    public const int nFrames = 8; // able to flick away for 15 frames or 0.5 seconds assuming 
    private const float secondaryClickTime = 1.2f;
    private const float primaryClickTime = 0.25f;
    public static float DwellTimeDefault = 1.0f;
    public const bool twoStepEnable = false;
    public static Vector3 goldenPos_init;
    public static Vector3 goldenPos_zoomMenu;
    


    public static bool discreteEnable;
    private bool continousEnable;
    private bool dwellInProgress;
    public static bool rTimerStarted;
    public static string lastGazedAt;

    

    // Start is called before the first frame update
    void Start()
    {

        //secure reference to all menu associated gaze targets
        gazeWatcher = GameObject.FindGameObjectWithTag("continousController");
        topMenu = GameObject.FindGameObjectWithTag("topMenu");
        zoomMenu = GameObject.FindGameObjectWithTag("gazetarget");
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        hideMenuObject(true, zoomMenu.tag, goldenPos_zoomMenu);
       
        gaze_targets = new GameObject[6];
        
        gaze_targets[0] = GameObject.FindGameObjectWithTag("gt_init");

        //hideMenuObject(false, gaze_targets[0].tag, goldenPos_init);
        
        gaze_targets[1] = GameObject.FindGameObjectWithTag("gt_1x");
        gaze_targets[2] = GameObject.FindGameObjectWithTag("gt_2x");
        gaze_targets[3] = GameObject.FindGameObjectWithTag("gt_2.5x");
        gaze_targets[4] = GameObject.FindGameObjectWithTag("gt_3x");
        gaze_targets[5] = GameObject.FindGameObjectWithTag("gt_cancel");

        //zoom modes
        discreteEnable = true;
        topMenu.SetActive(discreteEnable);
   
        continousEnable = false;
        gazeWatcher.SetActive(continousEnable);

        //2 step revert timer related
        rTimerStarted = false;
        dwellInProgress = false;
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
        else if (Input.GetKeyDown(KeyCode.O))
        {
            UnityEngine.Debug.Log("Menu Debug | moved init button");
            //gaze_targets[0].transform.localposition = goldenPos_init;
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

    //will revert menu to initial state 
    public void revertMenu()
    {
        /***
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
        ***/
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

    public void resetGazeTimers()
    {
        UnityEngine.Debug.Log("MenuHandlerDebug - Gaze timer reset started.)");
        setupGazeRef("reset");
        UnityEngine.Debug.Log("MenuHandlerDebug - Gaze timer reset finished.)");
    }

    private void setupGazeRef(string mode)
    {

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
                    for(int i = 0; i < gaze_targets.Length; i++)
                    {
                        if( i == 0)
                        {
                            gaze_targets[i].GetComponent<VarjoGazeTarget>().setClickTimer(primaryClickTime);
                        }
                        else
                        {
                            gaze_targets[i].GetComponent<VarjoGazeTarget>().setClickTimer(secondaryClickTime);
                        }
                        
                    }
                }
                catch(Exception e)
                {
                     UnityEngine.Debug.Log("MenuHandlerDebug | caught exception | " + e);
                }
                break;
            case "reset":

                for (int i = 0; i < gaze_targets.Length; i++)
                {
                    try
                    {
                        UnityEngine.Debug.LogError("MenuHandlerDebug | trying to reset  " + gaze_targets[i].tag);
                        gaze_targets[i].GetComponent<VarjoGazeTarget>().resetGazeTimer();
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError("MenuHandlerDebug | caught exception - " + e.ToString() + " at index " + i.ToString());
                    }
                }
                break;
            default:
                break;
        }
        

    }

    public void hideMenuObject(bool hide, String go_tag, Vector3 return_pos)
    {
        GameObject menu_item = GameObject.FindGameObjectWithTag(go_tag);

        if (hide)
        {
            //move the zoomMenu far far away but DO NOT deactivate it
            menu_item.transform.Translate(new Vector3(0f, 0f, 30f));  
            //values made need to change when headset is tracked
        }
        else
        {
            //bring it back to the front and center
            menu_item.transform.Translate(new Vector3(0f, 0f, -30f));
        }
    }
}
