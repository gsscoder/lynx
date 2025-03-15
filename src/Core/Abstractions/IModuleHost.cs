namespace Lynx.Core.Abstractions;

public interface IModuleHost
{
    void RegisterModule<T>() where T : Module;

    Module GetModule(string name);
}
