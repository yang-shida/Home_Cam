import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UiService {

  camDetailPageVideoWidthSubject: BehaviorSubject<number> = new BehaviorSubject<number>(50);
  showInactiveCamSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
  cctvVideoScreensPerRowSubject: BehaviorSubject<number> = new BehaviorSubject<number>(2);

  constructor() { }

  // camDetailPageVideoWidthSubject
  setCamDetailPageVideoWidth(newWidth: number): void {
    this.camDetailPageVideoWidthSubject.next(newWidth);
  }
  onCamDetailPageVideoWidthChange(): Observable<number> {
    return this.camDetailPageVideoWidthSubject.asObservable();
  }

  // showInactiveCamSubject
  setShowInactiveCam(newVal: boolean): void{
    this.showInactiveCamSubject.next(newVal);
  }
  onShowInactiveCamChange(): Observable<boolean> {
    return this.showInactiveCamSubject.asObservable();
  }

  // cctvVideoScreensPerRowSubject
  setCctvVideoScreensPerRow(newVal: number): void{
    this.cctvVideoScreensPerRowSubject.next(newVal);
  }
  onCctvVideoScreensPerRowChange(): Observable<number> {
    return this.cctvVideoScreensPerRowSubject.asObservable();
  }
}
