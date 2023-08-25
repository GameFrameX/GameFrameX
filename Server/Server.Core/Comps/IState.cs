namespace Server.Core.Comps
{
    public interface IState
    {
        public Task ReadStateAsync();
    }
}