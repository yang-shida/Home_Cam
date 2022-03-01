import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CamSetting } from './objects/CamSetting';

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

  // camera setting services
  getCamSetting(camId: string): Observable<CamSetting> {
    const fullUrl = `${this.cameraSettingUrl}/${camId}`;
    return this.http.get<CamSetting>(fullUrl);
  }
}
