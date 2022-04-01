import { Component, Input, OnInit } from '@angular/core';
import { MatSliderChange } from '@angular/material/slider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, Subscription } from 'rxjs';
import { CameraService } from '../camera.service';
import { CamSetting } from '../objects/CamSetting';
import { SideBarSelectionService } from '../side-bar-selection.service';
import { UiService } from '../ui.service';

@Component({
  selector: 'app-cam-detail',
  templateUrl: './cam-detail.component.html',
  styleUrls: ['./cam-detail.component.css']
})
export class CamDetailComponent implements OnInit {
  // CSS Variables --------------------------
  videoWidth: number = this.uiService.camDetailPageVideoWidthSubject.value;
  videoWidthSubscription: Subscription;
  // End CSS Variables ----------------------

  camId: string = "N/A";
  currCamIdSubscription: Subscription;

  camSetting?: CamSetting;

  constructor(private sideBarSelectionServices: SideBarSelectionService, private uiService: UiService, private cameraService: CameraService, private _snackBar: MatSnackBar) {
    this.currCamIdSubscription = this.sideBarSelectionServices.onSelectedCamUpdate().subscribe(
      camId => {
        this.camId = camId;
        this.cameraService.getCamSetting(camId).subscribe(
          camSettingFromServer => {
            this.camSetting = camSettingFromServer;
          }
        );
      }
    );
    this.videoWidthSubscription = this.uiService.onCamDetailPageVideoWidthChange().subscribe(
      newWidth => {
        this.videoWidth = newWidth;
      }
    );
  }

  ngOnInit(): void {

  }

  videoWidthSliderDisplayFormat(num: number): string {
    return num + "%";
  }
  videoWidthSliderChange(sliderChangeEvent: MatSliderChange): void {
    this.uiService.setCamDetailPageVideoWidth(sliderChangeEvent.value == null ? this.videoWidth : sliderChangeEvent.value);
  }

  onChangeSetting(): void {
    if (this.camSetting != null) {
      this.cameraService.updateCamSetting(this.camSetting).subscribe(
        feedbackCamSetting => {
          if(feedbackCamSetting.UniqueId=="N/A"){
            this._snackBar.open('Something is wrong. Unable to update settings!', undefined, { duration: 2000 })
          }
          else{
            this._snackBar.open('Setting updated!', undefined, { duration: 2000 })
            this.camSetting = feedbackCamSetting
          }
        }
      );
    }
  }

}
