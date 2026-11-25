namespace TaskManager.Domain.Exceptions;

public class NotFoundException(string message) : Exception(message);