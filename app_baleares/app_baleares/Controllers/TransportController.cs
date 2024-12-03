using app_baleares.Models;
using app_baleares.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCoreBackend.Models;

namespace app_baleares.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TransportController : Controller
    {
        private readonly ITransportService _transportService;

        public TransportController(ITransportService transportService)
        {
            _transportService = transportService;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SaveContactAPI(Transporte transporte)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            try
            {
                var savedContact = await _transportService.SaveTransportAsync(transporte);
                if (savedContact == null)
                {
                    return BadRequest(new { message = "No se pudo guardar el transporte." });
                }
                return Ok(savedContact);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al guardar el transporte.", error = ex.Message });
            }
        }


    }
}
