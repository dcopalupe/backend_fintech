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

        var loan = await _loanService.CreateLoanAsync(createLoanDto);

        if (loan == null)
        {
            return BadRequest("No se pudo crear el préstamo");
        }

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            Amount = loan.Amount,
            Term = loan.Term,
            InterestRate = loan.InterestRate,
            LoanType = loan.LoanType,
            Status = loan.Status,
            MonthlyPayment = loan.MonthlyPayment,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };

        return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, loanDto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetAllLoans([FromQuery] string? userId = null)
    {
        var loans = await _loanService.GetAllLoansAsync(userId);

        var loanDtos = loans.Select(loan => new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            Amount = loan.Amount,
            Term = loan.Term,
            InterestRate = loan.InterestRate,
            LoanType = loan.LoanType,
            Status = loan.Status,
            MonthlyPayment = loan.MonthlyPayment,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        });

        return Ok(loanDtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> GetLoanById(int id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);

        if (loan == null)
        {
            return NotFound($"Préstamo con ID {id} no encontrado");
        }

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            Amount = loan.Amount,
            Term = loan.Term,
            InterestRate = loan.InterestRate,
            LoanType = loan.LoanType,
            Status = loan.Status,
            MonthlyPayment = loan.MonthlyPayment,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };

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

        var schedule = await _loanService.GetPaymentScheduleAsync(id);

        var scheduleDtos = schedule.Select(s => new PaymentScheduleDto
        {
            Id = s.Id,
            LoanId = s.LoanId,
            PaymentNumber = s.PaymentNumber,
            DueDate = s.DueDate,
            TotalPayment = s.TotalPayment,
            Principal = s.Principal,
            Interest = s.Interest,
            RemainingBalance = s.RemainingBalance,
            Status = s.Status,
            PaidDate = s.PaidDate
        });

        return Ok(scheduleDtos);
    }

    [HttpPatch("{id}/approve")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanDto>> ApproveLoan(int id)
    {
        var loan = await _loanService.ApproveLoanAsync(id);

        if (loan == null)
        {
            return BadRequest($"No se pudo aprobar el préstamo. Verifique que existe y está en estado Pending");
        }

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            Amount = loan.Amount,
            Term = loan.Term,
            InterestRate = loan.InterestRate,
            LoanType = loan.LoanType,
            Status = loan.Status,
            MonthlyPayment = loan.MonthlyPayment,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };

        return Ok(loanDto);
    }

    [HttpPatch("{id}/reject")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanDto>> RejectLoan(int id)
    {
        var loan = await _loanService.RejectLoanAsync(id);

        if (loan == null)
        {
            return BadRequest($"No se pudo rechazar el préstamo. Verifique que existe y está en estado Pending");
        }

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            Amount = loan.Amount,
            Term = loan.Term,
            InterestRate = loan.InterestRate,
            LoanType = loan.LoanType,
            Status = loan.Status,
            MonthlyPayment = loan.MonthlyPayment,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };

        return Ok(loanDto);
    }
}
