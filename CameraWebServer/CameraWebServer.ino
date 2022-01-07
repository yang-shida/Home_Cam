#include <WiFi.h>
#include "esp_camera.h"

#include <HTTPClient.h>
#include <Arduino_JSON.h>

// MOD
// WARNING!!! Make sure that you have either selected ESP32 Wrover Module,
//            or another board which has PSRAM enabled
//

// Select camera model
//#define CAMERA_MODEL_WROVER_KIT
//#define CAMERA_MODEL_ESP_EYE
//#define CAMERA_MODEL_M5STACK_PSRAM
//#define CAMERA_MODEL_M5STACK_WIDE
#define CAMERA_MODEL_AI_THINKER

#include "camera_pins.h"

#include "Int64String.h"

const char* ssid = "TP-LINK_2C56";
const char* password = "cici1616";

const char* server_ip = "192.168.1.106";
const char* server_port = "8080";

//const char* ssid = "YANG";
//const char* password = "417645885";
//
//const char* server_ip = "192.168.50.85";
//const char* server_port = "8080";

uint8_t dis_count = 0;

void startCameraServer();

void setup() {
  Serial.begin(115200);
  
  Serial.setDebugOutput(true);
  Serial.println();

  camera_config_t config;
  config.ledc_channel = LEDC_CHANNEL_0;
  config.ledc_timer = LEDC_TIMER_0;
  config.pin_d0 = Y2_GPIO_NUM;
  config.pin_d1 = Y3_GPIO_NUM;
  config.pin_d2 = Y4_GPIO_NUM;
  config.pin_d3 = Y5_GPIO_NUM;
  config.pin_d4 = Y6_GPIO_NUM;
  config.pin_d5 = Y7_GPIO_NUM;
  config.pin_d6 = Y8_GPIO_NUM;
  config.pin_d7 = Y9_GPIO_NUM;
  config.pin_xclk = XCLK_GPIO_NUM;
  config.pin_pclk = PCLK_GPIO_NUM;
  config.pin_vsync = VSYNC_GPIO_NUM;
  config.pin_href = HREF_GPIO_NUM;
  config.pin_sscb_sda = SIOD_GPIO_NUM;
  config.pin_sscb_scl = SIOC_GPIO_NUM;
  config.pin_pwdn = PWDN_GPIO_NUM;
  config.pin_reset = RESET_GPIO_NUM;
  config.xclk_freq_hz = 20000000;
  config.pixel_format = PIXFORMAT_JPEG;
  //init with high specs to pre-allocate larger buffers
  if(psramFound()){
    config.frame_size = FRAMESIZE_UXGA;
    config.jpeg_quality = 10;
    config.fb_count = 2;
  } else {
    config.frame_size = FRAMESIZE_SVGA;
    config.jpeg_quality = 12;
    config.fb_count = 1;
  }

#if defined(CAMERA_MODEL_ESP_EYE)
  pinMode(13, INPUT_PULLUP);
  pinMode(14, INPUT_PULLUP);
#endif

  // camera init
  esp_err_t err = esp_camera_init(&config);
  if (err != ESP_OK) {
    Serial.printf("Camera init failed with error 0x%x", err);
    return;
  }

  sensor_t * s = esp_camera_sensor_get();
  //initial sensors are flipped vertically and colors are a bit saturated
  s->set_vflip(s, 1);//flip it back
  s->set_hmirror(s, 1);

  //drop down frame size for higher initial frame rate
  s->set_framesize(s, FRAMESIZE_XGA);


//umcomment for static IP
//  IPAddress ip(192, 168, 1, 199);
//  IPAddress gateway(192, 168, 1, 1);
//  IPAddress subnet(255, 255, 255, 0);
//  IPAddress dnsAdrr(8, 8, 8, 8);
//  Serial.println(F("Wifi config..."));
//  WiFi.config(ip, gateway, subnet, dnsAdrr);

//Comment out for static IP
//  WiFi.config(INADDR_NONE, INADDR_NONE, INADDR_NONE); // helps set host name
  
//  WiFi.setHostname("ESP32C_STREET");
  
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.print(".");
    dis_count++;
    if(dis_count>60)
    {
      Serial.println();
      Serial.println("Restarting ESP");
      delay(1000);
      ESP.restart();
    }
  }
  
  Serial.println("");
  Serial.println("WiFi connected");

  startCameraServer();

  Serial.print("Camera Ready! Use 'http://");
  Serial.print(WiFi.localIP());
  Serial.println("' to connect");

  // get initial setting
  char macStr[18];
  byte array[6];
  WiFi.macAddress(array);
  snprintf(macStr, sizeof(macStr), "%02x:%02x:%02x:%02x:%02x:%02x", array[0], array[1], array[2], array[3], array[4], array[5]);

  WiFiClient client;
  HTTPClient http;
  
  String get_setting_string="http://"+String(server_ip)+":"+String(server_port)+"/api/camSettings/"+String(macStr)+"?ipAddr="+WiFi.localIP().toString()+"?camTime="+int64String(esp_timer_get_time(), DEC, false);
  http.begin(client, get_setting_string);

  int httpResponseCode = http.GET();
  
  String payload = "{}";
  if (httpResponseCode>0) {
    payload = http.getString();
    JSONVar myObject = JSON.parse(payload);
    if (JSON.typeof(myObject) != "undefined") {
      JSONVar keys = myObject.keys();
      for (int i = 0; i < keys.length(); i++) {
        JSONVar value = myObject[keys[i]];
        sensor_t * s = esp_camera_sensor_get();
        String key_string = JSONVar::stringify(keys[i]);
        if(key_string==String("\"frameSize\""))
        {
          s->set_framesize(s, (framesize_t)int(value));
        }
        else if(key_string==String("\"flashLightOn\""))
        {
          #define LED_BUILTIN 4
          pinMode(LED_BUILTIN, OUTPUT);
          digitalWrite(LED_BUILTIN, bool(value)?1:0);
        }
        else if(key_string==String("\"horizontalMirror\""))
        {
          s->set_hmirror(s, bool(value));
        }
        else if(key_string==String("\"verticalMirror\""))
        {
          s->set_vflip(s, bool(value));
        }
      }
    }
  }
  // unable to reach server, use default setting
  else {
    sensor_t * s = esp_camera_sensor_get();
    s->set_framesize(s, (framesize_t)6);
    #define LED_BUILTIN 4
    pinMode(LED_BUILTIN, OUTPUT);
    digitalWrite(LED_BUILTIN, 0);
    s->set_hmirror(s, false);
    s->set_vflip(s, false);
  }

  http.end();

  
  
}

void loop() {
  //Reboot ESP32 if Wi-fi connection is lost (fixes some issues)
  delay(1000);
  if(WiFi.status() != WL_CONNECTED)
  {
    dis_count++;
    Serial.println("Not Connected to Wi-Fi | " + String(dis_count));
  }
  else
  {
    dis_count=0;
  }

  if(dis_count>60)
  {
    Serial.println("Restarting ESP");
    delay(1000);
    ESP.restart();
  }
}
