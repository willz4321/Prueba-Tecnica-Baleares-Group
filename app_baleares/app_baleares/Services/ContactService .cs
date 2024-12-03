using NetCoreBackend.Models;
using NetCoreBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using app_baleares.Data;
using app_baleares.Models;
using app_baleares.Services.Interfaces;

namespace NetCoreBackend.Services
{
    public class ContactService: IContactService
    {
        private readonly AppDbContext _context;
        private readonly ITransportService transportService;

        public ContactService(AppDbContext context, ITransportService transportServiceD)
        {
            _context = context;
            transportService = transportServiceD;
        }

        public async Task<IEnumerable<Contacts>> GetAllContactsAsync()
        {
            return await _context.Contacts
                 .Include(c => c.TransporteContacto)
                 .ToListAsync();
        }

        public async Task<Contacts> GetContactByIdAsync(int id)
        {
            return await _context.Contacts.FindAsync(id);
        }

        public async Task<Contacts> SaveContactAsync(Contacts contact)
        {
            try
            {
                
                if (contact.TransporteContacto != null)
                {
                    Transporte transporte = await transportService.GetTransportByIdAsync(contact.TransporteContacto.Id);

                    if (transporte != null)
                    {
                        contact.TransporteContacto = transporte;
                    }
                    else
                    {
                        contact.TransporteContacto = null;
                        Console.WriteLine("El transporte no se encontró.");
                    }
                }
                else
                {
                    contact.TransporteContacto = null;
                    Console.WriteLine("El transporte del contacto es null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar el transporte: {ex.Message}");
            }

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

            try
            {

                if (contact.TransporteContacto != null)
                {
                    Console.WriteLine($"id de contacto: {contact.TransporteContacto.Id}");
                    Transporte transportUpdate = await transportService.GetTransportByIdAsync(contact.TransporteContacto.Id);

                    if (transportUpdate != null)
                    {
                        contact.TransporteId = transportUpdate.Id;
                    }
                    else
                    {
                        contact.TransporteId = (await transportService.SaveTransportAsync(existingContact.TransporteContacto)).Id;
                    }
                }
                else
                {
                    contact.TransporteContacto = null;
                    Console.WriteLine("El transporte del contacto es null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar el transporte: {ex.Message}");
            }
   
              
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
