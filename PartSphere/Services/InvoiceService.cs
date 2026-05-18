using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PartSphere.Models;

namespace PartSphere.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateSalesInvoicePdf(SalesInvoice invoice);
    }

    public class InvoiceService : IInvoiceService
    {
        public byte[] GenerateSalesInvoicePdf(SalesInvoice invoice)
        {

            Document document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("PartSphere AI").FontSize(20).SemiBold().FontColor(Colors.Indigo.Medium);
                            col.Item().Text("Advanced Vehicle Inventory System");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"Invoice #{invoice.Id}").FontSize(14).SemiBold();
                            col.Item().Text($"Date: {invoice.Date:dd MMM yyyy}");
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(innerCol =>
                            {
                                innerCol.Item().Text("Bill To:").SemiBold();
                                innerCol.Item().Text(invoice.Customer.Name);
                                innerCol.Item().Text(invoice.Customer.Email);
                                innerCol.Item().Text(invoice.Customer.Phone);
                            });

                            if (invoice.Vehicle != null)
                            {
                                row.RelativeItem().AlignRight().Column(innerCol =>
                                {
                                    innerCol.Item().Text("Vehicle:").SemiBold();
                                    innerCol.Item().Text($"{invoice.Vehicle.Brand} {invoice.Vehicle.Model}");
                                    innerCol.Item().Text($"Plate: {invoice.Vehicle.VehicleNumber}");
                                    innerCol.Item().Text($"Mileage: {invoice.MileageAtSale} km");
                                });
                            }
                        });

                        col.Item().PaddingTop(1, Unit.Centimetre).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(40);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("Item / Description");
                                header.Cell().Element(CellStyle).AlignRight().Text("Unit Price");
                                header.Cell().Element(CellStyle).AlignCenter().Text("Qty");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            foreach (var item in invoice.Items.Select((x, i) => new { Item = x, Index = i + 1 }))
                            {
                                table.Cell().Element(ItemStyle).Text(item.Index.ToString());
                                table.Cell().Element(ItemStyle).Text(item.Item.VehiclePart.Name);
                                table.Cell().Element(ItemStyle).AlignRight().Text($"${item.Item.UnitPrice:F2}");
                                table.Cell().Element(ItemStyle).AlignCenter().Text(item.Item.Quantity.ToString());
                                table.Cell().Element(ItemStyle).AlignRight().Text($"${item.Item.TotalPrice:F2}");

                                IContainer ItemStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        });

                        col.Item().AlignRight().PaddingTop(10).Column(innerCol =>
                        {
                            innerCol.Item().Text($"Subtotal: ${invoice.TotalAmount + invoice.DiscountAmount:F2}");
                            innerCol.Item().Text($"Discount: -${invoice.DiscountAmount:F2}").FontColor(Colors.Green.Medium);
                            innerCol.Item().Text($"Total Due: ${invoice.TotalAmount:F2}").FontSize(14).SemiBold();
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
