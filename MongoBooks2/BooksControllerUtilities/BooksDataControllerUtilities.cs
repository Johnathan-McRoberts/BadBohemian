// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BooksControllerUtilities.cs" company="N/A">
//   2017-2086
// </copyright>
// <summary>
//   The utilities class for the BooksDataController.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace BooksControllerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CsvHelper;

    using BooksControllerUtilities.DataClasses;
    using BooksControllerUtilities.RequestsResponses;
    using BooksControllerUtilities.Settings;
    using BooksControllerUtilities.Utilities;

    using BooksCore.Books;
    using BooksCore.Geography;
    using BooksCore.Provider;
    using BooksCore.Users;

    using BooksDatabase.Implementations;
    using BooksMailbox;

    using BooksPdfGenerators;
    using BooksPdfGenerators.Interfaces;
    using BooksPdfGenerators.Generators;

    public class BooksDataControllerUtilities : BaseControllerUtilities
    {
        #region Constants

        public const string DefaultDatabaseConnectionString = "mongodb://localhost:27017";

        /// <summary>
        /// The connection string for the database.
        /// </summary>
        public readonly string DatabaseConnectionString;

        /// <summary>
        /// The export directory to put temporary files into.
        /// </summary>
        public readonly string ExportDirectory;

        public readonly DateTime EarliestDate = DateTime.Now.AddYears(-20);

        public readonly string[] SuffixedDaysOfMonths =
        {
            "0th",  "1st",  "2nd",  "3rd",  "4th",  "5th",  "6th",  "7th",  "8th",  "9th",
            "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th",
            "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th",
            "30th", "31st"
        };

        private readonly string AuthCodeText = "AUTH-CODE";

        private readonly string ActivateTimeText = "ACTIVATE-TIME";

        private readonly string AuthMessageSubject = "McBob's Books Authentication";

        private readonly string AuthMessageBodyTop =
            @"<body>
                <p>Please enter the following code to activate the new user login:</p>";

        private readonly string AuthMessageBodyAuthCode =
            @"<h4>AUTH-CODE</h4>";

        private readonly string AuthMessageBodyActivateTime =
            @"<p>This activation code will expire at: <b>ACTIVATE-TIME</b><p>";

        private readonly string AuthMessageBodyBottom =
            @"<p>Thanks and enjoy the trip!<p>
                </body>";


        private readonly string ExportNoteText = "EXPORT-NOTE";

        private readonly string ExportMessageSubject = "McBob's Books Export";

        private readonly string ExportMessageBodyTop =
            @"<body>
                <p>Please find attached the latest books read:</p>";

        private readonly string ExportMessageBodyNote =
            @"<p>EXPORT-NOTE</p>";

        private readonly string ExporthMessageBodyBottom =
            @"<p>Thanks and hope you enjoy!<p>
                </body>";


        #endregion

        #region Private data

        /// <summary>
        /// The books read database.
        /// </summary>
        private readonly BooksReadDatabase _booksReadDatabase;

        /// <summary>
        /// The nations read database.
        /// </summary>
        private readonly NationDatabase _nationsReadDatabase;

        /// <summary>
        /// The users read database.
        /// </summary>
        private readonly UserDatabase _userDatabase;

        /// <summary>
        /// The books read from database.
        /// </summary>
        private ObservableCollection<BookRead> _booksReadFromDatabase;

        /// <summary>
        /// The nations read from database.
        /// </summary>
        private ObservableCollection<Nation> _nationsReadFromDatabase;

        /// <summary>
        /// The users read from database.
        /// </summary>
        private ObservableCollection<User> _usersReadFromDatabase;

        /// <summary>
        /// The books read from database.
        /// </summary>
        private ObservableCollection<Book> _books;

        /// <summary>
        /// The authors read from database.
        /// </summary>
        private ObservableCollection<Author> _authors;

        /// <summary>
        /// The nations read from database.
        /// </summary>
        private ObservableCollection<NationGeography> _nations;

        /// <summary>
        /// The pdf report generator.
        /// </summary>
        private readonly IBooksReportGenerator _booksReportGenerator;

        private readonly SmtpConfig _smtpConfig;

        #endregion

        #region Utility Functions

        /// <summary>
        /// Gets the providers for the books and geography data.
        /// </summary>
        /// <param name="geographyProvider">The geography data provider on exit.</param>
        /// <param name="booksReadProvider">The books data provider on exit.</param>
        /// <returns>True if successful, false otherwise.</returns>
        protected bool GetProviders(out GeographyProvider geographyProvider, out BooksReadProvider booksReadProvider)
        {
            geographyProvider = null;
            booksReadProvider = null;

            if (!_booksReadDatabase.ReadFromDatabase)
            {
                _booksReadDatabase.ConnectToDatabase();
            }

            if (!_nationsReadDatabase.ReadFromDatabase)
            {
                _nationsReadDatabase.ConnectToDatabase();
            }

            if (_booksReadDatabase.ReadFromDatabase && _nationsReadDatabase.ReadFromDatabase)
            {
                // Setup the providers.
                geographyProvider = new GeographyProvider();
                geographyProvider.Setup(_nationsReadDatabase.LoadedItems);

                booksReadProvider = new BooksReadProvider();
                booksReadProvider.Setup(_booksReadDatabase.LoadedItems, geographyProvider);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets if there is a valid new book read based on the request.
        /// </summary>
        /// <param name="addRequest">The request to add a book read.</param>
        /// <param name="newBook">The new book  to add on exit.</param>
        /// <returns>True if a valid new book, false otherwise.</returns>
        private bool GetBookRead(Book addRequest, out BookRead newBook)
        {
            newBook = new BookRead
            {
                Author = addRequest.Author,
                Title = addRequest.Title,
                Pages = (ushort)addRequest.Pages,
                Note = addRequest.Note,
                Nationality = addRequest.Nationality,
                OriginalLanguage = addRequest.OriginalLanguage,
                ImageUrl = addRequest.ImageUrl,
                Tags = addRequest.Tags.ToList()
            };

            return CheckBookValidity(newBook, addRequest.Format, addRequest.Date);
        }

        private bool CheckBookValidity(
            BookRead newBook,
            string format,
            DateTime addDate)
        {
            // Check the required strings are ok.
            if (string.IsNullOrWhiteSpace(newBook.Author)
                || string.IsNullOrWhiteSpace(newBook.Title)
                || string.IsNullOrWhiteSpace(newBook.Nationality)
                || string.IsNullOrWhiteSpace(newBook.OriginalLanguage))
            {
                return false;
            }

            // Check the format is valid
            switch (format)
            {
                case "Book":
                    newBook.Format = BookFormat.Book;
                    break;
                case "Comic":
                    newBook.Format = BookFormat.Comic;
                    break;
                case "Audio":
                    newBook.Format = BookFormat.Audio;
                    break;
                default:
                    return false;
            }

            // Check the date
            if (DateTime.Now < addDate || addDate < EarliestDate)
            {
                return false;
            }

            // Set the date string 
            newBook.Date = addDate;
            newBook.DateString =
                SuffixedDaysOfMonths[newBook.Date.Day] + newBook.Date.ToString(" MMMM yyyy");

            return true;
        }

        /// <summary>
        /// Gets if there is a valid new book read based on the request.
        /// </summary>
        /// <param name="addRequest">The request to add a book read.</param>
        /// <param name="newBook">The new book  to add on exit.</param>
        /// <returns>True if a valid new book, false otherwise.</returns>
        private bool GetBookRead(BookReadAddRequest addRequest, out BookRead newBook)
        {
            newBook = new BookRead
            {
                Author = addRequest.Author,
                Title = addRequest.Title,
                Pages = addRequest.Pages,
                Note = addRequest.Note,
                Nationality = addRequest.Nationality,
                OriginalLanguage = addRequest.OriginalLanguage,
                ImageUrl = addRequest.ImageUrl,
                Tags = addRequest.Tags.ToList()
            };

            // Check the required strings are ok.
            if (string.IsNullOrWhiteSpace(newBook.Author)
                || string.IsNullOrWhiteSpace(newBook.Title)
                || string.IsNullOrWhiteSpace(newBook.Nationality)
                || string.IsNullOrWhiteSpace(newBook.OriginalLanguage))
            {
                return false;
            }

            // Check the format is valid
            switch (addRequest.Format)
            {
                case "Book":
                    newBook.Format = BookFormat.Book;
                    break;
                case "Comic":
                    newBook.Format = BookFormat.Comic;
                    break;
                case "Audio":
                    newBook.Format = BookFormat.Audio;
                    break;
                default:
                    return false;
            }

            // Check the date
            if (DateTime.Now < addRequest.Date || addRequest.Date < EarliestDate)
            {
                return false;
            }

            // Set the date string 
            newBook.Date = addRequest.Date;
            newBook.DateString =
                SuffixedDaysOfMonths[newBook.Date.Day] + newBook.Date.ToString(" MMMM yyyy");

            return true;
        }

        private string GetExportFileName(string exportDirectory, string fileName, string extension = ".csv")
        {
            DateTime time = DateTime.Now;
            fileName += " ";
            fileName += time.ToString("yyyy MMMM dd H-mm-ss");
            fileName += extension;
            return Path.Combine(exportDirectory, fileName);
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private void WriteBooksToPdfFile(string fileName, List<BookRead> books)
        {
            PdfExportContent booksPdf = 
                _booksReportGenerator.GenerateBooksReport(books);

            using (MemoryStream memoryStream = new MemoryStream(ReadFully(booksPdf.FileData)))
            {
                using Stream streamToWriteTo = File.Open(fileName, FileMode.Create);

                memoryStream.Position = 0;
                memoryStream.CopyTo(streamToWriteTo);
            }           
        }

        public void WriteBooksToFile(string filename, List<BookRead> booksRead)
        {
            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                // write the header
                sw.WriteLine(
                    "Date,DD/MM/YYYY,Author,Title,Pages,Note,Nationality,Original Language,Book,Comic,Audio,Image,Tags"
                );

                // write the records
                var csv = new CsvWriter(sw);
                foreach (var book in booksRead)
                {
                    csv.WriteField(book.DateString);
                    csv.WriteField(book.Date.ToString("d/M/yyyy"));
                    csv.WriteField(book.Author);
                    csv.WriteField(book.Title);
                    csv.WriteField(book.Pages > 0 ? book.Pages.ToString() : "");
                    csv.WriteField(book.Note);
                    csv.WriteField(book.Nationality);
                    csv.WriteField(book.OriginalLanguage);
                    csv.WriteField(book.Format == BookFormat.Book ? "x" : "");
                    csv.WriteField(book.Format == BookFormat.Comic ? "x" : "");
                    csv.WriteField(book.Format == BookFormat.Audio ? "x" : "");
                    csv.WriteField(book.ImageUrl);
                    csv.WriteField(book.DisplayTags);
                    csv.NextRecord();
                }

                // tidy up
                sw.Close();
            }
        }

        public void WriteBooksToFile(string filename, List<BookRead> booksRead, out string formattedText)
        {
            BooksExporter.ExportToCsvFile(booksRead, out formattedText);

            WriteBooksToFile(filename, booksRead);
        }

        private void WriteNationsToFile(string fileName, List<Nation> nations, out string formattedText)
        {
            // Get the text
            NationsExporter.ExportToCsvFile(nations, out formattedText);

            // open the write stream
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                // loop through the lines of text
                using (StringReader reader = new StringReader(formattedText))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // write the line
                        sw.WriteLine(line);
                    }
                }

                // tidy up
                sw.Close();
            }
        }

        private void SendExportEmailToUser(ExportDataToEmailRequest exportRequest)
        {
            // Set up the providers
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            // Get the file export string 
            string formattedText = null;
            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                if (booksReadProvider.BooksRead != null && booksReadProvider.BooksRead.Any())
                {
                    List<BookRead> booksRead =
                        booksReadProvider.BooksRead.OrderBy(x => x.Date).ToList();

                    BooksExporter.ExportToCsvFile(booksRead, out formattedText);
                }
            }

            if (formattedText == null)
            {
                throw new Exception("Could not get books file");
            }

            // Set up the message
            string messageBody = ExportMessageBodyTop;

            if (!string.IsNullOrWhiteSpace(exportRequest.Note))
            {
                string messageNote =
                    ExportMessageBodyNote.Replace(ExportNoteText, exportRequest.Note);
                messageBody += messageNote;
            }

            messageBody += ExporthMessageBodyBottom;

            // Set up the mail connection parameters
            StmpConnection connection =
                new StmpConnection(_smtpConfig.EmailAddress, _smtpConfig.Password);

            // Set up the message parameters
            HtmlEmailDefinition emailDefinition =
                new HtmlEmailDefinition
                {
                    FromEmailDisplayName = _smtpConfig.Username,
                    ToEmail = exportRequest.DestinationEmail,
                    ToEmailDisplayName = "Guy we know",
                    Subject = ExportMessageSubject,
                    BodyHtml = messageBody
                };

            SmtpEmailer.SendHtmlEmailWithAttachment(
                connection, emailDefinition, formattedText);
        }

        #endregion

        #region HTTP Handlers

        public IEnumerable<Book> GetAllBooks()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (var bookRead in booksReadProvider.BooksRead)
                {
                    _books.Add(new Book(bookRead));
                }
            }

            return _books;
        }

        public IEnumerable<Author> GetAllAuthors()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _authors = new ObservableCollection<Author>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (var authorRead in booksReadProvider.AuthorsRead)
                {
                    _authors.Add(new Author(authorRead));
                }
            }

            return _authors;
        }

        public IEnumerable<LanguageAuthors> GetAllLanguageAuthors()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            var languageAuthors = new ObservableCollection<LanguageAuthors>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (AuthorLanguage authorLanguage in booksReadProvider.AuthorLanguages)
                {
                    languageAuthors.Add(new LanguageAuthors(authorLanguage));
                }
            }

            return languageAuthors;
        }

        public IEnumerable<CountryAuthors> GetAllCountryAuthors()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            var countryAuthors = new ObservableCollection<CountryAuthors>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (AuthorCountry authorCountry in booksReadProvider.AuthorCountries)
                {
                    countryAuthors.Add(new CountryAuthors(authorCountry));
                }
            }

            return countryAuthors;
        }

        public IEnumerable<BookTally> GetAllBookTallies()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            var bookTallies = new ObservableCollection<BookTally>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (TalliedBook talliedBook in booksReadProvider.TalliedBooks)
                {
                    bookTallies.Add(new BookTally(talliedBook));
                }
            }

            return bookTallies;
        }

        public IEnumerable<MonthlyTally> GetAllMonthlyTallies()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            var monthlyTallies = new ObservableCollection<MonthlyTally>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (TalliedMonth talliedMonth in booksReadProvider.TalliedMonths)
                {
                    monthlyTallies.Add(new MonthlyTally(talliedMonth));
                }
            }

            return monthlyTallies;
        }

        public IEnumerable<TagBooks> GetAllTagBooks()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            var tagBooks = new ObservableCollection<TagBooks>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (BookTag bookTag in booksReadProvider.BookTags)
                {
                    tagBooks.Add(new TagBooks(bookTag));
                }
            }

            return tagBooks;
        }

        public IEnumerable<DeltaBooks> GetAllBooksDeltas()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            var booksDeltas = new ObservableCollection<DeltaBooks>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (BooksDelta bookDeltas in booksReadProvider.BookDeltas)
                {
                    booksDeltas.Add(new DeltaBooks(bookDeltas));
                }
            }

            return booksDeltas;
        }

        public IEnumerable<YearlyTally> GetAllYearlyTallies()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            List<YearlyTally> yearlyTallies = new List<YearlyTally>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                yearlyTallies =
                    YearlyTally.GetYearlyTalliesForBooks(booksReadProvider.BooksRead.ToList());
            }

            return yearlyTallies;
        }

        public IEnumerable<NationGeography> GetAllNations()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _nations = new ObservableCollection<NationGeography>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (Nation nation in geographyProvider.Nations.OrderBy(x => x.Name))
                {
                    _nations.Add(new NationGeography(nation));
                }
            }

            return _nations;
        }

        public ExportText GetExportCsvText(string userId)
        {
            ExportText exportText = new ExportText { Format = "text/plain" };

            User foundUser = _userDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == userId);
            if (foundUser == null)
            {
                return exportText;
            }

            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                if (booksReadProvider.BooksRead != null && booksReadProvider.BooksRead.Any())
                {
                    List<BookRead> books =
                        booksReadProvider.BooksRead.Where(x => x.User == foundUser.Name).OrderBy(x => x.Date).ToList();

                    // Get the file export string 
                    string formattedText;

                    BooksExporter.ExportToCsvFile(books, out formattedText);


                    // Return the formatted text
                    exportText.FormattedText = formattedText;
                }
            }

            return exportText;
        }

        public FileStream GetExportCsvFile(string userId)
        {
            User foundUser = _userDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == userId);
            if (foundUser == null)
            {
                return null;
            }

            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                if (booksReadProvider.BooksRead != null && booksReadProvider.BooksRead.Any())
                {
                    List<BookRead> books =
                        booksReadProvider.BooksRead.Where(x => x.User == foundUser.Name).OrderBy(x => x.Date).ToList();

                    // Get the file export string 
                    string formattedText;
                    string fileName = GetExportFileName(ExportDirectory, "Books");
                    WriteBooksToFile(fileName, books, out formattedText);


                    // Return the formatted text
                    if (File.Exists(fileName))
                    {
                        return new FileStream(fileName, FileMode.Open);
                    }
                }
            }

            return null;
        }

        public ExportText GetExportNationsCsvText(string userId)
        {
            ExportText exportText = new ExportText { Format = "text/plain" };

            User foundUser = _userDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == userId);
            if (foundUser == null)
            {
                return exportText;
            }

            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                if (geographyProvider.Nations != null && geographyProvider.Nations.Any())
                {
                    List<Nation> nations =
                        geographyProvider.Nations.OrderBy(x => x.Name).ToList();

                    // Get the file export string 
                    string formattedText;

                    NationsExporter.ExportToCsvFile(nations, out formattedText);

                    // Return the formatted text
                    exportText.FormattedText = formattedText;
                }
            }

            return exportText;
        }

        public FileStream GetExportNationsCsvFile(string userId)
        {
            User foundUser = _userDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == userId);
            if (foundUser == null)
            {
                return null;
            }

            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                if (geographyProvider.Nations != null && geographyProvider.Nations.Any())
                {
                    List<Nation> nations =
                        geographyProvider.Nations.OrderBy(x => x.Name).ToList();

                    // Get the file export string 
                    string formattedText;
                    string fileName = GetExportFileName(ExportDirectory, "Nations");
                    WriteNationsToFile(fileName, nations, out formattedText);

                    // Return the formatted text
                    if (File.Exists(fileName))
                    {
                        return new FileStream(fileName, FileMode.Open);
                    }
                }
            }

            return null;
        }

        public FileStream GetExportPdfFile(string userId)
        {
            User foundUser = _userDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == userId);
            if (foundUser == null)
            {
                return null;
            }

            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                if (booksReadProvider.BooksRead != null && booksReadProvider.BooksRead.Any())
                {
                    List<BookRead> books =
                        booksReadProvider
                            .BooksRead
                            .Where(x => x.User == foundUser.Name)
                            .OrderBy(x => x.Date)
                            .ToList();

                    // Get the file export string 
                    string fileName = 
                        GetExportFileName(ExportDirectory, "Books", ".pdf");
                    WriteBooksToPdfFile(fileName, books);

                    // Return the formatted text
                    if (File.Exists(fileName))
                    {
                        return new FileStream(fileName, FileMode.Open);
                    }
                }
            }

            return null;
        }

        public EditorDetails GetEditorDetails()
        {
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            EditorDetails editorDetails = new EditorDetails();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                editorDetails.AuthorNames =
                    booksReadProvider.AuthorsRead.Select(x => x.Author).OrderBy(x => x).ToArray();

                editorDetails.CountryNames =
                    geographyProvider.Nations.Select(x => x.Name).OrderBy(x => x).ToArray();

                editorDetails.Languages =
                    booksReadProvider.AuthorLanguages.Select(x => x.Language).OrderBy(x => x).ToArray();

                editorDetails.Tags =
                    booksReadProvider.BookTags.Select(x => x.Tag).OrderBy(x => x).ToArray();
            }

            return editorDetails;
        }

        public IEnumerable<Book> GetAsDefaultUser(string userId)
        {
            var blankBooks = new List<Book>();

            User foundUser = _userDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == userId);
            if (foundUser == null)
            {
                return blankBooks;
            }

            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            List<BookRead> blankUserBooks = new List<BookRead>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                foreach (var bookRead in booksReadProvider.BooksRead)
                {
                    if (bookRead != null && string.IsNullOrEmpty(bookRead.User))
                    {
                        blankUserBooks.Add(bookRead);
                    }
                }
            }


            if (blankUserBooks.Any())
            {
                foreach (var itemToUpdate in blankUserBooks)
                {
                    itemToUpdate.User = foundUser.Name;
                    _booksReadDatabase.UpdateDatabaseItem(itemToUpdate);
                    blankBooks.Add(new Book(itemToUpdate));
                }
            }

            return blankBooks;
        }

        /// <summary>
        /// Adds a new user book read.
        /// </summary>
        /// <param name="bookReadAddRequest">The new book read to try to add.</param>
        /// <returns>The action result.</returns>
        public BookReadAddResponse AddNewBookRead(BookReadAddRequest bookReadAddRequest)
        {
            BookReadAddResponse response =
                new BookReadAddResponse
                {
                    ErrorCode = (int)BookReadAddResponseCode.Success,
                    FailReason = "",
                    UserId = bookReadAddRequest.UserId
                };

            // First check that the user exists
            User userLogin =
                _userDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == bookReadAddRequest.UserId);

            if (userLogin == null)
            {
                response.ErrorCode = (int)BookReadAddResponseCode.UnknownUser;
                response.FailReason = "Could not find this user.";
                return response;
            }

            // Check the book data is valid
            BookRead newBook;
            if (!GetBookRead(bookReadAddRequest, out newBook))
            {
                response.ErrorCode = (int)BookReadAddResponseCode.InvalidItem;
                response.FailReason = "Invalid book data please try again.";
                return response;
            }

            // Check if this is duplicate
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                BookRead match = booksReadProvider.BooksRead.FirstOrDefault(
                    x => x.Author == newBook.Author &&
                         x.Title == newBook.Title &&
                         x.DateString == newBook.DateString);

                if (match != null)
                {
                    response.ErrorCode = (int)BookReadAddResponseCode.Duplicate;
                    response.FailReason = "This book has already been added.";
                    return response;
                }
            }

            newBook.User = userLogin.Name;
            _booksReadDatabase.AddNewItemToDatabase(newBook);
            response.NewItem = new Book(newBook);

            return response;
        }

        public string AddBooksFromImport(string userName, List<BookRead> booksToImport)
        {
            string statuses = string.Empty;

            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();
           
            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                int count = 0;
                foreach (BookRead newBook in booksToImport)
                {
                    count++;

                    // Check the date
                    if (DateTime.Now < newBook.Date || newBook.Date < EarliestDate)
                    {
                        statuses += $"Invalid book date {newBook.Date} please try again.\n";
                        continue;
                    }

                    // Check if this is duplicate
                    BookRead match = booksReadProvider.BooksRead.FirstOrDefault(
                        x => x.Author == newBook.Author &&
                             x.Title == newBook.Title &&
                             x.DateString == newBook.DateString);

                    if (match != null)
                    {
                        statuses += $"This book {newBook.Author} - {newBook.Title} has already been added.";
                        continue;
                    }

                    newBook.User = userName;
                    _booksReadDatabase.AddNewItemToDatabase(newBook);

                    statuses += $"New book {newBook.Author} - {newBook.Title} added.";
                }

            }
            return statuses;
        }

        public BookReadAddResponse UpdateExistingBook(Book existingBook)
        {
            // set up the successful response
            BookReadAddResponse response = new BookReadAddResponse
            {
                ErrorCode = (int)UserResponseCode.Success,
                NewItem = new Book(existingBook),
                FailReason = "",
                UserId = ""
            };

            // First check that the user exists
            User userLogin =
                _userDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == existingBook.User);

            if (userLogin == null)
            {
                response.ErrorCode = (int)BookReadAddResponseCode.UnknownUser;
                response.FailReason = "Could not find this user.";
                return response;
            }

            // Check the book data is valid
            BookRead newBook;
            if (!GetBookRead(existingBook, out newBook))
            {
                response.ErrorCode = (int)BookReadAddResponseCode.InvalidItem;
                response.FailReason = "Invalid book data please try again.";
                return response;
            }

            // Find the item
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                BookRead itemToUpdate =
                    _booksReadDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == existingBook.Id);

                if (itemToUpdate == null)
                {
                    response.ErrorCode = (int)BookReadAddResponseCode.UnknownItem;
                    response.FailReason = "Could not find item";
                }
                else
                {
                    itemToUpdate.DateString = newBook.DateString;
                    itemToUpdate.Date = newBook.Date;
                    itemToUpdate.Author = newBook.Author;
                    itemToUpdate.Title = newBook.Title;
                    itemToUpdate.Pages = newBook.Pages;
                    itemToUpdate.Note = newBook.Note;
                    itemToUpdate.Nationality = newBook.Nationality;
                    itemToUpdate.OriginalLanguage = newBook.OriginalLanguage;
                    itemToUpdate.ImageUrl = newBook.ImageUrl;
                    itemToUpdate.Tags = newBook.Tags.ToList();
                    itemToUpdate.Format = newBook.Format;

                    _booksReadDatabase.UpdateDatabaseItem(itemToUpdate);
                }
            }

            return response;
        }

        public BookReadAddResponse DeleteBook(string id)
        {
            // set up the successful response
            BookReadAddResponse response = new BookReadAddResponse
            {
                ErrorCode = (int)BookReadAddResponseCode.Success,
                NewItem = new Book(),
                FailReason = "",
                UserId = ""
            };

            // Find the item
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                var itemToDelete =
                    _booksReadDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == id);

                if (itemToDelete == null)
                {
                    response.ErrorCode = (int)BookReadAddResponseCode.UnknownItem;
                    response.FailReason = "Could not find item";
                }
                else
                {
                    _booksReadDatabase.RemoveItemFromDatabase(itemToDelete);
                }
            }

            return response;
        }

        public ExportDataToEmailResponse SendExportEmail(
            ExportDataToEmailRequest exportRequest)
        {
            ExportDataToEmailResponse response = new ExportDataToEmailResponse()
            {
                DestinationEmail = exportRequest.DestinationEmail,
                SentSuccessfully = true,
                Error = string.Empty
            };

            try
            {
                SendExportEmailToUser(exportRequest);
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
                response.SentSuccessfully = false;
            }

            return response;
        }

        public NationDetailUpdateResponse UpdateNationDetail(NationDetail updateNation)
        {
            // set up the successful response
            NationDetailUpdateResponse response = new NationDetailUpdateResponse
            {
                ErrorCode = (int)UserResponseCode.Success,
                UpdateItem = new NationDetail(updateNation),
                FailReason = "",
                UserId = ""
            };

            // Find the item
            GeographyProvider geographyProvider;
            BooksReadProvider booksReadProvider;
            _books = new ObservableCollection<Book>();

            if (GetProviders(out geographyProvider, out booksReadProvider))
            {
                Nation itemToUpdate =
                    _nationsReadDatabase.LoadedItems.FirstOrDefault(x => x.Id.ToString() == updateNation.Id);

                if (itemToUpdate == null)
                {
                    response.ErrorCode = (int)BookReadAddResponseCode.UnknownItem;
                    response.FailReason = "Could not find item";
                }
                else
                {
                    itemToUpdate.ImageUri = updateNation.ImageUri;

                    _nationsReadDatabase.UpdateDatabaseItem(itemToUpdate);
                }
            }

            return response;
        }

        #endregion

        public BooksDataControllerUtilities(
            MongoDbSettings dbSettings, 
            SmtpConfig mailConfig) : base(dbSettings)
        {
            _smtpConfig = mailConfig;
            DatabaseConnectionString = dbSettings.DatabaseConnectionString;
            ExportDirectory = dbSettings.ExportDirectory;

            _books = new ObservableCollection<Book>();

            _booksReadFromDatabase = new ObservableCollection<BookRead>();
            _nationsReadFromDatabase = new ObservableCollection<Nation>();
            _usersReadFromDatabase = new ObservableCollection<User>();
            _booksReportGenerator = new BooksReportGenerator();

            if (!dbSettings.UseRemoteHost)
            {
                _booksReadDatabase = new BooksReadDatabase(DatabaseConnectionString);
                _nationsReadDatabase = new NationDatabase(DatabaseConnectionString);
                _userDatabase = new UserDatabase(DatabaseConnectionString);
            }
            else
            {
                _booksReadDatabase =
                    new BooksReadDatabase(string.Empty, false)
                    {
                        MongoClientFunc = GetRemoteConnection
                    };
                _booksReadDatabase.ConnectToDatabase();

                _nationsReadDatabase =
                    new NationDatabase(string.Empty, false)
                    {
                        MongoClientFunc = GetRemoteConnection
                    };
                _nationsReadDatabase.ConnectToDatabase();

                _userDatabase =
                    new UserDatabase(string.Empty, false)
                    {
                        MongoClientFunc = GetRemoteConnection
                    };
                _userDatabase.ConnectToDatabase();
            }
        }

    }
}
