import { TestBed } from '@angular/core/testing';

import { VkActivityService } from './vk-activity.service';

describe('VkActivityService', () => {
  let service: VkActivityService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VkActivityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
