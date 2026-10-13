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
/// HTTP-triggered endpoints for notes shared within a study group, nested under
/// study-groups/{groupId}/notes. All routes require a valid auth token and group membership -
/// see GroupNoteService for the NotFound-vs-Forbidden rules this enforces.
/// </summary>
public class GroupNoteFunctions(
    IGroupNoteService noteService,
    ICurrentUserService currentUser,
    IValidator<CreateGroupNoteRequest> createValidator,
    IValidator<UpdateGroupNoteRequest> updateValidator,
    ILogger<GroupNoteFunctions> logger)
{
    [Function("ListGroupNotes")]
    public async Task<HttpResponseData> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "study-groups/{groupId:guid}/notes")] HttpRequestData req,
        Guid groupId)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            var notes = await noteService.ListAsync(userId, groupId);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(notes);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("CreateGroupNote")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "study-groups/{groupId:guid}/notes")] HttpRequestData req,
        Guid groupId)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<CreateGroupNoteRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        try
        {
            var created = await noteService.CreateAsync(userId, groupId, request);
            logger.LogInformation("Created note {NoteId} in study group {StudyGroupId}", created.Id, groupId);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(created);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("UpdateGroupNote")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "study-groups/{groupId:guid}/notes/{noteId:guid}")] HttpRequestData req,
        Guid groupId,
        Guid noteId)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<UpdateGroupNoteRequest>();
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
            var updated = await noteService.UpdateAsync(userId, groupId, noteId, request);
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

    [Function("DeleteGroupNote")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "study-groups/{groupId:guid}/notes/{noteId:guid}")] HttpRequestData req,
        Guid groupId,
        Guid noteId)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await noteService.DeleteAsync(userId, groupId, noteId);
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
}
