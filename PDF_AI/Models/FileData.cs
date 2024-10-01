using PdfSharp.Pdf;

namespace PDF_AI.Models
{
    public class FileData
    {
        public string? File {  get; set; }
        public string? FileLocation { get; set; }
        public PdfDocument? Document { get; set; }
    }
}
