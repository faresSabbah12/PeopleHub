# PeopleHub Backend — Explained From Zero

This document explains the **server side** of PeopleHub to someone who has never
written C# or ASP.NET Core. It walks through what every folder is for, what every
keyword means, and why the code is shaped the way it is.

If you only want to *call* the API from React, read
[API_CLIENT_GUIDE.md](API_CLIENT_GUIDE.md) instead.

---

## 1. What this thing actually is

A **REST API**. It is a program that sits and waits for HTTP requests, and answers
them with JSON. It has no UI. Your React app is a separate program that talks to it.

```
┌──────────────┐   HTTP GET /api/employees    ┌────────────────┐
│  React app   │ ───────────────────────────► │  PeopleHub API │
│ localhost:5173│                              │ localhost:7059 │
│              │ ◄─────────────────────────── │                │
└──────────────┘        JSON response          └────────────────┘
```

It stores its data **in memory** — there is no database. When the process stops,
every change you made is gone and the next start regenerates the same fake data
from scratch. That is deliberate: it means zero setup, and identical data every
time you refresh the UI you are building.

### Running it

```bash
dotnet run --project PeopleHub/PeopleHub
```

| Thing | Value |
|---|---|
| HTTPS URL | `https://localhost:7059` |
| HTTP URL | `http://localhost:5259` |
| Swagger UI | `https://localhost:7059/swagger` |
| Framework | .NET 8 (`net8.0`) |
| Only NuGet package | `Swashbuckle.AspNetCore` (that's Swagger) |

Ports live in `PeopleHub/PeopleHub/Properties/launchSettings.json`.

**Swagger** is a page listing every endpoint with a "Try it out" button. It is the
fastest way to see what an endpoint returns before you write the fetch. It is only
switched on in Development.

There is also `PeopleHub/PeopleHub/PeopleHub.http` — a plain-text file with one
request per endpoint. In Visual Studio or VS Code (REST Client extension) each
`GET`/`POST` line gets a clickable "Send request" link above it.

---

## 2. The shape of a request

Every single call goes through the same five steps:

```
1. Kestrel        the built-in web server accepts the TCP request
2. Middleware     the pipeline in Program.cs (HTTPS redirect, CORS, ...)
3. Routing        the URL is matched to one method on one controller
4. Controller     that method runs — it validates input and calls a service
5. Service        the service does the actual work and returns objects
                  → the objects are serialised to JSON and sent back
```

A controller should stay thin: check the input, call a service, return a status
code. All the real logic lives in the services. That is why
`FinancialController` is 60 lines while `EmployeeService` is 400.

---

## 3. The folders

```
PeopleHub/PeopleHub/
├── Program.cs          the entry point — wiring, not logic
├── Controllers/        the HTTP layer: one class per URL group
│   ├── DashboardController.cs
│   ├── EmployeesController.cs
│   ├── AttendanceController.cs
│   ├── RequestsController.cs
│   ├── FinancialController.cs
│   └── LookupsController.cs
├── Services/           the logic + the in-memory "database"
│   ├── IEmployeeService.cs   / EmployeeService.cs
│   ├── IRequestService.cs    / RequestService.cs
│   ├── IAttendanceService.cs / AttendanceService.cs
│   └── SeedSelfCheck.cs
├── Models/             the data shapes (these become the JSON)
│   ├── Employee.cs, AttendanceRecord.cs, EmployeeRequest.cs
│   ├── PagedResult.cs, Queries.cs, RequestDtos.cs
│   ├── DashboardModels.cs, LookupItem.cs
├── Data/               static reference data
│   ├── Departments.cs, RequestCatalog.cs
└── Helpers/            small shared utilities
    ├── Paging.cs, DateRanges.cs
```

The rule of thumb: **Controllers** know about HTTP. **Services** know about data.
**Models** know about nothing — they are just shapes.

---

## 4. C# things that will look strange

You know JavaScript/TypeScript, so here is only what is genuinely different.

### `namespace`

```csharp
namespace PeopleHub.Services { ... }
```

A folder-like label for your types. To use a type from another namespace you write
`using PeopleHub.Services;` at the top of the file. It is roughly `import`, except
you import the *namespace*, not the file, and you never write a path.

### Classes and properties

```csharp
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
}
```

`{ get; set; }` makes a **property** — readable and writable, like a plain JS
field. `public` means other files can see it; `private` means only this class can.

`= string.Empty` is the default value. It is there because of the next point.

### `?` — nullable reference types

The project has `<Nullable>enable</Nullable>` in the `.csproj`. That means:

```csharp
public string Reason { get; set; }    // must NEVER be null
public string? LeaveType { get; set; } // MAY be null
```

The compiler warns if you assign null to the first, or use the second without
checking. That is why every non-nullable string in the models is initialised to
`string.Empty`. In TypeScript terms, it is `strictNullChecks` for the whole project.

Related operators, identical to JS:

```csharp
r.ReviewedAt ?? r.SubmittedAt      // ?? — use the right side if the left is null
employee?.Department                // ?. — don't blow up if employee is null
r.CheckIn!.Value                    // ! — "I promise this isn't null", same as TS
```

### Computed properties

```csharp
public string FullName => $"{FirstName} {MiddleName} {LastName}";
public int Days => (EndDate.Date - StartDate.Date).Days + 1;
```

`=>` with no `set` means "recalculate this every time someone reads it". It is a
getter, not a stored field — so it can never go stale. **It still appears in the
JSON**, which is why the frontend sees `fullName`, `age`, `days`, `workedHours`,
`lateMinutes` even though nothing ever assigns them.

`$"..."` is a template string — the C# spelling of `` `...` ``.

### `record`

```csharp
public record LookupItem(int Id, string Name);
public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
```

A one-line immutable class. The parameters in the brackets automatically become
read-only properties. Used everywhere in this project for response shapes, because
a response shape has no behaviour — it is just a bag of values.

Create one with `new LookupItem(1, "Engineering")`, or name the arguments for
readability, which the code does a lot:

```csharp
new SalarySummary(
    EmployeeCount: salaries.Count,
    TotalMonthly: total,
    ...);
```

`<T>` is a generic — `PagedResult<Employee>` and `PagedResult<AttendanceRecord>`
are the same shape holding different row types.

### `switch` expressions

```csharp
private static int DurationDays(Random random, string type) => type switch
{
    RequestCatalog.Leave      => random.Next(1, 15),
    RequestCatalog.RemoteWork => random.Next(1, 6),
    _                         => 1          // _ is "anything else" (default)
};
```

Same idea as a JS `switch`, but it **returns a value** instead of assigning inside
cases. There is also the range form used in `PickType`:

```csharp
random.Next(100) switch
{
    < 55 => RequestCatalog.Leave,       // 0-54  → 55% chance
    < 75 => RequestCatalog.RemoteWork,  // 55-74 → 20% chance
    ...
};
```

### `is` / `or` pattern matching

```csharp
if (employee is null) return NotFound();
r.Status is AttendanceStatus.Present or AttendanceStatus.Late
```

`is null` is the idiomatic null check. `x is A or B` saves writing `x == A || x == B`.

### `^1`

```csharp
Max: salaries[^1]
```

"Index from the end" — `[^1]` is the last item, `[^2]` the second to last.

---

## 5. LINQ — the part you will read the most

LINQ is C#'s query syntax for collections. It is almost exactly the JS array
methods you already know, with SQL names.

| LINQ | JavaScript | SQL |
|---|---|---|
| `.Where(e => e.Salary > 1000)` | `.filter(...)` | `WHERE` |
| `.Select(e => e.Salary)` | `.map(...)` | `SELECT` |
| `.OrderBy(e => e.Name)` | `.sort(...)` | `ORDER BY` |
| `.OrderByDescending(...)` | `.sort()` reversed | `ORDER BY ... DESC` |
| `.ThenBy(...)` | — | second `ORDER BY` column |
| `.GroupBy(e => e.Department)` | — | `GROUP BY` |
| `.Count()` / `.Count(e => ...)` | `.length` / `.filter().length` | `COUNT(*)` |
| `.Sum(e => e.Salary)` | `.reduce(...)` | `SUM()` |
| `.Any(e => ...)` | `.some(...)` | `EXISTS` |
| `.All(e => ...)` | `.every(...)` | — |
| `.FirstOrDefault(e => ...)` | `.find(...)` | `LIMIT 1` |
| `.Skip(20).Take(10)` | `.slice(20, 30)` | `OFFSET / FETCH` |
| `.Distinct()` | `new Set(...)` | `DISTINCT` |
| `.ToList()` | `[...]` | run the query |

A real example from `EmployeeService.GetHeadcountByDepartment()`:

```csharp
return _employees
    .GroupBy(e => e.Department)                    // one bucket per department
    .Select(g => new DepartmentHeadcount(
        g.Key,                                     // g.Key = the department name
        g.Count(),                                 // how many are in this bucket
        Math.Round(g.Count() * 100.0 / total, 2)))
    .OrderByDescending(d => d.Headcount)
    .ToList();
```

### The one LINQ trap: it is lazy

`.Where(...)` does **not** run when you write it. It builds a recipe. The recipe
only runs when something consumes it — `.ToList()`, `.Count()`, a `foreach`.

That matters because consuming it **twice runs it twice**. This is exactly why
`Helpers/Paging.cs` materialises before counting:

```csharp
var all = source as IList<T> ?? source.ToList();   // run the filters ONCE
var totalCount = all.Count;
var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
```

Without that line, every filter and sort would be evaluated a second time just to
produce the count.

---

## 6. Controllers: how a URL finds a method

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    [HttpGet]
    public ActionResult<PagedResult<Employee>> GetAll([FromQuery] EmployeeQuery query) =>
        Ok(_employeeService.Query(query));

    [HttpGet("{id}")]
    public ActionResult<Employee> GetById(int id) { ... }
}
```

Those `[Bracketed]` things are **attributes** — metadata the framework reads.

| Attribute | Effect |
|---|---|
| `[ApiController]` | Turns on API conventions: automatic 400s on malformed JSON, automatic `[FromBody]` for complex POST parameters |
| `[Route("api/[controller]")]` | Base URL. `[controller]` is replaced by the class name minus "Controller" → `api/employees` |
| `[HttpGet]` | This method answers `GET` on the base URL |
| `[HttpGet("{id}")]` | `GET /api/employees/5` — `{id}` is captured into the `int id` parameter |
| `[HttpPost]` `[HttpPut]` `[HttpPatch]` `[HttpDelete]` | Same idea for the other verbs |
| `[FromQuery]` | Fill this parameter from the **query string**, not the body |

`ControllerBase` is the class you inherit from to get `Ok()`, `NotFound()`,
`BadRequest()` and friends.

### Model binding

This is the piece that saves the most code. You declare a class:

```csharp
public class EmployeeQuery : PagedQuery
{
    public string? Keyword { get; set; }
    public string? Department { get; set; }
    public decimal? MinSalary { get; set; }
    ...
}
```

and the framework fills it in from the URL, matching **case-insensitively** by name:

```
GET /api/employees?keyword=ahmad&department=Engineering&minSalary=1000&page=2
                    ↓             ↓                      ↓              ↓
                    Keyword       Department             MinSalary      Page
