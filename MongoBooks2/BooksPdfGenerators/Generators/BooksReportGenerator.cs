using System.Text;

using BooksCore.Books;

using BooksPdfGenerators.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace BooksPdfGenerators.Generators
{
    public class BooksReportGenerator : IBooksReportGenerator
    {
        private readonly string _dummyPdfContent =
            @"%PDF-1.
1 0 obj<</Pages 2 0 R>>endobj
2 0 obj<</Kids[3 0 R]/Count 1>>endobj
3 0 obj<</Parent 2 0 R>>endobj
trailer <</Root 1 0 R>>";

        public PdfExportContent GenerateBooksReport(List<BookRead> books)
        {
            MemoryStream pdfStream = new MemoryStream();
            PdfWriter writer = new PdfWriter(pdfStream);
            PdfDocument pdfDocument = new PdfDocument(writer.SetSmartMode(true));

            Document doc =
                new Document(pdfDocument, iText.Kernel.Geom.PageSize.LETTER);
            AddBooksContentToDocument(doc, books);

            doc.Close();

            byte[] pdfContent = pdfStream.ToArray();

            PdfExportContent content =
                new PdfExportContent(new MemoryStream(pdfContent), "BooksReport.pdf");

            return content;
        }

        private static void AddBooksContentToDocument(
            Document doc, 
            List<BookRead> books)
        {
            doc.Add(new Paragraph("Hello world!"));
        }
    }
}
