import { TestBed } from '@angular/core/testing';

import { SideBarSelectionService } from './side-bar-selection.service';

describe('SideBarSelectionService', () => {
  let service: SideBarSelectionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SideBarSelectionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
