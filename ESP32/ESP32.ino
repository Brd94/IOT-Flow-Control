/// https://github.com/knolleary/pubsubclient/archive/master.zip ricorda di controlla anche qua
#include <SPI.h>
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

char *mqtt_server = "192.168.178.20";

WiFiClient espClient;
PubSubClient client(espClient);
long lastMsg = 0;
long lastRetryMQTT = 0;
long lastRSTPress = 0;
char msg[50];
int value = 0;

bool MQTTSyncPending = true;

char station_id[20];

U8G2_SSD1306_128X64_NONAME_F_HW_I2C u8g2(U8G2_R0, 16, 15, 4);
WebServer server(80);

anag anagra;
//WebServer server;

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

  preferences.begin("EEPROM", false);

  anagra.address = preferences.getString("anagra_address");
  anagra.business_name = preferences.getString("anagra_bname");
  anagra.pcount = preferences.getInt("anagra_pcount");
  anagra.pc = preferences.getString("anagra_pc", anagra.pc);
  anagra.city = preferences.getString("anagra_city", anagra.city);

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
  //EEPROM.begin(4096);
  //EEPROM.get(0, anagra); //Carico da memoria

  server.on("/", handle_onConnect);

  server.begin();
  u8g2.drawStr(3, 32, "Init WebServer...OK");
  u8g2.sendBuffer();

  pinMode(0, INPUT_PULLUP);

  delay(700);
  //client.setCallback(callback);
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
      anagra.business_name = server.arg(i);
      Serial.println("SET BNAME");
      preferences.putString("anagra_bname", anagra.business_name);
      break;

    case 2:
      anagra.pcount = server.arg(i).toInt();
      Serial.println("SET PPL");
      preferences.putInt("anagra_pcount", anagra.pcount);
      break;

    case 3:
      anagra.address = server.arg(i);
      Serial.println("SET ADDR");
      preferences.putString("anagra_address", anagra.address);
      break;

    case 4:
      anagra.pc = server.arg(i);
      Serial.println("SET PC");
      preferences.putString("anagra_pc", anagra.pc);
      break;

    case 5:
      anagra.city = server.arg(i);
      Serial.println("SET CITY");
      preferences.putString("anagra_city", anagra.city);
      break;

    default:
      break;
    }
  }

  // EEPROM.put(0,anagra);
  // EEPROM.commit();

  MQTTSyncPending = true;

  server.sendHeader("Location", String("/"), true);

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
  Serial.print("Message arrived on topic: ");
  Serial.print(topic);
  Serial.print(". Message: ");
  String messageTemp;

  for (int i = 0; i < length; i++)
  {
    Serial.print((char)message[i]);
    messageTemp += (char)message[i];
  }
  Serial.println();

  if (String(topic) == "esp32/output")
  {
    Serial.print("Changing output to ");
    if (messageTemp == "on")
    {
      Serial.println("on");
    }
    else if (messageTemp == "off")
    {
      Serial.println("off");
    }
  }
}

