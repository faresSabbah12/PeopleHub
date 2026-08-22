using PeopleHub.Models;

namespace PeopleHub.Services
{
    // The contract. Controllers depend on this, not on EmployeeService directly.
    // Why: when Employees move to a real database later, you write a new class
    // that implements this same interface and swap it in Program.cs — nothing
    // in the controllers has to change.
    public interface IEmployeeService
    {
        List<Employee> GetAll();
        Employee? GetById(int id);
        Employee Create(Employee employee);
        bool Update(int id, Employee employee);
        bool Delete(int id);

        // Filtered + sorted, NOT paged. The Financial endpoints need the whole
        // matching set to total it up, so filtering and paging are separate steps.
        List<Employee> Filter(EmployeeQuery query);

        // Filtered + sorted + paged — what the Employees table calls.
        PagedResult<Employee> Query(EmployeeQuery query);

        int GetEmployeeCount();
        decimal GetTotalSalary();

        // ---- Aggregates the Dashboard / Financial pages need ----

        int GetHeadcountAsOf(DateTime date);
        TrendStat GetHeadcountTrend();                       // now vs one year ago
        decimal GetPayrollForYear(int year, int throughMonth = 12);
        TrendStat GetPayrollTrend();                         // this year-to-date vs same period last year
        List<YearlyPayrollPoint> GetPayrollByYear();
        List<DepartmentHeadcount> GetHeadcountByDepartment();

        // Take an already-filtered set so "total salaries of Engineering" and
        // "total salaries of everyone" run through the exact same maths.
        SalarySummary SummariseSalaries(IEnumerable<Employee> employees);
        List<DepartmentSalary> SummariseByDepartment(IEnumerable<Employee> employees);
    }
}
