import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {MatListModule} from '@angular/material/list';
import {MatDividerModule} from '@angular/material/divider';
import {MatIconModule} from '@angular/material/icon';
import {MatButtonModule} from '@angular/material/button';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {DragDropModule} from '@angular/cdk/drag-drop';
import { RouterModule, Routes } from '@angular/router';


import { AppComponent } from './app.component';
import { CameraCardComponent } from './camera-card/camera-card.component';
import { SideMenuComponent } from './side-menu/side-menu.component';
import { CameraCardListComponent } from './camera-card-list/camera-card-list.component';
import { VideoScreenComponent } from './video-screen/video-screen.component';
import { CctvViewComponent } from './cctv-view/cctv-view.component';
import { CamDetailComponent } from './cam-detail/cam-detail.component';
import { GeneralSettingComponent } from './general-setting/general-setting.component';

const appRoutes: Routes = [
  {path: 'card-view', component: CameraCardListComponent},
  {path: '', redirectTo: '/card-view', pathMatch: 'full'},
  {path: 'cctv-view', component: CctvViewComponent},
  {path: 'cam-detail', component: CamDetailComponent},
  {path: 'setting', component: GeneralSettingComponent}
]


@NgModule({
  declarations: [
    AppComponent,
    CameraCardComponent,
    SideMenuComponent,
    CameraCardListComponent,
    VideoScreenComponent,
    CctvViewComponent,
    CamDetailComponent,
    GeneralSettingComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatListModule,
    MatDividerModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    DragDropModule,
    RouterModule.forRoot(appRoutes)
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
