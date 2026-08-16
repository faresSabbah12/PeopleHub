using Microsoft.AspNetCore.Mvc;
using PeopleHub.Services;

namespace PeopleHub.Controllers
{
    // Base route: /api/dashboard
    // Separate from EmployeesController on purpose: the dashboard aggregates
    // numbers across domains (employees today, maybe attendance/leave later),
    // while EmployeesController stays focused on plain employee CRUD.
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public DashboardController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET /api/dashboard/summary
        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            return Ok(new
            {
                employeeCount = _employeeService.GetEmployeeCount(),
                totalSalaries = _employeeService.GetTotalSalary()
            });
        }
    }
}