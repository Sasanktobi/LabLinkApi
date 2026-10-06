using System;

namespace Backend.Exceptions
{
    // Thrown by services; ExceptionHandlingMiddleware maps each type to an HTTP status code.

    public class NotFoundException : Exception
    {
        public NotFoundException(string message):base(message){}
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message):base(message){}
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message):base(message){}
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message):base(message){}
    }
}
