using Microsoft.AspNetCore.Http;
using FashionHubWeb.Models;
using FashionHubWeb.ViewModels;

namespace FashionHubWeb.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
