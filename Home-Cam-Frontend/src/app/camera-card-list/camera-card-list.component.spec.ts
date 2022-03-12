import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CameraCardListComponent } from './camera-card-list.component';

describe('CameraCardListComponent', () => {
  let component: CameraCardListComponent;
  let fixture: ComponentFixture<CameraCardListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CameraCardListComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CameraCardListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
