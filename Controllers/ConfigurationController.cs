using System;
using System.Net;
using System.Threading.Tasks;
using IsIoTWeb.Models;
using IsIoTWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text;
using IsIoTWeb.Utils;
using System.Linq;

namespace IsIoTWeb.Controllers
{
    [Authorize(Roles = "ADMINISTRATOR")]
    public class ConfigurationController : Controller
    {
        ISinkRepository _sinkRepository;

        public ConfigurationController(ISinkRepository sinkRepository)
        {
            _sinkRepository = sinkRepository;
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public async Task<IActionResult> AddCollector(string id)
        {
            try
            {
                await _sinkRepository.AddCollector(id);
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "Failed to add collector!" } });
            }

            return StatusCode((int)HttpStatusCode.OK);
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCollector(string id)
        {
            try
            {
                await _sinkRepository.RemoveCollector(id);
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "Failed to remove collector!" } });
            }
            return StatusCode((int)HttpStatusCode.OK);
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public async Task<ActionResult> GetCollectors()
        {
            try
            {
                Sink sink = await _sinkRepository.GetSink();
                return Json(sink.Collectors.ToList());
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "An error occured when fetching collectors' data!" } });
            }
        }
    }
}
