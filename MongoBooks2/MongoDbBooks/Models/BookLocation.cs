﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbBooks.Models
{
    public class BookLocation
    {
        public BookRead Book { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
