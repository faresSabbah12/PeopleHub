# Consuming the PeopleHub API from React

Everything you need to call this backend from the client: the correct base URL,
every endpoint with its parameters and a **real** response body, then the
`fetch` + `useEffect` patterns to wire them up.

The backend itself is explained in [BACKEND_GUIDE.md](BACKEND_GUIDE.md).

---

## 1. Start here: the base URL

```
https://localhost:7059      ← HTTPS (the default launch profile)
http://localhost:5259       ← HTTP
```

> ⚠️ `Client/src/pages/dashboard/DashboardPage.tsx` currently fetches
> `https://localhost:7000`. **Nothing binds port 7000** — that fetch cannot
> succeed. The port is `7059`.
>
> The real ports live in `PeopleHub/PeopleHub/Properties/launchSettings.json`.
> If you change the launch profile, change them here too.

### Two traps behind that

**1. The dev certificate.** `https://localhost:7059` uses a self-signed cert. Until
your machine trusts it, the browser silently blocks the request:

```bash
dotnet dev-certs https --trust
```

Then open `https://localhost:7059/swagger` once in the browser and accept the
prompt if you still get one. Until that works in the address bar, it will not work
from `fetch` either.

**2. `UseHttpsRedirection`.** The API redirects plain HTTP to HTTPS, so calling
`http://localhost:5259` gives you a `307` hop rather than a direct answer. Use the
HTTPS URL.

### Where to put the URL

Pick one. Any of the three is fine — just don't scatter the literal across files.

**Option A — one constant** (simplest, good enough to start):

```ts
// Client/src/lib/api.ts
export const API_BASE = 'https://localhost:7059/api';
```

**Option B — an env var** (Vite reads `.env` at the `Client/` root; the `VITE_`
prefix is required for it to reach the browser):

```bash
# Client/.env.development
VITE_API_URL=https://localhost:7059/api
```
```ts
export const API_BASE = import.meta.env.VITE_API_URL;
```

**Option C — a dev proxy** (no CORS, no cert prompt, relative URLs):

```ts
// Client/vite.config.ts
export default defineConfig({
  // ...existing plugins and the @ alias
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7059',
        changeOrigin: true,
        secure: false,          // accept the self-signed dev cert
      },
    },
  },
});
```

Then just `fetch('/api/employees')`. Vite forwards it server-to-server, so the
browser never sees a cross-origin request or an untrusted certificate. **This is
the least painful option**, but it only applies in dev — production needs a real
URL anyway.

### CORS

The API allows exactly one origin, set in `Program.cs`:

```csharp
policy.WithOrigins("http://localhost:5173")
```

That is Vite's default port. If your dev server starts on `5174` because `5173`
was busy, **every request will fail with a CORS error** until you add that origin
to the list. Credentials are not allowed — don't send cookies.

---

## 2. `PagedResult<T>` — read this before anything else

Every list endpoint (`/employees`, `/attendance`, `/requests`,
`/financial/salaries`) returns an **envelope**, not a bare array:

```json
{
  "items": [ /* just this page's rows */ ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 200,
  "totalPages": 8
}
```

```ts
// Client/src/types/api.ts
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;   // rows matching your filters, BEFORE paging
  totalPages: number;
}
```

- `totalCount` is what you show as "200 results" — it is not `items.length`.
- `pageSize` defaults to **25** and is **capped at 200**. Asking for 99999 gives
  you 200, not an error.
- `page` is 1-based. `?page=0` is treated as page 1.

> **This changes the fetch you already have.** `DashboardPage.tsx` does
> `setEmployees(data)` where `data` was an `Employee[]`. It now needs:
>
> ```ts
> const data: PagedResult<Employee> = await response.json();
> setEmployees(data.items);
> ```
>
> Without `.items`, `employees.length` is `undefined` and `employees.map` throws.
> If all you want is the headcount, `/api/dashboard/summary` is one much smaller
> call than fetching 200 employee objects to count them.

Endpoints that are **not** paged and return a plain array or object:
everything under `/api/dashboard/*`, `/api/lookups/*`, `/api/financial/summary`,
`/api/financial/by-department`, `/api/financial/payroll-by-year`,
`/api/attendance/summary`, `/api/requests/summary`.

---

## 3. Endpoint reference

All routes are relative to `https://localhost:7059`.

### Shared conventions

| Parameter | Applies to | Notes |
|---|---|---|
| `page` | all list endpoints | 1-based, default `1` |
| `pageSize` | all list endpoints | default `25`, max `200` |
| `sort` | all list endpoints | see each endpoint for allowed values |
| `sortDir` | all list endpoints | `asc` (default) or `desc` |

Dates go in and out as `YYYY-MM-DD` (a full ISO timestamp also works).

---

### Dashboard

#### `GET /api/dashboard/summary`

The four top cards, in one call.

```json
{
  "headcount":      { "current": 200, "previous": 181, "changePercent": 10.5 },
  "attendanceRate": { "current": 96.12, "previous": 95.92, "changePercent": 0.21 },
  "leave":          { "onLeaveToday": 4, "activeLeaveRequests": 43 },
  "yearlyPayroll":  { "current": 1618852, "previous": 1469218, "changePercent": 10.18 },
  "generatedAt": "2026-08-19T01:57:32.19+03:00"
}
```

