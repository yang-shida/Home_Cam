import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CamDetailComponent } from './cam-detail.component';

describe('CamDetailComponent', () => {
  let component: CamDetailComponent;
  let fixture: ComponentFixture<CamDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CamDetailComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CamDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
