using NotificationSvc.Dal;
using Common.Model.NotificationSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Model;
using NotificationSvc.Dal.Repositories;

namespace NotificationSvc.Api.Controllers
{
    [ApiController]
    [Route("notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationRepository _repository;

        public NotificationController(ILogger<NotificationController> logger, INotificationRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ListResult<NotificationDto>> GetUserNotifications([FromQuery] int start, [FromQuery] int size)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            (Notification[] notifications, int total) = await _repository.GetUserNotificationsAsync(userId, start, size, HttpContext.RequestAborted);
            return new ListResult<NotificationDto>(
                notifications.Select(n => new NotificationDto
                    {
                        UserId = n.UserId,
                        NotificationId = n.NotificationId,
                        Data = n.Data,
                        CreatedDate = n.CreatedDate
                    }).ToArray(),
                total);
        }
    }
}