- `headcount` — employees now vs one year ago.
- `attendanceRate` — this month-to-date vs all of last month, as a **percentage**
  (so `current: 96.12` means 96.12%, and `changePercent: 0.21` is the change *in
  that rate*, not in attendance itself).
- `leave.activeLeaveRequests` — leave requests still `Pending`, any date.
- `yearlyPayroll` — this year through the current month vs last year through the
  same month, in JD.

`changePercent` is already computed and already rounded. Use it directly, or
recompute from `current`/`previous` if you prefer — both are in the payload.

#### `GET /api/dashboard/hiring-by-year`

```json
[{ "year": 2011, "count": 9 }, { "year": 2012, "count": 10 }, { "year": 2013, "count": 12 }]
```

#### `GET /api/dashboard/headcount-by-department`

```json
[
  { "department": "Engineering", "headcount": 33, "share": 16.5 },
  { "department": "Operations",  "headcount": 27, "share": 13.5 }
]
```
`share` is a percentage of total headcount.

#### `GET /api/dashboard/attendance-trend?days=7`

`days` defaults to 7, capped at 180. **Working days only** — a 7-day window
returns 5 points, because Friday and Saturday have no attendance at all. Don't
assume `days` and `array.length` match.

```json
[{ "date": "2026-08-19T00:00:00+03:00", "present": 98, "late": 66,
   "absent": 3, "onLeave": 4, "remote": 1, "attendanceRate": 98.47 }]
```

#### `GET /api/dashboard/team-status`

```json
{ "date": "2026-08-19T00:00:00+03:00", "inOffice": 192, "remote": 1,
  "onLeave": 4, "absent": 3, "total": 200 }
```
`inOffice` = present + late + early-leave. On a Friday or Saturday this reports
the **most recent working day** — `date` tells you which, so label it rather than
saying "today".

#### `GET /api/dashboard/recent-activity?take=8`

`take` defaults to 8, capped at 50.

```json
[{ "requestId": 549, "employeeId": 81, "employeeName": "Tariq Raed Shaqran",
   "avatarUrl": "...", "type": "Leave", "status": "Approved",
   "at": "2026-08-18T00:00:00+03:00", "description": "Leave request approved" }]
```

---

### Employees

#### `GET /api/employees`

| Parameter | Type | Notes |
|---|---|---|
| `keyword` | string | partial match over name, code, email, phone, department, job title |
| `department` | string | exact |
| `jobTitle` | string | exact |
| `gender` | string | `Male` / `Female` |
| `maritalStatus` | string | `Single` / `Married` / `Divorced` |
| `minSalary` / `maxSalary` | number | inclusive |
| `hiredFrom` / `hiredTo` | date | inclusive |
| `sort` | string | `name`, `salary`, `hireDate`, `department`, `jobTitle`, `age`, `code` |

Returns `PagedResult<Employee>`:

```json
{
  "items": [{
    "id": 3,
    "employeeCode": "2016010001",
    "avatarUrl": "https://randomuser.me/api/portraits/women/74.jpg",
    "firstName": "Huda", "middleName": "Yousef", "lastName": "Khatib",
    "fullName": "Huda Yousef Khatib",
    "gender": "Female",
    "birthDate": "1975-10-01T00:00:00", "age": 50,
    "hireDate": "2016-01-17T00:00:00",
    "department": "Marketing", "jobTitle": "Marketing Manager",
    "salary": 1861,
    "phoneNumber": "0770263395", "email": "huda.khatib@peoplehub.com",
    "maritalStatus": "Single"
  }],
  "page": 1, "pageSize": 25, "totalCount": 200, "totalPages": 8
}
```

`fullName` and `age` are computed server-side — send them or not on write, they
are ignored.

#### `GET /api/employees/{id}` → `200` `Employee` | `404`
#### `POST /api/employees` → `201` `Employee`
#### `PUT /api/employees/{id}` → `204` | `404` — send the **whole** object
#### `DELETE /api/employees/{id}` → `204` | `404`

Body for POST/PUT (omit `id`, `fullName`, `age`, `employeeCode` — the server
generates the code):

```json
{
  "firstName": "Lina", "middleName": "Omar", "lastName": "Haddad",
  "gender": "Female",
  "birthDate": "1995-03-14", "hireDate": "2026-01-05",
  "department": "Finance", "jobTitle": "Accountant",
  "salary": 780,
  "phoneNumber": "0791234567", "email": "lina.haddad@peoplehub.com",
  "maritalStatus": "Single"
}
```

---

### Attendance

Office hours are **09:00–17:00**, working week **Sunday–Thursday**.

#### `GET /api/attendance`

