import { TestBed } from '@angular/core/testing';

import { BackendSettingService } from './backend-setting.service';

describe('BackendSettingService', () => {
  let service: BackendSettingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BackendSettingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
