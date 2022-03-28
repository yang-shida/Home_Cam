import { Component, Input, OnInit } from '@angular/core';
import { MatSliderChange } from '@angular/material/slider';
import { Subject, Subscription } from 'rxjs';
import { SideBarSelectionService } from '../side-bar-selection.service';
import { UiService } from '../ui.service';

@Component({
  selector: 'app-cam-detail',
  templateUrl: './cam-detail.component.html',
  styleUrls: ['./cam-detail.component.css']
})
export class CamDetailComponent implements OnInit {
  // CSS Variables --------------------------
  videoWidth: number = this.uiService.camDetailPageVideoWidthSubject.value;
  videoWidthSubscription: Subscription;
  // End CSS Variables ----------------------

  camId: string = "N/A";
  currCamIdSubscription: Subscription;

  constructor(private sideBarSelectionServices: SideBarSelectionService, private uiService: UiService) {
    this.currCamIdSubscription = this.sideBarSelectionServices.onSelectedCamUpdate().subscribe(
      camId => {
        this.camId = camId;
      }
    );
    this.videoWidthSubscription = this.uiService.onCamDetailPageVideoWidthChange().subscribe(
      newWidth => {
        this.videoWidth = newWidth;
      }
    );
  }

  ngOnInit(): void {
  }

  videoWidthSliderDisplayFormat(num: number): string {
    return num+"%";
  }
  videoWidthSliderChange(sliderChangeEvent: MatSliderChange): void {
    this.uiService.setCamDetailPageVideoWidth(sliderChangeEvent.value==null?this.videoWidth:sliderChangeEvent.value);
  }

}