| Parameter | Type | Notes |
|---|---|---|
| `keyword` | string | the page's "any keyword" box — name, code, department, job title, status |
| `name` | string | the dedicated name field, partial match |
| `employeeCode` | string | the dedicated id field, **partial** — `2018` matches the whole 2018 cohort |
| `employeeId` | number | exact |
| `department` | string | exact |
| `status` | string | `Present` `Late` `EarlyLeave` `Absent` `OnLeave` `Remote` |
| `date` | date | one exact day — **overrides** `from`/`to` |
| `from` / `to` | date | inclusive range |
| `sort` | string | `latestCheckIn`, `earliestCheckIn`, `date`, `name`, `lateMinutes`, `workedHours`, `status` |

**`sort=latestCheckIn` / `sort=earliestCheckIn` is the "latest vs earliest
attendance time" select.** They carry their own direction, so `sortDir` is ignored
for those two. Rows with no clock-in (absent, on leave) always sort to the bottom.

Default order is newest day first. With no filters, `totalCount` is ~95,000 — the
response is still fast, but always send a date range or a filter for a real table.

```json
{
  "items": [{
    "id": 74119,
    "employeeId": 157, "employeeCode": "2019120001",
    "employeeName": "Ahmad Fadi Rawashdeh",
    "department": "Engineering", "jobTitle": "Frontend Developer",
    "avatarUrl": "https://randomuser.me/api/portraits/men/0.jpg",
    "date": "2026-08-19T00:00:00+03:00",
    "checkIn": "08:56:00", "checkOut": "17:43:00",
    "status": "Present",
    "workedHours": 8.78, "lateMinutes": 0, "earlyLeaveMinutes": 0
  }],
  "page": 1, "pageSize": 25, "totalCount": 95540, "totalPages": 3822
}
```

`checkIn` / `checkOut` are `"HH:mm:ss"` strings, or `null` for absent/on-leave.
They are times, **not** timestamps — don't pass them to `new Date()`. To display,
`checkIn?.slice(0, 5)` gives `"08:56"`.

#### `GET /api/attendance/summary`

Takes **the same filters**, aggregated instead of paged.

```json
{
  "total": 2800, "present": 1398, "late": 882, "earlyLeave": 298,
  "absent": 105, "onLeave": 97, "remote": 20,
  "attendanceRate": 96.12, "punctualityRate": 66.05,
  "averageCheckIn": "09:07", "totalLateMinutes": 33277
}
```

`attendanceRate` = attended ÷ scheduled, with approved leave excluded from **both**
sides — a month full of holidays doesn't look like a month full of absences.
`punctualityRate` = on-time ÷ attended.

---

### Requests

Types: `Leave`, `RemoteWork`, `Overtime`, `Loan`, `Resignation`.
Statuses: `Pending`, `Approved`, `Rejected`.

#### `GET /api/requests`

| Parameter | Type | Notes |
|---|---|---|
| `period` | string | `day` \| `month` \| `year` \| `all` — **defaults to `month`** |
| `date` | date | the anchor `period` is measured around, defaults to today |
| `status` | string | the toggle |
| `type` | string | |
| `leaveType` | string | `Annual` `Sick` `Unpaid` `Maternity` `Bereavement` |
| `department` | string | exact |
| `employeeId` | number | exact |
| `keyword` | string | name, code, department, type, reason |
| `sort` | string | `submittedAt`, `startDate`, `name`, `days`, `type`, `status` |

**The period window filters on `submittedAt`** — "requests from this month" means
the ones *filed* this month, not the ones whose leave falls in it. Default sort is
newest-filed first.

So `GET /api/requests` with no parameters already gives you the page's default
view. `?period=day` and `?period=year` are the other two toggle positions.

Expect roughly 25–40 results for `month`, ~1,380 for `all`, and **often zero for
`day`** — only about two requests are filed on an average day across 200 staff.
An empty "today" view is correct data, not a broken filter; design an empty state
for it.

```json
{
  "items": [{
    "id": 549,
    "employeeId": 81, "employeeCode": "2022070001",
    "employeeName": "Tariq Raed Shaqran",
    "department": "IT", "jobTitle": "Network Engineer", "avatarUrl": "...",
    "type": "Leave", "leaveType": "Unpaid",
    "startDate": "2026-08-18T00:00:00+03:00",
    "endDate": "2026-08-27T00:00:00+03:00",
    "days": 10,
    "status": "Pending",
    "reason": "Travelling abroad",
    "amount": null,
    "submittedAt": "2026-08-17T00:00:00+03:00",
    "reviewedAt": null, "reviewedBy": null
  }],
  "page": 1, "pageSize": 25, "totalCount": 27, "totalPages": 2
}
```

`leaveType` is only set for `Leave`; `amount` only for `Loan`. `reviewedAt` /
`reviewedBy` are `null` while `Pending`.

#### `GET /api/requests/summary`

The boxes at the top. Same filters — but it **deliberately ignores `status`**, so
all three counts stay visible while one is selected in the toggle.

```json
{
  "total": 27, "pending": 5, "approved": 15, "rejected": 7,
  "byType": { "Leave": 19, "RemoteWork": 5, "Overtime": 2, "Loan": 1 },
  "onLeaveToday": 4,
  "activeLeaveRequests": 43,
  "from": "2026-08-01T00:00:00", "to": "2026-08-31T00:00:00"
}
```

