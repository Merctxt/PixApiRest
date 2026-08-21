using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.HttpOverrides;
using DotNetEnv;
using PixApiRest.Services;
using Scalar.AspNetCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var serverAddress = Environment.GetEnvironmentVariable("SERVER_ADDRESS") ?? "0.0.0.0";
var serverPort = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "8080";
builder.WebHost.UseUrls($"http://{serverAddress}:{serverPort}");

builder.Services.AddScoped<PixPayloadService>();
builder.Services.AddScoped<QrCodeService>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title = "PIX API REST";
        doc.Info.Version = "v1";
        doc.Info.Description = "API para geração de pagamentos e QR Codes PIX";
        return Task.CompletedTask;
    });
});

var app = builder.Build();
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "PIX API REST";
    options.Theme = ScalarTheme.Purple;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.MapPost("/pix/payment", (PixPaymentRequest req, PixPayloadService pixPayloadService) =>
{
    var results = new List<ValidationResult>();
    if (!Validator.TryValidateObject(req, new ValidationContext(req), results, validateAllProperties: true))
        return Results.ValidationProblem(results.ToDictionary(r => r.MemberNames.FirstOrDefault() ?? "", r => new[] { r.ErrorMessage ?? "" }));

    var payload = pixPayloadService.GerarPayload(
        req.PixKey,
        req.Amount,
        req.ReceiverName,
        req.ReceiverCity,
        req.MerchantCategoryCode ?? "0000");

    return Results.Ok(new PixPaymentResponse(payload));
})
.WithName("CreatePixPayment")
.WithSummary("Gerar payload PIX")
.WithDescription("Gera o payload no padrão EMV/QR Code PIX a partir dos dados do pagamento.")
.WithTags("PIX")
.Produces<PixPaymentResponse>()
.ProducesValidationProblem();

app.MapPost("/pix/payment_static", (PixStaticPaymentRequest req, PixPayloadService pixPayloadService) =>
{
    var results = new List<ValidationResult>();
    if (!Validator.TryValidateObject(req, new ValidationContext(req), results, validateAllProperties: true))
        return Results.ValidationProblem(results.ToDictionary(r => r.MemberNames.FirstOrDefault() ?? "", r => new[] { r.ErrorMessage ?? "" }));

    var payload = pixPayloadService.GerarPayload(
        req.PixKey,
        req.ReceiverName,
        req.ReceiverCity,
        req.MerchantCategoryCode ?? "0000");

    return Results.Ok(new PixPaymentResponse(payload));
})
.WithName("CreateStaticPixPayment")
.WithSummary("Gerar payload PIX Estático")
.WithDescription("Gera o payload PIX sem valor definido para que o valor seja escolhido pelo pagador no momento da leitura.")
.WithTags("PIX")
.Produces<PixPaymentResponse>()
.ProducesValidationProblem();

app.MapPost("/pix/qrcode", (PixQrCodeRequest req, QrCodeService qrCodeService) =>
{
    var results = new List<ValidationResult>();
    if (!Validator.TryValidateObject(req, new ValidationContext(req), results, validateAllProperties: true))
        return Results.ValidationProblem(results.ToDictionary(r => r.MemberNames.FirstOrDefault() ?? "", r => new[] { r.ErrorMessage ?? "" }));

    var imageBytes = qrCodeService.GerarQrCode(req.Payload);
    return Results.File(imageBytes, "image/png");
})
.WithName("GenerateQrCode")
.WithSummary("Gerar QR Code PIX")
.WithDescription("Gera a imagem PNG do QR Code a partir de um payload PIX EMV.")
.WithTags("PIX")
.Produces<byte[]>(contentType: "image/png")
.ProducesValidationProblem();

app.Run();

// --- Request / Response models ---

record PixStaticPaymentRequest(
    [property: Required(ErrorMessage = "A chave PIX é obrigatória")]
    [property: MaxLength(77, ErrorMessage = "A chave PIX deve ter no máximo 77 caracteres")]
    string PixKey,

    [property: Required(ErrorMessage = "O nome do recebedor é obrigatório")]
    [property: MaxLength(25, ErrorMessage = "O nome deve ter no máximo 25 caracteres")]
    string ReceiverName,

    [property: Required(ErrorMessage = "A cidade do recebedor é obrigatória")]
    [property: MaxLength(15, ErrorMessage = "A cidade deve ter no máximo 15 caracteres")]
    string ReceiverCity = "SAO PAULO",

    [property: RegularExpression("^[0-9]{4}$", ErrorMessage = "O código de categoria deve ter 4 dígitos")]
    string? MerchantCategoryCode = "0000"
);

record PixPaymentRequest(
    [property: Required(ErrorMessage = "O valor é obrigatório")]
    [property: Range(0.01, 99999999.99, ErrorMessage = "O valor deve estar entre R$ 0,01 e R$ 99.999.999,99")]
    decimal Amount,

    [property: Required(ErrorMessage = "A chave PIX é obrigatória")]
    [property: MaxLength(77, ErrorMessage = "A chave PIX deve ter no máximo 77 caracteres")]
    string PixKey,

    [property: Required(ErrorMessage = "O nome do recebedor é obrigatório")]
    [property: MaxLength(25, ErrorMessage = "O nome deve ter no máximo 25 caracteres")]
    string ReceiverName,

    [property: Required(ErrorMessage = "A cidade do recebedor é obrigatória")]
    [property: MaxLength(15, ErrorMessage = "A cidade deve ter no máximo 15 caracteres")]
    string ReceiverCity = "SAO PAULO",

    [property: RegularExpression("^[0-9]{4}$", ErrorMessage = "O código de categoria deve ter 4 dígitos")]
    string? MerchantCategoryCode = "0000"
);

record PixPaymentResponse(string Payload);

record PixQrCodeRequest(
    [property: Required(ErrorMessage = "O payload é obrigatório")]
    string Payload
);
