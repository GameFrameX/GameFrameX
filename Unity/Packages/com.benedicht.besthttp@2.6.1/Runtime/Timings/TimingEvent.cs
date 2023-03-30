using System;

namespace BestHTTP.Timings
{
    public struct TimingEvent : IEquatable<TimingEvent>
    {
        public static readonly TimingEvent Empty = new TimingEvent(null, TimeSpan.Zero);
        /// <summary>
        /// Name of the event
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Duration of the event.
        /// </summary>
        public readonly TimeSpan Duration;

        /// <summary>
        /// When the event occurred.
        /// </summary>
        public readonly DateTime When;

        public TimingEvent(string name, TimeSpan duration)
        {
            this.Name = name;
            this.Duration = duration;
            this.When = DateTime.Now;
        }

        public TimingEvent(string name, DateTime when, TimeSpan duration)
        {
            this.Name = name;
            this.When = when;
            this.Duration = duration;
        }

        public TimeSpan CalculateDuration(TimingEvent @event)
        {
            if (this.When < @event.When)
                return @event.When - this.When;

            return this.When - @event.When;
        }

        public bool Equals(TimingEvent other)
        {
            return this.Name == other.Name &&
                   this.Duration == other.Duration &&
                   this.When == other.When;
        }

        public override bool Equals(object obj)
        {
            if (obj is TimingEvent)
                return this.Equals((TimingEvent)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return (this.Name != null ? this.Name.GetHashCode() : 0) ^
                this.Duration.GetHashCode() ^
                this.When.GetHashCode();
        }

        public static bool operator ==(TimingEvent lhs, TimingEvent rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TimingEvent lhs, TimingEvent rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override string ToString()
        {
            return string.Format("['{0}': {1}]", this.Name, this.Duration);
        }
    }
}
