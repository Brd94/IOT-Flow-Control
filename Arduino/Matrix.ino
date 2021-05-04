
#include <Wire.h>
#include "Adafruit_AMG88xx.h"
#include "Structures.h"
#include <WiFi.h>
#include <PubSubClient.h>
// #include <U8g2lib.h>

#define PIN_ADD 5
#define PIN_SUB 18

#define LED_0 12
#define LED_1 23
#define LED_2 17

#define MATRIX 32
#define MAX_PPL 3

WiFiClient espClient;
PubSubClient client(espClient);

Adafruit_AMG88xx amg;
TaskHandle_t task_output;

pin gpio_pin_0;

float pixels[AMG88xx_PIXEL_ARRAY_SIZE];

float matrix_sensor[8][8];
float matrix_interpolated[MATRIX][MATRIX];
float matrix_base_max[MATRIX][MATRIX];
float matrix_without_max_background[MATRIX][MATRIX];
byte matrix_centroids[MATRIX][MATRIX];

int matrix_processed = 0;
int matrix_cut_value = 3;

int current_delta = 0;
int total_delta = 0;

bool amg_status;

// U8G2_SSD1306_128X64_NONAME_F_HW_I2C u8g2(U8G2_R0, 16, 15, 4);

void setup()
{

  pinMode(PIN_ADD, OUTPUT);
  pinMode(PIN_SUB, OUTPUT);
  pinMode(LED_0, OUTPUT);
  pinMode(LED_1, OUTPUT);
  pinMode(LED_2, OUTPUT);
  pinMode(0, INPUT_PULLUP);

  digitalWrite(PIN_ADD, LOW);
  digitalWrite(PIN_SUB, LOW);
  digitalWrite(LED_0, LOW);
  digitalWrite(LED_1, LOW);
  digitalWrite(LED_2, LOW);

  gpio_pin_0.id = 0;
  gpio_pin_0.current_state = digitalRead(gpio_pin_0.id);
  gpio_pin_0.last_state = digitalRead(gpio_pin_0.id);

  client.setServer("192.168.178.40", 1883);

  xTaskCreatePinnedToCore(
      loop_output,
      "task_output",
      10000,
      NULL,
      0,
      &task_output,
      0);

  current_delta = -5;

  digitalWrite(PIN_SUB, HIGH);
  digitalWrite(PIN_ADD, HIGH);

  delay(1000);
}

bool canPrint = false;

String s;

void loop_output(void *parameter)
{

  // u8g2.begin();
  // u8g2.setFont(u8g2_font_4x6_tf);

  // u8g2.setFontRefHeightExtendedText();
  // u8g2.setDrawColor(1);
  // u8g2.setFontPosTop();
  // u8g2.setFontDirection(0);

  // u8g2.clearBuffer();
  // u8g2.drawStr(0, 0, "LCD OK");

  WiFi.begin("Brd", "12345678");

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(200);
  }

  s = "";

  //Serial.begin(500000);

  digitalWrite(LED_0, LOW);

  String mac_address = String(WiFi.macAddress());

  int bufferlen = 40;
  char *buffer = (char *)malloc(sizeof(char) * bufferlen);

  while (true)
  {
    if (!client.connected())
    {
      client.connect(mac_address.c_str(), "Brd", "Errata");
    }
    else
    {
      client.loop();

      // if (bufferlen < s.length())
      // {
      //   buffer = (char *)realloc(buffer, sizeof(char) * s.length());
      // }

      //buffer = &s.toCharArray()[0];

      //if (canPrint)
      {
        digitalWrite(LED_0, HIGH);

       

        client.publish("esp32/debug", "test");
        //Serial.println(s);
        s = "";
        canPrint = false;
        digitalWrite(LED_0, LOW);
      }

      delay(100);
    }
  }
}

