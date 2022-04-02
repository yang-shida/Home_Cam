import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Subscription } from 'rxjs';
import { CameraService } from '../camera.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ControlledFrame } from '../objects/ControlledFrame';
import { ContentObserver } from '@angular/cdk/observers';

@Component({
  selector: 'app-video-screen',
  templateUrl: './video-screen.component.html',
  styleUrls: ['./video-screen.component.css']
})
export class VideoScreenComponent implements OnInit {

  @Input() camId: string = "N/A";
  @Input() startTime: number = -1;
  @Output() currFrameTimeSinceStartMs: EventEmitter<number> = new EventEmitter();
  @Output() streamFinished: EventEmitter<number> = new EventEmitter();
  videoUrl: SafeUrl = "";

  videoFrameSubscription?: Subscription;
  camListSubscription?: Subscription;

  isPlaying: boolean = true;

  lastReceivedFrameTimeSinceStartMs: number = 0;

  constructor(private cameraServices: CameraService, private domSanitizer: DomSanitizer) {
    this.camListSubscription = this.cameraServices.onLocalCamListUpdate().subscribe(
      newCamList => {
        if (this.startTime != 0 && this.videoFrameSubscription==null && this.cameraServices.isCamActive(this.camId)) {
          this.videoFrameSubscription = this.cameraServices.connentVideo(this.camId, this.startTime == -1 ? null : this.startTime).subscribe(
            data => {
              this.frameProcessingFunction(data)
            }
          );
        }
        else {
          this.videoUrl = this.startTime==0?this.domSanitizer.bypassSecurityTrustUrl("../../assets/missing_image.jpg"):this.domSanitizer.bypassSecurityTrustUrl("../../assets/cam_not_active.jpg");
        }
      }
    )
  }

  ngOnInit(): void {
  }

  ngOnChanges(): void {
    // console.log("change", this.camId, this.startTime, this.videoFrameSubscription == null)
    this.videoFrameSubscription == null ? "" : this.videoFrameSubscription.unsubscribe();
    if (this.startTime != 0 && (this.cameraServices.isCamActive(this.camId) || this.startTime != -1)) {
      this.videoFrameSubscription = this.cameraServices.connentVideo(this.camId, this.startTime == -1 ? null : this.startTime).subscribe(
        data => {
          this.frameProcessingFunction(data)
        }
      );
    }
    else {
      this.videoUrl = this.startTime==0?this.domSanitizer.bypassSecurityTrustUrl("../../assets/missing_image.jpg"):this.domSanitizer.bypassSecurityTrustUrl("../../assets/cam_not_active.jpg");
    }

  }

  frameProcessingFunction = (data: string) => {
    if(data=="Stream Finished!"){
      this.videoFrameSubscription == null ? "" : this.videoFrameSubscription.unsubscribe();
      this.streamFinished==null?"":this.streamFinished.emit(this.lastReceivedFrameTimeSinceStartMs);
      return;
    }
    if(this.startTime==-1)
    {
      this.videoUrl = this.domSanitizer.bypassSecurityTrustUrl(`data:image/jpeg;base64, ${data}`);
    }
    else
    {
      let frame: ControlledFrame = JSON.parse(data)
      this.videoUrl = this.domSanitizer.bypassSecurityTrustUrl(`data:image/jpeg;base64, ${frame.ImageBase64Str}`);
      this.currFrameTimeSinceStartMs.emit(frame.TimeSinceStartMs);
      this.lastReceivedFrameTimeSinceStartMs = frame.TimeSinceStartMs;
    }
  }

  ngOnDestroy(): void {
    // console.log("destroy", this.camId, this.startTime, this.videoFrameSubscription == null)
    this.videoFrameSubscription == null ? "" : this.videoFrameSubscription.unsubscribe();
  }

}
