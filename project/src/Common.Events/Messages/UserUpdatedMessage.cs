using Common.Model.UserSvc;

namespace Common.Events.Messages;

public class UserUpdatedMessage : MessageBase
{
    public string UserId { get; set; }
    public UserDto? User { get; set; }
    public DateTime? DeletedDate { get; set; }
}