`byType` only contains types that actually occur in the window — read it as
`byType.Loan ?? 0`, never assume all five keys exist.

`onLeaveToday` and `activeLeaveRequests` are **not** scoped to the period; they
are always "right now", and match the numbers on `/api/dashboard/summary`.

#### `GET /api/requests/{id}` → `200` | `404`

#### `POST /api/requests` → `201` | `400`

```json
{
  "employeeId": 1,
  "type": "Leave",
  "leaveType": "Annual",
  "startDate": "2026-09-01",
  "endDate": "2026-09-05",
  "reason": "Family vacation",
  "amount": null
}
```

New requests are always created as `Pending` — sending a `status` does nothing.
The employee's name/department/avatar are filled from the employee record, not
from your body.

`400` responses carry a readable message:

```json
{ "message": "endDate cannot be before startDate." }
{ "message": "No employee with id 9999." }
{ "message": "Type must be one of: Leave, RemoteWork, Overtime, Loan, Resignation." }
```

#### `PATCH /api/requests/{id}/status` → `200` (the updated request) | `400` | `404`

```json
{ "status": "Approved", "reviewedBy": "HR Manager" }
```

`status` is case-insensitive — `"approved"` works. `reviewedBy` is optional and
defaults to `"HR Manager"`. Setting it back to `Pending` clears `reviewedAt` and
`reviewedBy`.

An unknown status is `400`; an unknown id is `404`.

#### `DELETE /api/requests/{id}` → `204` | `404`

---

### Financial

All four take **the same filters as `/api/employees`** (`keyword`, `department`,
`jobTitle`, `minSalary`, `maxSalary`, `hiredFrom`, `hiredTo`, `sort`, `sortDir`).

#### `GET /api/financial/salaries` → `PagedResult<SalaryRow>`

```json
{
  "items": [{
    "employeeId": 1, "employeeCode": "2013110001",
    "employeeName": "Nour Omar Hamdan", "avatarUrl": "...",
    "department": "Human Resources", "jobTitle": "HR Manager",
    "hireDate": "2013-11-27T00:00:00",
    "monthlySalary": 1708, "annualSalary": 20496
  }],
  "page": 1, "pageSize": 25, "totalCount": 200, "totalPages": 8
}
```

#### `GET /api/financial/summary`

Totals the **entire filtered set**, not just the page you are looking at. So
"total salaries of Engineering" is literally `?department=Engineering`.

```json
{ "employeeCount": 200, "totalMonthly": 207521, "totalAnnual": 2490252,
  "average": 1037.60, "median": 827, "min": 450, "max": 2562 }
```

#### `GET /api/financial/by-department`

```json
[{ "department": "Engineering", "headcount": 33, "totalMonthly": 32404,
   "averageMonthly": 981.94, "share": 15.61 }]
```
`share` is a percentage of the total wage bill.

#### `GET /api/financial/payroll-by-year`

```json
{
  "series": [
    { "year": 2011, "payroll": 32225, "headcount": 9 },
    { "year": 2012, "payroll": 179399, "headcount": 19 }
  ],
  "trend": { "current": 1618852, "previous": 1469218, "changePercent": 10.18 }
}
```

The **last** entry in `series` is year-to-date, so it will look low next to the
previous full year. Use `trend` for the card — it compares equal numbers of months.

---

### Lookups

All return `[{ "id": 1, "name": "Engineering" }, ...]` — use `name` as the value
you send back as a filter, and translate it for display with your existing i18n.

| Route | |
|---|---|
| `GET /api/lookups/departments` | 8 departments |
| `GET /api/lookups/job-titles?department=Finance` | omit `department` for all titles |
| `GET /api/lookups/departments-with-titles` | `[{ department, jobTitles: [...] }]` — for dependent dropdowns |
| `GET /api/lookups/request-types` | |
| `GET /api/lookups/leave-types` | |
| `GET /api/lookups/request-statuses` | |
| `GET /api/lookups/attendance-statuses` | |
| `GET /api/lookups/work-hours` | `{ start, end, lateGraceMinutes, earlyLeaveGraceMinutes, workdays }` |

Every list here is read from the same data used to seed the mock records, so a
dropdown can never offer a value that matches nothing.

---

## 4. Fetch patterns

These build up in order. Read them in order once; after that, jump to the one you
need.

### Pattern 1 — the minimal fetch

```tsx
import { useEffect, useState } from 'react';
import type { Employee } from '@/types/employee';
import type { PagedResult } from '@/types/api';

const API = 'https://localhost:7059/api';

export function EmployeeList() {
  const [employees, setEmployees] = useState<Employee[]>([]);

  useEffect(() => {
    fetch(`${API}/employees`)
      .then((res) => res.json())
      .then((data: PagedResult<Employee>) => setEmployees(data.items));
  }, []);   // [] = run once, after the first render

  return <ul>{employees.map((e) => <li key={e.id}>{e.fullName}</li>)}</ul>;
}
```

Three things to notice:

- `useState<Employee[]>([])` starts as an **empty array**, not `undefined`. The
  first render happens *before* the fetch finishes, so `.map` runs on `[]` first.
  Start with `null`/`undefined` and that first render crashes.
