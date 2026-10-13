using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Assignments;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoints for the homework/assignment tracker. All routes require a valid
/// Bearer access token (populated by JwtAuthMiddleware into ICurrentUserService); every
/// operation is implicitly scoped to the current user.
/// </summary>
public class AssignmentFunctions(
    IAssignmentService assignmentService,
    ICurrentUserService currentUser,
    IValidator<CreateAssignmentRequest> createValidator,
    IValidator<UpdateAssignmentRequest> updateValidator,
    ILogger<AssignmentFunctions> logger)
{
    [Function("ListAssignments")]
    public async Task<HttpResponseData> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "assignments")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        AssignmentStatus? status = null;
        var statusParam = GetQueryParam(req, "status");
        if (!string.IsNullOrEmpty(statusParam))
        {
            if (!Enum.TryParse<AssignmentStatus>(statusParam, ignoreCase: true, out var parsedStatus))
            {
                return await ApiResponses.BadRequestAsync(req, $"Invalid status filter '{statusParam}'.");
            }

            status = parsedStatus;
        }

        var assignments = await assignmentService.ListAsync(userId, status);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(assignments);
        return response;
    }

    [Function("GetAssignment")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "assignments/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            var assignment = await assignmentService.GetAsync(userId, id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(assignment);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("CreateAssignment")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "assignments")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<CreateAssignmentRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        var created = await assignmentService.CreateAsync(userId, request);
        logger.LogInformation("Created assignment {AssignmentId} for user {UserId}", created.Id, userId);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created);
        return response;
    }

    [Function("UpdateAssignment")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "assignments/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<UpdateAssignmentRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        try
        {
            var updated = await assignmentService.UpdateAsync(userId, id, request);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updated);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("DeleteAssignment")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "assignments/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await assignmentService.DeleteAsync(userId, id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    private static string? GetQueryParam(HttpRequestData req, string name)
    {
        var query = req.Url.Query.TrimStart('?');
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]).Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}
