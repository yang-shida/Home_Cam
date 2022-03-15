import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SideBarSelectionService {

  selectedMenuItem: string = "N/A";
  selectedCam: string = "N/A";

  selectedMenuItemSubject: Subject<string>=new Subject<string>();
  selectedCamSubject: Subject<string>=new Subject<string>();

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

  getSelectedMenuItem(): string{
    return this.selectedMenuItem;
  }

  getSelectedCam(): string{
    return this.selectedCam==null?"N/A":this.selectedCam;
  }

}
