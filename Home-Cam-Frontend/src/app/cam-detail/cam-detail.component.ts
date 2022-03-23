import { Component, Input, OnInit } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { SideBarSelectionService } from '../side-bar-selection.service';

@Component({
  selector: 'app-cam-detail',
  templateUrl: './cam-detail.component.html',
  styleUrls: ['./cam-detail.component.css']
})
export class CamDetailComponent implements OnInit {

  camId: string = "N/A";
  currCamIdSubscription: Subscription;

  constructor(private sideBarSelectionServices: SideBarSelectionService) {
    
    this.currCamIdSubscription = this.sideBarSelectionServices.onSelectedCamUpdate().subscribe(
      camId => {
        this.camId=camId;
      }
    )
  }

  ngOnInit(): void {
  }

}
