using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.StudyGroups;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoints for study groups: create/list-mine/get/update/delete plus
/// join-by-invite-code/leave. All routes require a valid auth token. Unlike Assignments/Courses/
/// Exams, access is scoped by group *membership* rather than a single owning user id - see
/// StudyGroupService for the NotFound-vs-Forbidden rules this enforces.
/// </summary>
public class StudyGroupFunctions(
    IStudyGroupService studyGroupService,
    ICurrentUserService currentUser,
    IValidator<CreateStudyGroupRequest> createValidator,
    IValidator<UpdateStudyGroupRequest> updateValidator,
    IValidator<JoinStudyGroupRequest> joinValidator,
    ILogger<StudyGroupFunctions> logger)
{
    [Function("ListMyStudyGroups")]
    public async Task<HttpResponseData> ListMine(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "study-groups")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var groups = await studyGroupService.ListMineAsync(userId);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(groups);
        return response;
    }

    [Function("GetStudyGroup")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "study-groups/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            var group = await studyGroupService.GetAsync(userId, id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(group);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("CreateStudyGroup")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "study-groups")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<CreateStudyGroupRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        var created = await studyGroupService.CreateAsync(userId, request);
        logger.LogInformation("Created study group {StudyGroupId} for user {UserId}", created.Id, userId);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created);
        return response;
    }

    [Function("UpdateStudyGroup")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "study-groups/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<UpdateStudyGroupRequest>();
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
            var updated = await studyGroupService.UpdateAsync(userId, id, request);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updated);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return await ApiResponses.ForbiddenAsync(req, ex.Message);
        }
    }

    [Function("DeleteStudyGroup")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "study-groups/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await studyGroupService.DeleteAsync(userId, id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return await ApiResponses.ForbiddenAsync(req, ex.Message);
        }
    }

    [Function("JoinStudyGroup")]
    public async Task<HttpResponseData> Join(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "study-groups/join")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<JoinStudyGroupRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await joinValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        try
        {
            var joined = await studyGroupService.JoinAsync(userId, request);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(joined);
            return response;
        }
        catch (DomainException ex)
        {
            return await ApiResponses.BadRequestAsync(req, ex.Message);
        }
    }

    [Function("LeaveStudyGroup")]
    public async Task<HttpResponseData> Leave(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "study-groups/{id:guid}/leave")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await studyGroupService.LeaveAsync(userId, id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
        catch (DomainException ex)
        {
            return await ApiResponses.BadRequestAsync(req, ex.Message);
        }
    }
}