```

Anything the caller omits keeps the default written in the class. That is the
whole mechanism behind "the Requests page shows this month by default" —
`RequestQuery.Period` is initialised to `"month"`, so a bare `/api/requests`
already means "this month".

Types are converted for you: `?minSalary=1000` becomes a `decimal`,
`?date=2026-08-19` becomes a `DateTime`, `?page=abc` produces an automatic 400.

### Return types and status codes

```csharp
return Ok(thing);                      // 200 + JSON body
return NotFound();                     // 404, empty
return BadRequest(new { message });     // 400 + JSON body
return NoContent();                    // 204, empty (used for DELETE/PUT)
return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
                                       // 201 + JSON body + a Location header
                                       // pointing at the new resource
```

`ActionResult<Employee>` vs plain `IActionResult`: the generic form tells Swagger
what the success body looks like, so the docs are useful. Both let you return any
status code. Prefer `ActionResult<T>`.

### The status codes this API actually uses

| Code | When |
|---|---|
| `200 OK` | A successful GET, or a PATCH that returns the updated object |
| `201 Created` | POST succeeded — body is the created object |
| `204 No Content` | PUT/DELETE succeeded, nothing to send back |
| `400 Bad Request` | Your input was wrong (unknown employee, backwards dates, bad status) |
| `404 Not Found` | The id in the URL doesn't exist |

Note the distinction the code is careful about, in `RequestsController.UpdateStatus`:
a **bad status value** is 400 (your fault, fixable), a **missing id** is 404
(nothing there). That is why the status is validated *before* the service is
called — the service's `false` return then unambiguously means "not found".

---

## 7. Dependency injection — the bit that looks like magic

Look at a controller's constructor:

```csharp
public DashboardController(
    IEmployeeService employeeService,
    IAttendanceService attendanceService,
    IRequestService requestService)
{
    _employeeService = employeeService;
    ...
}
```

Nothing anywhere calls `new DashboardController(...)`. So who passes those in?

The **DI container**, configured in `Program.cs`:

```csharp
builder.Services.AddSingleton<IEmployeeService, EmployeeService>();
builder.Services.AddSingleton<IRequestService, RequestService>();
builder.Services.AddSingleton<IAttendanceService, AttendanceService>();
```

Read each line as: *"whenever anything asks for an `IEmployeeService`, give it an
`EmployeeService`."*

When a request arrives, the framework needs a `DashboardController`. It looks at
the constructor, sees three interfaces, finds the registration for each, builds
them, and passes them in. `RequestService`'s own constructor asks for an
`IEmployeeService` — so the container builds that first. It resolves the whole
graph itself; registration order in `Program.cs` is for readability only.

### Why `AddSingleton` and not the others

| Lifetime | You get | 
|---|---|
| `AddSingleton` | **One** instance for the entire app lifetime |
| `AddScoped` | One instance per HTTP request |
| `AddTransient` | A brand new instance every time it is asked for |

Our data lives in a `List<T>` **inside** the service instance. With `AddScoped`,
every request would build a fresh service, re-seed 200 employees and 95,000
attendance rows, and forget every `POST` you just made. Singleton is what makes an
in-memory store work at all.

With a real database it would be the opposite: the *DbContext* is `Scoped`, and
the data survives because it is in the database, not in the object.

### Why interfaces at all

Every service is registered as `IThing → Thing`. Controllers only ever mention
`IThing`. The payoff is that swapping the mock for a real database is a one-line
change in `Program.cs`:

```csharp
builder.Services.AddScoped<IEmployeeService, SqlEmployeeService>();
```

Not one controller has to change, because none of them ever knew which
implementation they were talking to.

---

## 8. The mock data

Three services each own one in-memory "table", seeded once at startup.

### Employees — `Services/EmployeeService.cs`

- **200** employees, generated in the constructor.
- Hire dates uniformly random between **2011-05-01** (`CompanyFoundedOn`, when the
  company opened) and today.
- Arabic/Jordanian names, `077`/`078`/`079` phone numbers, salaries in JD.
- Salary scales off the job title: managers 1400–2600, senior/lead 950–1500,
  everyone else 450–950.
- Birth dates are derived from the hire date so that everyone was 22–55 when hired
  — the data stays internally consistent instead of producing 14-year-old managers.
- `EmployeeCode` is `yyyyMM` + a 4-digit sequence within that hiring cohort, e.g.
  `2018070003` = the 3rd person hired in July 2018.
- Emails are de-duplicated with a `HashSet<string>`; collisions get a numeric suffix.

### Requests — `Services/RequestService.cs`

- 4–10 per employee across the last **2 years** (~1,380 total), never before their
  hire date.
- Weighted types: Leave 55%, RemoteWork 20%, Overtime 15%, Loan 8%, Resignation 2%.
- Status depends on age: filed in the last 10 days → usually still `Pending`;
  older → 72% `Approved`, 22% `Rejected`, 6% `Pending`.
- Resignations are never `Approved` — this roster never shrinks, so an approved
  resignation would contradict the employee still being listed.
- `Loan` requests get an `amount`; `Leave` requests get a `leaveType`.

### Attendance — `Services/AttendanceService.cs`

The big one: **~95,000 rows**, one per employee per working day for two years.

- Working week is **Sunday–Thursday** (`DateRanges.IsWorkday`). Friday and
  Saturday produce no rows at all.
- A row starts at the later of "two years ago" and the employee's hire date.
- Office hours are **09:00–17:00** (`AttendanceRecord.WorkStart` / `WorkEnd`).
- Arrival is a weighted distribution around 09:00: 65% land in 08:40–09:05, then a
  tail out to 11:30.
- Departure clusters around 17:00, with early leavers and overtime either side —
  and never less than an hour after arrival.
- **Status priority**: an approved leave day wins (`OnLeave`, no clock-in), then
  ~4% `Absent`, then approved remote-work days become `Remote`, then `Late` if more
  than 5 minutes past 09:00, then `EarlyLeave` if more than 15 minutes before
  17:00, else `Present`.
- Grace periods exist because clocking in at 09:03 is not "late" on any real HR
  system.

Note the dependency: `AttendanceService` takes `IRequestService` in its
constructor **specifically** so that an approved holiday shows up as `OnLeave`
rather than as an unexplained absence. Before generating days, it builds a
`Dictionary<int, List<(start, end)>>` of approved ranges per employee — one pass
over 1,380 requests instead of re-scanning them for each of 95,000 days.

### Determinism

Each service seeds with a **fixed** `Random` seed (`42`, `7`, `1337`). Same seed →
same sequence → identical data on every restart. You can hardcode "employee 3 is
Huda Yousef Khatib" in a UI test and it stays true.

The one thing that *does* move is anything relative to `DateTime.Today` — the two
year windows slide forward each day.

### Honest limitations

1. **Nothing persists.** Restart and your POSTs are gone.
2. **Nobody ever leaves.** There is no termination date, so "headcount as of date"
   is just `count(HireDate <= date)` and only ever grows. Real attrition would need
   an `EndDate` on `Employee` and an extra condition in `GetHeadcountAsOf`.
3. **Nobody gets a raise.** Salary is a single number with no history, so
   year-over-year payroll growth comes purely from headcount growth.
4. **Not thread-safe.** A singleton holding a plain `List<T>` that `Create`/`Delete`
   mutate. A `POST` landing at the same instant as a `GET` can throw. Fine for one
   developer; wrong for anything real.

---

## 9. Two decisions worth understanding

### Denormalisation on attendance and requests

`AttendanceRecord` and `EmployeeRequest` both **copy** the employee's name,
department, job title, code and avatar onto every row, instead of just storing
`EmployeeId`.

In a real database that is usually a mistake — rename an employee and 500 rows go
stale. Here it is deliberate:

- The attendance table endpoint returns everything the UI needs in one flat object,
  with no second request and no join.
- Keyword search can hit `EmployeeName` directly instead of resolving 95,000 ids.

`RequestService.Create` shows the trade-off being handled: it takes the `Employee`
object and copies the display fields from **that**, ignoring whatever the client
posted. Otherwise a client could file a request under a real id but someone else's
name.

### Percentages computed on the server

Every "vs last period" card uses one shared shape:

```csharp
public record TrendStat(decimal Current, decimal Previous, double ChangePercent)
{
    public static TrendStat From(decimal current, decimal previous)
    {
        double percent = previous == 0
            ? (current == 0 ? 0 : 100)
            : (double)((current - previous) / previous) * 100;
        return new TrendStat(current, previous, Math.Round(percent, 2));
    }
}
```

The divide-by-zero guard exists in exactly one place. That is the whole reason for
the factory method: without it, every card computing its own percentage would each
need to remember that a zero baseline produces `Infinity`, and one of them would
forget. The raw `current`/`previous` are still in the JSON if you would rather do
the arithmetic in React.

Two comparisons in there are subtler than they look:

- **Payroll** compares this year *through the current month* against last year
  *through the same month*. Comparing 8 months of this year against 12 months of
  last year would make every year look like a collapse until December.
- **Attendance** compares this month-to-date against *all* of last month. That is
  fine because it is a **rate**, not a count — ratios are comparable across
  different-length windows in a way that raw totals are not.

---

## 10. The self-check

`Services/SeedSelfCheck.cs` runs once at startup, in Development only, and throws
if the generated data is inconsistent:

```csharp
Require(attendance.All(r => DateRanges.IsWorkday(r.Date)),
    "attendance was generated on a non-working day (Fri/Sat)");
