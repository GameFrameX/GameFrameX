using System;

namespace Framework.EventSystem
{
    public interface IEventSystem
    {
        void Add(int id, EventArgs args);
        void Remove(int id);
        void Fire(object sender, EventArgs args);
        void FireNow(object sender, EventArgs args);
    }

    public class EventSystem : GameComponentModule, IEventSystem
    {
        public void Fire(object sender, EventArgs args)
        {
        }

        public override int Priority => 0;


        public override void Active()
        {
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override void Shutdown()
        {
        }
    }
}