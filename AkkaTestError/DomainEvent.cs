using System;

namespace AkkaTestError
{
    public abstract class DomainEvent
    {
        protected DomainEvent(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        protected bool Equals(DomainEvent other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DomainEvent) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}