import { Component, OnInit } from '@angular/core';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { Subscription } from 'rxjs';
import { CameraService } from '../camera.service';
import { SideBarSelectionService } from '../side-bar-selection.service';
import { UiService } from '../ui.service';

@Component({
  selector: 'app-camera-card-list',
  templateUrl: './camera-card-list.component.html',
  styleUrls: ['./camera-card-list.component.css']
})
export class CameraCardListComponent implements OnInit {

  completeCamIdList: string[] = [];

  camIdList: string[] = [];
  showRefreshSpinner: boolean = false;
  showInactiveCam: boolean = this.uiService.showInactiveCamSubject.value;

  showInactiveCamSubscription: Subscription;

  constructor(private cameraServices: CameraService, private sideBarSelectionServices: SideBarSelectionService, private uiService: UiService) {
    this.showInactiveCamSubscription = this.uiService.onShowInactiveCamChange().subscribe(
      newVal => {
        this.showInactiveCam = newVal;
      }
    )
  }

  ngOnInit(): void {
    this.cameraServices.getActiveCameras().subscribe(
      camInfoList => {
        if (camInfoList.length == 1 && camInfoList[0].IpAddr == 'N/A') {
          this.completeCamIdList = [];
          this.camIdList = [];
        }
        else {
          this.completeCamIdList = camInfoList.map(
            camInfo => {
              return camInfo.UniqueId;
            }
          )
          if (!this.showInactiveCam) {
            this.camIdList = this.completeCamIdList.filter(camId => this.cameraServices.isCamActive(camId));
          }
          else {
            this.camIdList = this.completeCamIdList;
          }
        }
      }
    );
  }

  onRefresh(): void {
    this.showRefreshSpinner = true;
    this.cameraServices.refreshCameraList().subscribe(
      camInfoList => {
        this.showRefreshSpinner = false;
        if (camInfoList.length == 1 && camInfoList[0].IpAddr == 'N/A') {
          this.completeCamIdList = [];
          this.camIdList = [];
        }
        else {
          this.completeCamIdList = camInfoList.map(
            camInfo => {
              return camInfo.UniqueId;
            }
          )
          if (!this.showInactiveCam) {
            this.camIdList = this.completeCamIdList.filter(camId => this.cameraServices.isCamActive(camId));
          }
          else {
            this.camIdList = this.completeCamIdList;
          }
        }
      }
    );
  }

  onShowInactiveCamSliderChange(toggleChangeEvent: MatSlideToggleChange): void {
    this.uiService.setShowInactiveCam(toggleChangeEvent.checked);
    if (!toggleChangeEvent.checked) {
      this.camIdList = this.completeCamIdList.filter(camId => this.cameraServices.isCamActive(camId));
    }
    else {
      this.camIdList = this.completeCamIdList;
    }
  }

  onClickCamCard(camId: string): void {
    this.sideBarSelectionServices.selectMenuItem('Cameras');
    this.sideBarSelectionServices.selectCamera(camId);
  }

}
