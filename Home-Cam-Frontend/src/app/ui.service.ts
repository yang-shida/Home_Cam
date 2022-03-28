import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UiService {

  camDetailPageVideoWidthSubject: BehaviorSubject<number> = new BehaviorSubject(50);

  constructor() { }

  // camDetailPageVideoWidthSubject
  setCamDetailPageVideoWidth(newWidth: number): void {
    this.camDetailPageVideoWidthSubject.next(newWidth);
  }
  onCamDetailPageVideoWidthChange(): Observable<number> {
    return this.camDetailPageVideoWidthSubject.asObservable();
  }
}
