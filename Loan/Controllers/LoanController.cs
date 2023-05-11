using Loan.Core.Enums;
using Loan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Loanclass = Loan.Core.Entities.Loan;
using Loan.Data;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User, Operator")]
    public class LoanController : ControllerBase
    {
        private readonly FinalProjectDbContext _dbContext;

        private readonly ILogger<UserController> _logger;

        public LoanController(FinalProjectDbContext dbContext, ILogger<UserController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // Get all loans
        [HttpGet]
        public async Task<IActionResult> GetLoans()
        {
            try
            {
                var loans = await _dbContext.Loan.ToListAsync();
                return Ok(loans);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                _logger.LogError(ex, "An error occurred while fetching loans");

                var response = new
                {
                    error = "An error occurred while fetching loans",
                    message = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // Get a loan by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoan(int id)
        {
            try
            {
                var loan = await _dbContext.Loan.FindAsync(id);

                if (loan == null)
                {
                    return NotFound();
                }

                return Ok(loan);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                _logger.LogError(ex, "An error occurred while fetching the loan with ID: {Id}", id);

                var response = new
                {
                    error = "An error occurred while fetching the loan",
                    message = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // Create a new loan request
        [HttpPost("RequestLoan")]
        public async Task<IActionResult> RequestLoan([FromBody] LoanDto loanDto)
        {
            try
            {
                // Check if user is blacklisted
                var userId = User.FindFirstValue("sub");
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }


                if (user.IsBlocked)
                {
                    return BadRequest(new { message = "User is blacklisted and cannot request a loan" });
                }

                var loan = new Loanclass
                {
                    Type = loanDto.Type,
                    Amount = loanDto.Amount,
                    Currency = loanDto.Currency,
                    Period = loanDto.Period,
                    Status = Convert.ToInt32(LoanStatus.InProcess),
                    UserId = userId
                };

                // Save the loan request to the database
                await _dbContext.Loan.AddAsync(loan);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Loan request created successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                 _logger.LogError(ex, "An error occurred while creating the loan request");

                var response = new
                {
                    error = "An error occurred while creating the loan request",
                    message = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // Update an existing loan request
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanDto loanDto)
        {
            try
            {
                var loan = await _dbContext.Loan.FindAsync(id);

                if (loan == null)
                {
                    return NotFound();
                }

                // Only allow updating if loan is under processing
                if (loan.Status != Convert.ToInt32(LoanStatus.InProcess))
                {
                    return BadRequest(new { message = "Loan cannot be updated as it is not under processing" });
                }

                // Only allow the operator to change the status
                if (User.IsInRole("User"))
                {
                    loan.Status = Convert.ToInt32(LoanStatus.InProcess);
                }
                else
                {
                    loan.Status = loanDto.Status;
                }

                loan.Type = loanDto.Type;
                loan.Amount = loanDto.Amount;
                loan.Currency = loanDto.Currency;
                loan.Period = loanDto.Period;

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Loan request updated successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                _logger.LogError(ex, "An error occurred while updating the loan request");

                var response = new
                {
                    error = "An error occurred while updating the loan request",
                    message = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // Delete an existing loan request
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            try
            {
                var loan = await _dbContext.Loan.FindAsync(id);

                if (loan == null)
                {
                    return NotFound();
                }

                // Only allow deleting if loan is under processing
                if (loan.Status != Convert.ToInt32(LoanStatus.InProcess))
                {
                    return BadRequest(new { message = "Loan cannot be deleted as it is not under processing" });
                }

                _dbContext.Loan.Remove(loan);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Loan request deleted successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                _logger.LogError(ex, "An error occurred while deleting the loan request");

                var response = new
                {
                    error = "An error occurred while deleting the loan request",
                    message = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