```

It verifies there are exactly 200 employees, that no hire date predates May 2011,
that no attendance row exists before its employee was hired or on a weekend, that
no row checks out before it checks in, that absent rows have no clock-in, that
every request points at a real employee and ends after it starts, and that no
dashboard percentage is `NaN` or `Infinity`.

The point: the seeding code is ~300 lines of random rolls. If one is wrong, the API
still returns `200 OK` — it just returns numbers that are quietly nonsense. This
turns that into a startup crash with a message.

It also has a useful side effect. Singletons are built lazily, on first use.
Calling `GetRequiredService` here forces all three to be constructed **during
startup**, so the ~95,000 attendance rows are generated before the app starts
listening rather than during whichever unlucky request arrives first.

---

## 11. Why the JSON is camelCase

This confuses everyone once. In C# the property is `EmployeeCode`. In the JSON
your browser receives it is `employeeCode`.

ASP.NET Core's JSON serialiser (`System.Text.Json`) applies a **camelCase naming
policy by default**. It works both ways — a POST body with `"employeeId": 1` binds
correctly to a C# `EmployeeId` property.

So: **PascalCase in C#, camelCase in JSON, always.** No configuration involved.

Two related serialisation facts:

- `TimeSpan?` becomes `"09:14:00"` or `null`.
- `DateTime` becomes an ISO string. You will notice some are `"2016-01-17T00:00:00"`
  and others `"2026-08-19T00:00:00+03:00"`. That is because a `DateTime` built with
  `new DateTime(2016, 1, 17)` has an *unspecified* kind, while one derived from
  `DateTime.Today` is *local* and keeps its offset. Harmless — but for date-only
  display, prefer splitting the string on `T` over constructing a JS `Date`, which
  would apply timezone maths you don't want.

---

## 12. Adding an endpoint yourself

Say you want `GET /api/employees/birthdays-this-month`.

**1. Add it to the interface** — `Services/IEmployeeService.cs`:

```csharp
List<Employee> GetBirthdaysThisMonth();
```

The project will now fail to compile until you implement it. That is the interface
doing its job.

**2. Implement it** — `Services/EmployeeService.cs`:

```csharp
public List<Employee> GetBirthdaysThisMonth() => _employees
    .Where(e => e.BirthDate.Month == DateTime.Today.Month)
    .OrderBy(e => e.BirthDate.Day)
    .ToList();
