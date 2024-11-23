using app_baleares.Models;
using app_baleares.Views.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCoreBackend.Models;
using NetCoreBackend.Models.Enum;
using NetCoreBackend.Services.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace app_baleares.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContactService _contactService;

        public HomeController(ILogger<HomeController> logger, IContactService contactService)
        {
            _logger = logger;
            _contactService = contactService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("contactos")]
        public async Task<IActionResult> Contactos()
        {
            try
            {
                var viewModel = new ViewModelsContacts
                {

                    Contactos = await _contactService.GetAllContactsAsync(),
                    NuevoContacto = new Contacts()
                };

                if (viewModel.Contactos == null)
                {
                    viewModel.Contactos = new List<Contacts>();
                }
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los contactos.");
                return View("Error");
            }
        }
       
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Buscar(string buscar, string tipo)
        {
            try
            {
                IEnumerable<Contacts> contactos;

                if (string.IsNullOrEmpty(buscar))
                {
                    contactos = await _contactService.GetAllContactsAsync();
                }
                else
                {
                    switch (tipo.ToLower())
                    {
                        case "ciudad":
                            contactos = await _contactService.GetContactsByCityAsync(buscar);
                            if (!contactos.Any())
                            {
                                ViewData["ErrorSearch"] = "No se encontro el usuario que viva en:" + buscar;
                            }
                            break;
                        case "id":
                            if (int.TryParse(buscar, out int id))
                            {
                                var contacto = await _contactService.GetContactByIdAsync(id);
                                contactos = contacto != null ? new List<Contacts> { contacto } : new List<Contacts>();
                                if (!contactos.Any())
                                {
                                    ViewData["ErrorSearch"] = "No se encontro el usuario con ID: " + id;
                                }
                            }
                            else
                            {
                                _logger.LogError("ID no válido");
                                ViewData["ErrorSearch"] = "Escribio un id de tipo incorrecto (solo numeros) ";
                                contactos = new List<Contacts>();
                            }
                            break;
                        case "emailtelefono":
                            contactos = await _contactService.GetContactByEmailOrTelefonoAsync(buscar);
                          
                            if (!contactos.Any())
                            {
                                ViewData["ErrorSearch"] = "No se encontro el usuario que el correo:" + buscar;
                            }
                            break;
                        default:
                            _logger.LogError("Tipo de búsqueda no reconocido");
                            contactos = new List<Contacts>();
                            ViewData["ErrorSearch"] = "No se encontro un resultado con:" + buscar;
                            break;
                    }
                }

                var viewModel = new ViewModelsContacts
                {
                    Contactos = contactos,
                    NuevoContacto = new Contacts()
                };

                if (viewModel.Contactos == null)
                {
                    viewModel.Contactos = new List<Contacts>();
                }
                return View("contactos",viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los contactos.");
                return View("Error");
            }
        }

        [HttpPost("CreateContact")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateContact([FromForm] Contacts contact)
        {
            try
            {
                var savedContact = await _contactService.SaveContactAsync(contact);
                if (savedContact == null)
                {
                    ViewData["Error"] = "No se pudo guardar el contacto.";
                    return View("contactos");
                }
                var viewModel = new ViewModelsContacts
                {
                    Contactos = await _contactService.GetAllContactsAsync(),
                    NuevoContacto = new Contacts()
                };

                if (viewModel.Contactos == null)
                {
                    viewModel.Contactos = new List<Contacts>();
                }
                return View("contactos", viewModel);
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "Ocurrió un error al guardar el contacto:" + ex.Message;
                return View("contactos");
            }
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> EditContact(int id)
        {
            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> EditContact(Contacts contact)
        {
            if (ModelState.IsValid)
            {
                var updatedContact = await _contactService.UpdateContactAsync(contact);
                if (updatedContact != null)
                {
                    return RedirectToAction("contactos"); 
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al actualizar el contacto.");
                }
            }
            return View(contact);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> DeleteContact(int id)
        {
       
            var result = await _contactService.DeleteContactAsync(id);
            if (result)
            {
                return RedirectToAction("contactos"); 
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No se pudo eliminar el contacto.");
                return RedirectToAction("contactos"); 
            }
        }

    }
}