void reconnect()
{
  //while (!client.connected())
  {
    Serial.print("Attempting MQTT connection...");
    if (client.connect(WiFi.macAddress().c_str(), "Brd", "Errata"))
    {
      Serial.println("connected");

      // Subscribe
      //client.subscribe("esp32/output"); //pe l'input

      //client.subscribe("esp32/localeTest/");
    }
    else
    {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      //delay(5000);
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
  u8g2.drawLine(0, 0, 128, 0);
  u8g2.drawLine(0, 0, 0, 63);
  u8g2.drawLine(127, 0, 127, 63);
  u8g2.drawLine(0, 12, 128, 12);
  u8g2.drawLine(0, 63, 128, 63);

  //Scrivo l'IP
  char ipAddressStr[16];
  sprintf(ipAddressStr, "%03d.%03d.%03d.%03d", WiFi.localIP()[0], WiFi.localIP()[1], WiFi.localIP()[2], WiFi.localIP()[3]);
  u8g2.drawStr(3, 2, ipAddressStr);

  //Scrivo il messaggio di stato
  String stateMessage = anagra.business_name + " - " + anagra.address + "   ";

  int indexStart1Row = (now / 1000) % max((int)stateMessage.length() - 20, 1);
  //Serial.println(indexStart1Row);

  char ragSoc[20];
  stateMessage.toCharArray(ragSoc, 20, indexStart1Row);
  u8g2.drawStr(3, 15, ragSoc);

  //Scrivo il numero delle persone
  if (digitalRead(0) == 0)
  {
    u8g2.drawStr(3, 25, "Rset Pers.2',Wifi 8'");

    if (now - lastRSTPress > 2000)
    {
      anagra.pcount = 0;
      preferences.putInt("anagra_pcount", anagra.pcount);
      u8g2.drawStr(3, 25, "Reset Persone...OK");
      delay(2000);

      //Cancello SSID E PWD
    }

    if (now - lastRSTPress > 8000)
    {
      u8g2.drawStr(3, 25, "Reset Wifi...OK");
      delay(2000);

      //Cancello SSID E PWD
    }
  }
  else
  {
    lastRSTPress = now;

    char str[20];
    sprintf(str, "Persone : %d", anagra.pcount);
    u8g2.drawStr(3, 25, str);
  }

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

    server.handleClient();
  }

  //u8g2.drawGlyph(5, 20, 0x2603);

  u8g2.drawVLine(121, 7, 3);
  u8g2.drawLine(119, 7, 123, 7);
  u8g2.drawVLine(119, 5, 2);
  u8g2.drawVLine(123, 5, 2);
  u8g2.drawVLine(121, 2, 2);

  if (!client.connected())
  {
    MQTTSyncPending = true;

    if (now - lastRetryMQTT > 5000) //Ogni 5 secondi riprovo a connettermi al broker
    {
      lastRetryMQTT = now;
      reconnect();
    }
    u8g2.drawLine(118, 9, 124, 3);
  }
  else
  {
    client.loop(); //A che serve??

    if (now - lastMsg > 1000)
    {
      lastMsg = now;
     
     if(MQTTSyncPending)
      MQTTSyncPending = !UpdateAnagra(&anagra);
    }

    
  }

  u8g2.sendBuffer();
}

bool UpdateAnagra(anag *anagra_to_publish)
{
  bool published = true;
 
  published = published && client.publish("esp/anagra_bname", String(anagra_to_publish->business_name).c_str());
  published = published && client.publish("esp/anagra_address", String(anagra_to_publish->address).c_str());
  published = published && client.publish( "esp/anagra_pc", String(anagra_to_publish->pc).c_str());
  published = published && client.publish("esp/anagra_city", String(anagra_to_publish->city).c_str());

  return published;
}

bool UpdatePCount(int old_pcount,int new_pcount)
{
  bool published = true;

  published = published && client.publish("esp/anagra_pcount_delta", String(new_pcount-old_pcount).c_str());
  
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
  ptr += "' style=\"width: 350px;\"></label>";
  ptr += "<p><label>Indirizzo : <input name=\"3\" value='" + anagra.address;
  ptr += "' style=\"width: 300px;\"></label>";
  ptr += "<p><label>CAP : <input name=\"4\" value='" + anagra.pc;
  ptr += "' style=\"width:50px\"></label>";
  ptr += "<label>Citta : <input name=\"5\" value='" + anagra.city;
  ptr += "'></label>";
  ptr += "<p><h2 style=\"margin-top:30px\">Persone : <input type=\"number\" name=\"2\" value='" + String(anagra.pcount);
  ptr += "'style=\"font-size: 30px;text-align:center;width:100px\"></h2>";
  ptr += "<p><button class=button style=\"margin-top:50px\">Salva</button></form>";
  ptr += "</body>\n";
  ptr += "</html>\n";

  return ptr;
}