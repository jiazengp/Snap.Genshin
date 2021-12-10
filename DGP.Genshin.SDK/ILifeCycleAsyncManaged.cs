namespace DGP.Genshin.SDK
{
    public interface ILifeCycleAsyncManaged
    {
        Task InitializeAsync();
        Task UnInitializeAsync();
    }
}