bool get_pin_change(pin &gpio_pin, int min_change_duration = 0)
{

  bool changed = false;

  gpio_pin.current_state = digitalRead(gpio_pin.id);

  //Serial.print("Check PIN ");
  //Serial.print(gpio_pin.id);

  if (gpio_pin.current_state != gpio_pin.last_state && millis() - gpio_pin.last_change > min_change_duration)
  {

    if (gpio_pin.current_state == HIGH)
    {
      changed = true;
      //Serial.print("...CHANGED (HIGH)!");
    }

    gpio_pin.last_change = millis();
  }
  else
  {
    //Serial.print("...NOT CHANGED");
  }

  //Serial.println();

  gpio_pin.last_state = gpio_pin.current_state;

  return changed;
}

long millis_pin_add = 0; //Ultimo cambio di stato
long millis_pin_sub = 0; //Ultimo cambio di stato

int v_button_delay = 200;

void SyncDeltas()
{

  long delta_time_add = millis() - millis_pin_add;

  if (delta_time_add > v_button_delay)
  {
    int status_add = digitalRead(PIN_ADD);
    //Serial.println(status_add);

    if (status_add == 0 && current_delta > 0)
    {
      digitalWrite(PIN_ADD, HIGH);
      millis_pin_add = millis();
      // Serial.println("Begin push +1");
    }
    else if (status_add == 1)
    {
      digitalWrite(PIN_ADD, LOW);
      --current_delta;
      millis_pin_add = millis();
      // Serial.println("End push +1");
    }
  }

  long delta_time_sub = millis() - millis_pin_sub;

  if (delta_time_sub > v_button_delay)
  {
    int status_sub = digitalRead(PIN_SUB);
    //Serial.println(status_sub);

    if (status_sub == 0 && current_delta < 0)
    {
      digitalWrite(PIN_SUB, HIGH);
      millis_pin_sub = millis();
      // Serial.println("Begin push -1");
    }
    else if (status_sub == 1)
    {
      digitalWrite(PIN_SUB, LOW);
      ++current_delta;
      millis_pin_sub = millis();
      // Serial.println("End push -1");
    }
  }

  if (current_delta > 0)
  {
    // Serial.print("TO SYNC : ");
    // Serial.print(current_delta);
    // Serial.println();
  }
}

long last_acq;
long pass;

