namespace AlvTime.Business;

public enum ErrorCodes
{
    InvalidAction = 1,
    MissingEntity = 2,
    EntityAlreadyExists = 3,
    RequestMissingProperty = 4,
    RequestInvalidProperty = 5,
    SQLError = 6,
    AuthorizationError = 7
}
