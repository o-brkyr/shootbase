using Sandbox;

namespace Shootbase;

public abstract partial class SimulateComponent : EntityComponent
{
    public virtual void Simulate(IClient player) { }
}

public abstract class SimulateComponent<T> : SimulateComponent
    where T : Entity
{
    public new T Entity => base.Entity as T;
}
