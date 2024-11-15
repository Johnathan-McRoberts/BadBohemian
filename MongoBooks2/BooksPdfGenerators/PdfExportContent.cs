namespace BooksPdfGenerators
{
    public class PdfExportContent
    {
        /// <summary>
        /// The content type for pdf documents.
        /// </summary>
        public const string WordContentType = "application/pdf";

        /// <summary>
        /// The file name that will be used in the Content-Disposition header of the response.
        /// </summary>
        public string FileDownloadName { get; set; }

        /// <summary>
        /// The bytes that represent the file contents.
        /// </summary>
        public Stream FileData { get; set; }

        public PdfExportContent(Stream fileData, string fileDownloadName)
        {
            FileData = fileData;
            FileDownloadName = fileDownloadName;
        }
    }
}
