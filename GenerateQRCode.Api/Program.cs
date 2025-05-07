using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;

QuestPDF.Settings.License = LicenseType.Community; 

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/qrcode-pdf", GenerateQRCodePdf);

static IResult GenerateQRCodePdf()
{
    string text = "52023288851";

    try
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Results.BadRequest(new
            {
                Code = 400,
                Message = "Texto é obrigatório",
                Example = "/qrcode-pdf?text=SeuTextoAqui"
            });
        }

        var qrCodeImage = GenerateQRCodeImage(text);

        var pdfBytes = Document.Create(container =>
        {

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header()
                    .AlignCenter()
                    .Text("PIX ISA E GUI")
                    .FontSize(20);

                page.Content()
                    .Image(qrCodeImage)
                    .FitWidth();
            });
        }).GeneratePdf();

        return Results.File(pdfBytes, "application/pdf", $"qrcode-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Erro na geração do PDF",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError,
            extensions: new Dictionary<string, object?>
            {
                {"stackTrace", ex.StackTrace}
            }
        );
    }
}

static byte[] GenerateQRCodeImage(string text)
{
    using var qrGenerator = new QRCodeGenerator();
    var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
    var qrCode = new PngByteQRCode(qrCodeData);
    return qrCode.GetGraphic(10, ColorTranslator.FromHtml("#2D3748"), ColorTranslator.FromHtml("#F7FAFC"));
}

await app.RunAsync();