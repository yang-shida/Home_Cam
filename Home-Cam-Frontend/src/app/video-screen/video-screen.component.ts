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
  @Output() streamFinished: EventEmitter<boolean> = new EventEmitter();
  videoUrl: SafeUrl = "";

  isPlaying: boolean = true;

  videoFrameSubscription?: Subscription;
  camListSubscription?: Subscription;

  constructor(private cameraServices: CameraService, private domSanitizer: DomSanitizer) {
    this.camListSubscription = this.cameraServices.onLocalCamListUpdate().subscribe(
      newCamList => {
        if (this.videoFrameSubscription==null && this.cameraServices.isCamActive(this.camId)) {
          this.videoFrameSubscription = this.cameraServices.connentVideo(this.camId, this.startTime == -1 ? null : this.startTime).subscribe(
            data => {
              this.frameProcessingFunction(data)
            }
          );
        }
        else {
          this.videoUrl = this.domSanitizer.bypassSecurityTrustUrl("../../assets/cam_not_active.jpg");
        }
      }
    )
  }

  ngOnInit(): void {
  }

  ngOnChanges(): void {
    // console.log("change", this.camId, this.startTime, this.videoFrameSubscription == null)
    this.videoFrameSubscription == null ? "" : this.videoFrameSubscription.unsubscribe();
    if (this.cameraServices.isCamActive(this.camId) || this.startTime != -1) {
      this.videoFrameSubscription = this.cameraServices.connentVideo(this.camId, this.startTime == -1 ? null : this.startTime).subscribe(
        data => {
          this.frameProcessingFunction(data)
        }
      );
    }
    else {
      this.videoUrl = this.domSanitizer.bypassSecurityTrustUrl("../../assets/cam_not_active.jpg");
    }

  }

  frameProcessingFunction = (data: string) => {
    if(data=="Stream Finished!"){
      this.videoFrameSubscription == null ? "" : this.videoFrameSubscription.unsubscribe();
      this.streamFinished==null?"":this.streamFinished.emit(true);
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
    }
  }

  ngOnDestroy(): void {
    // console.log("destroy", this.camId, this.startTime, this.videoFrameSubscription == null)
    this.videoFrameSubscription == null ? "" : this.videoFrameSubscription.unsubscribe();
  }

}
