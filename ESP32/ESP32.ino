/// https://github.com/knolleary/pubsubclient/archive/master.zip ricorda di controlla anche qua
#include <SPI.h>
#include <ArduinoJson.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <Wire.h>
#include <U8g2lib.h>
#include <WebServer.h>
#include <EEPROM.h>
#include "Structures.h"
//#include "WebServer.h"
#include <Preferences.h>
Preferences preferences;

char *ssid;
char *password;

char *mqtt_server = "192.168.178.40";

WiFiClient espClient;
PubSubClient client(espClient);
long lastMsg = 0;
long lastRetryMQTT = 0;
long lastRSTPress = 0;
char msg[50];
int value = 0;

bool MQTTSyncPending_delta = true;

char station_id[20];

String mac_address = String(WiFi.macAddress());

U8G2_SSD1306_128X64_NONAME_F_HW_I2C u8g2(U8G2_R0, 16, 15, 4);
WebServer server(80);

anag anagra;
//WebServer server;
TaskHandle_t task_mqtt;
pin gpio_pin_add;

void setup()
{

  u8g2.begin();
  u8g2.setFont(u8g2_font_6x10_tf);
  //u8g2.setFont(u8g2_font_unifont_t_symbols);

  u8g2.setFontRefHeightExtendedText();
  u8g2.setDrawColor(1);
  u8g2.setFontPosTop();
  u8g2.setFontDirection(0);

  u8g2.drawStr(3, 2, "Init.............OK");
  u8g2.sendBuffer();

  Serial.begin(115200);

  int i;

  Serial.println();

  //Inizializzo il pin che gestisce l'incremento del delta
  gpio_pin_add.id = 0;
  gpio_pin_add.current_state = 0;
  gpio_pin_add.last_state = 0;

  preferences.begin("EEPROM", false);

  anagra.id = -505;
  //anagra.address = preferences.getString("anagra_address");
  //anagra.business_name = NULL;
  //anagra.pcount = 0;
  setdelta(0);
  Serial.println("NOT SYNCED : " + preferences.getString("delta"));
  anagra.not_synced_delta = preferences.getString("delta").toInt();

  if (anagra.not_synced_delta < 0)
    setdelta(0);

  Serial.println(anagra.not_synced_delta);
  anagra.pcount_server = -1;
  //anagra.pc = preferences.getString("anagra_pc", anagra.pc);
  //anagra.city = preferences.getString("anagra_city", anagra.city);

  String saved_ssid = preferences.getString("SSID");
  String saved_wpwd = preferences.getString("WPWD");

  Serial.println("Saved SSID = " + saved_ssid);
  Serial.println("Saved WPWD = " + saved_wpwd);

  ssid = (char *)malloc(saved_ssid.length() + 1);
  password = (char *)malloc(saved_wpwd.length() + 1);

  saved_ssid.toCharArray(ssid, saved_ssid.length() + 1);
  saved_wpwd.toCharArray(password, saved_wpwd.length() + 1);

  Serial.println(ssid);
  Serial.println(password);

  u8g2.drawStr(3, 12, "Init prefs........OK");
  u8g2.sendBuffer();

  WiFi.begin(ssid, password);
  u8g2.drawStr(3, 22, "Init wifi........OK");
  u8g2.sendBuffer();

  //setup_wifi();
  client.setServer(mqtt_server, 1883);
  client.setCallback(callback);

  //EEPROM.begin(4096);
  //EEPROM.get(0, anagra); //Carico da memoria

  server.on("/", handle_onConnect);

  server.begin();
  u8g2.drawStr(3, 32, "Init WebServer...OK");
  u8g2.sendBuffer();

  pinMode(0, INPUT_PULLUP);

  xTaskCreatePinnedToCore(
      loop_mqtt,
      "task_mqtt",
      10000,
      NULL,
      0,
      &task_mqtt,
      0);

  delay(1700);
}

