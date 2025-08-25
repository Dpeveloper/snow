using snow1.enums;
using snow1.Refrigerant;

namespace snow1.Interface
{
    public interface IComponent
    {
        string Name { get; }
        ComponentType Type { get; }
        RefrigerantState Process(RefrigerantState input);
        bool CanConnectTo(IComponent other);
    }
}
