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

  submissionLoading: boolean = false;

  camId: string = this.sideBarSelectionServices.selectedCamSubject.value;
  currCamIdSubscription: Subscription;

  camSetting?: CamSetting;
  recordingIntervals: CamTimeInterval[] = [];

  pickedDate?: Date;
  pickedDateTimeIntervals: CamTimeInterval[] = [];

  percentage: string = "0%";
  currStartTimeMs: number = 0;
  currIntervalIsFinished: boolean = false;
  currFrameTimeSinceStartMs: number = 0;
  pausedTimeMarkMs: number = 0;

  isDown: boolean = false;
  isPlaying: boolean = false;
  playIconUrl: string = '../../assets/play_icon.png';
  pauseIconUrl: string = '../../assets/pause_icon.png';


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
            if (recordingIntervalsFromServer.length > 0) {
              this.recordingIntervals = recordingIntervalsFromServer;
              this.pickedDate = new Date(this.recordingIntervals[this.recordingIntervals.length - 1].End);
              if (this.pickedDate) {
                this.pickedDate = new Date(
                  this.pickedDate.getFullYear(),
                  this.pickedDate.getMonth(),
                  this.pickedDate.getDate()
                );
                this.pickedDateTimeIntervals = [];
                this.cameraService.getAvailableRecordingTimeIntervals(camId, this.pickedDate.getTime(), this.pickedDate.getDate() + 1000 * 3600 * 24).subscribe(
                  pickedDateTimeIntervalsFromServer => {
                    this.pickedDateTimeIntervals = pickedDateTimeIntervalsFromServer;
                  }
                )
              }
            }
            else{
              this.recordingIntervals=[];
              this.pickedDate=new Date();
              this.pickedDate = new Date(
                this.pickedDate.getFullYear(),
                this.pickedDate.getMonth(),
                this.pickedDate.getDate()
              );
              this.pickedDateTimeIntervals = [];
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
    this.submissionLoading = true;
    if (this.camSetting != null) {
      this.cameraService.updateCamSetting(this.camSetting).subscribe(
        feedbackCamSetting => {
          this.submissionLoading = false;
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
    if (this.pickedDate) {
      this.pickedDateTimeIntervals = [];
      this.cameraService.getAvailableRecordingTimeIntervals(this.camId, this.pickedDate.getTime(), this.pickedDate.getDate() + 1000 * 3600 * 24).subscribe(
        pickedDateTimeIntervalsFromServer => {
          this.pickedDateTimeIntervals = pickedDateTimeIntervalsFromServer;
        }
      )
    }
  }


  onMouseDown(evt: any): void {
    this.isDown = true;
    this.percentage = (evt.offsetX >= 0 ? evt.offsetX : 0) / evt.currentTarget.offsetWidth * 100 + '%';
  }
  onMouseMove(evt: any): void {
    if (this.isDown) {
      this.percentage = (evt.offsetX >= 0 ? evt.offsetX : 0) / evt.currentTarget.offsetWidth * 100 + '%';
    }
  }
  onMouseUp(evt: any): void {
    this.isDown = false;

    let percentNum: number = Number(this.percentage.substring(0, this.percentage.length - 1));
    // console.log(percentNum)
    let selectedTimeMark: number = Math.round(percentNum / 100 * 1000 * 3600 * 24 + this.pickedDate!.getTime());
    let insideIntervalIndex: number = this.pickedDateTimeIntervals.findIndex(int => int.Start < selectedTimeMark && int.End > selectedTimeMark);
    let nextIntervalIndex: number = this.pickedDateTimeIntervals.findIndex(int => int.Start > selectedTimeMark);

    // inside valid interval
    if (insideIntervalIndex != -1) {
      this.currStartTimeMs = selectedTimeMark;
      this.percentage = this.timeMarkToPercent(this.currStartTimeMs);
      this.isPlaying = true;
    }
    // snap to the next available interval
    else {
      if (nextIntervalIndex != -1) {
        this.currStartTimeMs = this.pickedDateTimeIntervals[nextIntervalIndex].Start;
        this.percentage = this.timeMarkToPercent(this.currStartTimeMs);
        this.isPlaying = true;
      }
      else {
        this.currStartTimeMs = 0;
        this.isPlaying = false;
      }
    }
  }
  onPlayPauseButtonClick(): void {
    this.isPlaying = !this.isPlaying;
    if (this.isPlaying) {
      if (this.pausedTimeMarkMs == 0) {
        this.pausedTimeMarkMs = this.pickedDate!.getTime();
      }
      this.currStartTimeMs = this.pausedTimeMarkMs;
      this.percentage = this.timeMarkToPercent(this.currStartTimeMs);
    }
    else {
      if (this.currStartTimeMs == 0) {
        this.pausedTimeMarkMs = this.pickedDate!.getTime();
      }
      else {
        this.pausedTimeMarkMs = this.currStartTimeMs + this.currFrameTimeSinceStartMs;
      }
      this.currStartTimeMs = 0;
    }
  }
  timeMarkToPercent(timeMark: number): string {
    if (this.pickedDate != null) {
      let percentNum: number = Math.round((timeMark - this.pickedDate.getTime()) / (1000 * 3600 * 24) * 10000) / 100;
      return percentNum + '%';
    }
    else {
      return "0%";
    }
  }
  intervalToPercent(interval: CamTimeInterval): string {
    let percentNum: number = Math.round((interval.End - interval.Start) / (1000 * 3600 * 24) * 10000) / 100;
    return percentNum + '%';
  }
  onVideoPlayProgressUpdate(timeSinceStartMs: number): void {
    if (!this.isDown) {
      this.percentage = this.timeMarkToPercent(this.currStartTimeMs + timeSinceStartMs);
    }
    this.currFrameTimeSinceStartMs = timeSinceStartMs;
    this.isPlaying = true;
  }
  onCurrentIntervalFinishChange(lastReceivedFrameTimeSinceStartMs: number): void {
    this.currFrameTimeSinceStartMs = lastReceivedFrameTimeSinceStartMs;
    // find next available interval (skip 500ms)
    let nextInterval = this.pickedDateTimeIntervals.find(int => int.Start > (this.currStartTimeMs + lastReceivedFrameTimeSinceStartMs + 500));
    if (nextInterval == null) {
      this.currStartTimeMs = 0;
      this.isPlaying = false;
    }
    else {
      this.currStartTimeMs = nextInterval.Start;
      this.isPlaying = true;
    }
  }
  getCurrentPlayingTime(): string {
    return this.isPlaying ?
      (new Date(this.currStartTimeMs + this.currFrameTimeSinceStartMs)).toString() :
      this.pausedTimeMarkMs == 0 ?
        (this.pickedDate == null ? "N/A" : this.pickedDate.toString()) :
        (new Date(this.pausedTimeMarkMs)).toString();
  }

}

