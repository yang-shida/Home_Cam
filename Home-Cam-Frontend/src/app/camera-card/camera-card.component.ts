import { Component, OnInit } from '@angular/core';
import { CamSetting } from '../objects/CamSetting';
import { CameraService } from '../camera.service';

@Component({
  selector: 'app-camera-card',
  templateUrl: './camera-card.component.html',
  styleUrls: ['./camera-card.component.css']
})
export class CameraCardComponent implements OnInit {

  camId?: string;
  camLocation?: string;
  imageUrl?: string;

  constructor(private cameraServices: CameraService) { }

  ngOnInit(): void {
    this.cameraServices.getCamSetting("94:b9:7e:fa:e1:28").subscribe(
      camSettings => {
        this.camId = camSettings.uniqueId;
        this.camLocation = camSettings.location;
      }
    );
  }

  buttonClick(): void {
    // toggle flash
    // this.cameraServices.getCamSetting("94:b9:7e:fa:e1:28").subscribe(
    //   camSettings => {
    //     this.cameraServices.updateCamSetting({...camSettings, flashLightOn: !camSettings.flashLightOn}).subscribe()
    //   }
    // );

    // list cameras
    // this.cameraServices.getActiveCameras().subscribe(
    //   camList => {
    //     console.log(camList)
    //   }
    // )

    // refresh then list
    // this.cameraServices.refreshCameraList().subscribe(
    //   camList => {
    //     console.log(camList)
    //   }
    // )

    // preview
    // this.imageUrl = this.cameraServices.getCameraPreviewImageUrl("94:b9:7e:fa:e1:28");

    // stream
    // this.imageUrl = this.cameraServices.getStreamingUrl("94:b9:7e:fa:e1:28");

    // playback
    // this.imageUrl = this.cameraServices.getPlaybackUrl("94:b9:7e:fa:e1:28", 1645192638880);

    // show time interval
    // this.cameraServices.getAvailableRecordingTimeIntervals("94:b9:7e:fa:e1:28", 1645192638880, 1000*3600*24*14).subscribe(
    //   intervals => {
    //     console.log(intervals)
    //   }
    // )
    
  }

}
