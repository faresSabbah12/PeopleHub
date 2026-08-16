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

        int GetEmployeeCount();
        decimal GetTotalSalary();
    }
}