- `[]` as the dependency array. Leave it off entirely and the effect runs after
  **every** render — including the one caused by `setEmployees` — which is an
  infinite request loop.
- `data.items`, not `data`. See §2.

### Pattern 2 — loading and error state

`fetch` only rejects on a **network** failure. A `404` or `500` still resolves,
with `res.ok === false`. You must check it yourself.

```tsx
export function EmployeeList() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(`${API}/employees`);
        if (!res.ok) throw new Error(`Request failed: ${res.status}`);

        const data: PagedResult<Employee> = await res.json();
        setEmployees(data.items);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Something went wrong');
      } finally {
        setLoading(false);   // runs whether it succeeded or threw
      }
    };

    load();   // the effect callback itself cannot be async — so define, then call
  }, []);

  if (loading) return <Spinner />;
  if (error) return <ErrorBox message={error} />;
  return <ul>{employees.map((e) => <li key={e.id}>{e.fullName}</li>)}</ul>;
}
```

**Why `load` is defined and then called:** an effect callback may only return a
cleanup function or nothing. An `async` function returns a Promise, so
`useEffect(async () => ...)` is a bug React warns about.

### Pattern 3 — `AbortController`, and the race it fixes

The bug: you type "ahm", a request goes out. You type "ahmad", a second goes out.
The **first** response arrives last. Your table now shows results for "ahm" while
the box says "ahmad".

```tsx
useEffect(() => {
  const controller = new AbortController();

  const load = async () => {
    try {
      const res = await fetch(`${API}/employees?keyword=${keyword}`, {
        signal: controller.signal,
      });
      if (!res.ok) throw new Error(`Request failed: ${res.status}`);
      const data: PagedResult<Employee> = await res.json();
      setEmployees(data.items);
    } catch (err) {
      // An abort is not a failure — it is us cancelling on purpose.
      if (err instanceof DOMException && err.name === 'AbortError') return;
      setError('Could not load employees');
    }
  };

  load();

  // Cleanup: runs before the next effect and when the component unmounts.
  return () => controller.abort();
}, [keyword]);
```

The returned function is the **cleanup**. React calls it before re-running the
effect, so the previous request is cancelled the moment a new one starts. It also
stops the "setState on an unmounted component" warning when you navigate away
mid-request.

### Pattern 4 — the dependency array

The effect re-runs whenever any value in the array changes, compared with `===`.

```tsx
useEffect(() => { ... }, [keyword, page, department]);   // ✅ primitives
```

The classic trap:

```tsx
const filters = { keyword, department };          // ❌ new object every render
useEffect(() => { ... }, [filters]);              // ❌ so this runs every render
```

`{} === {}` is `false`, so `filters` is "different" on every render — infinite
loop. Three ways out:

```tsx
// A. list the primitives (simplest, do this)
useEffect(() => { ... }, [filters.keyword, filters.department]);

// B. depend on the serialised form
const qs = new URLSearchParams(filters).toString();
useEffect(() => { ... }, [qs]);

// C. stabilise the object
const filters = useMemo(() => ({ keyword, department }), [keyword, department]);
```

### Pattern 5 — building the query string

Never hand-concatenate `&` and `?`. `URLSearchParams` escapes values for you
(important the moment someone types `&` or a space into a search box).

```ts
// Client/src/lib/api.ts
export function buildQuery(params: Record<string, unknown>): string {
  const search = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    // Skip empties: keeps the URL readable and the request cacheable.
    if (value === undefined || value === null || value === '') continue;
    search.set(key, String(value));
  }

  const qs = search.toString();
  return qs ? `?${qs}` : '';
}
```

```ts
buildQuery({ keyword: 'ahmad', department: '', page: 2, minSalary: 1000 });
// → "?keyword=ahmad&page=2&minSalary=1000"
```

The backend is forgiving here — it treats `?department=` the same as omitting it
(every filter is guarded with `IsNullOrWhiteSpace`), so an empty value will not
silently return zero rows. Skipping them is still worth doing: it keeps the URL
short and readable, and — because the `useFetch` in Pattern 9 depends on the path
string — it stops `''` → `undefined` transitions from triggering a pointless
refetch to an identical result.

### Pattern 6 — debounced search

Firing a request on every keystroke is 5 requests for "ahmad". Wait until typing
stops.

```tsx
function useDebounced<T>(value: T, delay = 400): T {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(timer);   // each keystroke cancels the pending timer
  }, [value, delay]);

  return debounced;
}
```

```tsx
const [keyword, setKeyword] = useState('');
const debouncedKeyword = useDebounced(keyword);

useEffect(() => {
  // ...fetch using debouncedKeyword
}, [debouncedKeyword]);        // ← the debounced value, not the raw one

<input value={keyword} onChange={(e) => setKeyword(e.target.value)} />
```

The input stays instant (`keyword`); only the fetch waits (`debouncedKeyword`).

### Pattern 7 — pagination

