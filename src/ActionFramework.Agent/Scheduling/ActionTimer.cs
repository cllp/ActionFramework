﻿using ActionFramework.Agent.App;
using ActionFramework.Scheduling;
using System;
using System.Threading;

namespace ActionFrameworkAgent.Scheduling
{
    public class ActionTimer
    {
        public string AppName { get; }
        public string ActionName { get; }
        public Timer Timer { get; set; }

        public ActionTimer(ActionSchedule actionSchedule)
        {
            AppName = actionSchedule.AppName;
            ActionName = actionSchedule.ActionName;

            var timerState = new TimerState(actionSchedule);

            var dueTime = TimeSpan.FromSeconds(1); //default due time
            if (actionSchedule.NextRun > DateTime.UtcNow)
            {
                dueTime = actionSchedule.NextRun - DateTime.UtcNow;
            }

            // Create a timer. The timer fires once, and gets a new due-time when the action has finished running. 
            Timer = new Timer(RunScheduledAction, timerState, dueTime, TimeSpan.FromMilliseconds(-1)); //-1 disables periodic runs

            // Keep a handle to the timer, so it can be disposed.
            timerState.Timer = Timer;
        }

        static void RunScheduledAction(object state)
        {
            var s = (TimerState)state;
            s.Counter++;
            Console.WriteLine("{0} Running scheduled action {1}.", DateTime.UtcNow.TimeOfDay, s.Counter);
           
            if ((s.ActionSchedule.StopDateTime != DateTime.MinValue && s.ActionSchedule.StopDateTime <= DateTime.UtcNow))
            {
                Console.WriteLine("disposing timer because stop date..."); //todo: remove
                s.Timer.Dispose();
                s.Timer = null;
            }
            else
            {
                AppRepository.RunAction(s.ActionSchedule.AppName, s.ActionSchedule.ActionName);

                var nextRunDate = DateTime.UtcNow.AddSeconds(s.ActionSchedule.IntervalAsSeconds());

                //update timers due time to trigger the next run
                s.Timer.Change(nextRunDate - DateTime.UtcNow, TimeSpan.FromMilliseconds(-1));

                //update schedule with next run date
                s.ActionSchedule.NextRun = nextRunDate;
                ActionScheduleRepository.SaveActionSchedule(s.ActionSchedule);
            }

        }

    }


    public class TimerState
    {
        public int Counter { get; set; } //will this be used?
        //public bool IsRunning { get; set; }
        public Timer Timer { get; set; }
        public ActionSchedule ActionSchedule { get; set; }

        public TimerState(ActionSchedule actionSchedule)
        {
            Counter = 0;
            //IsRunning = false;
            ActionSchedule = actionSchedule;
        }
    }
}
