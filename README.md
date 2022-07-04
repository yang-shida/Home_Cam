# Home_Cam
Use ASP.NET and Angular to build a web app that uses ESP32-CAM as home cameras
## File structure
- CameraWebServer: the modified firmware for the ESP-32CAM
- Home-Cam-Frontend: the frontend written in Angular
- Home_Cam_Backend: the backend written in ASP.NET
## Backend main functions
- Search for available ESP32-CAM in the Wi-Fi network
- Configure available ESP32-CAM settings based on stored info in the database
- Continuously fetch video feed from all cameras, store frames on disk, and log frame data in database
- Forward live stream to frontend when requested
- Fetch stored frames and send to frontend when playback is requested
## Frontend demo screenshots
- Note: the cameras images are missing because I don't have the cameras with me now
- Camera List: a card view showing each camera's preview image and its basic information
![Camera List](https://github.com/yang-shida/Home_Cam/blob/main/pictures/cam%20cards.PNG)
- CCTV View: all cameras' live stream are played simultaneously on the same screen
![CCTV View](https://github.com/yang-shida/Home_Cam/blob/main/pictures/CCTV.PNG)
- Camera Detail - Live: live stream of the selected camera
![Camera Detail - Live](https://github.com/yang-shida/Home_Cam/blob/main/pictures/live.PNG)
- Camera Detail - Playback: playback of the selected camera (available time marked in cyan)
![Camera Detail - Playback](https://github.com/yang-shida/Home_Cam/blob/main/pictures/playback.PNG)
- Camera Detail - Setting: each camera's setting
![Camera Detail - Setting](https://github.com/yang-shida/Home_Cam/blob/main/pictures/cam%20setting.PNG)
- Backend Control: shows backend settings and logs
![Backend Control](https://github.com/yang-shida/Home_Cam/blob/main/pictures/backend%20control.PNG)