void loop()
{

  // if (digitalRead(LED_1) == 0)
  //   digitalWrite(LED_1, HIGH);
  // else
  //   digitalWrite(LED_1, LOW);

  //String serialOut = "";

  //u8g2.clearBuffer();
  SyncDeltas();

  if (get_pin_change(gpio_pin_0))
  {
    int state_sub = digitalRead(PIN_SUB);
    int state_add = digitalRead(PIN_ADD);

    digitalWrite(PIN_SUB, HIGH);
    digitalWrite(PIN_ADD, HIGH);

    delay(1000);

    digitalWrite(PIN_SUB, LOW);
    digitalWrite(PIN_ADD, LOW);

    delay(500);

    if (total_delta < 0)
    {
      for (int i = 0; i > total_delta; i--)
      {
        digitalWrite(PIN_SUB, HIGH);
        delay(500);
        digitalWrite(PIN_SUB, LOW);
        delay(500);
      }
    }
    else
    {
      for (int i = 0; i < total_delta; i++)
      {
        digitalWrite(PIN_ADD, HIGH);
        delay(500);
        digitalWrite(PIN_ADD, LOW);
        delay(500);
      }
    }

    digitalWrite(PIN_SUB, state_sub);
    digitalWrite(PIN_ADD, state_add);
  }

  // if (amg_status)
  //   u8g2.drawStr(0, 0, "Sensore OK");
  // else
  //   u8g2.drawStr(0, 0, "Sensore ERR");

  // char buf[16];
  // ltoa(millis() / 1000, buf, 10);
  //u8g2.drawStr(80, 0, buf);

  if (!amg_status)
  {
    amg_status = amg.begin();

    if (!amg_status)
    {
      Serial.println("!! ERRORE !! - Sensore non trovato");
    }

    delay(100);
  }
  else
  {

    amg.readPixels(pixels);
    last_acq = millis();

    int y = 0;

    for (int i = 0; i < 8; i++)
    {
      for (int j = 0; j < 8; j++)
      {
        matrix_sensor[i][j] = pixels[i * 8 + j];
      }
    }

    digitalWrite(LED_2, HIGH);
    // Serial.println("LOOP IN " + String(toPrint));
    BicubicInterpolation(matrix_sensor, 8, 8, matrix_interpolated, 32, 32);

    for (int i = 0; i < 32; i++)
    {
      for (int j = 0; j < 32; j++)
      {

        if (matrix_processed < 50 && matrix_interpolated[i][j] > matrix_base_max[i][j])
        {
          matrix_base_max[i][j] = matrix_interpolated[i][j];
        }
        else if (matrix_processed >= 50)
        {
          //s += String(matrix_interpolated[i][j] - matrix_base_max[i][j]) + "...";
          if (matrix_interpolated[i][j] - matrix_base_max[i][j] >= matrix_cut_value)
          {
            //s += "Assegnato";
            matrix_without_max_background[i][j] = matrix_interpolated[i][j] - matrix_base_max[i][j];
          }
          else
            matrix_without_max_background[i][j] = 0;

          matrix_centroids[i][j] = 0;
        }
        //s += "\n\n";

        //s+= String( matrix_without_max_background[i][j]);
        //Serial.print(matrix_without_max_background[i][j]);
        //Serial.print(" ");
      }

      //s +="\n";
      //Serial.println();
    }

    matrix_processed++;

    CalculateCentroids(matrix_without_max_background, matrix_centroids);
    int delta = CalculateDeltas(matrix_centroids);
    current_delta += delta;
    total_delta += delta;
    //s += "Delta : " + String(current_delta);

    digitalWrite(LED_2, LOW);
    // char result[4];
    // dtostrf(amg.readThermistor(), 2, 2, result);
    //u8g2.drawStr(0, 7, result);

    // serialOut += "EndMatrix\r\n";

    // Serial.println("EndMatrix");

    // Serial.println("BeginTerm");

    // Serial.println(amg.readThermistor());

    // Serial.println("EndTerm");

    // Serial.print(serialOut);

    // char buf_pass[16];
    // ltoa(pass, buf_pass, 10);
    //u8g2.drawStr(100, 0, buf_pass);
  }

  if (!canPrint)
  {

    s += "Processed : " + String(matrix_processed) + "\n";
    s += "Total delta : " + String(total_delta) + "\n";
    //s = "";
    //s += "BeginMatrix\r\n";
    // for (int i = 0; i < 8; i++)
    // {
    //   for (int j = 0; j < 8; j++)
    //   {
    //     s += String(matrix_sensor[i][j]) + " ";
    //   }
    //   //s += "\n";
    // }

    //s += "EndMatrix\r\n";

    //icount++;

    //if (icount % 1 == 0)
    canPrint = true;
  }

  pass = millis() - last_acq;

  if (pass < 100)
  {
    delay(100 - pass);
  }
  //u8g2.sendBuffer();
}

float InterpolateCubic(float v0, float v1, float v2, float v3, float fraction)
{
  float p = (v3 - v2) - (v0 - v1);
  float q = (v0 - v1) - p;
  float r = v2 - v0;
  return (fraction * ((fraction * ((fraction * p) + q)) + r)) + v1;
}

void BicubicInterpolation(float data[8][8], int width, int height, float data_out[32][32], int outWidth, int outHeight)
{

  int rowsPerChunk = 6000 / outWidth;

  if (rowsPerChunk == 0)
  {
    rowsPerChunk = 1;
  }

  int chunkCount = (outHeight / rowsPerChunk) + (outHeight % rowsPerChunk != 0 ? 1 : 0);

  for (int i = 0; i < chunkCount; ++i)
  {
    int jStart = i * rowsPerChunk;
    int jStop = jStart + rowsPerChunk;
    if (jStop > outHeight)
    {
      jStop = outHeight;
    }
    for (int j = jStart; j < jStop; ++j)
    {
      float jLocationFraction = j / (float)outHeight;
      float jFloatPosition = height * jLocationFraction;
      int j2 = (int)jFloatPosition;
      float jFraction = jFloatPosition - j2;
      int j1 = j2 > 0 ? j2 - 1 : j2;
      int j3 = j2 < height - 1 ? j2 + 1 : j2;
      int j4 = j3 < height - 1 ? j3 + 1 : j3;

      for (int k = 0; k < outWidth; ++k)
      {
        float iLocationFraction = k / (float)outWidth;
        float iFloatPosition = width * iLocationFraction;
        int i2 = (int)iFloatPosition;
        float iFraction = iFloatPosition - i2;
        int i1 = i2 > 0 ? i2 - 1 : i2;
        int i3 = i2 < width - 1 ? i2 + 1 : i2;
        int i4 = i3 < width - 1 ? i3 + 1 : i3;
        float jValue1 = InterpolateCubic(data[j1][i1], data[j1][i2], data[j1][i3], data[j1][i4], iFraction);
        float jValue2 = InterpolateCubic(data[j2][i1], data[j2][i2], data[j2][i3], data[j2][i4], iFraction);
        float jValue3 = InterpolateCubic(data[j3][i1], data[j3][i2], data[j3][i3], data[j3][i4], iFraction);
        float jValue4 = InterpolateCubic(data[j4][i1], data[j4][i2], data[j4][i3], data[j4][i4], iFraction);
        data_out[j][k] = InterpolateCubic(jValue1, jValue2, jValue3, jValue4, jFraction);
      }
    }
  }
}

