namespace AutoOrganize.Library.Services.Config;

public interface IConfig
{
    IConfig Clone();
    void CopyTo(IConfig target);
    void CopyFrom(IConfig source);

    bool CanCopy(IConfig target);
}

public interface IConfig<TSelf> : IConfig
    where TSelf : class, IConfig<TSelf>, new()
{
    static abstract void Copy(TSelf target, TSelf source);

    void CopyTo(TSelf target) => TSelf.Copy(target, (TSelf)this);

    void CopyFrom(TSelf source) => TSelf.Copy((TSelf)this, source);

    IConfig IConfig.Clone()
    {
        var clone = new TSelf();
        TSelf.Copy(clone, (TSelf)this);
        return clone;
    }

    new TSelf Clone()
    {
        var clone = new TSelf();
        TSelf.Copy(clone, (TSelf)this);
        return clone;
    }

    void IConfig.CopyTo(IConfig target) => CopyTo(target);

    void IConfig.CopyFrom(IConfig source) => CopyFrom(source);

    bool IConfig.CanCopy(IConfig target)
    {
        return target is TSelf;
    }
}