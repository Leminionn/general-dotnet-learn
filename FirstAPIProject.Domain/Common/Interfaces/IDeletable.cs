using System;
using System.Collections.Generic;
using System.Text;

namespace FirstAPIProject.Domain.Common.Interfaces
{
    public interface IDeletable
    {
        bool IsDeleted { get; }

        DateTime? DeletedAt { get; }

        void Delete();

        void Restore();
    }
}
