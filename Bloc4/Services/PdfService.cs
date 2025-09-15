// Nécessite le package PdfSharpCore
// dotnet add package PdfSharpCore

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bloc4.Data.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Bloc4.Services
{
    public class PdfService : IPdfService
    {
        public async Task ExportSalarieAsync(Salarie s, string? outputPath = null)
        {
            _ = await GenerateSalariePdfAsync(s, outputPath);
        }

        public Task<string> ExportPdfAsync(Salarie s, string? outputPath = null)
            => GenerateSalariePdfAsync(s, outputPath);

        public Task<string> GenerateSalariePdfAsync(Salarie s, string? outputPath = null)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            string Safe(string? txt) => string.IsNullOrWhiteSpace(txt) ? "-" : txt.Trim();
            string SanitizeFile(string name) => Regex.Replace(name, @"[^\w\-. ]+", "_");

            var baseName = SanitizeFile($"Fiche_{Safe(s.Nom)}_{Safe(s.Prenom)}.pdf");
            var file = outputPath ?? Path.Combine(Directory.GetCurrentDirectory(), baseName);

            var dir = Path.GetDirectoryName(file);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var doc = new PdfDocument())
            {
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;

                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    const double margin = 50;
                    double x = margin, y = margin, w = page.Width - 2 * margin;

                    var titleFont = new XFont("Arial", 22, XFontStyle.Bold);
                    var h2Font    = new XFont("Arial", 14, XFontStyle.Bold);
                    var textFont  = new XFont("Arial", 12, XFontStyle.Regular);

                    gfx.DrawString("Fiche Salarié", titleFont, XBrushes.Black, new XRect(x, y, w, 30), XStringFormats.TopLeft);
                    y += 38;

                    gfx.DrawLine(XPens.Gray, x, y, x + w, y);
                    y += 14;

                    gfx.DrawString("Identité", h2Font, XBrushes.Black, new XRect(x, y, w, 20), XStringFormats.TopLeft);
                    y += 26;

                    void Line(string label, string? value)
                    {
                        gfx.DrawString($"{label} : {Safe(value)}", textFont, XBrushes.Black, new XRect(x, y, w, 16), XStringFormats.TopLeft);
                        y += 18;
                    }

                    Line("Nom", s.Nom);
                    Line("Prénom", s.Prenom);
                    Line("Email", s.Email);
                    Line("Téléphone fixe", s.TelephoneFixe);
                    Line("Téléphone portable", s.TelephonePortable);

                    y += 8;
                    gfx.DrawLine(XPens.LightGray, x, y, x + w, y);
                    y += 14;

                    gfx.DrawString("Affectation", h2Font, XBrushes.Black, new XRect(x, y, w, 20), XStringFormats.TopLeft);
                    y += 26;

                    Line("Service", s.Service?.Nom);
                    Line("Site", s.Site?.Ville);

                    var footer = $"Généré le {DateTime.Now:dd/MM/yyyy HH:mm}";
                    gfx.DrawString(footer, new XFont("Arial", 9, XFontStyle.Italic), XBrushes.Gray,
                                   new XRect(margin, page.Height - margin + 5, page.Width - 2 * margin, 12),
                                   XStringFormats.BottomLeft);
                }

                doc.Save(file);
            }

            try { Process.Start(new ProcessStartInfo { FileName = file, UseShellExecute = true }); } catch { }
            return Task.FromResult(file);
        }
    }
}