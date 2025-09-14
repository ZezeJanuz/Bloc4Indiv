// Nécessite le package PdfSharpCore
// dotnet add package PdfSharpCore

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Bloc4.Data.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Bloc4.Services
{
    public interface IPdfService
    {
        Task<string> GenerateSalariePdfAsync(Salarie s, string? outputPath = null);
    }

    public class PdfService : IPdfService
    {
        public Task<string> GenerateSalariePdfAsync(Salarie s, string? outputPath = null)
        {
            var file = outputPath ?? Path.Combine(Directory.GetCurrentDirectory(), $"Fiche_{s.Nom}_{s.Prenom}.pdf");

            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var title = new XFont("Arial", 22, XFontStyle.Bold);
            var text = new XFont("Arial", 12);

            gfx.DrawString("Fiche Salarié", title, XBrushes.Black, new XRect(0, 40, page.Width, 40), XStringFormats.TopCenter);

            int y = 120;
            void Line(string label, string? value)
            {
                gfx.DrawString($"{label} : {value}", text, XBrushes.Black, new XPoint(60, y));
                y += 24;
            }

            Line("Nom", s.Nom);
            Line("Prénom", s.Prenom);
            Line("Email", s.Email);
            Line("Téléphone fixe", s.TelephoneFixe);
            Line("Téléphone portable", s.TelephonePortable);
            Line("Service", s.Service?.Nom);
            Line("Site", s.Site?.Ville);

            doc.Save(file);
            doc.Dispose();

            try { Process.Start(new ProcessStartInfo { FileName = file, UseShellExecute = true }); } catch { }

            return Task.FromResult(file);
        }
    }
}