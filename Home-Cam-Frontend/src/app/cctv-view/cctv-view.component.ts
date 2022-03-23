import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { CameraService } from '../camera.service';

@Component({
  selector: 'app-cctv-view',
  templateUrl: './cctv-view.component.html',
  styleUrls: ['./cctv-view.component.css']
})
export class CctvViewComponent implements OnInit {

  camIdList: string[] = []
  videoScreensPerRow: number = 3;

  private localCamListSubscription: Subscription;

  constructor(private cameraServices: CameraService) {
    this.localCamListSubscription = this.cameraServices.onLocalCamListUpdate().subscribe(
      camList=>{
        this.camIdList=camList.map(
          camInfo=>{
            return camInfo.uniqueId;
          }
        )
      }
    );
  }

  ngOnInit(): void {
  }

  onSelectItemsPerRow(num: number): void {
    this.videoScreensPerRow=num;
  }

  ngOnDestroy(): void{
    window.stop();
  }

}
