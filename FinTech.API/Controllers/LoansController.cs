using Microsoft.AspNetCore.Mvc;
using FinTech.API.DTOs;
using FinTech.API.Services.Interfaces;

namespace FinTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost("simulate")]
    [ProducesResponseType(typeof(LoanSimulationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanSimulationResultDto>> SimulateLoan([FromBody] SimulateLoanDto simulateLoanDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _loanService.SimulateLoanAsync(simulateLoanDto);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanDto>> CreateLoan([FromBody] CreateLoanDto createLoanDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var loanDto = await _loanService.CreateLoanAsync(createLoanDto);

        if (loanDto == null)
        {
            return BadRequest("No se pudo crear el préstamo");
        }

        return CreatedAtAction(nameof(GetLoanById), new { id = loanDto.Id }, loanDto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetAllLoans([FromQuery] string? userId = null)
    {
        var loanDtos = await _loanService.GetAllLoansAsync(userId);
        return Ok(loanDtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> GetLoanById(int id)
    {
        var loanDto = await _loanService.GetLoanByIdAsync(id);

        if (loanDto == null)
        {
            return NotFound($"Préstamo con ID {id} no encontrado");
        }

        return Ok(loanDto);
    }

    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(IEnumerable<PaymentScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PaymentScheduleDto>>> GetPaymentSchedule(int id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);

        if (loan == null)
        {
            return NotFound($"Préstamo con ID {id} no encontrado");
        }

        var scheduleDtos = await _loanService.GetPaymentScheduleAsync(id);
        return Ok(scheduleDtos);
    }

    [HttpPatch("{id}/approve")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanDto>> ApproveLoan(int id)
    {
        var loanDto = await _loanService.ApproveLoanAsync(id);

        if (loanDto == null)
        {
            return BadRequest($"No se pudo aprobar el préstamo. Verifique que existe y está en estado Pending");
        }

        return Ok(loanDto);
    }

    [HttpPatch("{id}/reject")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanDto>> RejectLoan(int id)
    {
        var loanDto = await _loanService.RejectLoanAsync(id);

        if (loanDto == null)
        {
            return BadRequest($"No se pudo rechazar el préstamo. Verifique que existe y está en estado Pending");
        }

        return Ok(loanDto);
    }
}
