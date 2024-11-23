using NetCoreBackend.Models;
using NetCoreBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using app_baleares.Data;

namespace NetCoreBackend.Services
{
    public class ContactService: IContactService
    {
        private readonly AppDbContext _context;

        public ContactService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contacts>> GetAllContactsAsync()
        {
            return await _context.Contacts.ToListAsync();
        }

        public async Task<Contacts> GetContactByIdAsync(int id)
        {
            return await _context.Contacts.FindAsync(id);
        }

        public async Task<Contacts> SaveContactAsync(Contacts contact)
        {
            await _context.Contacts.AddAsync(contact); 
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task<List<Contacts>> GetContactsByCityAsync(string ciudad)
        {
            if (string.IsNullOrWhiteSpace(ciudad))
            {
                throw new ArgumentException("La ciudad no puede estar vacía.", nameof(ciudad));
            }

            return await _context.Contacts
                .Include(c => c.Direccion)
                .Where(c => c.Direccion.Localidad.ToLower().StartsWith(ciudad.ToLower()))
                .ToListAsync();
        }

        public async Task<List<Contacts>> GetContactByEmailOrTelefonoAsync(string emailOrTelefono)
        {
            //En este metodo basicamente primero confirmo si el string que llega es un numero o no, en caso de que si entonces busco por telefono.
            if (string.IsNullOrWhiteSpace(emailOrTelefono))
            {
                throw new ArgumentException("El parámetro de búsqueda no puede estar vacío.", nameof(emailOrTelefono));
            }

            bool isPhone = emailOrTelefono.All(char.IsDigit);

            return await _context.Contacts
                .Where(c => isPhone
                    ? c.Telefono.Contains(emailOrTelefono) 
                    : c.Email.ToLower().Contains(emailOrTelefono.ToLower())) 
                .ToListAsync();
        }

        public async Task<Contacts> UpdateContactAsync(Contacts contact)
        {
            var existingContact = await _context.Contacts.FindAsync(contact.Id);
            if (existingContact == null) return null;

            _context.Entry(existingContact).CurrentValues.SetValues(contact);

            await _context.SaveChangesAsync();
            return existingContact;
        }

        public async Task<bool> DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return false;

            _context.Contacts.Remove(contact);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
