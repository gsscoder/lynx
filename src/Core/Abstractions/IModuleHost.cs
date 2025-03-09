namespace Lynx.Core.Abstractions;

public interface IModuleHost
{
    void RegisterModule<T>() where T : Module;

    T GetModule<T>(string name) where T : Module;
}
