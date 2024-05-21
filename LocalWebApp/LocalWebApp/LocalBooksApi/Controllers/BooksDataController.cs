using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Options;

using BooksControllerUtilities;
using BooksControllerUtilities.DataClasses;
using BooksControllerUtilities.RequestsResponses;
using BooksControllerUtilities.Settings;
using BooksControllerUtilities.Utilities;

namespace LocalBooksApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BooksDataController : Controller
    {
        #region Private data

        /// <summary>
        /// The books data controller utilities.
        /// </summary>
        private readonly BooksDataControllerUtilities _booksDataControllerUtilities;

        #endregion

        #region HTTP Handlers

        [HttpGet("[action]")]
        public IEnumerable<Book> GetAllBooks()
        {
            return _booksDataControllerUtilities.GetAllBooks();
        }

        [HttpGet("[action]")]
        public IEnumerable<Author> GetAllAuthors()
        {
            return _booksDataControllerUtilities.GetAllAuthors();
        }

        [HttpGet("[action]")]
        public IEnumerable<LanguageAuthors> GetAllLanguageAuthors()
        {
            return _booksDataControllerUtilities.GetAllLanguageAuthors();
        }

        [HttpGet("[action]")]
        public IEnumerable<CountryAuthors> GetAllCountryAuthors()
        {
            return _booksDataControllerUtilities.GetAllCountryAuthors();
        }

        [HttpGet("[action]")]
        public IEnumerable<BookTally> GetAllBookTallies()
        {
            return _booksDataControllerUtilities.GetAllBookTallies();
        }

        [HttpGet("[action]")]
        public IEnumerable<MonthlyTally> GetAllMonthlyTallies()
        {
            return _booksDataControllerUtilities.GetAllMonthlyTallies();
        }

        [HttpGet("[action]")]
        public IEnumerable<TagBooks> GetAllTagBooks()
        {
            return _booksDataControllerUtilities.GetAllTagBooks();
        }

        [HttpGet("[action]")]
        public IEnumerable<DeltaBooks> GetAllBooksDeltas()
        {
            return _booksDataControllerUtilities.GetAllBooksDeltas();
        }

        [HttpGet("[action]")]
        public IEnumerable<YearlyTally> GetAllYearlyTallies()
        {
            return _booksDataControllerUtilities.GetAllYearlyTallies();
        }

        [HttpGet("[action]")]
        public IEnumerable<NationGeography> GetAllNations()
        {
            return _booksDataControllerUtilities.GetAllNations();
        }

        [HttpGet("[action]")]
        public IEnumerable<NationDetail> GetAllNationDetails()
        {
            IEnumerable<NationGeography> nations = _booksDataControllerUtilities.GetAllNations();
            return nations.Select(nation => new NationDetail(nation)).ToList();
        }

        /// <summary>
        /// Updates the flag for a country.
        /// </summary>
        /// <param name="updateNation">The updated nation details.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        [Route("UpdateNationDetail")]
        public IActionResult UpdateNationDetail([FromBody] NationDetail updateNation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            NationDetailUpdateResponse response =
                _booksDataControllerUtilities.UpdateNationDetail(updateNation);

            return Ok(response);
        }

        [HttpGet("[action]/{userId}")]
        [ProducesResponseType(201, Type = typeof(ExportText))]
        public ExportText GetExportCsvText(string userId)
        {
            return _booksDataControllerUtilities.GetExportCsvText(userId);
        }

        [HttpGet("[action]/{userId}")]
        [ProducesResponseType(201, Type = typeof(ExportText))]
        [ProducesResponseType(404)]
        public IActionResult GetExportCsvFile(string userId)
        {
            FileStream exportFileStream =
                _booksDataControllerUtilities.GetExportCsvFile(userId);

            if (exportFileStream == null)
            {
                return NotFound();
            }

            return new FileStreamResult(exportFileStream, "text/csv");
        }

        [HttpGet("[action]/{userId}")]
        [ProducesResponseType(201, Type = typeof(ExportText))]
        public ExportText GetExportNationsCsvText(string userId)
        {
            return _booksDataControllerUtilities.GetExportNationsCsvText(userId);
        }

        [HttpGet("[action]/{userId}")]
        [ProducesResponseType(201, Type = typeof(ExportText))]
        [ProducesResponseType(404)]
        public IActionResult GetExportNationsCsvFile(string userId)
        {
            FileStream exportFileStream =
                _booksDataControllerUtilities.GetExportNationsCsvFile(userId);

            if (exportFileStream == null)
            {
                return NotFound();
            }

            return new FileStreamResult(exportFileStream, "text/csv");
        }

        [HttpGet("[action]")]
        [ProducesResponseType(200, Type = typeof(EditorDetails))]
        [ProducesResponseType(404)]
        public ActionResult<EditorDetails> GetEditorDetails()
        {
            return _booksDataControllerUtilities.GetEditorDetails();
        }

        [HttpGet("[action]/{userId}")]
        public IEnumerable<Book> GetAsDefaultUser(string userId)
        {
            return _booksDataControllerUtilities.GetAsDefaultUser(userId);
        }

        /// <summary>
        /// Adds a new user book read.
        /// </summary>
        /// <param name="bookReadAddRequest">The new book read to try to add.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        public IActionResult Post([FromBody] BookReadAddRequest bookReadAddRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BookReadAddResponse response =
                _booksDataControllerUtilities.AddNewBookRead(bookReadAddRequest);

            return Ok(response);
        }

        [HttpPut]
        public IActionResult UpdateAlbum([FromBody] Book existingBook)
        {
            BookReadAddResponse response =
                _booksDataControllerUtilities.UpdateExistingBook(existingBook);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            // set up the successful response
            BookReadAddResponse response =
                _booksDataControllerUtilities.DeleteBook(id);

            return Ok(response);
        }

        /// <summary>
        /// Sends export email to a user.
        /// </summary>
        /// <param name="exportDataRequest">The export parameters.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        [Route("ExportToEmail")]
        public IActionResult SendEmail([FromBody] ExportDataToEmailRequest exportDataRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ExportDataToEmailResponse response =
                _booksDataControllerUtilities.SendExportEmail(exportDataRequest);

            return Ok(response);
        }

        [HttpPost]
        [Route("UploadBooksCsv")]
        public IActionResult UploadBooksCsv(IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check that this is a csv file that has valid contents.
            if (string.IsNullOrEmpty(file.FileName) ||
                !file.FileName.ToLower().EndsWith(".csv") ||
                file.ContentType.ToLower() != "text/csv" ||
                file.Length < 1)
            {
                return BadRequest("Invalid file for upload");
            }

            BooksFromCsvFileImporter importer = new BooksFromCsvFileImporter();

            importer.ImportCsvFromStream(new StreamReader(file.OpenReadStream()));

            string result =
                _booksDataControllerUtilities.AddBooksFromImport(
                    "JMcR", importer.ImportedBooks);

            return Ok(result);
        }

        #endregion

        #region Constructor

        public BooksDataController(
            IOptions<MongoDbSettings> config,
            IOptions<SmtpConfig> mailConfig)
        {
            MongoDbSettings dbSettings = config.Value;

            _booksDataControllerUtilities =
                new BooksDataControllerUtilities(dbSettings, mailConfig.Value);
        }

        #endregion
    }
}
