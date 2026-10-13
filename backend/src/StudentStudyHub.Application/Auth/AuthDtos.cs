namespace StudentStudyHub.Application.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    int GradeLevel,
    Guid? SchoolId);

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    int GradeLevel,
    Guid? SchoolId);

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserDto User);
