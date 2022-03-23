import { Component, Input, OnInit } from '@angular/core';
import { CameraService } from '../camera.service';

@Component({
  selector: 'app-video-screen',
  templateUrl: './video-screen.component.html',
  styleUrls: ['./video-screen.component.css']
})
export class VideoScreenComponent implements OnInit {

  videoUrl: string = "N/A";
  @Input() camId: string = "N/A";
  @Input() startTime: number = -1;

  isPlaying: boolean = true;

  constructor(private cameraServices: CameraService) { }

  ngOnInit(): void {
    if(this.startTime==-1){
      this.videoUrl=this.cameraServices.getStreamingUrl(this.camId);
    }
    else{
      this.videoUrl=this.cameraServices.getPlaybackUrl(this.camId, this.startTime)
    }
    
  }

}
