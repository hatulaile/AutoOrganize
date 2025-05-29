using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Library.Services.Config;

public interface IConfig : INotifyPropertyChanged
{
    IConfig Clone();

    void CopyTo(IConfig target);

    void CopyFrom(IConfig source);

    bool CanCopy(IConfig target);
}

public interface IConfig<TSelf> : IConfig
    where TSelf : class, IConfig<TSelf>, new()
{
    void CopyTo(TSelf target);

    void CopyFrom(TSelf source);

    new TSelf Clone();
}

public abstract class ConfigBase<TSelf> : ObservableObject, IConfig<TSelf>
    where TSelf : ConfigBase<TSelf>, new()
{
    protected abstract void CopyMembers(TSelf target, TSelf source);

    public static void Copy(TSelf source, TSelf target) =>
        source.CopyTo(target);

    public virtual void CopyTo(TSelf target)
    {
        CopyMembers(target, (TSelf)this);
    }

    public virtual void CopyFrom(TSelf source)
    {
        CopyMembers((TSelf)this, source);
    }

    public virtual TSelf Clone()
    {
        var clone = new TSelf();
        clone.CopyFrom(this);
        return clone;
    }

    IConfig IConfig.Clone() => Clone();

    public void CopyTo(IConfig target) => CopyTo((TSelf)target);

    public void CopyFrom(IConfig source) => CopyFrom((TSelf)source);

    public virtual bool CanCopy(IConfig target)
    {
        return target is TSelf;
    }
}