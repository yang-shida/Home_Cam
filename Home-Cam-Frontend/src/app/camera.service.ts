import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, of, Subject, tap } from 'rxjs';
import { CamSetting } from './objects/CamSetting';
import { CamBasicInfo } from './objects/CamBasicInfo';
import { CamTimeInterval } from './objects/CamTimeInterval';

@Injectable({
  providedIn: 'root'
})
export class CameraService {
  private cameraUrl: string = "api/cam";
  private cameraSettingUrl: string = "api/camSettings";

  private httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };

  private localCamList: CamBasicInfo[] = [];
  private localCamListRefreshPeriodSec: number = 60;
  private lastLocalCamListUpdateTime: number;

  private localCamListSubject: Subject<CamBasicInfo[]> = new Subject<CamBasicInfo[]>();

  constructor(private http: HttpClient) {
    this.lastLocalCamListUpdateTime = Date.now() - (this.localCamListRefreshPeriodSec * 1000 + 1);
  }

  delay(ms: number) {
      return new Promise( resolve => setTimeout(resolve, ms) );
  }

  // camera services
  onLocalCamListUpdate(): Observable<CamBasicInfo[]>{
    return this.localCamListSubject.asObservable();
  }

  getActiveCameras(): Observable<CamBasicInfo[]> {
    if (Date.now() - this.lastLocalCamListUpdateTime < this.localCamListRefreshPeriodSec) {
      return of(this.localCamList);
    }
    // only ask for new list when current list is too old
    const fullUrl = `${this.cameraUrl}?needRescan=false`;
    return this.http.get<any[]>(fullUrl).pipe(
      tap(
        (camInfoList) => {
          this.lastLocalCamListUpdateTime = Date.now();
          this.localCamList=camInfoList;
          this.localCamListSubject.next(camInfoList);
        }
      ),
      catchError(
        () => {
          let emptyCamBasicInfo: CamBasicInfo = {
            ipAddr: "N/A",
            uniqueId: "N/A"
          }
          return of([emptyCamBasicInfo]);
        }
      )
    )
  }

  refreshCameraList(): Observable<CamBasicInfo[]> {
    const fullUrl = `${this.cameraUrl}?needRescan=true`;
    return this.http.get<CamBasicInfo[]>(fullUrl).pipe(
      tap(
        camInfoList => {
          this.lastLocalCamListUpdateTime = Date.now();
          this.localCamList=camInfoList;
          this.localCamListSubject.next(camInfoList);
        }
      ),
      catchError(
        () => {
          let emptyCamBasicInfo: CamBasicInfo = {
            ipAddr: "N/A",
            uniqueId: "N/A"
          }
          return of([emptyCamBasicInfo]);
        }
      )
    )
  }

  getCameraPreviewImageUrl(camId: string): string {
    return `${this.cameraUrl}/${camId}/preview?cb=${Date.now()}`;
  }

  getStreamingUrl(camId: string): string {
    return `${this.cameraUrl}/${camId}?cb=${Date.now()}`;
  }

  getPlaybackUrl(camId: string, startTimeUtc: number): string {
    return `${this.cameraUrl}/${camId}?startTimeUtc=${startTimeUtc}&cb=${Date.now()}`;
  }

  getAvailableRecordingTimeIntervals(camId: string, start: number, length: number): Observable<CamTimeInterval[]> {
    const fullUrl = `${this.cameraUrl}/${camId}/available_recording_time_intervals?startTimeUtc=${Math.trunc(start)}&timeLengthMillis=${Math.trunc(length)}`;
    return this.http.get<CamTimeInterval[]>(fullUrl).pipe(
      catchError(
        (err, caught) => {
          let emptyCamTimeInterval: CamTimeInterval = {
            start: 0,
            end: 0
          };
          return of([emptyCamTimeInterval]);
        }
      )
    )
  }

  // camera setting services
  getCamSetting(camId: string): Observable<CamSetting> {
    const fullUrl = `${this.cameraSettingUrl}/${camId}`;
    return this.http.get<CamSetting>(fullUrl).pipe(
      catchError(
        (err, caught) => {
          let emptyCamSetting: CamSetting = {
            uniqueId: "N/A",
            location: "N/A",
            frameSize: -1,
            flashLightOn: false,
            horizontalMirror: false,
            verticalMirror: false
          };
          return of(emptyCamSetting);
        }
      )
    );
  }

  updateCamSetting(newCamSetting: CamSetting): Observable<CamSetting> {
    const fullUrl = `${this.cameraSettingUrl}`;
    return this.http.put<CamSetting>(fullUrl, newCamSetting, this.httpOptions).pipe(
      catchError(
        (err, caught) => {
          let emptyCamSetting: CamSetting = {
            uniqueId: "N/A",
            location: "N/A",
            frameSize: -1,
            flashLightOn: false,
            horizontalMirror: false,
            verticalMirror: false
          };
          return of(emptyCamSetting);
        }
      )
    )
  }

}
