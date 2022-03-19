import { TestBed } from '@angular/core/testing';

import { Utilities } from './utilities';

describe('UtilitiesService', () => {
  let service: Utilities;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Utilities);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
