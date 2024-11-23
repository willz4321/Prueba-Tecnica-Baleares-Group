using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCoreBackend.Models;
using NetCoreBackend.Services.Interfaces;

namespace NetCoreBackend.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContactsAPI()
        {
            try
            {
                var contacts = await _contactService.GetAllContactsAsync();
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener todos los contactos.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactByIdAPI(int id)
        {
            try
            {
                var contact = await _contactService.GetContactByIdAsync(id);
                if (contact == null) return NotFound(new { message = "Contacto no encontrado." });
                return Ok(contact);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener el contacto por ID.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SaveContactAPI(Contacts contact)
        {
            try
            {
                var savedContact = await _contactService.SaveContactAsync(contact);
                if (savedContact == null)
                {
                    return BadRequest(new { message = "No se pudo guardar el contacto." });
                }
                return CreatedAtAction(nameof(GetContactByIdAPI), new { id = savedContact.Id }, savedContact);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al guardar el contacto.", error = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateContactAPI(Contacts contact)
        {
            try
            {
                var updatedContact = await _contactService.UpdateContactAsync(contact);
                if (updatedContact == null)
                {
                    return NotFound(new { message = "No se encontró el contacto para actualizar." });
                }
                return Ok(updatedContact);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al actualizar el contacto.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteContactAPI(int id)
        {
            try
            {
                var success = await _contactService.DeleteContactAsync(id);
                if (!success) return NotFound(new { message = "No se encontró el contacto para eliminar." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar el contacto.");
            }
        }

        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetContactByEmailAPI(string email)
        {
            try
            {
                var contact = await _contactService.GetContactByEmailOrTelefonoAsync(email);
                if (contact == null)
                {
                    return NotFound(new { message = "No se encontró ningún contacto con ese email." });
                }
                return Ok(contact);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener el contacto por email.");
            }
        }

        [HttpGet("by-city/{city}")]
        public async Task<IActionResult> GetContactsByCityAPI(string city)
        {
            try
            {
                var contacts = await _contactService.GetContactsByCityAsync(city);
                if (contacts == null || !contacts.Any())
                {
                    return NotFound(new { message = "No se encontraron contactos en la ciudad especificada." });
                }
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener los contactos por ciudad.");
            }
        }

        [HttpGet("getAll-by-email")]
        public async Task<IActionResult> GetContactsSortedByEmailAPI()
        {
            try
            {
                var contacts = await _contactService.GetAllContactsAsync();
                var sortedContacts = contacts.OrderBy(c => c.Email);
                return Ok(sortedContacts);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener los contactos ordenados por correo.");
            }
        }

        private IActionResult HandleException(Exception ex, string customMessage)
        {
            Console.WriteLine($"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = customMessage,
                detail = "Ha ocurrido un error interno en el servidor."
            });
        }
    }

}