void handle_onConnect()
{
  //server.send(200, "text/html", SendHTML());

  int i;

  for (i = 0; i < server.args(); i++)
  {

    switch (server.argName(i).toInt())
    {
    case 1:
      //anagra.business_name = server.arg(i);
      //Serial.println("SET BNAME");
      //preferences.putString("anagra_bname", anagra.business_name);
      break;

    case 2:
      //anagra.pcount = anagra.pcount + server.arg(i).toInt();
      setdelta(anagra.not_synced_delta + server.arg(i).toInt());
      Serial.println("SET PPL " + anagra.not_synced_delta);
      MQTTSyncPending_delta = true;
      break;

    case 3:
      //anagra.address = server.arg(i);
      //Serial.println("SET ADDR");
      //preferences.putString("anagra_address", anagra.address);
      break;

    case 4:
      //anagra.pc = server.arg(i);
      //Serial.println("SET PC");
      //preferences.putString("anagra_pc", anagra.pc);
      break;

    case 5:
      //anagra.city = server.arg(i);
      //Serial.println("SET CITY");
      //preferences.putString("anagra_city", anagra.city);
      break;

    default:
      break;
    }
  }

  // EEPROM.put(0,anagra);
  // EEPROM.commit();

  //server.sendHeader("Location", String("/"), true);

  server.send(200, "text/html", SendHTML()); //Refresh
}

// void setup_wifi()
// {
//   delay(10);
//   Serial.println();
//   Serial.print("Connecting to ");
//   Serial.println(ssid);

//   WiFi.begin(ssid, password);

//   while (WiFi.status() != WL_CONNECTED)
//   {
//     delay(200);
//     Serial.println("Connetting to WiFi...");
//   }

//   Serial.println("");
//   Serial.println("WiFi connected");
//   Serial.println("IP address: ");
//   Serial.println(WiFi.localIP());
// }

void callback(char *topic, byte *message, unsigned int length)
{
  // Serial.println("Message arrived on topic: ");
  // Serial.print(topic);

  String message_string;

  for (int i = 0; i < length; i++)
  {
    message_string += (char)message[i];
  }

  Serial.println(message_string);

  DynamicJsonDocument doc(1024);
  deserializeJson(doc, message_string);

  // if (String(topic) == ("brokr/" + mac_address + "/id_location"))
  // {
  //   anagra.id = doc["ID_Location"];
  //   Serial.println("SET ID");
  //   Serial.println(anagra.id);
  // }

  if (String(topic) == ("brokr/" + mac_address + "/reset_delta"))
  {
    setdelta(0);
    Serial.println("RST DELTA");
    client.publish("esp/get_pcount", "");
  }

  if (String(topic) == ("brokr/" + String(anagra.id) + "/pcount"))
  {
    anagra.pcount_server = doc["People_Count"];
    Serial.println("SET PCOUNT");
    Serial.println(anagra.pcount_server);
  }

  if (String(topic) == ("brokr/" + mac_address + "/anagra"))
  {
    anagra.id = doc["ID_Location"];
    anagra.business_name = doc["Business_Name"].as<String>();
    anagra.address = doc["Address"].as<String>();
    anagra.pc = doc["PostalCode"].as<String>();
    anagra.city = doc["City"].as<String>();
    //anagra.pcount_server = doc["People_Count"];

    client.subscribe(("brokr/" + String(anagra.id) + "/#").c_str()); //Se ho l'id,inizio ad ascoltare i messaggi broadcast

    Serial.println("SET ID");
    Serial.println(anagra.id);
    Serial.println("SET ANAGRA");
    Serial.println(anagra.business_name);
  }
}

void reconnect()
{
  Serial.println("Attempting MQTT connection...");

  if (client.connect(mac_address.c_str(), "Brd", "Errata"))
  {

    Serial.println("connected");

    client.subscribe(("brokr/" + mac_address + "/#").c_str());

    client.publish("esp/get_anagra", "");
  }
  else
  {
  }
}

bool get_pin_change(pin &gpio_pin)
{

    bool changed = false;

  gpio_pin.current_state = digitalRead(gpio_pin.id);

  // Serial.println("Stato :");
  // Serial.print(gpio_pin.current_state);
  // Serial.println("Stato :");
  // Serial.print(gpio_pin.last_state);

  if (gpio_pin.current_state != gpio_pin.last_state)
  {
    if (gpio_pin.current_state == HIGH)
    {
      changed = true;
    }
  }

  gpio_pin.last_state = gpio_pin.current_state;

  return changed;
}

void loop_mqtt(void *parameter)
{
  while (true)
  {
    long now = millis();

    if (!client.connected())
    {
      MQTTSyncPending_delta = true;

      if (now - lastRetryMQTT > 5000) //Ogni 5 secondi riprovo a connettermi al broker
      {
        lastRetryMQTT = now;
        reconnect();
      }
      //u8g2.drawLine(118, 9, 124, 3);
    }
    else
    {
      client.loop();

      if (now - lastMsg > 1000)
      {
        lastMsg = now;

        if (MQTTSyncPending_delta)
          MQTTSyncPending_delta = !UpdateDelta();
      }
    }
  }
}

