import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { CameraService } from '../camera.service';
import { SideBarSelectionService } from '../side-bar-selection.service';
import { Router } from '@angular/router';
import { Location } from '@angular/common';


@Component({
  selector: 'app-side-menu',
  templateUrl: './side-menu.component.html',
  styleUrls: ['./side-menu.component.css']
})
export class SideMenuComponent implements OnInit {

  menuItems: string[] = ["Camera Cards", "CCTV Style", "Cameras", "Settings"];
  camIdList: string[] = []
  showingCamIdList: boolean = false;
  selectedMenuItem?: string;
  selectedCamId?: string;

  private localCamListSubscription: Subscription;
  private selectedMenuItemSubscription: Subscription;
  private selectedCameraSubscription: Subscription;

  constructor(private cameraServices: CameraService,
    private sideBarSelectionServices: SideBarSelectionService,
    private router: Router,
    private location: Location
  ) {
    this.selectBasedOnUrl(this.location.path());
    this.localCamListSubscription = this.cameraServices.onLocalCamListUpdate().subscribe(
      camList => {
        this.camIdList = camList.map(
          value => value.uniqueId
        )
      }
    );
    this.selectedMenuItemSubscription = this.sideBarSelectionServices.onSelectedMenuItemUpdate().subscribe(
      item => {
        this.selectedMenuItem = item;
      }
    )
    this.selectedCameraSubscription = this.sideBarSelectionServices.onSelectedCamUpdate().subscribe(
      camId => {
        this.selectedCamId = camId;
      }
    )
  }

  ngOnInit(): void {
  }

  toggleShowCamIdList(): void {
    this.showingCamIdList = !this.showingCamIdList;
  }

  onClickMenuItem(menuItem: string): void {
    this.sideBarSelectionServices.selectMenuItem(menuItem);

    if (menuItem === 'Cameras') {
      this.toggleShowCamIdList();
    }
    else {
      if (this.showingCamIdList) {
        this.toggleShowCamIdList();
      }
    }
  }

  onClickCamId(camId: string): void {
    this.sideBarSelectionServices.selectCamera(camId);
  }

  menuItemToUrl(menuItem: string): string {
    switch (menuItem) {
      case "Camera Cards":
        return "card-view";
      case "CCTV Style":
        return "cctv-view";
      case "Settings":
        return "setting";
      case "Cameras":
        return this.selectedCamId == null ? this.location.path() : 'cam-detail';
      default:
        return this.location.path();
    }
  }

  selectBasedOnUrl(url: string): void {
    switch (url) {
      case "/card-view":
        this.selectedMenuItem = "Camera Cards";
        break;
      case "/cctv-view":
        this.selectedMenuItem = "CCTV Style";
        break;
      case "/setting":
        this.selectedMenuItem = "Settings";
        break;
      case "/cam-detail":
        this.selectedMenuItem = "Cameras";
        break;
      default:
        console.log("selectBasedOnUrl: Unknown URL: "+url)
        this.selectedMenuItem = this.menuItems[0];
        break;
    }
  }

}
