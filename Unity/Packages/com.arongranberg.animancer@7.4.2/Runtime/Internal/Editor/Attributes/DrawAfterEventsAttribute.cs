// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;

namespace Animancer
{
    /// <summary>[Editor-Conditional]
    /// Causes an Inspector field in an <see cref="ITransition"/> to be drawn after its events where the events would
    /// normally be drawn last.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/DrawAfterEventsAttribute
    /// 
    [AttributeUsage(AttributeTargets.Field)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class DrawAfterEventsAttribute : Attribute { }
}

