﻿
namespace BooksCore.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections.ObjectModel;
    using BooksCore.Books;
    using BooksCore.Geography;
    using BooksCore.Interfaces;

    public class BooksReadProvider : IBooksReadProvider
    {
        private Dictionary<string, WorldCountry> _worldCountryLookup;

        private IGeographyProvider _geographyProvider;

        /// <summary>
        /// Gets the books.
        /// </summary>
        public ObservableCollection<BookRead> BooksRead { get; private set; }

        /// <summary>
        /// Gets the changes between books.
        /// </summary>
        public ObservableCollection<BooksDelta> BookDeltas { get; }

        /// <summary>
        /// Gets the changes per year between books.
        /// </summary>
        public ObservableCollection<BooksDelta> BookPerYearDeltas { get; }

        /// <summary>
        /// Gets the authors.
        /// </summary>
        public ObservableCollection<BookAuthor> AuthorsRead { get; }

        /// <summary>
        /// Gets the book location deltas.
        /// </summary>
        public ObservableCollection<BookLocationDelta> BookLocationDeltas { get; private set; }

        /// <summary>
        /// Gets the author countries.
        /// </summary>
        public ObservableCollection<AuthorCountry> AuthorCountries { get; }
        
        public ObservableCollection<TalliedMonth> TalliedMonths { get; set; }

        public TalliedMonth SelectedMonthTally { get; set; }

        private void UpdateCountries(out int booksReadWorldwide, out uint pagesReadWorldwide)
        {
            // clear the list & counts
            AuthorCountries.Clear();
            booksReadWorldwide = 0;
            pagesReadWorldwide = 0;

            // get the uniquely named countries + the counts
            Dictionary<string, AuthorCountry> countrySet = new Dictionary<string, AuthorCountry>();
            foreach (BookAuthor author in AuthorsRead)
            {
                booksReadWorldwide += author.TotalBooksReadBy;
                pagesReadWorldwide += author.TotalPages;

                if (countrySet.ContainsKey(author.Nationality))
                    countrySet[author.Nationality].AuthorsFromCountry.Add(author);
                else
                {
                    AuthorCountry country = new AuthorCountry(_geographyProvider) { Country = author.Nationality };
                    country.AuthorsFromCountry.Add(author);
                    countrySet.Add(country.Country, country);
                }
            }

            // Update the country totals + add to the list
            foreach (var country in countrySet.Values.ToList())
            {
                country.TotalBooksWorldWide = booksReadWorldwide;
                country.TotalPagesWorldWide = pagesReadWorldwide;
                AuthorCountries.Add(country);
            }
        }

        private void UpdateBookDeltas()
        {
            // clear the list and the counts
            BookDeltas.Clear();
            if (BooksRead.Count < 1) return;
            DateTime startDate = BooksRead[0].Date;

            // get all the dates a book has been read (after the first quarter)
            Dictionary<DateTime, DateTime> bookReadDates = GetBookReadDates(startDate);

            // then add the delta made up of the books up to that date
            foreach (var date in bookReadDates.Keys.ToList())
            {
                BooksDelta delta = new BooksDelta(date, startDate);
                foreach (var book in BooksRead)
                {
                    if (book.Date <= date)
                        delta.BooksReadToDate.Add(book);
                    else
                        break;
                }
                delta.UpdateTallies();
                BookDeltas.Add(delta);
            }
        }

        private void UpdateBookLocationDeltas()
        {
            // clear the list and the counts
            BookLocationDeltas.Clear();
            if (BooksRead.Count < 1) return;
            DateTime startDate = BooksRead[0].Date;

            // get all the dates a book has been read (after the first quarter)
            Dictionary<DateTime, DateTime> bookReadDates = GetBookReadDates(startDate);

            // then add the delta made up of the books up to that date
            foreach (DateTime date in bookReadDates.Keys.ToList())
            {
                BookLocationDelta delta = new BookLocationDelta(date, startDate);
                foreach (var book in BooksRead)
                {
                    if (book.Date <= date)
                    {
                        WorldCountry country = GetCountryForBook(book);
                        if (country != null)
                        {
                            BookLocation location =
                                new BookLocation() { Book = book, Latitude = country.Latitude, Longitude = country.Longitude };

                            delta.BooksLocationsToDate.Add(location);
                        }
                    }
                    else
                        break;
                }
                //delta.UpdateTallies();
                BookLocationDeltas.Add(delta);
            }
        }

        private WorldCountry GetCountryForBook(BookRead book)
        {
            if (_worldCountryLookup == null || _worldCountryLookup.Count == 0 ||
                !_worldCountryLookup.ContainsKey(book.Nationality)) return null;
            return _worldCountryLookup[book.Nationality];
        }

        private Dictionary<DateTime, DateTime> GetBookReadDates(DateTime startDate)
        {
            Dictionary<DateTime, DateTime> bookReadDates = new Dictionary<DateTime, DateTime>();
            foreach (var book in BooksRead)
            {
                if (!bookReadDates.ContainsKey(book.Date))
                {
                    TimeSpan ts = book.Date - startDate;
                    if (ts.Days >= 90)
                        bookReadDates.Add(book.Date, book.Date);
                }
            }
            return bookReadDates;
        }

        private void UpdateAuthors()
        {
            AuthorsRead.Clear();

            Dictionary<string, BookAuthor> authorsSet = new Dictionary<string, BookAuthor>();
            foreach (BookRead book in BooksRead)
            {
                if (authorsSet.ContainsKey(book.Author))
                    authorsSet[book.Author].BooksReadBy.Add(book);
                else
                {
                    BookAuthor author =
                        new BookAuthor() { Author = book.Author, Language = book.OriginalLanguage, Nationality = book.Nationality };
                    author.BooksReadBy.Add(book);
                    authorsSet.Add(book.Author, author);
                }
            }

            foreach (var author in authorsSet.Values.ToList())
                AuthorsRead.Add(author);
        }

        private void UpdateWorldCountryLookup()
        {
            _worldCountryLookup = new Dictionary<string, WorldCountry>();
            foreach (var country in _geographyProvider.WorldCountries)
                if (!_worldCountryLookup.ContainsKey(country.Country))
                    _worldCountryLookup.Add(country.Country, country);
        }

        private void UpdateBookPerYearDeltas()
        {
            // clear the list and the counts
            BookPerYearDeltas.Clear();
            if (BooksRead.Count < 1) return;
            DateTime startDate = BooksRead[0].Date;
            startDate = startDate.AddYears(1);

            // get all the dates a book has been read that are at least a year since the start
            Dictionary<DateTime, DateTime> bookReadDates = new Dictionary<DateTime, DateTime>();
            foreach (var book in BooksRead)
            {
                if (startDate > book.Date) continue;
                if (!bookReadDates.ContainsKey(book.Date))
                {
                    bookReadDates.Add(book.Date, book.Date);
                }
            }

            // then add the delta made up of the books up to that date
            foreach (var date in bookReadDates.Keys.ToList())
            {
                DateTime startYearDate = date;
                startYearDate = startYearDate.AddYears(-1);
                BooksDelta delta = new BooksDelta(date, startYearDate);
                foreach (var book in BooksRead)
                {
                    if (book.Date < startYearDate)
                        continue;

                    if (book.Date <= date)
                        delta.BooksReadToDate.Add(book);
                    else
                        break;
                }
                delta.UpdateTallies();
                BookPerYearDeltas.Add(delta);
            }
        }


        private void UpdateBooksPerMonth()
        {
            // clear the list and the counts
            Dictionary<DateTime, List<BookRead>> bookMonths = new Dictionary<DateTime, List<BookRead>>();
            if (BooksRead.Count < 1) return;
            DateTime startDate = BooksRead[0].Date;
            DateTime endDate = BooksRead.Last().Date;
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

            DateTime monthStart = new DateTime(startDate.Year, startDate.Month, 1);
            DateTime monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

            // get all the months a book has been read
            while (monthStart <= endDate)
            {
                List<BookRead> monthList = new List<BookRead>();

                foreach (BookRead book in BooksRead)
                {
                    if (book.Date >= monthStart && book.Date <= monthEnd)
                    {
                        monthList.Add(book);
                    }
                }

                if (monthList.Count > 0)
                {
                    bookMonths.Add(monthStart, monthList);
                }

                monthStart = monthStart.AddMonths(1);
                monthEnd = monthStart.AddMonths(1).AddSeconds(-1);
            }

            TalliedMonths.Clear();
            foreach (DateTime date in bookMonths.Keys.OrderBy(x => x))
            {
                TalliedMonths.Add(new TalliedMonth(date, bookMonths[date]));
            }
        }

        public void Setup(IList<BookRead> books, IGeographyProvider geographyProvider)
        {
            _geographyProvider = geographyProvider;

            BooksRead.Clear();
            foreach (BookRead book in books.OrderBy(x => x.Date))
                BooksRead.Add(book);

            UpdateBookDeltas();
            UpdateBookPerYearDeltas();
            UpdateAuthors();
            UpdateWorldCountryLookup();
            int booksReadWorldwide;
            uint pagesReadWorldwide;
            UpdateCountries(out booksReadWorldwide, out pagesReadWorldwide);
            BookLocationDeltas = new ObservableCollection<BookLocationDelta>();
            UpdateBookLocationDeltas();
            UpdateBooksPerMonth();
            SelectedMonthTally = TalliedMonths.FirstOrDefault();
        }

        public BooksReadProvider()
        {
            BooksRead = new ObservableCollection<BookRead>();
            BookDeltas = new ObservableCollection<BooksDelta>();
            BookPerYearDeltas = new ObservableCollection<BooksDelta>();
            AuthorsRead = new ObservableCollection<BookAuthor>();
            BookLocationDeltas = new ObservableCollection<BookLocationDelta>();
            AuthorCountries = new ObservableCollection<AuthorCountry>();
            TalliedMonths = new ObservableCollection<TalliedMonth>();
        }
    }
}