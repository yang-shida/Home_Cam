import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CctvViewComponent } from './cctv-view.component';

describe('CctvViewComponent', () => {
  let component: CctvViewComponent;
  let fixture: ComponentFixture<CctvViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CctvViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CctvViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
