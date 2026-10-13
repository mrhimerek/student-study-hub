using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Courses;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoints for Course CRUD, which backs the GPA calculator. Mirrors
/// AssignmentFunctions: all routes require a valid auth token and every operation is
/// implicitly scoped to the current user.
/// </summary>
public class CourseFunctions(
    ICourseService courseService,
    ICurrentUserService currentUser,
    IValidator<CreateCourseRequest> createValidator,
    IValidator<UpdateCourseRequest> updateValidator,
    ILogger<CourseFunctions> logger)
{
    [Function("ListCourses")]
    public async Task<HttpResponseData> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "courses")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var courses = await courseService.ListAsync(userId);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(courses);
        return response;
    }

    [Function("GetCourse")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "courses/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            var course = await courseService.GetAsync(userId, id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(course);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("CreateCourse")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "courses")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<CreateCourseRequest>();
        if (request is null)
        {
            return await ApiResponses.BadRequestAsync(req, "Request body is required.");
        }

        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ApiResponses.ValidationFailedAsync(req, validation);
        }

        var created = await courseService.CreateAsync(userId, request);
        logger.LogInformation("Created course {CourseId} for user {UserId}", created.Id, userId);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created);
        return response;
    }

    [Function("UpdateCourse")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "courses/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var request = await req.ReadFromJsonAsync<UpdateCourseRequest>();
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
            var updated = await courseService.UpdateAsync(userId, id, request);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updated);
            return response;
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("DeleteCourse")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "courses/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await courseService.DeleteAsync(userId, id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }
}
