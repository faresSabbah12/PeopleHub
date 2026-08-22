using Microsoft.AspNetCore.Mvc;
using PeopleHub.Helpers;
using PeopleHub.Models;
using PeopleHub.Services;

namespace PeopleHub.Controllers
{
    // Base route: /api/financial
    // Backs the Salaries / Financial page. It reuses EmployeeQuery, so every
    // filter that works on the Employees page works here too — which is the
    // whole point: "total salaries of Engineering" is just /summary?department=Engineering.
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public FinancialController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET /api/financial/salaries?keyword=&department=&jobTitle=&minSalary=&maxSalary=
        //                            &sort=salary&sortDir=desc&page=1&pageSize=25
        [HttpGet("salaries")]
        public ActionResult<PagedResult<SalaryRow>> GetSalaries([FromQuery] EmployeeQuery query)
        {
            var rows = _employeeService.Filter(query).Select(ToRow);
            return Ok(rows.ToPagedResult(query.Page, query.PageSize));
        }

        // GET /api/financial/summary — totals for whatever the filters matched.
        // Paging is irrelevant here: it always totals the FULL filtered set, not
        // just the page you are looking at.
        [HttpGet("summary")]
        public ActionResult<SalarySummary> GetSummary([FromQuery] EmployeeQuery query) =>
            Ok(_employeeService.SummariseSalaries(_employeeService.Filter(query)));

        // GET /api/financial/by-department
        [HttpGet("by-department")]
        public ActionResult<List<DepartmentSalary>> GetByDepartment([FromQuery] EmployeeQuery query) =>
            Ok(_employeeService.SummariseByDepartment(_employeeService.Filter(query)));

        // GET /api/financial/payroll-by-year
        // series = wage bill per year since the company opened (the current year
        // is year-to-date). trend = this year vs the SAME number of months last
        // year, so a partial year is not compared against a full one.
        [HttpGet("payroll-by-year")]
        public IActionResult GetPayrollByYear() =>
            Ok(new
            {
                series = _employeeService.GetPayrollByYear(),
                trend = _employeeService.GetPayrollTrend()
            });

        private static SalaryRow ToRow(Employee e) => new(
            e.Id, e.EmployeeCode, e.FullName, e.AvatarUrl,
            e.Department, e.JobTitle, e.HireDate,
            e.Salary, e.Salary * 12);
    }
}
