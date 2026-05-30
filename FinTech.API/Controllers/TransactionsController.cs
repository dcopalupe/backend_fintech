using FinTech.API.DTOs;
using FinTech.API.Models.Enums;
using FinTech.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.API.Controllers;

/// <summary>
/// Transaction management endpoints
/// </summary>
[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    /// <summary>
    /// Create transaction
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionDto>> CreateTransaction([FromBody] CreateTransactionDto createTransactionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Creating transaction: Key={Key}, Type={Type}", 
            createTransactionDto.IdempotencyKey, createTransactionDto.Type);

        var transaction = await _transactionService.CreateTransactionAsync(createTransactionDto);

        if (transaction == null)
            return BadRequest(new { message = "Failed to create transaction. Loan may not exist." });

        return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
    }

    /// <summary>
    /// List transactions (with optional filters)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions(
        [FromQuery] int? loanId = null,
        [FromQuery] TransactionType? type = null,
        [FromQuery] TransactionStatus? status = null)
    {
        IEnumerable<TransactionDto> transactions;

        if (loanId.HasValue)
        {
            transactions = await _transactionService.GetTransactionsByLoanIdAsync(loanId.Value);
        }
        else
        {
            transactions = await _transactionService.GetAllTransactionsAsync();
        }

        // Apply additional filters
        if (type.HasValue)
        {
            transactions = transactions.Where(t => t.Type == type.Value);
        }

        if (status.HasValue)
        {
            transactions = transactions.Where(t => t.Status == status.Value);
        }

        return Ok(transactions);
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> GetTransactionById(int id)
    {
        var transaction = await _transactionService.GetTransactionByIdAsync(id);

        if (transaction == null)
            return NotFound(new { message = $"Transaction with ID {id} not found" });

        return Ok(transaction);
    }
}
