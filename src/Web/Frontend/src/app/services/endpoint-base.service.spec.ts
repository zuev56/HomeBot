import { TestBed } from '@angular/core/testing';

import { EndpointBaseService } from './endpoint-base.service';

describe('EndpointBaseService', () => {
  let service: EndpointBaseService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EndpointBaseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
