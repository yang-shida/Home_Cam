import { Component, Input, OnInit } from '@angular/core';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { MatSliderChange } from '@angular/material/slider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, Subscription } from 'rxjs';
import { CameraService } from '../camera.service';
import { CamSetting } from '../objects/CamSetting';
import { CamTimeInterval } from '../objects/CamTimeInterval';
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

  camId: string = this.sideBarSelectionServices.selectedCamSubject.value;
  currCamIdSubscription: Subscription;

  camSetting?: CamSetting;
  recordingIntervals: CamTimeInterval[] = [];

  pickedDate?: Date;
  pickedDateTimeIntervals: CamTimeInterval[] = [];

  constructor(private sideBarSelectionServices: SideBarSelectionService, private uiService: UiService, private cameraService: CameraService, private _snackBar: MatSnackBar) {
    this.currCamIdSubscription = this.sideBarSelectionServices.onSelectedCamUpdate().subscribe(
      camId => {
        this.camId = camId;
        this.cameraService.getCamSetting(camId).subscribe(
          camSettingFromServer => {
            this.camSetting = camSettingFromServer;
          }
        );
        this.cameraService.getAvailableRecordingTimeIntervals(camId).subscribe(
          recordingIntervalsFromServer => {
            this.recordingIntervals = recordingIntervalsFromServer;
            this.pickedDate = new Date(this.recordingIntervals[this.recordingIntervals.length - 1].End);
            if(this.pickedDate){
              this.pickedDate = new Date(
                this.pickedDate.getFullYear(),
                this.pickedDate.getMonth(),
                this.pickedDate.getDate()
              );
              this.cameraService.getAvailableRecordingTimeIntervals(camId, this.pickedDate.getTime(), this.pickedDate.getDate() + 1000*3600*24).subscribe(
                pickedDateTimeIntervalsFromServer => {
                  this.pickedDateTimeIntervals = pickedDateTimeIntervalsFromServer;
                }
              )
            }
          }
        )
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
          if (feedbackCamSetting.UniqueId == "N/A") {
            this._snackBar.open('Something is wrong. Unable to update settings!', undefined, { duration: 2000 })
          }
          else {
            this._snackBar.open('Setting updated!', undefined, { duration: 2000 })
            this.camSetting = feedbackCamSetting
          }
        }
      );
    }
  }

  myFilter = (d: Date | null): boolean => {
    if (d == null) {
      return false;
    }
    const dateStart = d;
    const dateEnd = new Date(dateStart.getTime() + 3600 * 24 * 1000);

    return this.recordingIntervals.some(
      timeInterval => {
        const timeIntervalStartDate = new Date(timeInterval.Start);
        const timeIntervalEndDate = new Date(timeInterval.End);
        return timeIntervalStartDate < dateEnd && timeIntervalEndDate > dateStart;
      }
    )
  };

  onPickedDateChange(pickedDateChangeEvent: MatDatepickerInputEvent<Date>): void {
    this.pickedDate = pickedDateChangeEvent.value == null ? undefined : pickedDateChangeEvent.value;
    if(this.pickedDate){
      this.cameraService.getAvailableRecordingTimeIntervals(this.camId, this.pickedDate.getTime(), this.pickedDate.getDate() + 1000*3600*24).subscribe(
        pickedDateTimeIntervalsFromServer => {
          this.pickedDateTimeIntervals = pickedDateTimeIntervalsFromServer;
        }
      )
    }
  }

}
