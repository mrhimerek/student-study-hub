using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.CollegeApplications;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoints for the college application tracker. Mirrors
/// ScholarshipFunctions/ExamFunctions: all routes require a valid auth token and every
/// operation is implicitly scoped to the current user.
/// </summary>
public class CollegeApplicationFunctions(
    ICollegeApplicationService applicationService,
    ICurrentUserService currentUser,
    IValidator<CreateCollegeApplicationRequest> createValidator,
    IValidator<UpdateCollegeApplicationRequest> updateValidator,
    ILogger<CollegeApplicationFunctions> logger)
{
    [Function("ListCollegeApplications")]
    public async Task<HttpResponseData> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "college-applications")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var applications = await applicationService.ListAsync(userId);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(applications);
        return response;
    }

    [Function("GetCollegeApplication")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "college-applications/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            var application = await applicationService.GetAsync(userId, id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(application);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("CreateCollegeApplication")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "college-applications")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<CreateCollegeApplicationRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        var created = await applicationService.CreateAsync(userId, request);
        logger.LogInformation("Created college application {ApplicationId} for user {UserId}", created.Id, userId);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created);
        return response;
    }

    [Function("UpdateCollegeApplication")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "college-applications/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<UpdateCollegeApplicationRequest>();
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
            var updated = await applicationService.UpdateAsync(userId, id, request);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updated);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("DeleteCollegeApplication")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "college-applications/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await applicationService.DeleteAsync(userId, id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }
}
