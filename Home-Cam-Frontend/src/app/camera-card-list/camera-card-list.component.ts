import { Component, OnInit } from '@angular/core';
import { CameraService } from '../camera.service';

@Component({
  selector: 'app-camera-card-list',
  templateUrl: './camera-card-list.component.html',
  styleUrls: ['./camera-card-list.component.css']
})
export class CameraCardListComponent implements OnInit {

  camIdList: string[] = [];
  showRefreshSpinner: boolean = false;

  constructor(private cameraServices: CameraService) { }

  ngOnInit(): void {
    this.cameraServices.getActiveCameras().subscribe(
      camInfoList => {
        if(camInfoList.length==1 && camInfoList[0].IpAddr=='N/A'){
          this.camIdList=[];
        }
        else{
          this.camIdList=camInfoList.map(
            camInfo=>{
              return camInfo.UniqueId;
            }
          )
        }
      }
    );
  }

  onRefresh(): void{
    this.showRefreshSpinner=true;
    this.cameraServices.refreshCameraList().subscribe(
      camInfoList => {
        this.showRefreshSpinner=false;
        if(camInfoList.length==1 && camInfoList[0].IpAddr=='N/A'){
          this.camIdList=[];
        }
        else{
          this.camIdList=camInfoList.map(
            camInfo=>{
              return camInfo.UniqueId;
            }
          )
        }
      }
    );
  }

}
