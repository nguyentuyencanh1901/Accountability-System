using Example.Common.Base.Interfaces;

namespace Example.Common.Base
{
    public abstract class EntityBase<TKey> : IEntityBase<TKey>
    {
        public TKey Id { get; set; }
    }
}
