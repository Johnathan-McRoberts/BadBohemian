import { Component, OnInit, AfterViewInit } from '@angular/core';
import { BooksDataService } from './../../../Services/books-data.service';

import { SeriesColors } from './../../../Models/SeriesColors';
import { ChartUtilities } from './../../../Models/ChartUtilities';

import { DeltaBooks, ICategoryTotal } from './../../../Models/DeltaBooks';
import { LanguageAuthors } from './../../../Models/LanguageAuthors';

@Component({
    selector: 'app-by-language-charts',
    templateUrl: './by-language-charts.component.html',
    styleUrls: ['./by-language-charts.component.scss']
})
/** ByLanguageCharts component*/
export class ByLanguageChartsComponent
    implements OnInit, AfterViewInit
{
    /** ByLanguageCharts ctor */
    constructor(booksDataService: BooksDataService)
    {
      this.componentTitle = "Loading books charts from database...";
      this.booksDataService = booksDataService;
    }

    private booksDataService: BooksDataService;
    public componentTitle: string;
    public languageAuthors: LanguageAuthors[];
    public deltaBooks: DeltaBooks[];

    //#region Component Implementation

    ngOnInit()
    {
        this.booksDataService.fetchAllLanguageAuthorsData().then(() =>
        {
            this.languageAuthors = new Array<LanguageAuthors>();

            for (let item of this.booksDataService.languageAuthors)
            {
                var languageAuthor: LanguageAuthors = item;
                this.languageAuthors.push(languageAuthor);
            }

            this.setupAllCharts();
        });

        this.booksDataService.fetchAllDeltaBooksData().then(() =>
        {
            this.deltaBooks = new Array<DeltaBooks>();

            for (let item of this.booksDataService.deltaBooks)
            {
                let deltaBook: DeltaBooks = item;
                this.deltaBooks.push(deltaBook);
            }

            this.setupAllCharts();
        });
    }

    ngAfterViewInit()
    {
        this.setupAllCharts();
    }

    //#endregion

    //#region General Chart Data

    public plotlyConfig =
    {
        "displaylogo": false,
    }

    public setupAllCharts(): void
    {
        if (this.languageAuthors != null && this.languageAuthors.length > 0)
            this.setupBooksAndPagesReadByLanguageCharts();

        if (this.deltaBooks != null && this.deltaBooks.length > 0)
            this.setupAllLanguageByTallyCharts();
    }

    public setupAllLanguageByTallyCharts(): void
    {
        this.setupPercentageOfBooksReadByLanguageCharts();
        this.setupTotalBooksReadByLanguageCharts();
        this.setupPercentageOfPagesReadByLanguageCharts();
        this.setupTotalPagesReadByLanguageCharts();
    }

    //#endregion

    //#region Percentage of Books Read by Language

    public percentageOfBooksReadByLanguageLayout: any;

    public percentageOfBooksReadByLanguageData = null;

    public setupPercentageOfBooksReadByLanguageLayout(): void
    {
        this.percentageOfBooksReadByLanguageLayout =
        {
            xaxis:
            {
                autorange: true,
                title: "Date"
            },
            yaxis:
            {
                autorange: true,
                title: "% Books Read",
                titlefont: { color: SeriesColors.liveChartsColors[0] },
                tickfont: { color: SeriesColors.liveChartsColors[0] }
            },
            hovermode: 'closest',

            width: ChartUtilities.chartWidth,
            height: ChartUtilities.chartHeight,
            showlegend: true,
            legend:
            {
                "orientation": "h",
                x: 0.1,
                y: 1
            },
            margin:
            {
                l: 55,
                r: 55,
                b: 55,
                t: 45,
                pad: 4
            },
        };
    }

    public setupPercentageOfBooksReadByLanguageCharts(): void
    {
        this.setupPercentageOfBooksReadByLanguageLayout();

        // Get the list of languages for the final language tally
        let numberLanguageTallies = this.deltaBooks.length;
        let finalLanguageTotals = this.deltaBooks[numberLanguageTallies - 1].languageTotals;

        const sortedByBooks: ICategoryTotal[] = finalLanguageTotals.sort((t1, t2) =>
        {
            const ttl1 = t1.percentageBooks;
            const ttl2 = t2.percentageBooks;

            if (ttl1 > ttl2) { return -1; }
            if (ttl1 < ttl2) { return 1; }
            return 0;
        });

        const maxLanguages: number = SeriesColors.liveChartsColors.length;
        const includeOtherLanguage: boolean = (sortedByBooks.length > maxLanguages);

        let otherLabel: string = "Other";
        var displayedLanguagePercentagesByTime: Map<string, number[]> = new Map<string, number[]>();
        let displayedLanguages: string[] = new Array<string>();

        for (let i = 0; i < sortedByBooks.length; i++)
        {
            let categoryTotal: ICategoryTotal = sortedByBooks[i];

            if (i < maxLanguages - 1)
            {
                displayedLanguagePercentagesByTime.set(categoryTotal.name, new Array<number>());
                displayedLanguages.push(categoryTotal.name);
            }
            else
            {
                displayedLanguagePercentagesByTime.set(otherLabel, new Array<number>());
            }
        }

        // Go through the deltas adding the percentages to the appropriate languages for the dates
        let deltaDates: Date[] = new Array<Date>();
        for (let i = 0; i < numberLanguageTallies; i++)
        {
            let deltaDate = new Date(this.deltaBooks[i].date);
            deltaDates.push(deltaDate);

            let languageTotals = this.deltaBooks[i].languageTotals;

            // if there are other languages set the default value for this delta to zero
            let otherTotal: number = 0;
            let languagesAdded: string[] = new Array<string>();

            for (let j = 0; j < languageTotals.length; j++)
            {
                let languageTotal = languageTotals[j];

                if (displayedLanguagePercentagesByTime.has(languageTotal.name))
                {
                    displayedLanguagePercentagesByTime.get(languageTotal.name).push(languageTotal.percentageBooks);
                    languagesAdded.push(languageTotal.name);
                }
                else
                {
                   otherTotal += languageTotal.percentageBooks;
                }
            }

            for (let j = 0; j < displayedLanguages.length; j++)
            {
                let displayedLanguage = displayedLanguages[j];
                if (languagesAdded.indexOf(displayedLanguage) === -1)
                {
                    displayedLanguagePercentagesByTime.get(displayedLanguage).push(0);
                }
            }

            if (includeOtherLanguage)
                displayedLanguagePercentagesByTime.get(otherLabel).push(otherTotal);
        }

        // Create a series per language & display the series on the plot
        this.percentageOfBooksReadByLanguageData =
          ChartUtilities.getLineSeriesForCategories(displayedLanguagePercentagesByTime, deltaDates);
    }

    //#endregion

    //#region Total Books Read by Language

    public totalBooksReadByLanguageLayout: any;

    public totalBooksReadByLanguageData = null;

    public setupTotalBooksReadByLanguageLayout(): void
    {
        this.totalBooksReadByLanguageLayout =
            {
                xaxis:
                {
                    autorange: true,
                    title: "Date"
                },
                yaxis:
                {
                    autorange: true,
                    title: "Total Books Read",
                },
                hovermode: 'closest',

                width: ChartUtilities.chartWidth,
                height: ChartUtilities.chartHeight,
                showlegend: true,
                legend:
                {
                    "orientation": "h",
                    x: 0.1,
                    y: 1
                },
                margin:
                {
                    l: 55,
                    r: 55,
                    b: 55,
                    t: 45,
                    pad: 4
                },
            };
    }

    public setupTotalBooksReadByLanguageCharts(): void
    {
        this.setupTotalBooksReadByLanguageLayout();

        // Get the list of languages for the final language tally
        let numberLanguageTallies = this.deltaBooks.length;
        let finalLanguageTotals = this.deltaBooks[numberLanguageTallies - 1].languageTotals;

        const sortedByBooks: ICategoryTotal[] = finalLanguageTotals.sort((t1, t2) =>
        {
            const ttl1 = t1.totalBooks;
            const ttl2 = t2.totalBooks;

            if (ttl1 > ttl2) { return -1; }
            if (ttl1 < ttl2) { return 1; }
            return 0;
        });

        const maxLanguages: number = SeriesColors.liveChartsColors.length;
        const includeOtherLanguage: boolean = (sortedByBooks.length > maxLanguages);

        let otherLabel: string = "Other";
        var displayedLanguageTotalsByTime: Map<string, number[]> = new Map<string, number[]>();
        let displayedLanguages: string[] = new Array<string>();

        for (let i = 0; i < sortedByBooks.length; i++)
        {
            let categoryTotal: ICategoryTotal = sortedByBooks[i];

            if (i < maxLanguages - 1)
            {
                displayedLanguageTotalsByTime.set(categoryTotal.name, new Array<number>());
                displayedLanguages.push(categoryTotal.name);
            }
            else
            {
                displayedLanguageTotalsByTime.set(otherLabel, new Array<number>());
            }
        }

        // Go through the deltas adding the percentages to the appropriate languages for the dates
        let deltaDates: Date[] = new Array<Date>();
        for (let i = 0; i < numberLanguageTallies; i++)
        {
            let deltaDate = new Date(this.deltaBooks[i].date);
            deltaDates.push(deltaDate);

            let languageTotals = this.deltaBooks[i].languageTotals;

            // if there are other languages set the default value for this delta to zero
            let otherTotal: number = 0;
            let languagesAdded: string[] = new Array<string>();

            for (let j = 0; j < languageTotals.length; j++)
            {
                let languageTotal = languageTotals[j];

                if (displayedLanguageTotalsByTime.has(languageTotal.name))
                {
                    displayedLanguageTotalsByTime.get(languageTotal.name).push(languageTotal.totalBooks);
                    languagesAdded.push(languageTotal.name);
                }
                else
                {
                    otherTotal += languageTotal.totalBooks;
                }
            }

            for (let j = 0; j < displayedLanguages.length; j++)
            {
                let displayedLanguage = displayedLanguages[j];
                if (languagesAdded.indexOf(displayedLanguage) === -1)
                {
                    displayedLanguageTotalsByTime.get(displayedLanguage).push(0);
                }
            }

            if (includeOtherLanguage)
                displayedLanguageTotalsByTime.get(otherLabel).push(otherTotal);
        }

        // Create a series per language & display the series on the plot
        this.totalBooksReadByLanguageData =
            ChartUtilities.getStackedAreaSeriesForCategories(displayedLanguageTotalsByTime, deltaDates);
    }

    //#endregion

    //#region Percentage of Books Read by Language

    public percentageOfPagesReadByLanguageLayout: any;

    public percentageOfPagesReadByLanguageData = null;

    public setupPercentageOfPagesReadByLanguageLayout(): void
    {
        this.percentageOfPagesReadByLanguageLayout =
            {
                xaxis:
                {
                    autorange: true,
                    title: "Date"
                },
                yaxis:
                {
                    autorange: true,
                    title: "% Pages Read",
                    titlefont: { color: SeriesColors.liveChartsColors[0] },
                    tickfont: { color: SeriesColors.liveChartsColors[0] }
                },
                hovermode: 'closest',

                width: ChartUtilities.chartWidth,
                height: ChartUtilities.chartHeight,
                showlegend: true,
                legend:
                {
                    "orientation": "h",
                    x: 0.1,
                    y: 1
                },
                margin:
                {
                    l: 55,
                    r: 55,
                    b: 55,
                    t: 45,
                    pad: 4
                },
            };
    }

    public setupPercentageOfPagesReadByLanguageCharts(): void
    {
        this.setupPercentageOfPagesReadByLanguageLayout();

        // Get the list of languages for the final language tally
        let numberLanguageTallies = this.deltaBooks.length;
        let finalLanguageTotals = this.deltaBooks[numberLanguageTallies - 1].languageTotals;

        const sortedByPages: ICategoryTotal[] = finalLanguageTotals.sort((t1, t2) =>
        {
            const ttl1 = t1.percentagePages;
            const ttl2 = t2.percentagePages;

            if (ttl1 > ttl2) { return -1; }
            if (ttl1 < ttl2) { return 1; }
            return 0;
        });

        const maxLanguages: number = SeriesColors.liveChartsColors.length;
        const includeOtherLanguage: boolean = (sortedByPages.length > maxLanguages);

        let otherLabel: string = "Other";
        var displayedLanguagePercentagesByTime: Map<string, number[]> = new Map<string, number[]>();

        for (let i = 0; i < sortedByPages.length; i++)
        {
            let categoryTotal: ICategoryTotal = sortedByPages[i];

            if (i < maxLanguages - 1)
            {
                displayedLanguagePercentagesByTime.set(categoryTotal.name, new Array<number>());
            }
            else
            {
                displayedLanguagePercentagesByTime.set(otherLabel, new Array<number>());
            }
        }

        // Go through the deltas adding the percentages to the appropriate languages for the dates
        let deltaDates: Date[] = new Array<Date>();
        for (let i = 0; i < numberLanguageTallies; i++)
        {
            let deltaDate = new Date(this.deltaBooks[i].date);
            deltaDates.push(deltaDate);

            let languageTotals = this.deltaBooks[i].languageTotals;

            // if there are other languages set the default value for this delta to zero
            let otherTotal: number = 0;

            for (let j = 0; j < languageTotals.length; j++)
            {
                let languageTotal = languageTotals[j];

                if (displayedLanguagePercentagesByTime.has(languageTotal.name))
                {
                    displayedLanguagePercentagesByTime.get(languageTotal.name).push(languageTotal.percentagePages);
                }
                else
                {
                    otherTotal += languageTotal.percentagePages;
                }
            }

            if (includeOtherLanguage)
                displayedLanguagePercentagesByTime.get(otherLabel).push(otherTotal);
        }

        // Create a series per language & display the series on the plot
        this.percentageOfPagesReadByLanguageData =
            ChartUtilities.getLineSeriesForCategories(displayedLanguagePercentagesByTime, deltaDates);
    }

    //#endregion

    //#region Total Pages Read by Language

    public totalPagesReadByLanguageLayout: any;

    public totalPagesReadByLanguageData = null;

    public setupTotalPagesReadByLanguageLayout(): void
    {
        this.totalPagesReadByLanguageLayout =
            {
                xaxis:
                {
                    autorange: true,
                    title: "Date"
                },
                yaxis:
                {
                    autorange: true,
                    title: "Total Pages Read",
                },
                hovermode: 'closest',

                width: ChartUtilities.chartWidth,
                height: ChartUtilities.chartHeight,
                showlegend: true,
                legend:
                {
                    "orientation": "h",
                    x: 0.1,
                    y: 1
                },
                margin:
                {
                    l: 55,
                    r: 55,
                    b: 55,
                    t: 45,
                    pad: 4
                },
            };
    }

    public setupTotalPagesReadByLanguageCharts(): void
    {
        this.setupTotalPagesReadByLanguageLayout();

        // Get the list of languages for the final language tally
        let numberLanguageTallies = this.deltaBooks.length;
        let finalLanguageTotals = this.deltaBooks[numberLanguageTallies - 1].languageTotals;

        const sortedByPages: ICategoryTotal[] = finalLanguageTotals.sort((t1, t2) =>
        {
            const ttl1 = t1.totalPages;
            const ttl2 = t2.totalPages;

            if (ttl1 > ttl2) { return -1; }
            if (ttl1 < ttl2) { return 1; }
            return 0;
        });

        const maxLanguages: number = SeriesColors.liveChartsColors.length;
        const includeOtherLanguage: boolean = (sortedByPages.length > maxLanguages);

        let otherLabel: string = "Other";
        var displayedLanguageTotalsByTime: Map<string, number[]> = new Map<string, number[]>();
        let displayedLanguages: string[] = new Array<string>();

        for (let i = 0; i < sortedByPages.length; i++)
        {
            let categoryTotal: ICategoryTotal = sortedByPages[i];

            if (i < maxLanguages - 1)
            {
                displayedLanguageTotalsByTime.set(categoryTotal.name, new Array<number>());
                displayedLanguages.push(categoryTotal.name);
            }
            else
            {
                displayedLanguageTotalsByTime.set(otherLabel, new Array<number>());
            }
        }

        // Go through the deltas adding the percentages to the appropriate languages for the dates
        let deltaDates: Date[] = new Array<Date>();
        for (let i = 0; i < numberLanguageTallies; i++)
        {
            let deltaDate = new Date(this.deltaBooks[i].date);
            deltaDates.push(deltaDate);

            let languageTotals = this.deltaBooks[i].languageTotals;

            // if there are other languages set the default value for this delta to zero
            let otherTotal: number = 0;
            let languagesAdded: string[] = new Array<string>();

            for (let j = 0; j < languageTotals.length; j++) {
                let languageTotal = languageTotals[j];

                if (displayedLanguageTotalsByTime.has(languageTotal.name))
                {
                    displayedLanguageTotalsByTime.get(languageTotal.name).push(languageTotal.totalPages);
                    languagesAdded.push(languageTotal.name);
                }
                else
                {
                    otherTotal += languageTotal.totalPages;
                }
            }

            for (let j = 0; j < displayedLanguages.length; j++)
            {
                let displayedLanguage = displayedLanguages[j];
                if (languagesAdded.indexOf(displayedLanguage) === -1)
                {
                    displayedLanguageTotalsByTime.get(displayedLanguage).push(0);
                }
            }

            if (includeOtherLanguage)
                displayedLanguageTotalsByTime.get(otherLabel).push(otherTotal);
        }

        // Create a series per language & display the series on the plot
        this.totalPagesReadByLanguageData =
            ChartUtilities.getStackedAreaSeriesForCategories(displayedLanguageTotalsByTime, deltaDates);
    }

    //#endregion

    //#region Books and Pages Read by Language

    public currentPieChartByLanguageLayout: any;

    public booksReadByLanguageData = null;
    public pagesReadByLanguageData = null;

    public setupBooksAndPagesReadByLanguageLayout(): void
    {
        this.currentPieChartByLanguageLayout =
        {
            width: ChartUtilities.chartWidth/2,
            height: ChartUtilities.chartHeight,
            showlegend: true,
            legend: { "orientation": "h" },
            margin:
            {
                l: 25,
                r: 25,
                b: 25,
                t: 25,
                pad: 4
            },
        };
    }

    public setupBooksAndPagesReadByLanguageCharts(): void
    {
        this.setupBooksAndPagesReadByLanguageLayout();

        const sortedByBooks: LanguageAuthors[] = this.languageAuthors.sort((t1, t2) =>
        {
            const ttl1 = t1.totalBooksReadInLanguage;
            const ttl2 = t2.totalBooksReadInLanguage;

            if (ttl1 > ttl2) { return -1; }
            if (ttl1 < ttl2) { return 1; }
            return 0;
        });

        const sortedByPages: LanguageAuthors[] = this.languageAuthors.sort((t1, t2) =>
        {
            const ttl1 = t1.totalPagesReadInLanguage;
            const ttl2 = t2.totalPagesReadInLanguage;

            if (ttl1 > ttl2) { return -1; }
            if (ttl1 < ttl2) { return 1; }
            return 0;
        });

        const maxCategories: number = SeriesColors.liveChartsColors.length;
        const includeOtherCategory: boolean = (sortedByBooks.length > maxCategories);

        let otherLabel: string = "Other";
        let otherTotal: number = 0;
        let otherColor: string = SeriesColors.liveChartsColors[maxCategories - 1];

        let ttlLabels: string[] = new Array<string>();
        let ttlValues: number[] = new Array<number>();
        let colors: string[] = new Array<string>();
        
        for (let i = 0; i < sortedByBooks.length; i++)
        {
            let sortedItem = sortedByBooks[i];
            if (i >= (maxCategories - 1))
            {
                otherTotal += sortedItem.totalBooksReadInLanguage;
            }
            else
            {
                ttlLabels.push(sortedItem.name);
                ttlValues.push(sortedItem.totalBooksReadInLanguage);
                colors.push(SeriesColors.liveChartsColors[i]);
            }
        }

        if (includeOtherCategory)
        {
            ttlLabels.push(otherLabel);
            ttlValues.push(otherTotal);
            colors.push(otherColor);
        }

        this.booksReadByLanguageData =
          ChartUtilities.getPiePlotData(ttlLabels, ttlValues, colors);

        otherTotal = 0;
        ttlLabels = new Array<string>();
        ttlValues = new Array<number>();
        colors = new Array<string>();

        for (let i = 0; i < sortedByPages.length; i++)
        {
            let sortedItem = sortedByPages[i];
            if (i >= (maxCategories - 1))
            {
                otherTotal += sortedItem.totalPagesReadInLanguage;
            }
            else
            {
                ttlLabels.push(sortedItem.name);
                ttlValues.push(sortedItem.totalPagesReadInLanguage);
                colors.push(SeriesColors.liveChartsColors[i]);
            }
        }

        if (includeOtherCategory)
        {
            ttlLabels.push(otherLabel);
            ttlValues.push(otherTotal);
            colors.push(otherColor);
        }

        this.pagesReadByLanguageData =
            ChartUtilities.getPiePlotData(ttlLabels, ttlValues, colors);
    }

    //#endregion

}