int CalculateCentroid(float matrix[MATRIX][MATRIX])
{

  float sum_array_x;
  float sum_array_y;

  float sum = 0;

  float mp_x = 0;
  float mp_y = 0;

  for (int i = 0; i < MATRIX; i++)
  {

    sum_array_x = 0;
    sum_array_y = 0;

    for (int j = 0; j < MATRIX; j++)
    {
      //s+= "+y=" + String(matrix[i][j]) + "\n";
      sum_array_x += matrix[i][j];
      sum_array_y += matrix[j][i];
      sum += matrix[i][j];
    }

    mp_x += (i * sum_array_x);
    mp_y += (i * sum_array_y);
  }

  // for (int i = 0; i < MATRIX; i++)
  // {
  //   //s += "X : " + String(i * sum_array_x[i]) + "\n";
  //   //s += "Y : " + String(sum_array_y[i]) + "\n";
  //   mp_x += (i * sum_array_x[i]);
  //   mp_y += (i * sum_array_y[i]);
  // }

  mp_x /= sum;
  mp_y /= sum;

  // s += "X:" + String(mp_x) + "\n";
  // s += "y:" + String(mp_y) + "\n";
  // s += "Sum:" + String(sum) + "\n";

  // s += "POSIZ=" + String(mp_x * MATRIX + mp_y) + "\n";
  //int a = mp_x * MATRIX + mp_y;

  return mp_x * MATRIX + mp_y;
}

void CalculateCentroids(float matrix[MATRIX][MATRIX], byte matrix_out[MATRIX][MATRIX])
{
  // int width = MATRIX;
  // int height = MATRIX;

  //int *centroids = (int *)malloc(0 * sizeof(int));

  int totalVals = 0;

  float matrix_wrk[MATRIX][MATRIX] = {{0}};

  for (int i = 0; i < MATRIX; i++)
  {
    for (int j = 0; j < MATRIX; j++)
    {
      if (matrix[i][j] >= matrix_cut_value && totalVals < MAX_PPL)
      {
        // s += "Matrix prima : \n\n";

        // for (int k = 0; k < 32; k++)
        // {
        //   for (int u = 0; u < 32; u++)
        //   {
        //     s += String(matrix[k][u]) + " ";
        //   }
        //   s += "\n";
        // }
        //s+= "TX:"  + String(i) + "," + String(j) + "\n";
        ApplyFloodFill_Iterative(i, j, 0, matrix, matrix_wrk);

        // s += "Matrix dopo : \n\n";

        //  for (int i = 0; i < 32; i++)
        //   {
        //     for (int j = 0; j < 32; j++)
        //     {
        //       s += String(matrix_out[i][j]) + " ";
        //     }
        //     s += "\n";
        //   }

        int centroid = CalculateCentroid(matrix_wrk);
        totalVals++;
        short x = centroid / MATRIX;
        short y = centroid % MATRIX;
        //centroids = (int *)realloc(centroids, totalVals * sizeof(int));
        //centroids[totalVals - 1] = centroid;
        matrix_out[x][y] = 1;
        s += "Centroide " + String(totalVals) + " posizione " + x + "\n";
      }
    }
  }

  //free(centroids);
}

