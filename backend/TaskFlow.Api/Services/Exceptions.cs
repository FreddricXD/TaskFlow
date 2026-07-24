namespace TaskFlow.Api.Services;

public class AppException(string message, string? code = null) : Exception(message)
{
    public string? Code { get; } = code;
}

public class NotFoundException(string message) : AppException(message, "not_found");

public class ForbiddenException(string message) : AppException(message, "forbidden");

public class ConflictException(string message) : AppException(message, "conflict");

public class ValidationException(string message) : AppException(message, "validation");