```tsx
const [result, setResult] = useState<PagedResult<Employee> | null>(null);
const [page, setPage] = useState(1);

useEffect(() => {
  const controller = new AbortController();

  fetch(`${API}/employees${buildQuery({ page, pageSize: 25, keyword })}`, {
    signal: controller.signal,
  })
    .then((res) => res.json())
    .then(setResult)
    .catch((err) => { if (err.name !== 'AbortError') setError('Failed'); });

  return () => controller.abort();
}, [page, keyword]);

// Reset to page 1 when a FILTER changes — otherwise you can sit on page 8
// of a filtered set that only has 2 pages and see an empty table.
useEffect(() => { setPage(1); }, [keyword]);
```

```tsx
<button disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>Previous</button>
<span>Page {result?.page ?? 1} of {result?.totalPages ?? 1} · {result?.totalCount ?? 0} results</span>
<button disabled={!result || page >= result.totalPages} onClick={() => setPage((p) => p + 1)}>Next</button>
```

Keeping the whole `PagedResult` in state (rather than just `items`) is what makes
the footer possible without a second call.

### Pattern 8 — writing: POST / PUT / PATCH / DELETE

```ts
export async function createRequest(body: {
  employeeId: number;
  type: string;
  startDate: string;
  endDate: string;
  leaveType?: string;
  reason?: string;
}) {
  const res = await fetch(`${API}/requests`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },   // required, or you get a 415
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    // Our 400s carry { message }. Fall back if the body isn't JSON (500s won't be).
    const problem = await res.json().catch(() => null);
    throw new Error(problem?.message ?? `Request failed: ${res.status}`);
  }

  return res.json();   // 201 → the created request
}
```

```ts
export async function setRequestStatus(id: number, status: 'Approved' | 'Rejected') {
  const res = await fetch(`${API}/requests/${id}/status`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ status, reviewedBy: 'HR Manager' }),
  });
  if (!res.ok) throw new Error(`Failed to ${status.toLowerCase()} request ${id}`);
  return res.json();   // 200 → the updated request
}

export async function deleteRequest(id: number) {
  const res = await fetch(`${API}/requests/${id}`, { method: 'DELETE' });
  if (!res.ok) throw new Error(`Failed to delete request ${id}`);
  // 204 = No Content. DO NOT call res.json() — there is no body, it throws.
}
```

Two rules that catch everyone:

1. **`204` has no body.** `res.json()` on a `204` throws
   `Unexpected end of JSON input`. `DELETE` and `PUT /api/employees/{id}` both
   return `204`.
2. **Refresh after a write.** The server holds the truth. Either refetch the list,
   or fold the returned object into state — POST and PATCH both return the full
   saved entity, so you rarely need a second call:

```tsx
const handleApprove = async (id: number) => {
  const updated = await setRequestStatus(id, 'Approved');
  setRequests((prev) => prev.map((r) => (r.id === id ? updated : r)));
};
```

### Pattern 9 — the refactor destination

Once patterns 1–8 make sense, the boilerplate collapses into two small pieces.

```ts
// Client/src/lib/api.ts
export const API = 'https://localhost:7059/api';

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API}${path}`, {
    ...init,
    headers: {
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...init?.headers,
    },
  });

  if (!res.ok) {
    const problem = await res.json().catch(() => null);
    throw new Error(problem?.message ?? `${res.status} ${res.statusText}`);
  }

  // 204 No Content — nothing to parse.
  if (res.status === 204) return undefined as T;

  return res.json() as Promise<T>;
}
```

```ts
// Client/src/hooks/useFetch.ts
import { useEffect, useState } from 'react';
import { apiFetch } from '@/lib/api';

export function useFetch<T>(path: string | null) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(path !== null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (path === null) return;   // pass null to skip (e.g. waiting on an id)

    const controller = new AbortController();
    setLoading(true);
    setError(null);

    apiFetch<T>(path, { signal: controller.signal })
      .then(setData)
      .catch((err) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err.message);
      })
      .finally(() => setLoading(false));

    return () => controller.abort();
  }, [path]);   // a STRING dependency — stable by value, no useMemo needed

  return { data, loading, error };
}
```

Every read now fits on one line:

```tsx
const { data, loading, error } = useFetch<PagedResult<Employee>>(
  `/employees${buildQuery({ keyword: debouncedKeyword, department, page })}`
);
```

The reason `path` being a plain string works so well as a dependency: two renders
that produce the same URL are `===`, so no refetch — which is exactly the
behaviour you want, and exactly what an object dependency could not give you.

> **Note on `.finally` + abort:** when a request is aborted the `finally` still
> runs, briefly setting `loading` to `false` before the new effect sets it back to
> `true`. Harmless for a spinner. If it ever flickers, move `setLoading(false)`
> into the `then`/`catch` branches instead.

---

## 5. Per-page recipes

### Dashboard

```tsx
const summary  = useFetch<DashboardSummary>('/dashboard/summary');
const trend    = useFetch<DailyAttendancePoint[]>('/dashboard/attendance-trend?days=14');
const byDept   = useFetch<DepartmentHeadcount[]>('/dashboard/headcount-by-department');
const team     = useFetch<TeamStatusToday>('/dashboard/team-status');
const activity = useFetch<ActivityItem[]>('/dashboard/recent-activity?take=8');
```

Five independent hooks means five parallel requests and five independently
loading cards — better than one blocking `Promise.all`. The four top cards all
come from `summary` alone.

Rendering a trend arrow:

```tsx
function Trend({ stat }: { stat: TrendStat }) {
  const up = stat.changePercent >= 0;
  return (
    <span className={up ? 'text-emerald-600' : 'text-red-600'}>
      {up ? '▲' : '▼'} {Math.abs(stat.changePercent).toFixed(1)}%
    </span>
  );
}
```

Two cards need wording care:
- `attendanceRate.current` is already a percentage — render `96.12%`, don't multiply.
- `team-status` may be reporting the last working day. Label it from `team.date`,
  not "today".

`Upcoming Events` and `Quick Actions` have **no endpoint** — they are static UI
with no data behind them. Keep them on the hardcoded data in `dashboardData.ts`.

### Employees

```tsx
const [keyword, setKeyword] = useState('');
const [department, setDepartment] = useState('');
const [page, setPage] = useState(1);
const debounced = useDebounced(keyword);