```

**3. Expose it** — `Controllers/EmployeesController.cs`:

```csharp
[HttpGet("birthdays-this-month")]
public ActionResult<List<Employee>> GetBirthdays() =>
    Ok(_employeeService.GetBirthdaysThisMonth());
```

Put the literal route **before** `[HttpGet("{id}")]` in the file if you ever hit
ambiguity — ASP.NET prefers literal segments over parameters, but keeping them
ordered avoids surprises.

**4. Restart, open `/swagger`, click Try it out.**

No registration, no route table, no config file. A public method with an
`[Http...]` attribute on a class ending in `Controller` **is** an endpoint.

### If you need a new query filter

Add a property to the relevant class in `Models/Queries.cs`, then add one `if` to
that service's `ApplyFilters`. Model binding picks it up from the URL
automatically — nothing else to touch.

---

## 13. Where each page's data comes from

| Page | Endpoints | Service |
|---|---|---|
| Dashboard cards | `/api/dashboard/summary` | all three |
| Dashboard charts | `/api/dashboard/attendance-trend`, `/headcount-by-department`, `/team-status`, `/recent-activity`, `/hiring-by-year` | attendance, employee, request |
| Employees | `/api/employees` + CRUD | employee |
| Financial | `/api/financial/salaries`, `/summary`, `/by-department`, `/payroll-by-year` | employee |
| Requests | `/api/requests`, `/summary`, POST/PATCH/DELETE | request |
| Attendance | `/api/attendance`, `/summary` | attendance |
| Any dropdown | `/api/lookups/*` | none — reads `Data/` statics |

Two cards on the existing dashboard — **Upcoming Events** and **Quick Actions** —
have no endpoint on purpose. There is no data behind them; they are static UI. An
endpoint returning invented events would just be a lie with extra steps.

---

## 14. Cheat sheet

| I want to... | Go to |
|---|---|
| Add an endpoint | the matching `Controllers/*.cs` |
| Change what the fake data looks like | the `Seed...` methods in `Services/*Service.cs` |
| Add a filter | `Models/Queries.cs` + `ApplyFilters` in that service |
| Add a field to the JSON | the class in `Models/` |
| Add a dropdown option | `Data/Departments.cs` or `Data/RequestCatalog.cs` |
| Change office hours or the weekend | `Models/AttendanceRecord.cs` (`WorkStart`/`WorkEnd`), `Helpers/DateRanges.cs` (`IsWorkday`) |
| Change the page-size cap | `Helpers/Paging.cs` |
| Allow a different frontend origin | the CORS policy in `Program.cs` |
| See every endpoint | `/swagger`, or `PeopleHub.http` |
