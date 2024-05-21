import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NationalFlagImportComponent } from './national-flag-import.component';

describe('NationalFlagImportComponent', () => {
  let component: NationalFlagImportComponent;
  let fixture: ComponentFixture<NationalFlagImportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NationalFlagImportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NationalFlagImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
