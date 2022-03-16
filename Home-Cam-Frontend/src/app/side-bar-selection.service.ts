import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SideBarSelectionService {

  selectedMenuItem: string = "N/A";
  selectedCam: string = "N/A";

  selectedMenuItemSubject: BehaviorSubject<string>=new BehaviorSubject<string>("N/A");
  selectedCamSubject: BehaviorSubject<string>=new BehaviorSubject<string>("N/A");

  constructor() { }

  selectMenuItem(item: string): void {
    this.selectedMenuItem=item;
    this.selectedMenuItemSubject.next(item);
  }

  selectCamera(camId: string): void {
    this.selectedCam=camId;
    this.selectedCamSubject.next(camId);
  }

  onSelectedMenuItemUpdate(): Observable<string> {
    return this.selectedMenuItemSubject.asObservable();
  }

  onSelectedCamUpdate(): Observable<string>{
    return this.selectedCamSubject.asObservable();
  }

}
