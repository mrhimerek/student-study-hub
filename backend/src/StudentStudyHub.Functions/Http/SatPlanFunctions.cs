using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.SatPlanning;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoints for the SAT study planner. Unlike most other feature endpoints
/// (Exams/Scholarships/...), this is a single-record-per-user resource, so there's no {id:guid}
/// route segment for the plan itself - GET returns 204 when the user hasn't created one yet, and
/// POST/PUT/DELETE operate implicitly on "the current user's plan". Practice scores are nested
/// under the plan and return the refreshed plan (with the updated score list) on every mutation.
/// </summary>
public class SatPlanFunctions(
    ISatPlanService satPlanService,
    ICurrentUserService currentUser,
    IValidator<CreateSatPlanRequest> createValidator,
    IValidator<UpdateSatPlanRequest> updateValidator,
    IValidator<LogSatPracticeScoreRequest> logScoreValidator,
    ILogger<SatPlanFunctions> logger)
{
    [Function("GetSatPlan")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sat-plan")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var plan = await satPlanService.GetAsync(userId);
        if (plan is null)
        {
            return req.CreateResponse(HttpStatusCode.NoContent);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(plan);
        return response;
    }

    [Function("CreateSatPlan")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sat-plan")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<CreateSatPlanRequest>();
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
            var created = await satPlanService.CreateAsync(userId, request);
            logger.LogInformation("Created SAT plan {SatPlanId} for user {UserId}", created.Id, userId);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(created);
            return response;
        }
        catch (DomainException ex)
        {
            return await ApiResponses.BadRequestAsync(req, ex.Message);
        }
    }

    [Function("UpdateSatPlan")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "sat-plan")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<UpdateSatPlanRequest>();
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
            var updated = await satPlanService.UpdateAsync(userId, request);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updated);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("DeleteSatPlan")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "sat-plan")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await satPlanService.DeleteAsync(userId);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("LogSatPracticeScore")]
    public async Task<HttpResponseData> LogScore(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sat-plan/scores")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<LogSatPracticeScoreRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await logScoreValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        try
        {
            var updated = await satPlanService.LogPracticeScoreAsync(userId, request);
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(updated);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("DeleteSatPracticeScore")]
    public async Task<HttpResponseData> DeleteScore(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "sat-plan/scores/{scoreId:guid}")] HttpRequestData req,
        Guid scoreId)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            var updated = await satPlanService.DeletePracticeScoreAsync(userId, scoreId);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updated);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }
}
