import { Component } from '@angular/core';
import { Subscription } from 'rxjs';
import { BackendSettingService } from './backend-setting.service';
import { CameraService } from './camera.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Home-Cam-Frontend';

  backendLogSubscription: Subscription;

  constructor (private cameraServices: CameraService, private backendSettingService: BackendSettingService){
    this.cameraServices.getActiveCameras().subscribe();
    this.backendLogSubscription = this.backendSettingService.connectBackendLog().subscribe();
  }

  ngOnDestroy(): void{
    this.backendLogSubscription.unsubscribe();
  }
}
