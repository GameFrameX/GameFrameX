namespace Luban.Editor
{
    public interface IBeforeGen
    {
        void Process();
    }

    public interface IAfterGen
    {
        void Process();
    }
}