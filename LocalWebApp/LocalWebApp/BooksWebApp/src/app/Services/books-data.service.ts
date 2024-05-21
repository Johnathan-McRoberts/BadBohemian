import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

import { Book, BookReadAddResponse, BookReadAddRequest } from '../Models/book';
import { Author } from '../Models/author';
import { LanguageAuthors } from '../Models/language-authors';
import { CountryAuthors } from '../Models/country-authors';
import { BookTally } from '../Models/book-tally';
import { MonthlyTally } from '../Models/monthly-tally';
import { TagBooks } from '../Models/tag-books';
import { DeltaBooks } from '../Models/delta-books';
import { YearlyTally, IYearlyTally } from '../Models/yearly-tally';
import { NationGeography } from '../Models/nation-geography';
import { NationDetail, NationDetailResponse } from '../Models/nation-detail';
import { EditorDetails } from '../Models/editor-details';
import { ExportText } from '../Models/export-text';
import { ExportDataToEmailRequest, ExportDataToEmailResponse } from '../Models/export-data-to-email';
import { environment } from '../../environments/environment';

const httpOptions =
{
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
};

@Injectable({
    providedIn: 'root',
})
export class BooksDataService {
  constructor(public http: HttpClient) {

        let baseUrl = environment.baseUrl;
        this.requestUrl = baseUrl + 'booksdata/';
    }

    public requestUrl: string;

    // Get the promised data
    public books: Book[] | undefined;
    fetchAllBooksData()
    {
        return this.http.get<Book[]>(this.requestUrl + "GetAllBooks")
            .toPromise().then(result =>
                {
                    this.books = result as Book[];
                },
                error => console.error(error));
    }

    public authors: Author[] | undefined;
    fetchAllAuthorsData()
    {
        return this.http.get<Author[]>(this.requestUrl + "GetAllAuthors")
            .toPromise().then(result =>
                {
                    this.authors = result as Author[];
                },
                error => console.error(error));
    }

    public languageAuthors: LanguageAuthors[] | undefined;
    fetchAllLanguageAuthorsData()
    {
        return this.http.get<LanguageAuthors[]>(this.requestUrl + "GetAllLanguageAuthors")
            .toPromise().then(result =>
                {
                    this.languageAuthors = result as LanguageAuthors[];
                },
                error => console.error(error));
    }

    public countryAuthors: CountryAuthors[] | undefined;
    fetchAllCountryAuthorsData()
    {
        return this.http.get<CountryAuthors[]>(this.requestUrl + "GetAllCountryAuthors")
            .toPromise().then(result =>
                {
                    this.countryAuthors = result as CountryAuthors[];
                },
                error => console.error(error));
    }

    public bookTallies: BookTally[] | undefined;
    fetchAllBookTalliesData()
    {
        return this.http.get<BookTally[]>(this.requestUrl + "GetAllBookTallies")
            .toPromise().then(result =>
                {
                    this.bookTallies = result as BookTally[];
                },
                error => console.error(error));
    }

    public monthlyTallies: MonthlyTally[] | undefined;
    fetchAllMonthlyTalliesData()
    {
        return this.http.get<MonthlyTally[]>(this.requestUrl + "GetAllMonthlyTallies")
            .toPromise().then(result =>
                {
                    this.monthlyTallies = result as MonthlyTally[];
                },
                error => console.error(error));
    }

    public tagBooks: TagBooks[] | undefined;
    fetchAllTagBooksData()
    {
        return this.http.get<TagBooks[]>(this.requestUrl + "GetAllTagBooks")
            .toPromise().then(result =>
                {
                    this.tagBooks = result as TagBooks[];
                },
                error => console.error(error));
    }

    public deltaBooks: DeltaBooks[] | undefined;
    fetchAllDeltaBooksData()
    {
        return this.http.get<DeltaBooks[]>(this.requestUrl + "GetAllBooksDeltas")
            .toPromise().then(result =>
                {
                    this.deltaBooks = result as DeltaBooks[];
                },
                error => console.error(error));
    }

    public yearlyTallies: YearlyTally[] | undefined;
    fetchAllYearlyTalliesData()
    {
        return this.http.get<YearlyTally[]>(this.requestUrl + "GetAllYearlyTallies")
            .toPromise().then(result =>
                {
                    this.yearlyTallies = result as YearlyTally[];
                },
                error => console.error(error));
    }

    public nations: NationGeography[] | undefined;
    fetchAllNationsData()
    {
        return this.http.get<NationGeography[]>(this.requestUrl + "GetAllNations")
            .toPromise().then(result =>
                {
                    this.nations = result as NationGeography[];
                },
                error => console.error(error));
    }

    public nationDetails: NationDetail[] | undefined;
    fetchAllNationDetailsData() {
        return this.http.get<NationDetail[]>(this.requestUrl + "GetAllNationDetails")
            .toPromise().then(result => {
                this.nationDetails = result as NationDetail[];
            },
                error => console.error(error));
    }

    public updateNationResponse: any;
    async updateAsyncNation(request: NationDetail) {
        this.updateNationResponse =
            await this.http.post<NationDetailResponse>(this.requestUrl + "UpdateNationDetail", request, httpOptions).toPromise();

        console.log('No issues, waiting until promise is resolved...');
    }

    public editorDetails: EditorDetails | undefined;
    fetchEditorDetails()
    {
        return this.http.get<EditorDetails>(this.requestUrl + "GetEditorDetails")
            .toPromise().then(result =>
                {
                    this.editorDetails = result as EditorDetails;
                },
                error => console.error(error));
    }

