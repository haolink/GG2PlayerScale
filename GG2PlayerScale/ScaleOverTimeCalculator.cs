/**
 * Copyright 2018, haolink
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace GG2PlayerScale
{
    class ScaleOverTimeCalculator
    {
        /// <summary>
        /// Is the calculator enabled.
        /// </summary>
        private bool _enabled;

        /// <summary>
        /// Are we paused?
        /// </summary>
        private bool _paused;

        /// <summary>
        /// Getter for the enabled state.
        /// </summary>
        public bool Enabled { get { return _enabled;  } }

        /// <summary>
        /// Getter for the paused state.
        /// </summary>
        public bool Paused { get { return _paused; } }

        /// <summary>
        /// Start time.
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// Pause start.
        /// </summary>
        private DateTime _pauseStart;

        /// <summary>
        /// Start scale.
        /// </summary>
        private float _startScale;

        /// <summary>
        /// Is a target scale set?
        /// </summary>
        private float? _targetScale;

        /// <summary>
        /// Target scale getter.
        /// </summary>
        public float? TargetScale { get { return _targetScale;  } }

        /// <summary>
        /// Base per second.
        /// </summary>
        private double _base;

        /// <summary>
        /// Output time.
        /// </summary>
        private Label _timeBox;

        /// <summary>
        /// Last second display.
        /// </summary>
        private int _lastSecondDisplay;

        /// <summary>
        /// Last scale.
        /// </summary>
        private float _lastScale;

        /// <summary>
        /// Event on completed progess.
        /// </summary>
        public event EventHandler<EventArgs> Completed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeBox"></param>
        public ScaleOverTimeCalculator(Label timeBox)
        {
            this._timeBox = timeBox;
            this._enabled = false;
            this._paused = false;                 
            MainForm.WriteLabelThreadSafe(this._timeBox, "-");
        }

        /// <summary>
        /// Starts the scaling algorithm.
        /// </summary>
        /// <param name="startScale">Start scale of the player</param>
        /// <param name="baseScale">Base multiplication target over time</param>
        /// <param name="baseSeconds">Seconds required to reach target</param>
        /// <param name="countdown">Countdown until scaling starts.</param>
        /// <param name="targetScale">Target scale when to end the progress.</param>
        public void StartProcess(float startScale, float baseScale, double baseSeconds, int countdown, float? targetScale)
        {
            if (this._enabled)
            {
                throw new Exception("Already running");
            }

            this._startScale = startScale;
            this._base = Math.Pow(baseScale, (1.0 / baseSeconds));
            this._lastSecondDisplay = -1;

            DateTime startTime = DateTime.Now + new TimeSpan(0, 0, countdown);

            this._startTime = startTime;
            this._targetScale = targetScale;

            this._paused = false;
            this._enabled = true;                
        }

        /// <summary>
        /// Pause a process.
        /// </summary>
        public void Pause()
        {
            if (!this._enabled)
            {
                throw new Exception("Not running");
            }
            if (this._paused)
            {
                return;
                //Nothing to do
            }

            this._pauseStart = DateTime.Now;
            this._paused = true;
        }

        /// <summary>
        /// Resumes scaling.
        /// </summary>
        public void Resume()
        {
            if (!this._enabled)
            {
                throw new Exception("Not running");
            }
            if (!this._paused)
            {
                return;
                //Not paused nothing to do.
            }

            TimeSpan timeSincePause = DateTime.Now - this._pauseStart;
            this._startTime += timeSincePause;
            this._paused = false;
        }

        /// <summary>
        /// Stops the scaling.
        /// </summary>
        /// <returns>Last scale.</returns>
        public float StopProcess()
        {
            if (!this._enabled)
            {
                throw new Exception("Not running");
            }

            float lastValue = this.GetCurrentScale();
            this._enabled = false;
            this._paused = false;
            MainForm.WriteLabelThreadSafe(this._timeBox, "-");

            return lastValue;
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopProcessInternally()
        {
            if (!this._enabled)
            {
                throw new Exception("Not running");
            }

            MainForm.WriteLabelThreadSafe(this._timeBox, "-");
            this._enabled = false;
            this._paused = false;

            this.Completed?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets the current scale.
        /// </summary>
        public float GetCurrentScale()
        {
            if(!this._enabled)
            {
                throw new Exception("Not running");
            }

            if(this._paused)
            {
                return this._lastScale;
            }

            TimeSpan seconds = (DateTime.Now - this._startTime);

            this.UpdateSecondDisplay(seconds);

            double secondsFloat = seconds.TotalSeconds;
            if (secondsFloat < 0)
            {
                return this._startScale;
            }
            
            this._lastScale = (float)(this._startScale * Math.Pow(this._base, secondsFloat));

            if(this._targetScale != null)
            {
                if( (this._lastScale < this._targetScale && this._base < 1) || 
                    (this._lastScale > this._targetScale && this._base > 1))
                {
                    this._lastScale = this._targetScale.Value;
                    this.StopProcessInternally();
                }                
            }

            return this._lastScale;
        }

        /// <summary>
        /// Generates a new second display.
        /// </summary>
        private void UpdateSecondDisplay(TimeSpan seconds)
        {
            double totalSeconds = seconds.TotalSeconds;
            int secondsInt = (int)Math.Floor(totalSeconds);

            if(secondsInt == this._lastSecondDisplay)
            {
                return;
            }

            this._lastSecondDisplay = secondsInt;

            string timeDisplay = "";
            if (totalSeconds >= 0)
            {
                timeDisplay = String.Format("{0:0}:{1:00}:{2:00}", new object[] { seconds.Hours, seconds.Minutes, seconds.Seconds });
            }
            else
            {
                secondsInt = (int)Math.Round(seconds.Negate().TotalSeconds);
                timeDisplay = secondsInt.ToString();
            }

            MainForm.WriteLabelThreadSafe(this._timeBox, timeDisplay);
        }
    }
}
