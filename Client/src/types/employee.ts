// Mirrors PeopleHub/Models/Employee.cs — keep in sync with the backend model.
// Dates come back as ISO strings over JSON, not Date objects.
export interface Employee {
  id: number;
  employeeCode: string;
  avatarUrl: string;
  firstName: string;
  middleName: string;
  lastName: string;
  fullName: string;
  gender: string;
  birthDate: string;
  age: number;
  hireDate: string;
  department: string;
  jobTitle: string;
  salary: number;
  phoneNumber: string;
  email: string;
  maritalStatus: string;
}
