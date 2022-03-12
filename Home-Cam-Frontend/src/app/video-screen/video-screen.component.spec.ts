import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoScreenComponent } from './video-screen.component';

describe('VideoScreenComponent', () => {
  let component: VideoScreenComponent;
  let fixture: ComponentFixture<VideoScreenComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ VideoScreenComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(VideoScreenComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
