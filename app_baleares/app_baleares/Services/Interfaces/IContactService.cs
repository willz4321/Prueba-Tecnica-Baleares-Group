using NetCoreBackend.Models;

namespace NetCoreBackend.Services.Interfaces
{
    public interface IContactService
    {
        Task<IEnumerable<Contacts>> GetAllContactsAsync();
        Task<Contacts> GetContactByIdAsync(int id);
        Task<Contacts> SaveContactAsync(Contacts contact);
        Task<List<Contacts>> GetContactsByCityAsync(string ciudad);
        Task<List<Contacts>> GetContactByEmailOrTelefonoAsync(string emailOrTelefono);
        Task<Contacts> UpdateContactAsync(Contacts contact);
        Task<bool> DeleteContactAsync(int id);
    }
}
