namespace Common.Events;

public static class EventTypes
{
    // Rents
    public const string RentCreated = nameof(RentCreated);
    
    // Cars
    public const string CarReserved = nameof(CarReserved);
    public const string CarReservationFailed = nameof(CarReservationFailed);
    public const string CarStateUpdated = nameof(CarStateUpdated);
    
    // Billing
    public const string AccountAuthorized = nameof(AccountAuthorized);
    public const string AccountAuthorizationFailed = nameof(AccountAuthorizationFailed);
    
    // Notifications
    public const string Notification = nameof(Notification);
}