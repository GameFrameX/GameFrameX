using System;
using System.Collections.Generic;

namespace UnityGameFramework.Runtime
{
    public class MonoComponent : GameFrameworkComponent
    {
        private Queue<Action> updateQueue = new Queue<Action>();
        private Queue<Action> invokeUpdateQueue = new Queue<Action>();

        void Update()
        {
            ObjectHelper.Swap(ref this.invokeUpdateQueue, ref this.updateQueue);

            while (invokeUpdateQueue.Count > 0)
            {
                var action = invokeUpdateQueue.Dequeue();
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}