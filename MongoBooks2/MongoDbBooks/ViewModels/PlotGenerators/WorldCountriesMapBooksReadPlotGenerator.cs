﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

using MongoDbBooks.Models;
using MongoDbBooks.ViewModels.Utilities;

namespace MongoDbBooks.ViewModels.PlotGenerators
{
    public class WorldCountriesMapBooksReadPlotGenerator : IPlotGenerator
    {
        public OxyPlot.PlotModel SetupPlot(Models.MainBooksModel mainModel)
        {
            _mainModel = mainModel;
            return SetupWorldCountriesMapPlot();
        }
        private Models.MainBooksModel _mainModel;

        private PlotModel SetupWorldCountriesMapPlot()
        {
            // Create the plot model
            var newPlot = new PlotModel { Title = "Countries of the World" };
            //OxyPlotUtilities.SetupPlotLegend(newPlot, "Total Pages Read by Language With Time Plot");
            SetupLatitudeAndLongitudeAxes(newPlot);

            // make up a lit of the countries with books read
            int maxBooksRead = -1;
            Dictionary<string, int> countryToReadLookUp = new Dictionary<string, int>();
            foreach(var authorCountry in _mainModel.AuthorCountries)
            {
                maxBooksRead = Math.Max(authorCountry.TotalBooksReadFromCountry, maxBooksRead);
                countryToReadLookUp.Add(authorCountry.Country, authorCountry.TotalBooksReadFromCountry);
            }

            maxBooksRead *= 12;
            maxBooksRead /= 10;

            List<OxyColor> colors = new List<OxyColor>();
            foreach (var color in OxyPalettes.Jet(maxBooksRead).Colors)
            {
                var faintColor = OxyColor.FromArgb(128, color.R, color.G, color.B);
                colors.Add(faintColor);
            }

            OxyPalette faintPalette = new OxyPalette(colors);

            foreach (var country in _mainModel.CountryGeographies)
            {
                OxyColor color = OxyColors.LightGray;
                string tagString = "";

                if (countryToReadLookUp.ContainsKey(country.Name))
                {
                    color = colors[countryToReadLookUp[country.Name]];
                    tagString = "\nBooks Read = " + countryToReadLookUp[country.Name].ToString();
                }

                int i = 0;
                // just do the 5 biggest bits per country (looks enough)
                var landBlocks = country.LandBlocks.OrderByDescending(b => b.TotalArea);

                foreach (var boundary in landBlocks)
                {
                    var areaSeries = new AreaSeries
                    {
                        Color = color,
                        Title = country.Name,
                        RenderInLegend = false,
                        Tag = tagString
                    };
                    foreach (var point in boundary.Points)
                    {

                        double ptX = 0;
                        double ptY = 0;
                        point.GetCoordinates(out ptX, out ptY);
                        DataPoint dataPoint = new DataPoint(ptX, ptY);

                        areaSeries.Points.Add(dataPoint);

                    }

                    areaSeries.TrackerFormatString = "{0}\nLat/Long ( {4:0.###} ,{2:0.###} )"+ tagString;
                   

                    newPlot.Series.Add(areaSeries);

                    i++;
                    if (i > 10)
                        break;
                }
            }


            newPlot.Axes.Add(new LinearColorAxis
            { Position = AxisPosition.Right, Palette = faintPalette, Title = "Books Read" });

            // finally update the model with the new plot
            return newPlot;
        }
        private void SetupLatitudeAndLongitudeAxes(PlotModel newPlot)
        {
            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Longitude",
                Key = ChartAxisKeys.LongitudeKey,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                Maximum = 200,
                Minimum = -200
            };
            newPlot.Axes.Add(xAxis);

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Latitude",
                Key = ChartAxisKeys.LatitudeKey,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                Maximum = 100,
                Minimum = -100
            };
            newPlot.Axes.Add(yAxis);
        }

    }
}