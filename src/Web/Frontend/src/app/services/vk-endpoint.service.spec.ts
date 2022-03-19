import { TestBed } from '@angular/core/testing';

import { VkEndpointService } from './vk-endpoint.service';

describe('VkEndpointService', () => {
  let service: VkEndpointService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VkEndpointService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
