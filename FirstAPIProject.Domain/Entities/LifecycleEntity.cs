using FirstAPIProject.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirstAPIProject.Domain.Entities
{
    public abstract class LifecycleEntity : BaseEntity, IActivatable, IDeletable
    {
        public bool IsActive { get; private set; } = true;

        public bool IsDeleted { get; private set; }

        public DateTime? DeletedAt { get; private set; }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Delete()
        {
            IsDeleted = true;
            IsActive = false;
            DeletedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            IsActive = true;
            DeletedAt = null;
        }
    }
}
