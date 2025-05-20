namespace MauiCrudApp.Common.Interfaces;

public interface ILifecycle
{
    Task PerformInitializeAsync();
    Task PerformFinalizeAsync();
}
