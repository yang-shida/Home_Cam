import { Component, Input, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { CameraService } from '../camera.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-video-screen',
  templateUrl: './video-screen.component.html',
  styleUrls: ['./video-screen.component.css']
})
export class VideoScreenComponent implements OnInit {

  @Input() camId: string = "N/A";
  @Input() startTime: number = -1;
  currFrame: string = "";
  videoUrl: SafeUrl = "";

  isPlaying: boolean = true;

  videoFrameSubscription?: Subscription;

  constructor(private cameraServices: CameraService, private domSanitizer: DomSanitizer) { 
    
  }

  ngOnInit(): void {
    this.videoFrameSubscription = this.cameraServices.connentVideo(this.camId, this.startTime==-1?null: this.startTime).subscribe(
      imgBase64 => {
        this.videoUrl = this.domSanitizer.bypassSecurityTrustUrl(`data:image/jpeg;base64, ${imgBase64}`);
      }
    );
  }

  ngOnDestroy(): void{
    this.videoFrameSubscription==null?"":this.videoFrameSubscription.unsubscribe();
  }

}
