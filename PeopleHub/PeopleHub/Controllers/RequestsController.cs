using Microsoft.AspNetCore.Mvc;
using PeopleHub.Data;
using PeopleHub.Models;
using PeopleHub.Services;

namespace PeopleHub.Controllers
{
    // Base route: /api/requests
    // Backs the Requests page: the boxes at the top come from /summary, the
    // table below from the list endpoint. Both default to the CURRENT MONTH.
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IEmployeeService _employeeService;

        public RequestsController(IRequestService requestService, IEmployeeService employeeService)
        {
            _requestService = requestService;
            _employeeService = employeeService;
        }

        // GET /api/requests
        //   ?period=day|month|year|all   (default month)
        //   &date=2026-08-19             anchor for period, defaults to today
        //   &status=Pending|Approved|Rejected
        //   &type=Leave|RemoteWork|Overtime|Loan|Resignation
        //   &leaveType=&department=&employeeId=&keyword=
        //   &sort=submittedAt|startDate|name|days|type|status&sortDir=asc|desc
        //   &page=1&pageSize=25
        //
        // The period window filters on submittedAt — "requests FROM this month"
        // means the ones filed this month.
        [HttpGet]
        public ActionResult<PagedResult<EmployeeRequest>> Get([FromQuery] RequestQuery query) =>
            Ok(_requestService.Query(query));

        // GET /api/requests/summary — the boxes at the top of the page.
        // Takes the same filters, but deliberately IGNORES ?status so the
        // pending/approved/rejected counts stay visible while one is selected.
        [HttpGet("summary")]
        public ActionResult<RequestSummary> GetSummary([FromQuery] RequestQuery query) =>
            Ok(_requestService.GetSummary(query));

        [HttpGet("{id}")]
        public ActionResult<EmployeeRequest> GetById(int id)
        {
            var request = _requestService.GetById(id);
            if (request is null) return NotFound();
            return Ok(request);
        }

        // POST /api/requests
        [HttpPost]
        public ActionResult<EmployeeRequest> Create(CreateRequestDto dto)
        {
            // Validate at the boundary: anything past this point can assume the
            // data is sane. BadRequest(...) returns 400 with a readable message.
            var employee = _employeeService.GetById(dto.EmployeeId);
            if (employee is null)
                return BadRequest(new { message = $"No employee with id {dto.EmployeeId}." });

            if (!RequestCatalog.Types.Contains(dto.Type, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Type must be one of: {string.Join(", ", RequestCatalog.Types)}." });

            if (dto.EndDate.Date < dto.StartDate.Date)
                return BadRequest(new { message = "endDate cannot be before startDate." });

            bool isLeave = string.Equals(dto.Type, RequestCatalog.Leave, StringComparison.OrdinalIgnoreCase);
            if (isLeave && !string.IsNullOrWhiteSpace(dto.LeaveType)
                && !RequestCatalog.LeaveTypes.Contains(dto.LeaveType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"leaveType must be one of: {string.Join(", ", RequestCatalog.LeaveTypes)}." });

            var created = _requestService.Create(new EmployeeRequest
            {
                Type = dto.Type,
                LeaveType = isLeave ? dto.LeaveType : null,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                Reason = dto.Reason ?? string.Empty,
                Amount = dto.Amount,
                SubmittedAt = DateTime.Today
            }, employee);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PATCH /api/requests/{id}/status   body: { "status": "Approved", "reviewedBy": "..." }
        // PATCH rather than PUT because this changes one field, not the whole request.
        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, UpdateStatusDto dto)
        {
            // Check the status BEFORE hitting the service, so a bad status is a
            // 400 and a missing id is a 404 — two different problems.
            if (!RequestStatus.IsValid(dto.Status))
                return BadRequest(new { message = $"status must be one of: {string.Join(", ", RequestStatus.All)}." });

            if (!_requestService.UpdateStatus(id, dto.Status, dto.ReviewedBy))
                return NotFound();

            return Ok(_requestService.GetById(id));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!_requestService.Delete(id)) return NotFound();
            return NoContent();
        }
    }
}