void loop()
{
  //Serial.println(esp_get_free_heap_size());
  //server.runServer();

  long now = millis();

  u8g2.clearBuffer(); //Pulisco

  //Creo l'intestazione
  // u8g2.drawLine(0, 0, 128, 0);
  // u8g2.drawLine(0, 0, 0, 63);
  // u8g2.drawLine(127, 0, 127, 63);
   u8g2.drawLine(0, 12, 128, 12);
  // u8g2.drawLine(0, 63, 128, 63);

  //Scrivo l'IP
  char ipAddressStr[16];
  sprintf(ipAddressStr, "%03d.%03d.%03d.%03d", WiFi.localIP()[0], WiFi.localIP()[1], WiFi.localIP()[2], WiFi.localIP()[3]);
  u8g2.drawStr(3, 2, ipAddressStr);

  // //Scrivo il messaggio di stato
  String stateMessage = "";

  if (anagra.id < 0)
    stateMessage = "! ATTENZIONE !  - Nessuna associazione"; //da modificare
  else
    stateMessage = anagra.business_name + " - " + anagra.address + "  ";

  int indexStart1Row = (now / 1000) % max((int)stateMessage.length() - 20, 1);
  //Serial.println(indexStart1Row);

  char ragSoc[20];
  stateMessage.toCharArray(ragSoc, 20, indexStart1Row);
  u8g2.drawStr(3, 15, ragSoc);

  //Scrivo il numero delle persone
  char str[20];
  if (anagra.pcount_server >= 0)
    sprintf(str, "Persone : %d", anagra.pcount_server);

  u8g2.drawStr(3, 25, str);

  if (WiFi.status() != WL_CONNECTED)
  {
    u8g2.drawVLine(102, 8, 2);
    u8g2.drawVLine(104, 6, 4);
    u8g2.drawVLine(106, 4, 6);
    u8g2.drawVLine(108, 2, 8);

    u8g2.drawLine(101, 3, 103, 5);
    u8g2.drawLine(103, 3, 101, 5);

    Serial.println("WiFi connection lost");
    //setup_wifi();
  }
  else
  {
    //Scrivo la qualita del segnale e

    long rssi = WiFi.RSSI();
    //Serial.print("RSSI:");
    //Serial.println(rssi);

    if (rssi > -90)
      u8g2.drawVLine(102, 8, 2);

    if (rssi > -70)
      u8g2.drawVLine(104, 6, 4);

    if (rssi > -60)
      u8g2.drawVLine(106, 4, 6);

    if (rssi > -50)
      u8g2.drawVLine(108, 2, 8);

    u8g2.drawStr(3, 46, "Per config. digitare");
    u8g2.drawStr(3, 54, "l'ind.IP sul browser");

    if (anagra.id > 0) //Rispondo ai client dell'interfaccia web solo se l'anagrafica è valida
      server.handleClient();
  }

  //Gestisco il l'I/O
  // if (digitalRead(0) == 0)
  // {
  //   u8g2.drawStr(3, 25, "Rset  Wifi 8'");

  //   if (now - lastRSTPress > 8000)
  //   {
  //     u8g2.drawStr(3, 25, "Reset Wifi...OK");
  //     delay(2000);

  //     //Cancello SSID E PWD
  //   }

  //   if (get_pin_change(gpio_pin_add))
  //   {
  //     setdelta(anagra.not_synced_delta + 1);

  //     lastRSTPress = now;
  //     MQTTSyncPending_delta = true;
  //   }
  // }
  // else
  // {
  //   lastRSTPress = now;

  
  // }

  if (get_pin_change(gpio_pin_add))
  {
    setdelta(anagra.not_synced_delta + 1);

    lastRSTPress = now;
    MQTTSyncPending_delta = true;
  }

    char strsync[20];

      sprintf(strsync, "Da sincronizz. : %d", anagra.not_synced_delta);

    u8g2.drawStr(3, 35, strsync);

  //u8g2.drawGlyph(5, 20, 0x2603);

  u8g2.drawVLine(121, 7, 3);
  u8g2.drawLine(119, 7, 123, 7);
  u8g2.drawVLine(119, 5, 2);
  u8g2.drawVLine(123, 5, 2);
  u8g2.drawVLine(121, 2, 2);

  u8g2.sendBuffer();
}

