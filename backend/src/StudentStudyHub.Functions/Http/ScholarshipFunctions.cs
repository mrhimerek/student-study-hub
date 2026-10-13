using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Scholarships;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoints for the scholarship tracker. Mirrors ExamFunctions/CourseFunctions:
/// all routes require a valid auth token and every operation is implicitly scoped to the
/// current user.
/// </summary>
public class ScholarshipFunctions(
    IScholarshipService scholarshipService,
    ICurrentUserService currentUser,
    IValidator<CreateScholarshipRequest> createValidator,
    IValidator<UpdateScholarshipRequest> updateValidator,
    ILogger<ScholarshipFunctions> logger)
{
    [Function("ListScholarships")]
    public async Task<HttpResponseData> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "scholarships")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var scholarships = await scholarshipService.ListAsync(userId);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(scholarships);
        return response;
    }

    [Function("GetScholarship")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "scholarships/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            var scholarship = await scholarshipService.GetAsync(userId, id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(scholarship);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("CreateScholarship")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "scholarships")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<CreateScholarshipRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        var created = await scholarshipService.CreateAsync(userId, request);
        logger.LogInformation("Created scholarship {ScholarshipId} for user {UserId}", created.Id, userId);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created);
        return response;
    }

    [Function("UpdateScholarship")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "scholarships/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<UpdateScholarshipRequest>();
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
            var updated = await scholarshipService.UpdateAsync(userId, id, request);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updated);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("DeleteScholarship")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "scholarships/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await scholarshipService.DeleteAsync(userId, id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }
}
