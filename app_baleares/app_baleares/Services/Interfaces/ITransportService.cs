using app_baleares.Models;
using NetCoreBackend.Models;

namespace app_baleares.Services.Interfaces
{
    public interface ITransportService
    {
        Task<IEnumerable<Transporte>> GetAllTransportAsync();
        Task<Transporte> GetTransportByIdAsync(int id);
        Task<Transporte> SaveTransportAsync(Transporte transport);
        Task<Transporte> UpdateTransportAsync(Transporte transport);
        Task<Transporte> AsingContactIdTransportAsync(Transporte transport, int id);
        Task<bool> DeleteTransportAsync(int id);
    }
}

