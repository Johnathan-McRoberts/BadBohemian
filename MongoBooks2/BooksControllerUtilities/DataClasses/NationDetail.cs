namespace BooksControllerUtilities.DataClasses
{
    using BooksCore.Geography;

    public class NationDetail
    {
        /// <summary>
        /// Gets or sets the unique entity identifier string.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the country name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the capital.
        /// </summary>
        public string Capital { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the Uri for the flag image.
        /// </summary>
        public string ImageUri { get; set; }

        public NationDetail()
        {

        }

        public NationDetail(NationDetail book)
        {
            Id = book.Id;
            Name = book.Name;
            Capital = book.Capital;
            Latitude = book.Latitude;
            Longitude = book.Longitude;
            ImageUri = book.ImageUri;
        }

        public NationDetail(NationGeography book)
        {
            Id = book.Id;
            Name = book.Name;
            Capital = book.Capital;
            Latitude = book.Latitude;
            Longitude = book.Longitude;
            ImageUri = book.ImageUri;
        }

        public NationDetail(Nation nation)
        {
            Id = nation.Id.ToString();
            Name = nation.Name;
            Capital = nation.Capital;
            Latitude = nation.Latitude;
            Longitude = nation.Longitude;
            ImageUri = nation.ImageUri;
        }
    }
}
