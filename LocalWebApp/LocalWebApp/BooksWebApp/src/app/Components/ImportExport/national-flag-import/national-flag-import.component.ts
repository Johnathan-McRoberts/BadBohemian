import { Component, OnInit, AfterViewInit, ViewChild, TemplateRef } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';


import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';
import { MatAutocomplete } from '@angular/material/autocomplete';
import { MAT_DATE_LOCALE } from '@angular/material/core';


import * as FileSaver from 'file-saver';

import { BooksDataService } from './../../../Services/books-data.service';
import { CurrentLoginService } from './../../../Services/current-login.service';

import { INationDetail, NationDetail, NationDetailResponse } from './../../../Models/nation-detail';

import { ExportText } from './../../../Models/export-text';
import { ExportDataToEmailRequest } from './../../../Models/export-data-to-email';

@Component({
    selector: 'app-national-flag-import',
    templateUrl: './national-flag-import.component.html',
    styleUrls: ['./national-flag-import.component.scss']
})
export class NationalFlagImportComponent implements OnInit {

    constructor(
        private dialog: MatDialog,
        private formBuilder: FormBuilder,
        private booksDataService: BooksDataService,
        public currentLoginService: CurrentLoginService
    ) {
        this.componentTitle = "Loading national flag from database...";

        this.setupFormGroup();
    }

    //#region Accordion State implementation

    public showAllNationsPanelOpenState = true;
    public showEditNationPanelOpenState = true;
    public selectedNation: NationDetail | any = null;
    public editNation: NationDetail | any = null;
    public nationToEdit: boolean = false;
    public updateInProgress: boolean = false;
    public selectedNationToDisplay: boolean = false;
    public selectedNationToEdit: boolean = false;
    
    //#endregion

    //#region Populate Nations Table

    public nationsDisplayedColumns: string[] =
        [
            'name',
            'capital',
            'latitude',
            'longitude',
            'flag'
        ];

    @ViewChild('nationsTablePaginator') nationsTablePaginator: MatPaginator | any;
    @ViewChild('nationsTableSort') public nationsTableSort: MatSort | any;
    public nationsDataSource: MatTableDataSource<NationDetail> | any;

    private setupNationsPagingAndSorting(): void {
        if (this.nationDetails != null) {
            setTimeout(() => {
                this.nationsDataSource.paginator = this.nationsTablePaginator;
                this.nationsDataSource.sort = this.nationsTableSort;
                this.nationsTableSort.sortChange.subscribe(() => {
                    console.log('Sorting...');
                    this.nationsTablePaginator.pageIndex = 0;
                    this.nationsTablePaginator._changePageSize(this.nationsTablePaginator.pageSize);
                });
            });
        }
    }

    applyNationsFilter(eventTarget: any) {
        let filterValue: string = (eventTarget as HTMLInputElement).value;
        this.nationsDataSource.filter = filterValue.trim().toLowerCase();

        if (this.nationsDataSource.paginator) {
            this.nationsTablePaginator.pageIndex = 0;
            this.nationsTablePaginator._changePageSize(this.nationsTablePaginator.pageSize);
        }
    }

    onNationRowClicked(row: INationDetail) {
        const nation = NationDetail.fromData(row);
        this.selectedNation = nation;
        console.log('Nation Row clicked: ', this.selectedNation.name);
        this.selectedNationToDisplay = false;

        this.selectedNationToEdit = true;
        this.showEditNationPanelOpenState = true;
        this.editNation = nation;

        this.setCurrentValues();
        this.selectedNationToDisplay = true;
    }

    //#endregion

    //#region Images

    public readonly defaultImageUrl =
        "https://images-na.ssl-images-amazon.com/images/I/61TGJyLMu6L._SY606_.jpg";

    public imageUri: string = this.defaultImageUrl;

    //#endregion

    public editNationFormGroup: FormGroup | any;

    public setupFormGroup(): void {

        this.editNationFormGroup =
            this.formBuilder.group({
                name: [''],
                capital: [''],
                imageUri: ['']
            });
    }

    public setCurrentValues() {

        this.imageUri = "";
        if (this.editNation.imageUri !== undefined &&
            this.editNation.imageUri !== null &&
            this.editNation.imageUri !== '') {

            console.log("Setting imageUri to: " + this.editNation.imageUri);
            this.imageUri = this.editNation.imageUri;
        }


        console.log("Setting imageUrl : " + this.imageUri);

        this.editNationFormGroup.setValue(
            {
                name: this.editNation.name,
                capital: this.editNation.capital,
                imageUri: this.imageUri,
            });
    }


    //#region Data Setup

    ngOnInit() {
        this.booksDataService.fetchAllNationDetailsData().then(() => {
            this.nationDetails = this.booksDataService.nationDetails;
            this.nationsDataSource = new MatTableDataSource(this.nationDetails);
            this.setupNationsPagingAndSorting();
        });

    }

    ngAfterViewInit() {
    }

    public nationDetails: NationDetail[] | any;

    public componentTitle: string;

    public get loadingChartData(): boolean {

        return (!this.nationDetails);
    }

    //#endregion

    //#region Page Button handlers

    public onDisplayUpdated() {
        console.log("onDisplayUpdated");

        console.log("imageUri: " + this.editNationFormGroup.value.imageUri);
        this.imageUri = this.editNationFormGroup.value.imageUri;

        let editItem: NationDetail =
            new NationDetail(
                this.selectedNation.id,
                this.selectedNation.name,
                this.selectedNation.capital,
                this.selectedNation.latitude,
                this.selectedNation.longitude,
                this.imageUri);

        console.log("editItem: " + JSON.stringify(editItem, null, 2));

        //this.selectedBookToDisplay = true;
        //this.inputDateRead = this.selectedBookReadTime.value;
        //this.getUpdatedValues();
        //this.selectedBook = this.updatedBookRead;
    }

    public updateResponse : NationDetailResponse | any;

    public async onUpdateFlag() {
        console.log("onUpdateFlag");

        console.log("imageUri: " + this.editNationFormGroup.value.imageUri);
        this.imageUri = this.editNationFormGroup.value.imageUri;

        let updateItem: NationDetail =
            new NationDetail(
                this.selectedNation.id,
                this.selectedNation.name,
                this.selectedNation.capital,
                this.selectedNation.latitude,
                this.selectedNation.longitude,
                this.imageUri);

        console.log("updateItem: " + JSON.stringify(updateItem, null, 2));


        this.booksDataService.updateAsyncNation(updateItem).then(() => {
            this.updateResponse = this.booksDataService.updateNationResponse;
            console.log("updateItem: " + JSON.stringify(this.updateResponse, null, 2));
        });


        //this.selectedBookToDisplay = true;
        //this.inputDateRead = this.selectedBookReadTime.value;
        //this.getUpdatedValues();
        //this.selectedBook = this.updatedBookRead;
        //this.selectedBook.user = this.currentLoginService.userId;

        //this.updateInProgress = true;
        //await this.booksDataService.updateAsyncBook(this.selectedBook);
        //this.updateInProgress = false;

        //const resp = BookReadAddResponse.fromData(this.booksDataService.updateBookResponse);

        //this.displayOnResp(resp);
    }

    //#endregion

}
