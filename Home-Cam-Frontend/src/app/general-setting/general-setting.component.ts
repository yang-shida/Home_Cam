import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { BackendSettingService } from '../backend-setting.service';
import { SystemSetting } from '../objects/SystemSetting';

@Component({
  selector: 'app-general-setting',
  templateUrl: './general-setting.component.html',
  styleUrls: ['./general-setting.component.css']
})
export class GeneralSettingComponent implements OnInit {

  updateSettingForm = new FormGroup(
    {
      MaxSpaceGBs: new FormControl('', [Validators.required, Validators.pattern("^[0-9]*$"),Validators.max(50), Validators.min(1)]),
      PercentToDeleteWhenFull: new FormControl('', [Validators.required, Validators.pattern("^[0-9]*$"),Validators.max(90), Validators.min(5)]),
      SearchCamerasMinutes: new FormControl('', [Validators.required, Validators.pattern("^[0-9]*$"),Validators.max(60), Validators.min(1)]),
      ImageStorageSizeControlMinutes: new FormControl('', [Validators.required, Validators.pattern("^[0-9]*$"),Validators.max(30), Validators.min(1)]),
    }
  )

  currBackendSetting: SystemSetting = this.backendSettingService.systemSetting;

  settingFormIsOpen: boolean = this.backendSettingService.settingFormIsOpen;
  logIsOpen: boolean = this.backendSettingService.logIsOpen;
  submissionLoading: boolean = false;

  constructor(private backendSettingService: BackendSettingService) {
  }

  ngOnInit(): void {
    // this.backendSettingService.rebootBackend("41764585").subscribe();
    this.backendSettingService.getBackendSettings().subscribe(
      res=>{
        this.currBackendSetting = res;
        this.updateSettingForm.setValue(this.currBackendSetting);
      }
    )
    // this.backendSettingService.updateBackendSettings({
    //   MaxSpaceGBs: 15,
    //   PercentToDeleteWhenFull: 10,
    //   SearchCamerasMinutes: 15,
    //   ImageStorageSizeControlMinutes: 15
    // }).subscribe()
  }

  onUpdateSetting(): void {
    console.log(this.updateSettingForm.value);
  }

  toggleSettingForm(): void{
    this.backendSettingService.toggleSettingForm();
    this.settingFormIsOpen = this.backendSettingService.settingFormIsOpen;
  }

  toggleLogArea(): void{
    this.backendSettingService.toggleLogArea();
    this.logIsOpen = this.backendSettingService.logIsOpen;
  }

}
