namespace Common.Events;

public static class EventTypes
{
    // Rents
    public const string RentCreated = nameof(RentCreated);
    public const string RentFinishRequested = nameof(RentFinishRequested);
    public const string RentInvoiceCreated = nameof(RentInvoiceCreated);
    
    // Cars
    public const string CarStateUpdated = nameof(CarStateUpdated);
    public const string CarReserved = nameof(CarReserved);
    public const string CarReservationFailed = nameof(CarReservationFailed);
    public const string CarReadyToFinishRent = nameof(CarReadyToFinishRent);
    public const string CarNotReadyToFinishRent = nameof(CarNotReadyToFinishRent);
    
    // Billing
    public const string AccountAuthorized = nameof(AccountAuthorized);
    public const string AccountAuthorizationFailed = nameof(AccountAuthorizationFailed);
    public const string PaymentPerformed = nameof(PaymentPerformed);
    public const string PaymentPerformingFailed = nameof(PaymentPerformingFailed);
    
    // Notifications
    public const string Notification = nameof(Notification);
}