import { Component, OnInit } from '@angular/core';
import { CameraService } from '../camera.service';

@Component({
  selector: 'app-video-screen',
  templateUrl: './video-screen.component.html',
  styleUrls: ['./video-screen.component.css']
})
export class VideoScreenComponent implements OnInit {

  videoUrl: string = "N/A";

  constructor(private cameraServices: CameraService) { }

  ngOnInit(): void {
  }

}
