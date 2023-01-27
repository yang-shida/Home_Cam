import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, catchError, Observable, of, Subject, tap } from 'rxjs';
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

  private localCamListSubject: BehaviorSubject<CamBasicInfo[]> = new BehaviorSubject<CamBasicInfo[]>(this.localCamList);

  constructor(private http: HttpClient) {
    this.lastLocalCamListUpdateTime = Date.now() - (this.localCamListRefreshPeriodSec * 1000 + 1);
  }

  removeColonFromMacAddr(mac: string): string {
    return mac.replace(/\:/g, "");
  }

  // camera services
  onLocalCamListUpdate(): Observable<CamBasicInfo[]> {
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
          camInfoList.sort(
            (a, b) => {
              if (a.IpAddr == "N/A" && b.IpAddr != "N/A") {
                return 1;
              }
              if (a.IpAddr != "N/A" && b.IpAddr == "N/A") {
                return -1;
              }
              return 0;
            }
          );
          this.lastLocalCamListUpdateTime = Date.now();
          this.localCamList = camInfoList;
          this.localCamListSubject.next(camInfoList);
        }
      ),
      catchError(
        () => {
          let emptyCamBasicInfo: CamBasicInfo = {
            IpAddr: "N/A",
            UniqueId: "N/A"
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
          camInfoList.sort(
            (a, b) => {
              if (a.IpAddr == "N/A") {
                return 1;
              }
              if (b.IpAddr == "N/A") {
                return -1;
              }
              return 0;
            }
          );
          this.lastLocalCamListUpdateTime = Date.now();
          this.localCamList = camInfoList;
          this.localCamListSubject.next(camInfoList);
        }
      ),
      catchError(
        () => {
          let emptyCamBasicInfo: CamBasicInfo = {
            IpAddr: "N/A",
            UniqueId: "N/A"
          }
          return of([emptyCamBasicInfo]);
        }
      )
    )
  }

  getCameraPreviewImageUrl(camId: string): string {
    return this.isCamActive(camId) ? `${this.cameraUrl}/${this.removeColonFromMacAddr(camId)}/preview?cb=${Date.now()}` : "../../assets/cam_not_active.jpg";
  }

  connentVideo(camId: string, startTimeUtc: number | null = null): Observable<string> {
    let fullUrl: string = startTimeUtc == null ?
      `${this.cameraUrl}/${this.removeColonFromMacAddr(camId)}?cb=${Date.now()}` :
      `${this.cameraUrl}/${this.removeColonFromMacAddr(camId)}?startTimeUtc=${startTimeUtc}&cb=${Date.now()}`;

    return new Observable<string>(
      obs => {
        const es = new EventSource(fullUrl);
        es.addEventListener('message', (evt) => {
          obs.next(evt.data);
        });
        es.addEventListener('error', (evt) => {
          es.close();
          obs.next("Stream Finished!");
        });
        return ()=>{es.close()};
      }
    );
  }

  getStreamingUrl(camId: string): string {
    return `${this.cameraUrl}/${this.removeColonFromMacAddr(camId)}?cb=${Date.now()}`;
  }

  getPlaybackUrl(camId: string, startTimeUtc: number): string {
    return `${this.cameraUrl}/${this.removeColonFromMacAddr(camId)}?startTimeUtc=${startTimeUtc}&cb=${Date.now()}`;
  }

  getAvailableRecordingTimeIntervals(camId: string, start?: number, length?: number): Observable<CamTimeInterval[]> {
    const fullUrl = start == null || length == null ?
      `${this.cameraUrl}/${this.removeColonFromMacAddr(camId)}/available_recording_time_intervals` :
      `${this.cameraUrl}/${this.removeColonFromMacAddr(camId)}/available_recording_time_intervals?startTimeUtc=${Math.trunc(start)}&timeLengthMillis=${Math.trunc(length)}`;
    return this.http.get<CamTimeInterval[]>(fullUrl).pipe(
      catchError(
        (err, caught) => {
          let emptyCamTimeInterval: CamTimeInterval = {
            Start: 0,
            End: 0
          };
          return of([emptyCamTimeInterval]);
        }
      )
    )
  }

  isCamActive(camId: string): boolean {
    return this.localCamList.find(cam => cam.UniqueId == camId && cam.IpAddr != "N/A") !== undefined;
  }

  // camera setting services
  getCamSetting(camId: string): Observable<CamSetting> {
    const fullUrl = `${this.cameraSettingUrl}/${this.removeColonFromMacAddr(camId)}`;
    return this.http.get<CamSetting>(fullUrl).pipe(
      catchError(
        (err, caught) => {
          let emptyCamSetting: CamSetting = {
            UniqueId: "N/A",
            Location: "N/A",
            FrameSize: -1,
            FlashLightOn: false,
            HorizontalMirror: false,
            VerticalMirror: false
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
            UniqueId: "N/A",
            Location: "N/A",
            FrameSize: -1,
            FlashLightOn: false,
            HorizontalMirror: false,
            VerticalMirror: false
          };
          return of(emptyCamSetting);
        }
      )
    )
  }

}
