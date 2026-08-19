using Microsoft.AspNetCore.Mvc;
using PeopleHub.Models;
using PeopleHub.Services;

namespace PeopleHub.Controllers
{
    // Base route: /api/employees
    // This is the CRUD the future Employees page will call.
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        // ASP.NET Core hands us the registered IEmployeeService automatically
        // (dependency injection) — see Program.cs.
        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public ActionResult<List<Employee>> GetAll()
        {
            return Ok(_employeeService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Employee> GetById(int id)
        {
            var employee = _employeeService.GetById(id);
            if (employee is null) return NotFound();
            return Ok(employee);
        }

        [HttpPost]
        public ActionResult<Employee> Create(Employee employee)
        {
            var created = _employeeService.Create(employee);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Employee employee)
        {
            var updated = _employeeService.Update(id, employee);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _employeeService.Delete(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}