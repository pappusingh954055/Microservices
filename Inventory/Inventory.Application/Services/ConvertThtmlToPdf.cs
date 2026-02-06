using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Services
{
    public static class ConvertThtmlToPdf
    {
        public static string GenerateHtmlTemplate(CreditNotePrintDto data)
        {
            var sb = new StringBuilder();
            sb.Append(@"
        <html>
        <head>
            <style>
                .header { text-align: center; border-bottom: 2px solid #333; padding-bottom: 10px; }
                .info-table { width: 100%; margin: 20px 0; }
                .items-table { width: 100%; border-collapse: collapse; }
                .items-table th, .items-table td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                .total-section { float: right; width: 300px; margin-top: 20px; }
                .text-right { text-align: right; }
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>CREDIT NOTE</h1>
                <p>Electric Inventory System</p>
            </div>
            <table class='info-table'>
                <tr>
                    <td><strong>Customer:</strong> " + data.CustomerName + @"</td>
                    <td class='text-right'><strong>Date:</strong> " + data.ReturnDate.ToShortDateString() + @"</td>
                </tr>
                <tr>
                    <td><strong>Return No:</strong> " + data.ReturnNumber + @"</td>
                    <td class='text-right'><strong>Ref SO:</strong> " + data.SONumber + @"</td>
                </tr>
            </table>
            <table class='items-table'>
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Qty</th>
                        <th>Rate</th>
                        <th>Tax%</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>");

            foreach (var item in data.Items)
            {
                sb.Append($@"
                <tr>
                    <td>{item.ProductName}</td>
                    <td>{item.Qty}</td>
                    <td>{item.Rate:N2}</td>
                    <td>{item.TaxPercent}%</td>
                    <td>{item.Total:N2}</td>
                </tr>");
            }

            sb.Append($@"
                </tbody>
            </table>
            <div class='total-section'>
                <p>Sub-Total: {data.SubTotal:N2}</p>
                <p>Tax: {data.TotalTax:N2}</p>
                <hr/>
                <h3>Grand Total: ₹{data.GrandTotal:N2}</h3>
            </div>
        </body>
        </html>");

            return sb.ToString();
        }
    }
}
