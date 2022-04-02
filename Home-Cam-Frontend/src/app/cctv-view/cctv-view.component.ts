import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, OnInit } from '@angular/core';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { Subscription } from 'rxjs';
import { CameraService } from '../camera.service';
import { SideBarSelectionService } from '../side-bar-selection.service';
import { UiService } from '../ui.service';

@Component({
  selector: 'app-cctv-view',
  templateUrl: './cctv-view.component.html',
  styleUrls: ['./cctv-view.component.css']
})
export class CctvViewComponent implements OnInit {

  completeCamIdList: string[] = [];

  camIdList: string[] = [];
  videoScreensPerRow: number = this.uiService.cctvVideoScreensPerRowSubject.value;

  showInactiveCam: boolean = this.uiService.showInactiveCamSubject.value;

  showInactiveCamSubscription: Subscription;
  videoScreensPerRowSubscription: Subscription;

  private localCamListSubscription: Subscription;

  constructor(private sideBarSelectionServices: SideBarSelectionService, private cameraServices: CameraService, private uiService: UiService) {
    this.localCamListSubscription = this.cameraServices.onLocalCamListUpdate().subscribe(
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
    this.showInactiveCamSubscription = this.uiService.onShowInactiveCamChange().subscribe(
      newVal => {
        this.showInactiveCam = newVal;
      }
    );
    this.videoScreensPerRowSubscription = this.uiService.onCctvVideoScreensPerRowChange().subscribe(
      newVal => {
        this.videoScreensPerRow = newVal;
      }
    )
  }

  ngOnInit(): void {
  }

  onSelectItemsPerRow(num: number): void {
    this.uiService.setCctvVideoScreensPerRow(num);
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
