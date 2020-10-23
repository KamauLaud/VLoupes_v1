using System;

namespace VarjoExample
{
    public class Trial
    {
        public int tNum;
        public float timeCompleted;
        public bool misClicked;
        public int errorGazeActs;

        public Trial(int trialNum, float timeToComplete, bool misClick, int errGazeActs)
        {
            this.tNum = trialNum;
            this.timeCompleted = timeToComplete;
            this.misClicked = misClick;
            this.errorGazeActs = errGazeActs;
        }

        public Trial()
        {
            this.tNum = 0;
            this.timeCompleted = 0;
            this.misClicked = false;
            this.errorGazeActs = 0;
        }
    }
}
