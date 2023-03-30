// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Diagnostics;

namespace Animancer
{
    /// <summary>A very simple timer system based on a <see cref="System.Diagnostics.Stopwatch"/>.</summary>
    public struct SimpleTimer : IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The system used to track time.</summary>
        public static readonly Stopwatch
             Stopwatch = Stopwatch.StartNew();

        /// <summary>The amount of time that has passed (in seconds) since the first timer was started.</summary>
        public static double CurrentTime
             => Stopwatch.Elapsed.TotalSeconds;

        /************************************************************************************************************************/

        /// <summary>An optional prefix for <see cref="ToString"/>.</summary>
        public string name;

        /// <summary>The <see cref="CurrentTime"/> from when this timer instance was started.</summary>
        public double startTime;

        /// <summary>The total amount of time this timer instance has been running (in seconds).</summary>
        public double total;

        /// <summary>The number format used by <see cref="ToString"/>.</summary>
        const string Format = "0.000";

        /************************************************************************************************************************/

        /// <summary>Has <see cref="Start()"/> been called and <see cref="Stop"/> not?</summary>
        public bool IsStarted
            => startTime != 0;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SimpleTimer"/> with the specified `name`.</summary>
        /// <remarks>
        /// You will need to call <see cref="Start()"/> to start the timer. Or use the static
        /// <see cref="Start(string)"/>
        /// </remarks>
        public SimpleTimer(string name)
        {
            this.name = name;
            startTime = 0;
            total = 0;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SimpleTimer"/> with the specified `name` and starts it.</summary>
        public static SimpleTimer Start(string name = null)
             => new SimpleTimer
             {
                 name = name,
                 startTime = CurrentTime,
             };

        /************************************************************************************************************************/

        /// <summary>
        /// Stores the <see cref="CurrentTime"/> in <see cref="startTime"/> so that <see cref="Stop"/> will be able to
        /// calculate how much time has passed.
        /// </summary>
        /// <remarks>Does nothing if the <see cref="startTime"/> was already set.</remarks>
        public bool Start()
        {
            if (startTime != 0)
                return false;

            startTime = CurrentTime;
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Adds the amount of time that has passed since the <see cref="startTime"/> to the <see cref="total"/> and
        /// clears the <see cref="startTime"/>.
        /// </summary>
        /// <remarks>Does nothing if the <see cref="startTime"/> was already cleared (or not set).</remarks>
        public bool Stop()
        {
            if (startTime == 0)
                return false;

            var endTime = CurrentTime;
            total += endTime - startTime;
            startTime = 0;
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="Stop"/> and returns a string representation of the <see cref="total"/>.</summary>
        public override string ToString()
        {
            Stop();
            return string.IsNullOrEmpty(name)
                ? total.ToString(Format)
                : $"{name}: {total.ToString(Format)}";
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="ToString"/> and logs the result.</summary>
        public void Dispose()
        {
            var text = ToString();
            UnityEngine.Debug.Log(text);
        }

        /************************************************************************************************************************/
    }
}

