import { Component } from '@angular/core';
import { CameraService } from './camera.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Home-Cam-Frontend';
  constructor (private cameraServices: CameraService){
    this.cameraServices.getActiveCameras().subscribe();
  }
}
