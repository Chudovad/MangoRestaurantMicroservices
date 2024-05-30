using Mango.Services.Email.Data;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.Email.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;

        public EmailRepository(DbContextOptions<ApplicationDbContext> contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public async Task SendAndLogEmail(UpdatePaymentResultMessage message)
        {
            //send email
            EmailLog emailLog = new EmailLog()
            {
                Email = message.Email,
                EmailSent = DateTime.Now,
                Log = $"Order {message.OrderId} has been created successfully."
            };
            await using var _context = new ApplicationDbContext(_contextOptions);
            await _context.EmailLogs.AddAsync(emailLog);
            await _context.SaveChangesAsync();
        }
    }
}
