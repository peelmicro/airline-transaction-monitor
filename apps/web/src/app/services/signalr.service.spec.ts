import { SignalRService } from './signalr.service';

describe('SignalRService', () => {
  let service: SignalRService;

  beforeEach(() => {
    service = new SignalRService();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should not be connected initially', () => {
    expect(service.connected()).toBe(false);
  });

  it('should have empty transactions initially', () => {
    expect(service.transactions()).toEqual([]);
  });

  it('should have empty metrics initially', () => {
    expect(service.metrics()).toEqual([]);
  });

  it('should have empty alerts initially', () => {
    expect(service.alerts()).toEqual([]);
  });
});
