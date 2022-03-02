import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, of } from 'rxjs';
import { CamSetting } from './objects/CamSetting';
import { CamBasicInfo } from './objects/CamBasicInfo';
import { CamTimeInterval } from './objects/CamTimeInterval';

@Injectable({
  providedIn: 'root'
})
export class CameraService {
  // private serverIP: string = "localhost";
  // private serverPort: string = "5001";
  private cameraUrl: string = "api/cam";
  private cameraSettingUrl: string = "api/camSettings";

  private httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };

  constructor(private http: HttpClient) { }

  // camera services
  getActiveCameras(): Observable<CamBasicInfo[]> {
    const fullUrl = `${this.cameraUrl}?needRescan=false`;
    return this.http.get<CamBasicInfo[]>(fullUrl).pipe(
      catchError(
        (err, caught) => {
          let emptyCamBasicInfo: CamBasicInfo = {
            IpAddr: "N/A",
            MacAddr: "N/A"
          }
          return of([emptyCamBasicInfo]);
        }
      )
    )
  }

  refreshCameraList(): Observable<CamBasicInfo[]> {
    const fullUrl = `${this.cameraUrl}?needRescan=true`;
    return this.http.get<CamBasicInfo[]>(fullUrl).pipe(
      catchError(
        (err, caught) => {
          let emptyCamBasicInfo: CamBasicInfo = {
            IpAddr: "N/A",
            MacAddr: "N/A"
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
