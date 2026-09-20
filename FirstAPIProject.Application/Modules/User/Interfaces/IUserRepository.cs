using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirstAPIProject.Application.Modules.User.Interfaces
{
    public interface IUserRepository : IGenericRepository<FirstAPIProject.Domain.Entities.User>
    {
        Task<FirstAPIProject.Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
