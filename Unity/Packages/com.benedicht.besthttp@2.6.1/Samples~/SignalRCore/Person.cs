#if !BESTHTTP_DISABLE_SIGNALR_CORE

using System;
using System.Collections.Generic;

namespace BestHTTP.Examples
{
    public enum PersonStates
    {
        Unknown,
        Joined
    }

    /// <summary>
    /// Helper class to demonstrate strongly typed callbacks
    /// </summary>
    internal sealed class Person
    {
        public UnityEngine.Vector3[] Positions { get; set; }
        public string Name { get; set; }
        public long Age { get; set; }
        public DateTime Joined { get; set; }
        public PersonStates State { get; set; }
        public List<Person> Friends { get; set; }

        public override string ToString()
        {
            return string.Format("[Person Name: '{0}', Age: '<color=yellow>{1}</color>', Joined: {2}, State: {3}, Friends: {4}, Position: {5}]",
                this.Name, this.Age, this.Joined, this.State, this.Friends != null ? this.Friends.Count : 0, this.Positions != null ? this.Positions.Length : 0);
        }
    }
}

#endif