const { data, loading } = useFetch<PagedResult<Employee>>(
  `/employees${buildQuery({ keyword: debounced, department, page, pageSize: 25, sort: 'name' })}`
);

const departments = useFetch<LookupItem[]>('/lookups/departments');

useEffect(() => { setPage(1); }, [debounced, department]);
```

After a create/update/delete, force a refetch by bumping a counter you fold into
the path (`&_r=${refreshKey}`), or lift the fetch into a `useCallback` you can
call again.

### Financial

```tsx
const filters = buildQuery({ department, minSalary, sort: 'salary', sortDir: 'desc', page });

const rows    = useFetch<PagedResult<SalaryRow>>(`/financial/salaries${filters}`);
const totals  = useFetch<SalarySummary>(`/financial/summary${filters}`);
const byDept  = useFetch<DepartmentSalary[]>('/financial/by-department');
const payroll = useFetch<{ series: YearlyPayrollPoint[]; trend: TrendStat }>('/financial/payroll-by-year');
```

Send the **same** filter string to `/salaries` and `/summary` — that is what makes
the totals row describe the table above it rather than the whole company.

### Requests

```tsx
const [period, setPeriod] = useState<'day' | 'month' | 'year'>('month');
const [status, setStatus] = useState<string>('');   // '' = all

const listQuery    = buildQuery({ period, status, page, pageSize: 25 });
const summaryQuery = buildQuery({ period });        // no status — see below

const list    = useFetch<PagedResult<EmployeeRequest>>(`/requests${listQuery}`);
const summary = useFetch<RequestSummary>(`/requests/summary${summaryQuery}`);
```

The summary ignores `status` server-side anyway, so the three toggle badges keep
showing real counts while one is selected:

```tsx
<Toggle active={status === ''}         onClick={() => setStatus('')}>        All ({summary.data?.total})</Toggle>
<Toggle active={status === 'Pending'}  onClick={() => setStatus('Pending')}> Pending ({summary.data?.pending})</Toggle>
<Toggle active={status === 'Approved'} onClick={() => setStatus('Approved')}>Approved ({summary.data?.approved})</Toggle>
<Toggle active={status === 'Rejected'} onClick={() => setStatus('Rejected')}>Rejected ({summary.data?.rejected})</Toggle>
```

The on-leave card reads `summary.data.onLeaveToday` with
`summary.data.activeLeaveRequests` underneath.

### Attendance

```tsx
const [keyword, setKeyword] = useState('');
const [name, setName] = useState('');
const [employeeCode, setEmployeeCode] = useState('');
const [arrival, setArrival] = useState('');   // '' | 'latestCheckIn' | 'earliestCheckIn'
const [from, setFrom] = useState('2026-08-01');
const [to, setTo] = useState('2026-08-31');

const query = buildQuery({
  keyword: useDebounced(keyword),
  name: useDebounced(name),
  employeeCode: useDebounced(employeeCode),
  from, to,
  sort: arrival || 'date',
  sortDir: 'desc',
  page, pageSize: 25,
});

const rows    = useFetch<PagedResult<AttendanceRecord>>(`/attendance${query}`);
const summary = useFetch<AttendanceSummary>(`/attendance/summary${query}`);
```

The arrival select maps straight onto `sort`:

```tsx
<select value={arrival} onChange={(e) => setArrival(e.target.value)}>
  <option value=''>Newest first</option>
  <option value='latestCheckIn'>Latest arrivals</option>
  <option value='earliestCheckIn'>Earliest arrivals</option>