// void ApplyFloodFill(int x_node, int y_node, float limit_val, float matrix_in[MATRIX][MATRIX], float matrix_out[MATRIX][MATRIX])
// {
//   if (matrix_in[x_node][y_node] == limit_val)
//     return;

//   matrix_out[x_node][y_node] = matrix_in[x_node][y_node];
//   matrix_in[x_node][y_node] = limit_val;

//   ApplyFloodFill(max(x_node - 1, 0), y_node, limit_val, matrix_in, matrix_out);
//   ApplyFloodFill(min(x_node + 1, MATRIX - 1), y_node, limit_val, matrix_in, matrix_out);
//   ApplyFloodFill(x_node, max(y_node - 1, 0), limit_val, matrix_in, matrix_out);
//   ApplyFloodFill(x_node, min(y_node + 1, MATRIX - 1), limit_val, matrix_in, matrix_out);
// }

void ApplyFloodFill_Iterative(short x_node, short y_node, short limit_val, float matrix_in[MATRIX][MATRIX], float matrix_out[MATRIX][MATRIX])
{

  int coda = 0;
  int testa = 0;

  short matrix_queue[MATRIX * MATRIX];
  bool matrix_visited[MATRIX][MATRIX] = {{false}};

  matrix_visited[x_node][y_node] = true;
  matrix_queue[testa] = (x_node * MATRIX) + y_node;
  testa++;

  while (coda < testa)
  {
    short x = matrix_queue[coda] / MATRIX;
    short y = matrix_queue[coda] % MATRIX;

    // char buf[30];
    // sprintf(buf, "M_O(%f,%f)=n\n", x, y);
    // s += buf;

    if (matrix_in[x][y] != limit_val)
    {

      matrix_out[x][y] = matrix_in[x][y];
      matrix_in[x][y] = limit_val;
    }

    short x1 = max(x - 1, 0);
    //short y1 = y;

    if (matrix_in[x1][y] != limit_val && !matrix_visited[x1][y])
    {
      //s += "O\n";
      matrix_visited[x1][y] = true;
      matrix_queue[testa] = (x1 * MATRIX) + y;
      testa++;
    }

    short x2 = min(x + 1, MATRIX - 1);
    //short y2 = y;

    if (matrix_in[x2][y] != limit_val && !matrix_visited[x2][y])
    {
      //s += "E\n";
      matrix_visited[x2][y] = true;
      matrix_queue[testa] = (x2 * MATRIX) + y;
      testa++;
    }

    //short x3 = x;
    short y3 = max(y - 1, 0);

    if (matrix_in[x][y3] != limit_val && !matrix_visited[x][y3])
    {
      //s += "N\n";
      matrix_visited[x][y3] = true;
      matrix_queue[testa] = (x * MATRIX) + y3;
      testa++;
    }

    //short x4 = x;
    short y4 = min(y + 1, MATRIX - 1);

    if (matrix_in[x][y4] != limit_val && !matrix_visited[x][y4])
    {
      //s += "S\n";
      matrix_visited[x][y4] = true;
      matrix_queue[testa] = (x * MATRIX) + y4;
      testa++;
    }

    coda++;
  }
}

int max(int num1, int num2)
{
  return (num1 > num2) ? num1 : num2;
}

int min(int num1, int num2)
{
  return (num1 > num2) ? num2 : num1;
}

int top = 0;
int bottom = 0;

int CalculateDeltas(byte matrix[MATRIX][MATRIX])
{

  int delta = 0;
  int actual_top = 0;
  int actual_bottom = 0;

  for (int i = 0; i < 32; i++)
  {
    for (int j = 0; j < 32; j++)
    {
      if (matrix[i][j])
      {
        if (i < 16)
        {
          ++actual_top;
        }
        else
        {
          ++actual_bottom;
        }
      }
    }
  }

  if (actual_top != top && actual_bottom != bottom)
  {
    delta = actual_top - top;
  }

  top = actual_top;
  bottom = actual_bottom;

  return delta;
}