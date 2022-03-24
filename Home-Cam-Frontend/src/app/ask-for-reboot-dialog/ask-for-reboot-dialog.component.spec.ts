import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AskForRebootDialogComponent } from './ask-for-reboot-dialog.component';

describe('AskForRebootDialogComponent', () => {
  let component: AskForRebootDialogComponent;
  let fixture: ComponentFixture<AskForRebootDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AskForRebootDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AskForRebootDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
