using Microsoft.AspNetCore.Mvc;
using PeopleHub.Data;
using PeopleHub.Models;

namespace PeopleHub.Controllers
{
    // Shared reference data other pages/forms pull from (dropdowns, filters...).
    [ApiController]
    [Route("api/[controller]")]
    public class LookupsController : ControllerBase
    {
        [HttpGet("departments")]
        public ActionResult<List<LookupItem>> GetDepartments()
        {
            var departments = Departments.All
                .Select((name, index) => new LookupItem(index + 1, name))
                .ToList();
            return Ok(departments);
        }
    }
}