    public exportCsvText: ExportText | undefined;
    fetchExportCsvTextData(userId: string)
    {
        return this.http.get<ExportText>(this.requestUrl + "GetExportCsvText/" + userId)
            .toPromise().then(result =>
                {
                    this.exportCsvText = result as ExportText;
                },
                error => console.error(error));
    }

    fetchExportNationsCsvTextData(userId: string)
    {
        return this.http.get<ExportText>(this.requestUrl + "GetExportNationsCsvText/" + userId)
            .toPromise().then(result =>
                {
                    this.exportCsvText = result as ExportText;
                },
                error => console.error(error));
    }

    public booksWithDefaultUser: Book[] | undefined;
    getAsDefaultUser(userId: string)
    {
        return this.http.get<Book[]>(this.requestUrl + "GetAsDefaultUser/" + userId)
            .toPromise().then(result =>
                {
                    this.booksWithDefaultUser = result as Book[];
                },
                error => console.error(error));
    }

    public addBookReadResponse: any;
    async addAsyncBookRead(request: BookReadAddRequest)
    {
        this.addBookReadResponse =
            await this.http.post<BookReadAddResponse>(this.requestUrl, request, httpOptions).toPromise();

        console.log('No issues, waiting until promise is resolved...');
    }

    public exportEmailResponse: ExportDataToEmailResponse | any;
    async exportEmail(request: ExportDataToEmailRequest) {
        this.exportEmailResponse =
            await this.http.post<ExportDataToEmailResponse>(
                this.requestUrl + "ExportToEmail", request,
                httpOptions).toPromise();

        console.log('No issues, waiting until promise is resolved...');
    }


    public updateBookResponse: any;
    async  updateAsyncBook(book: Book)
    {
        this.updateBookResponse =
            await this.http.put<BookReadAddResponse>(this.requestUrl, book, httpOptions).toPromise();

        console.log('No issues, waiting until promise is resolved...');
    }

    public deleteBookResponse: any;
    async  deleteAsyncBook(book: Book)
    {
        this.deleteBookResponse =
            await this.http.delete<BookReadAddResponse>(
                this.requestUrl + "/" + book.id, httpOptions
            ).toPromise();

        console.log('No issues, waiting until promise is resolved...');
    }

    public readonly maxErrorLength: number = 255;
    public exportCsvFile: any;
    public exportCsvFileBlob: Blob | undefined;
    public exportCsvTextFile: ExportText | undefined;
    fetchExportCsvFileData(userId: string)
    {
        const requestUrl = this.requestUrl + "GetExportCsvFile/" + userId;

        const headers = new HttpHeaders().set('Content-Type', 'text/csv; charset=utf-8');

        return this.http.get(requestUrl, { headers, responseType: 'text' }).pipe(
            tap((result: any) =>
                {
                    let resultString = result.toString();

                    this.exportCsvTextFile = new ExportText('text/csv', resultString);

                    if (resultString.length > this.maxErrorLength)
                        resultString = resultString.substring(0, this.maxErrorLength) + "...";

                    this.log("fetched ExportCsvFileData " + resultString);
                    this.exportCsvFile = result;
                    this.exportCsvFileBlob = result as Blob;
                }
            ),
            catchError(this.handleError<any>('fetchExportCsvFileData'))
        ).toPromise();
    }

    fetchExportNationsCsvFileData(userId: string)
    {
        const requestUrl = this.requestUrl + "GetExportNationsCsvFile/" + userId;

        const headers = new HttpHeaders().set('Content-Type', 'text/csv; charset=utf-8');

        return this.http.get(requestUrl, { headers, responseType: 'text' }).pipe(
            tap((result: any) =>
                {
                    let resultString = result.toString();

                    this.exportCsvTextFile = new ExportText('text/csv', resultString);

                    if (resultString.length > this.maxErrorLength)
                        resultString = resultString.substring(0, this.maxErrorLength) + "...";

                    this.log("fetched ExportNationsCsvFileData " + resultString);
                    this.exportCsvFile = result;
                    this.exportCsvFileBlob = result as Blob;
                }
            ),
            catchError(this.handleError<any>('fetchExportNationsCsvFileData'))
        ).toPromise();

    }

    public exportDataToEmailResponse: any;
    async exportDataToEmailAsync(request: ExportDataToEmailRequest)
    {
        const exportDataToEmailUrl: string = this.requestUrl + 'ExportDataToEmail/';

        this.exportDataToEmailResponse =
            await this.http.post<ExportDataToEmailResponse>(exportDataToEmailUrl, request, httpOptions).toPromise();

        console.log('exportDataToEmailAsync: No issues, waiting until promise is resolved...');
    }

    public importBooksCsvResponse: any;
    async importBooksCsvAsync(request: FormData) {
        const importBooksCsvUrl: string = this.requestUrl + 'UploadBooksCsv/';

        //const headers = new HttpHeaders().set('Content-Type', 'text/csv; charset=utf-8');
        const headers = new HttpHeaders().set('Content-Type', 'text/plain');

        this.importBooksCsvResponse =
            await this.http.post<FormData>(importBooksCsvUrl, request, { headers }).toPromise();

        console.log('importBooksCsvAsync: No issues, waiting until promise is resolved...');
    }

    // utility methods

    private handleError<T>(operation = 'operation', result?: T)
    {
        return (error: any): Observable<T> =>
        {
            // TODO: send the error to remote logging infrastructure
            console.error(error); // log to console instead

            // TODO: better job of transforming error for user consumption
            this.log(`${operation} failed: ${error.message}`);

            // Let the app keep running by returning an empty result.
            return new Observable<T>();
        };
    }

    /** Log a QuoteService message with the MessageService */
    private log(message: string)
    {
        console.log(`BooksDataService: ${message}`);
    }
}