</select>
```

Always send `from`/`to` (or `date`) here — without one, `totalCount` is ~95,000.

Formatting a row:

```tsx
<td>{row.date.slice(0, 10)}</td>                    {/* "2026-08-19" */}
<td>{row.checkIn?.slice(0, 5) ?? '—'}</td>          {/* "08:56" */}
<td>{row.lateMinutes > 0 ? `+${row.lateMinutes}m` : '—'}</td>
```

---

## 6. Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `TypeError: Failed to fetch`, nothing in the API logs | Wrong port, or the API isn't running | It is `7059`, not `7000`. Open `https://localhost:7059/swagger` in the browser |
| `net::ERR_CERT_AUTHORITY_INVALID` | Untrusted dev certificate | `dotnet dev-certs https --trust`, restart the browser |
| `blocked by CORS policy` | Vite is on a port other than `5173` | Add that origin to the policy in `Program.cs`, or use the Vite proxy |
| `employees.map is not a function` | You assigned the envelope instead of the rows | `setEmployees(data.items)` |
| `totalCount` is `undefined` | Reading it off `items` instead of the envelope | Keep the whole `PagedResult` in state |
| `Unexpected end of JSON input` | Called `res.json()` on a `204` | `DELETE` and `PUT` return no body |
| `415 Unsupported Media Type` | Missing `Content-Type` on a write | `headers: { 'Content-Type': 'application/json' }` |
| `400` with no detail | `[ApiController]` rejected the body shape before your code ran | Check the response body — it lists the offending field |
| Requests fire forever | An object or array in the dependency array | Depend on primitives or on the query string |
| Search shows stale results | Overlapping requests resolving out of order | `AbortController` (Pattern 3) |
| Table is empty after filtering | Still on a page that no longer exists | Reset `page` to 1 when filters change |
| A date is off by a day | `new Date("2026-08-19T00:00:00")` shifts by timezone | For date-only display use `value.slice(0, 10)` |
| `checkIn` renders as `Invalid Date` | It is a time (`"08:56:00"`), not a timestamp | `checkIn?.slice(0, 5)` |
| Your `POST` vanished after a restart | The data is in memory only | Expected — the seed regenerates on every start |
| `attendance-trend?days=7` returns 5 points | Friday/Saturday are not working days | Don't assume `days === array.length` |

---

## 7. Types to mirror on the client

`Client/src/types/employee.ts` already exists. These are the rest — keep them in
sync with `PeopleHub/PeopleHub/Models/`.

```ts
// Client/src/types/api.ts
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface TrendStat {
  current: number;
  previous: number;
  changePercent: number;
}

export interface LookupItem { id: number; name: string }

export interface DashboardSummary {
  headcount: TrendStat;
  attendanceRate: TrendStat;
  leave: { onLeaveToday: number; activeLeaveRequests: number };
  yearlyPayroll: TrendStat;
  generatedAt: string;
}

export interface DepartmentHeadcount { department: string; headcount: number; share: number }

export interface DailyAttendancePoint {
  date: string;
  present: number; late: number; absent: number; onLeave: number; remote: number;
  attendanceRate: number;
}

export interface TeamStatusToday {
  date: string;
  inOffice: number; remote: number; onLeave: number; absent: number; total: number;
}

export interface ActivityItem {
  requestId: number; employeeId: number; employeeName: string; avatarUrl: string;
  type: string; status: string; at: string; description: string;
}

export type AttendanceStatus =
  | 'Present' | 'Late' | 'EarlyLeave' | 'Absent' | 'OnLeave' | 'Remote';

export interface AttendanceRecord {
  id: number;
  employeeId: number; employeeCode: string; employeeName: string;
  department: string; jobTitle: string; avatarUrl: string;
  date: string;
  checkIn: string | null;    // "08:56:00"
  checkOut: string | null;
  status: AttendanceStatus;
  workedHours: number; lateMinutes: number; earlyLeaveMinutes: number;
}

export interface AttendanceSummary {
  total: number; present: number; late: number; earlyLeave: number;
  absent: number; onLeave: number; remote: number;
  attendanceRate: number; punctualityRate: number;
  averageCheckIn: string | null; totalLateMinutes: number;
}

export type RequestType = 'Leave' | 'RemoteWork' | 'Overtime' | 'Loan' | 'Resignation';
export type RequestStatus = 'Pending' | 'Approved' | 'Rejected';

export interface EmployeeRequest {
  id: number;
  employeeId: number; employeeCode: string; employeeName: string;
  department: string; jobTitle: string; avatarUrl: string;
  type: RequestType;
  leaveType: string | null;
  startDate: string; endDate: string; days: number;
  status: RequestStatus;
  reason: string;
  amount: number | null;
  submittedAt: string;
  reviewedAt: string | null;
  reviewedBy: string | null;
}

export interface RequestSummary {
  total: number; pending: number; approved: number; rejected: number;
  byType: Partial<Record<RequestType, number>>;   // Partial — absent types are missing keys
  onLeaveToday: number; activeLeaveRequests: number;
  from: string; to: string;
}

export interface SalaryRow {
  employeeId: number; employeeCode: string; employeeName: string; avatarUrl: string;
  department: string; jobTitle: string; hireDate: string;
  monthlySalary: number; annualSalary: number;
}

export interface SalarySummary {
  employeeCount: number;
  totalMonthly: number; totalAnnual: number;
  average: number; median: number; min: number; max: number;
}

export interface DepartmentSalary {
  department: string; headcount: number;
  totalMonthly: number; averageMonthly: number; share: number;
}

export interface YearlyPayrollPoint { year: number; payroll: number; headcount: number }
```
