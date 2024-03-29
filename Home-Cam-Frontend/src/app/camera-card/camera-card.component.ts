import { Component, Input, OnInit } from '@angular/core';
import { CamSetting } from '../objects/CamSetting';
import { CameraService } from '../camera.service';

@Component({
  selector: 'app-camera-card',
  templateUrl: './camera-card.component.html',
  styleUrls: ['./camera-card.component.css']
})
export class CameraCardComponent implements OnInit {
  // CSS Variables --------------------------
  cardHeight: number=450;
  cardWidth: number=300;
  // End CSS Variables ----------------------

  @Input() camId: string = "N/A";

  camLocation: string = "N/A";
  imageUrl: string = "N/A";

  constructor(private cameraServices: CameraService) { }

  ngOnInit(): void {
    this.cameraServices.getCamSetting(this.camId as string).subscribe(
      camSettings => {
        this.camLocation = camSettings.Location;
      }
    );
    this.imageUrl = this.cameraServices.getCameraPreviewImageUrl(this.camId as string);
  }

  onRefresh(event: Event): void{
    event.stopPropagation();
    this.imageUrl = this.cameraServices.getCameraPreviewImageUrl(this.camId as string);
  }

}
