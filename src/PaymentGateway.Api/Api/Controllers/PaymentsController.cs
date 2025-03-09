using System.Net;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.DTOs.Requests;
using PaymentGateway.Api.Application.DTOs.Responses;
using PaymentGateway.Api.Application.Validators;
using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentsController(PaymentsRepository paymentsRepository, 
        IHttpClientFactory httpClientFactory)
    {
        _paymentsRepository = paymentsRepository;
        _httpClientFactory = httpClientFactory; }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentRequest?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        return (payment != null) ? new OkObjectResult(payment): new NotFoundObjectResult(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> CreatePaymentAsync([FromBody] PostPaymentRequest request)
    {
        //validate request
        //call service
        //return response
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("http://localhost:8080/payments", request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return new BadRequestResult();
        }

        //_paymentsRepository.Add(payment);

        return new OkObjectResult(new PostPaymentResponse());
    }
}