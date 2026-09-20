using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAvatarAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken cancellationToken = default);
    }
}
