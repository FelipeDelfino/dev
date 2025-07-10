using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;

namespace Questao5.Infrastructure.Services.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        [HttpGet("balance/{id}")]
        public IActionResult GetBalance(string id)
        {
            using var connection = new SqliteConnection("Data Source=database.sqlite");
            connection.Open();

            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT ativo FROM contacorrente WHERE idcontacorrente = @id";
            checkCmd.Parameters.AddWithValue("@id", id);
            var isActive = checkCmd.ExecuteScalar();

            if (isActive == null)
                return NotFound(new { error = "Account not found." });

            if (Convert.ToInt32(isActive) == 0)
                return BadRequest(new { error = "Account is inactive." });

            using var balanceCmd = connection.CreateCommand();
            balanceCmd.CommandText = @"
                SELECT 
                    SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE -valor END) 
                FROM movimento 
                WHERE idcontacorrente = @id";
            balanceCmd.Parameters.AddWithValue("@id", id);

            var result = balanceCmd.ExecuteScalar();
            double balance = result != DBNull.Value ? Convert.ToDouble(result) : 0.0;

            return Ok(new { accountId = id, balance });
        }

        [HttpPost("transaction")]
        public IActionResult PerformTransaction([FromBody] TransactionRequest request)
        {
            if (request.Value <= 0)
                return BadRequest(new { error = "Value must be greater than zero." });

            if (request.Type != 'C' && request.Type != 'D')
                return BadRequest(new { error = "Invalid transaction type. Use 'C' or 'D'." });

            using var connection = new SqliteConnection("Data Source=database.sqlite");
            connection.Open();

            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT ativo FROM contacorrente WHERE idcontacorrente = @id";
            checkCmd.Parameters.AddWithValue("@id", request.AccountId);
            var isActive = checkCmd.ExecuteScalar();

            if (isActive == null)
                return NotFound(new { error = "Account not found." });

            if (Convert.ToInt32(isActive) == 0)
                return BadRequest(new { error = "Account is inactive." });

            if (request.Type == 'D')
            {
                using var balanceCmd = connection.CreateCommand();
                balanceCmd.CommandText = @"
                    SELECT 
                        SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE -valor END) 
                    FROM movimento 
                    WHERE idcontacorrente = @id";
                balanceCmd.Parameters.AddWithValue("@id", request.AccountId);
                var result = balanceCmd.ExecuteScalar();
                double balance = result != DBNull.Value ? Convert.ToDouble(result) : 0.0;

                if (request.Value > balance)
                    return BadRequest(new { error = "Insufficient funds." });
            }

            var transactionId = Guid.NewGuid().ToString();

            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO movimento 
                (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
                VALUES (@id, @accountId, @date, @type, @value)";
            insertCmd.Parameters.AddWithValue("@id", transactionId);
            insertCmd.Parameters.AddWithValue("@accountId", request.AccountId);
            insertCmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("s"));
            insertCmd.Parameters.AddWithValue("@type", request.Type);
            insertCmd.Parameters.AddWithValue("@value", request.Value);
            insertCmd.ExecuteNonQuery();

            return CreatedAtAction(nameof(GetBalance), new { id = request.AccountId }, new
            {
                message = "Transaction completed successfully.",
                transactionId,
                accountId = request.AccountId
            });
        }
    }

    public class TransactionRequest
    {
        public string AccountId { get; set; }
        public char Type { get; set; } // 'C' or 'D'
        public double Value { get; set; }
    }
}
