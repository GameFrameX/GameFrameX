using System;
using System.Collections.Generic;

using BestHTTP.Core;

namespace BestHTTP.Timings
{
    public sealed class TimingCollector
    {
        public HTTPRequest ParentRequest { get; }

        /// <summary>
        /// When the TimingCollector instance created.
        /// </summary>
        public DateTime Start { get; private set; }

        /// <summary>
        /// List of added events.
        /// </summary>
        public List<TimingEvent> Events { get; private set; }

        public TimingCollector(HTTPRequest parentRequest)
        {
            this.ParentRequest = parentRequest;
            this.Start = DateTime.Now;
        }

        internal void AddEvent(string name, DateTime when, TimeSpan duration)
        {
            if (this.Events == null)
                this.Events = new List<TimingEvent>();

            if (duration == TimeSpan.Zero)
            {
                DateTime prevEventAt = this.Start;
                if (this.Events.Count > 0)
                    prevEventAt = this.Events[this.Events.Count - 1].When;
                duration = when - prevEventAt;
            }
            this.Events.Add(new TimingEvent(name, when, duration));
        }

        /// <summary>
        /// Add an event. Duration is calculated from the previous event or start of the collector.
        /// </summary>
        public void Add(string name)
        {
            RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.ParentRequest, name, DateTime.Now));
        }

        /// <summary>
        /// Add an event with a known duration.
        /// </summary>
        public void Add(string name, TimeSpan duration)
        {
            RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.ParentRequest, name, duration));
        }

        public TimingEvent FindFirst(string name)
        {
            if (this.Events == null)
                return TimingEvent.Empty;

            for (int i = 0; i < this.Events.Count; ++i)
            {
                if (this.Events[i].Name == name)
                    return this.Events[i];
            }

            return TimingEvent.Empty;
        }

        public TimingEvent FindLast(string name)
        {
            if (this.Events == null)
                return TimingEvent.Empty;

            for (int i = this.Events.Count - 1; i >= 0; --i)
            {
                if (this.Events[i].Name == name)
                    return this.Events[i];
            }

            return TimingEvent.Empty;
        }

        public override string ToString()
        {
            string result = string.Format("[TimingCollector Start: '{0}' ", this.Start.ToLongTimeString());

            if (this.Events != null)
                foreach (var @event in this.Events)
                    result += '\n' + @event.ToString();

            result += "]";

            return result;
        }
    }
}
