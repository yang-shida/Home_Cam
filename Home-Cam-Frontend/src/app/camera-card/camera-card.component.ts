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

  constructor(private cameraServices: CameraService) { }

  ngOnInit(): void {
    this.cameraServices.getCamSetting("94:b9:7e:fa:e1:28").subscribe(camSettings=>{
      console.log(camSettings);
      this.camId=camSettings.uniqueId;
      this.camLocation=camSettings.location;
    });
  }

}
