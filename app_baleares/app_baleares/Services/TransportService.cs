using app_baleares.Data;
using app_baleares.Models;
using app_baleares.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetCoreBackend.Models;
using System.Security.Cryptography.Xml;

namespace app_baleares.Services
{
    public class TransportService : ITransportService
    {
        private readonly AppDbContext _context;

        public TransportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transporte>> GetAllTransportAsync()
        {
            return await _context.Transport.ToListAsync();
        }

        public async Task<Transporte> GetTransportByIdAsync(int id)
        {
            return await _context.Transport.FindAsync(id);
        }

        public async Task<Transporte> SaveTransportAsync(Transporte transporte)
        {
            await _context.Transport.AddAsync(transporte);
            await _context.SaveChangesAsync();
            return transporte;
        }

        public async Task<Transporte> UpdateTransportAsync(Transporte transport)
        {
            var existingTransport = await _context.Transport.FindAsync(transport.Id);
            if (existingTransport == null) return null;

            _context.Entry(existingTransport).CurrentValues.SetValues(transport);

            await _context.SaveChangesAsync();
            return existingTransport;
        }
        public async Task<Transporte> AsingContactIdTransportAsync(Transporte transporte, int id)
        {
            var existingTransport = await _context.Transport.FindAsync(transporte.Id);
            if (existingTransport == null) return null;

            _context.Entry(existingTransport).CurrentValues.SetValues(transporte);

            await _context.SaveChangesAsync();
            return existingTransport;
        } 

        public async Task<bool> DeleteTransportAsync(int id)
        {
            var tranport = await _context.Transport.FindAsync(id);
            if (tranport == null) return false;

            _context.Transport.Remove(tranport);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
