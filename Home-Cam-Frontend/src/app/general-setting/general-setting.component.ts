import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, Subscription } from 'rxjs';
import { AskForRebootDialogComponent } from '../ask-for-reboot-dialog/ask-for-reboot-dialog.component';
import { BackendSettingService } from '../backend-setting.service';
import { EnterPwdDialogComponent } from '../enter-pwd-dialog/enter-pwd-dialog.component';
import { SystemSetting } from '../objects/SystemSetting';

@Component({
  selector: 'app-general-setting',
  templateUrl: './general-setting.component.html',
  styleUrls: ['./general-setting.component.css']
})
export class GeneralSettingComponent implements OnInit {

  updateSettingForm = new FormGroup(
    {
      MaxSpaceGBs: new FormControl('', [Validators.required, Validators.pattern("^[0-9]*$"), Validators.max(50), Validators.min(1)]),
      PercentToDeleteWhenFull: new FormControl('', [Validators.required, Validators.pattern("^[0-9]*$"), Validators.max(90), Validators.min(5)]),
      SearchCamerasMinutes: new FormControl('', [Validators.required, Validators.pattern("^[0-9]*$"), Validators.max(60), Validators.min(1)]),
      ImageStorageSizeControlMinutes: new FormControl('', [Validators.required, Validators.pattern("^[0-9]*$"), Validators.max(30), Validators.min(1)]),
    }
  )

  currBackendSetting: SystemSetting = this.backendSettingService.systemSetting;

  settingFormIsOpen: boolean = this.backendSettingService.settingFormIsOpen;
  logIsOpen: boolean = this.backendSettingService.logIsOpen;
  submissionLoading: boolean = false;
  pwdDialogIsOpen: boolean = false;

  backendLogSubscription: Subscription;
  backendLog: string = this.backendSettingService.backendLog;

  constructor(private backendSettingService: BackendSettingService, public dialog: MatDialog, private _snackBar: MatSnackBar) {
    this.backendLogSubscription = this.backendSettingService.onBackendLogChange().subscribe(
      log => {
        this.backendLog = log;
      }
    )
  }

  ngOnInit(): void {
    // this.backendSettingService.rebootBackend("41764585").subscribe();
    this.backendSettingService.getBackendSettings().subscribe(
      res => {
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
    this.getPwdInput().subscribe(
      pwd => {
        if (pwd !== undefined) {
          let checkPwdObservable = this.backendSettingService.checkPwd(pwd);
          this.submissionLoading = true;
          checkPwdObservable.subscribe(
            pwdIsRight => {
              this.submissionLoading = false;
              if (pwdIsRight) {
                let updateSettingObservable = this.backendSettingService.updateBackendSettings(this.updateSettingForm.value);
                this.submissionLoading = true;
                updateSettingObservable.subscribe(
                  returnedSettings => {
                    this.submissionLoading = false;
                    if (returnedSettings.MaxSpaceGBs == -1) {
                      this._snackBar.open('Something is wrong. Unable to update settings!', undefined, { duration: 2000 });
                    }
                    else {
                      this._snackBar.open('Settings updated!', undefined, { duration: 2000 });
                      this.dialog.open(AskForRebootDialogComponent).afterClosed().subscribe(
                        needToReboot => {
                          if (needToReboot) {
                            let rebootObservable = this.backendSettingService.rebootBackend(pwd);
                            this.submissionLoading = true;
                            rebootObservable.subscribe(
                              rebootIsSuccessful => {
                                if (rebootIsSuccessful == true) {
                                  this._snackBar.open('Backend Rebooted!', undefined, { duration: 2000 });
                                }
                                else {
                                  this._snackBar.open('Wrong password. Backend failed to reboot!', undefined, { duration: 2000 });
                                }
                                this.submissionLoading = false;
                              }
                            )
                          }
                        }
                      )
                    }
                  }
                )
              }
              else {
                this._snackBar.open('Wrong password. Unable to update settings!', undefined, { duration: 2000 });
              }

            }
          )
        }

      }
    )
  }

  toggleSettingForm(): void {
    this.backendSettingService.toggleSettingForm();
    this.settingFormIsOpen = this.backendSettingService.settingFormIsOpen;
  }

  toggleLogArea(): void {
    this.backendSettingService.toggleLogArea();
    this.logIsOpen = this.backendSettingService.logIsOpen;
  }

  getPwdInput(): Observable<string> {
    this.pwdDialogIsOpen = true;
    const dialogRef = this.dialog.open(EnterPwdDialogComponent);
    return dialogRef.afterClosed();
  }

  onRestartBackend(): void {
    this.getPwdInput().subscribe(
      pwd => {
        if (pwd !== undefined) {
          let rebootObservable = this.backendSettingService.rebootBackend(pwd);
          this.submissionLoading = true;
          rebootObservable.subscribe(
            rebootIsSuccessful => {
              if (rebootIsSuccessful == true) {
                this._snackBar.open('Backend Rebooted!', undefined, { duration: 2000 });
              }
              else {
                this._snackBar.open('Wrong password. Backend failed to reboot!', undefined, { duration: 2000 });
              }
              this.submissionLoading = false;
            }
          )

        }
      }
    )
  }

}
