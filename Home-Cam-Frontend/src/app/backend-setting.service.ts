import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, of, Subject, Subscriber, tap } from 'rxjs';
import { AuthInfo } from './objects/AuthInfo';
import { SystemSetting } from './objects/SystemSetting';

@Injectable({
  providedIn: 'root'
})
export class BackendSettingService {

  settingFormIsOpen: boolean = true;
  logIsOpen: boolean = true;

  private settingUrl = "api/system";

  systemSetting: SystemSetting = {
    MaxSpaceGBs: -1,
    PercentToDeleteWhenFull: - 1,
    SearchCamerasMinutes: -1,
    ImageStorageSizeControlMinutes: -1
  };

  backendLog: string = "";

  private backendLogSubject: Subject<string> = new Subject<string>();

  constructor(private http: HttpClient) {

  }

  toggleSettingForm(): void {
    this.settingFormIsOpen = !this.settingFormIsOpen;
  }

  toggleLogArea(): void {
    this.logIsOpen = !this.logIsOpen;
  }

  getBackendSettings(): Observable<SystemSetting> {
    let fullUrl = `${this.settingUrl}`;
    return this.http.get<SystemSetting>(fullUrl).pipe(
      tap(
        res => {
          this.systemSetting = res;
        }
      ),
      catchError(
        () => {
          return of({
            MaxSpaceGBs: -1,
            PercentToDeleteWhenFull: - 1,
            SearchCamerasMinutes: -1,
            ImageStorageSizeControlMinutes: -1
          });
        }
      )
    )
  }

  updateBackendSettings(newSetting: SystemSetting): Observable<SystemSetting> {
    let fullUrl = `${this.settingUrl}`;
    return this.http.put(fullUrl, newSetting, { responseType: 'text' }).pipe(
      tap(
        res => {
          this.systemSetting = newSetting;
        }
      ),
      catchError(
        (err, caught) => {
          console.log(err.error)
          return of({
            MaxSpaceGBs: -1,
            PercentToDeleteWhenFull: - 1,
            SearchCamerasMinutes: -1,
            ImageStorageSizeControlMinutes: -1
          });
        }
      )
    ) as Observable<SystemSetting>
  }

  rebootBackend(pwd: string): Observable<boolean> {
    let fullUrl = `${this.settingUrl}/restart`;
    let authInfo: AuthInfo = {
      CurrPwd: pwd,
      NewPwd: ""
    };
    return this.http.post(fullUrl, authInfo, { responseType: 'text' }).pipe(
      map(
        (res) => {
          return true;
        }
      ),
      catchError(
        (err) => {
          return of(false)
        }
      )
    ) as Observable<boolean>
  }

  checkPwd(pwd: string): Observable<boolean> {
    let fullUrl = `${this.settingUrl}/check-pwd`;
    let authInfo: AuthInfo = {
      CurrPwd: pwd,
      NewPwd: ""
    };
    return this.http.post(fullUrl, authInfo, { responseType: 'text' }).pipe(
      map(
        (res) => {
          return true;
        }
      ),
      catchError(
        (err) => {
          return of(false)
        }
      )
    ) as Observable<boolean>
  }

  connectBackendLog(): Observable<string> {
    return new Observable<string>(
      obs => {
        const es = new EventSource(`${this.settingUrl}/logs`);
        es.addEventListener('message', (evt) => {
          let htmlNewLineStr: string = evt.data;
          this.backendLog += (htmlNewLineStr + '<br>');
          this.backendLogSubject.next(this.backendLog);
          obs.next(evt.data);
        });
        return () => es.close();
      });
  }

  onBackendLogChange(): Observable<string> {
    return this.backendLogSubject.asObservable();
  }


}