void setdelta(int value)
{
  anagra.not_synced_delta = value;
  preferences.putString("delta", String(anagra.not_synced_delta));

  Serial.println("SET DELTA ");
  Serial.print(anagra.not_synced_delta);
}

bool UpdateDelta()
{

  if (anagra.id < 0) //Finche non ho un ID valido,non faccio il push del delta al server
    return false;

  bool published = true;

  DynamicJsonDocument doc(1024);

  doc["not_synced_delta"] = anagra.not_synced_delta;

  String serialized;
  serializeJson(doc, serialized);

  published = client.publish("esp/put_delta", serialized.c_str());

  Serial.println("PUSH DELTA");
  Serial.print(published);

  return published;
}

bool UpdateAnagra()
{
  bool published = true;

  Serial.println("Published : ");

  // published = published && client.publish("esp/local_people_delta", String(anagra.not_synced_delta).c_str());
  // published = published && client.publish("esp/anagra_bname", String(anagra.business_name).c_str());
  // published = published && client.publish("esp/anagra_address", String(anagra.address).c_str());
  // published = published && client.publish("esp/anagra_pc", String(anagra.pc).c_str());
  // published = published && client.publish("esp/anagra_city", String(anagra.city).c_str());

  DynamicJsonDocument doc(1024);

  doc["not_synced_delta"] = anagra.not_synced_delta;
  doc["business_name"] = anagra.business_name;
  doc["address"] = anagra.address;

  String serialized;
  serializeJson(doc, serialized);

  published = published && client.publish("esp/jsondata", serialized.c_str());

  Serial.print(published);

  if (published)
    anagra.not_synced_delta = 0;

  return published;
}

bool UpdatePCount(int old_pcount, int new_pcount)
{
  bool published = true;

  published = published && client.publish("esp/anagra_pcount_delta", String(new_pcount - old_pcount).c_str());

  return published;
}

String SendHTML()
{

  String ptr = "<!DOCTYPE html> <html>\n";
  ptr += "<head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n";
  ptr += "<title>IOT Personal Flow Control</title>\n";
  ptr += "<style>html { font-family: Helvetica; display: inline-block; margin: 0px auto; text-align: center;}\n";
  ptr += "body{margin-top: 50px;background-image: url(\"https://cdn.govexec.com/media/featured/wwt_iot_industry_day_2019_slate_back.png\");} h1 {color: #444444;margin: 50px auto 30px;} h3 {color: #444444;margin-bottom: 50px;}\n";
  ptr += ".button {display: block;width: 180px;background-color: #3498db;border: none;color: white;padding: 13px 30px;text-decoration: none;font-size: 25px;margin: 0px auto 35px;cursor: pointer;border-radius: 4px;}\n";
  ptr += ".button-on {background-color: #3498db;}\n";
  ptr += ".button-on:active {background-color: #2980b9;}\n";
  ptr += ".button-off {background-color: #34495e;}\n";
  ptr += ".button-off:active {background-color: #2c3e50;}\n";
  ptr += "p {font-size: 14px;color: #888;margin-bottom: 10px;}\n";
  ptr += "</style>\n";
  ptr += "</head>\n";
  ptr += "<body>\n";
  ptr += "<h1>IOT Personal Flow Control</h1>\n<form>";
  ptr += "<p><label>Nome : <input name=\"1\" value='" + anagra.business_name;
  ptr += "' style=\"width: 350px;\" readonly></label>";
  ptr += "<p><label>Indirizzo : <input name=\"3\" value='" + anagra.address;
  ptr += "' style=\"width: 300px;\" readonly></label>";
  ptr += "<p><label>CAP : <input name=\"4\" value='" + anagra.pc;
  ptr += "' style=\"width:50px\" readonly></label>";
  ptr += "<label>Citta : <input name=\"5\" value='" + anagra.city;
  ptr += "' readonly></label>";
  ptr += "<p><h2 style=\"margin-top:30px\">Aggiungi/Togli Persone : <input type=\"number\" name=\"2\" value='" + String(0);
  ptr += "'style=\"font-size: 30px;text-align:center;width:100px\"></h2>";
  ptr += "<p><button class=button style=\"margin-top:50px\">Salva</button></form>";
  ptr += "</body>\n";
  ptr += "</html>\n";

  return ptr;
}