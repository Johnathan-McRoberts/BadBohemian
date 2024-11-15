using BooksCore.Books;

namespace BooksPdfGenerators.Interfaces
{
    public interface IBooksReportGenerator
    {
        PdfExportContent GenerateBooksReport(List<BookRead> books);
    }
}
