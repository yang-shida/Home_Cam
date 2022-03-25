import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EnterPwdDialogComponent } from './enter-pwd-dialog.component';

describe('EnterPwdDialogComponent', () => {
  let component: EnterPwdDialogComponent;
  let fixture: ComponentFixture<EnterPwdDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EnterPwdDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EnterPwdDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
