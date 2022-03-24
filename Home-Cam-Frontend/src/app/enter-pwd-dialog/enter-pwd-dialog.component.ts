import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-enter-pwd-dialog',
  templateUrl: './enter-pwd-dialog.component.html',
  styleUrls: ['./enter-pwd-dialog.component.css']
})
export class EnterPwdDialogComponent implements OnInit {

  pwd: string = "";

  constructor(public dialogRef: MatDialogRef<EnterPwdDialogComponent>) { }

  ngOnInit(): void {
  }

}
