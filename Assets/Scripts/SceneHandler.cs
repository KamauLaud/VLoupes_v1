using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using VarjoExample;

public class SceneHandler : MonoBehaviour
{
    private readonly int numTrials = 50;
    private readonly float lowerBound = 1.0f;
    private readonly float upperBound = 7.0f;
    public const bool testing = false;
    private bool startedExp;
    private int completedTrials;
    public static int correctTarget;
    public static int errorGazeActs;

    private ArrayList trialLog;
    private Stopwatch taskCompletionTimer;
    private TextMesh targetDisplay;
    private GameObject bgnd;
    private GameObject parentGT;

    void OnEnable()
    {
        VarjoGazeTarget.GazeClicked += finishScene;

    }

    void OnDisable()
    {
        VarjoGazeTarget.GazeClicked -= finishScene;
    }

    // Start is called before the first frame update
    void Start()
    {
        correctTarget = -1;
        completedTrials = 0;
        errorGazeActs = 0;
        startedExp = false;
        
        trialLog = new ArrayList();
        taskCompletionTimer = new Stopwatch();
        targetDisplay = GameObject.FindGameObjectWithTag("textMesh_targetDisplay").GetComponent<TextMesh>();
        bgnd = GameObject.FindGameObjectWithTag("bgnd");
        parentGT = GameObject.FindGameObjectWithTag("gazetarget");
        parentGT.SetActive(false);
        bgnd.SetActive(false);
        targetDisplay.text = "Press J to start.";
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.J) && !startedExp){
            startedExp = true;
            setupTrial();
        }
      
    }

    private void setupTrial()
    {
        //choose target
        correctTarget = Random.Range((int)lowerBound, (int) upperBound);
        //tell user what number to dwell on
        bgnd.SetActive(true);
        targetDisplay.text = correctTarget.ToString();
        //maybe make number more visible or something
        parentGT.SetActive(true);

        //start timer
        taskCompletionTimer.Reset();
        taskCompletionTimer.Start();

    }

    IEnumerator setupTrial(float delay)
    {
        yield return new WaitForSeconds(delay);
        setupTrial();
    }

    private void finishScene(bool misClicked, int fakeActives )
    {
        //add trial to log and maybe setup Next scene?
        taskCompletionTimer.Stop();
        //TODO hide the number from correctGazeNumber from sight

        //Hide all gazeTargets
        parentGT.SetActive(false);
        bgnd.SetActive(false);

        Trial curTrial;
        //collect relevant data (errors, time, etc)
        if ( misClicked )
        {
            //if you select the wrong gaze target, the time is not recorded
            curTrial = new Trial(completedTrials, -1.0f, misClicked, fakeActives);
            trialLog.Add(curTrial);
        }
        else
        {
            
            curTrial = new Trial(completedTrials, (float)taskCompletionTimer.Elapsed.TotalSeconds, misClicked, fakeActives); 
            completedTrials += 1;
            trialLog.Add(curTrial);
        }

        errorGazeActs = 0;

        //if we have completed the number of trials correctly, display the data and shut it down
        if(completedTrials == numTrials)
        {
            //shut it all down
            targetDisplay.text = "DONE!";
            produceReport();
            Application.Quit();
        }
        else //start up the next trial after [2,7] seconds 
        {
            float delay = Random.Range(lowerBound, upperBound);
            StartCoroutine(setupTrial(delay));
        }
    }

    //Summarize all of the information from the X number of trials
    public void produceReport()
    {
        print("__________________________________________________________");
        print("------------------ Gaze study results --------------------");
        float totalTime = 0f;
        int totalMisClicks = 0;
        int totalErrGazeActs = 0;

        for(int i = 0; i < trialLog.Count; i++)
        {
            Trial t = (Trial)trialLog[i];
            if (!t.misClicked)
            {
                totalTime += t.timeCompleted;
            }
            else
            {
                totalMisClicks += 1;
            }
            totalErrGazeActs += t.errorGazeActs;
        }
        print("Number of actual trials attempted - " + trialLog.Count);
        print("Number of trials completed successfuly - " + numTrials);
        print("Number of falsely selected gazeTargets - " + totalMisClicks);
        print("Average trial completion time - " + totalTime/ ((float)numTrials) + " seconds");
        print("Total # falsely started gaze timers - " + totalErrGazeActs);
        print("Average # of falsely started gaze timers - " + totalErrGazeActs / ((float) trialLog.Count));
        print("__________________________________________________________");
    }

}