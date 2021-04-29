//#include <U8g2lib.h>

#include <Wire.h>
#include "Adafruit_AMG88xx.h"

#define PIN_ADD 8
#define PIN_SUB 7

#define LED_0 17
#define LED_1 5
#define LED_2 18

#define MATRIX 32

Adafruit_AMG88xx amg;
TaskHandle_t task_output;

float pixels[AMG88xx_PIXEL_ARRAY_SIZE];

float matrix_sensor[8][8];
float matrix_interpolated[MATRIX][MATRIX];
float matrix_base_max[MATRIX][MATRIX];
float matrix_without_max_background[MATRIX][MATRIX];

int matrix_base_index = 0;
int matrix_cut_value = 3;

int current_delta = 0;

bool amg_status;

//U8G2_SSD1306_128X64_NONAME_F_HW_I2C u8g2(U8G2_R0, 16, 15, 4);

void setup()
{
  //u8g2.begin();
  //u8g2.setFont(//u8g2_font_4x6_tf);

  //u8g2.setFontRefHeightExtendedText();
  //u8g2.setDrawColor(1);
  //u8g2.setFontPosTop();
  //u8g2.setFontDirection(0);

  pinMode(PIN_ADD, OUTPUT);
  pinMode(PIN_SUB, OUTPUT);
  pinMode(LED_0, OUTPUT);
  pinMode(LED_1, OUTPUT);
  pinMode(LED_2, OUTPUT);

  digitalWrite(PIN_ADD, LOW);
  digitalWrite(PIN_SUB, LOW);
  digitalWrite(LED_0, LOW);
  digitalWrite(LED_1, LOW);
  digitalWrite(LED_2, LOW);

  xTaskCreatePinnedToCore(
      loop_output,
      "task_output",
      10000,
      NULL,
      0,
      &task_output,
      0);

  current_delta = 20;
}

bool canPrint = false;

String s;

void loop_output(void *parameter)
{

  s = "";

  Serial.begin(500000);

  digitalWrite(LED_0, LOW);

  while (true)
  {
    //Serial.println("LOOP OUT " + String(toPrint));

    if (canPrint)
    {
      digitalWrite(LED_0, HIGH);

      Serial.println(s);
      s = "";
      canPrint = false;
      digitalWrite(LED_0, LOW);
    }
    else
    {
      delay(20);
    }
  }
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
      Serial.println("Begin push +1");
    }
    else if (status_add == 1)
    {
      digitalWrite(PIN_ADD, LOW);
      --current_delta;
      millis_pin_add = millis();
      Serial.println("End push +1");
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
      Serial.println("Begin push -1");
    }
    else if (status_sub == 1)
    {
      digitalWrite(PIN_SUB, LOW);
      ++current_delta;
      millis_pin_sub = millis();
      Serial.println("End push -1");
    }
  }

  if (current_delta > 0)
  {
    Serial.print("TO SYNC : ");
    Serial.print(current_delta);
    Serial.println();
  }
}

long last_acq;
long pass;

void loop()
{

  if (digitalRead(LED_1) == 0)
    digitalWrite(LED_1, HIGH);
  else
    digitalWrite(LED_1, LOW);

  //String serialOut = "";

  //u8g2.clearBuffer();
  //SyncDeltas();

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

    if (!canPrint)
    {

      //s = "";
      //s += "BeginMatrix\r\n";
      for (int i = 0; i < 8; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          s += String(matrix_sensor[i][j]) + " ";
        }
        //s += "\r\n";
      }

      //s += "EndMatrix\r\n";

      //icount++;

      //if (icount % 1 == 0)
      canPrint = true;
    }

    digitalWrite(LED_2, HIGH);
    // Serial.println("LOOP IN " + String(toPrint));
    BicubicInterpolation(matrix_sensor, 8, 8, matrix_interpolated, 32, 32);

    for (int i = 0; i < 32; i++)
    {
      for (int j = 0; j < 32; j++)
      {

        if (matrix_base_index < 50 && matrix_interpolated[i][j] > matrix_base_max[i][j])
        {
          matrix_base_max[i][j] = matrix_interpolated[i][j];
          matrix_base_index++;
        }
        else if (matrix_base_index >= 50)
        {
          if (matrix_interpolated[i][j] - matrix_base_max[i][j] > matrix_cut_value)
            matrix_without_max_background[i][j] = matrix_interpolated[i][j] - matrix_base_max[i][j];
          else
            matrix_without_max_background[i][j] = 0;
        }

        //Serial.print(matrix_without_max_background[i][j]);
        //Serial.print(" ");
      }
      //Serial.println();
    }

    CalculateCentroids(matrix_without_max_background);

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

    pass = millis() - last_acq;

    // char buf_pass[16];
    // ltoa(pass, buf_pass, 10);
    //u8g2.drawStr(100, 0, buf_pass);

    if (pass < 100)
    {
      delay(100 - pass);
    }
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
  int width = MATRIX;
  int height = MATRIX;

  float sum_array_x[MATRIX];
  float sum_array_y[MATRIX];

  float sum = 0;

  for (int i = 0; i < width; i++)
  {
    for (int j = 0; j < height; j++)
    {
      sum_array_x[i] += matrix[i][j];
      sum_array_y[i] += matrix[j][i];
      sum += matrix[i][j];
    }
  }

  float mp_x = 0;
  float mp_y = 0;

  for (int i = 0; i < width; i++)
  {
    mp_x += (i * sum_array_x[i]);
    mp_y += (i * sum_array_y[i]);
  }

  mp_x /= sum;
  mp_y /= sum;

  int a = mp_x * MATRIX + mp_y;

  return a;
}

void CalculateCentroids(float matrix[MATRIX][MATRIX])
{
  int width = MATRIX;
  int height = MATRIX;

  int *centroids = (int *)malloc(0 * sizeof(int));

  int totalVals = 0;

  float matrix_out[MATRIX][MATRIX];

  for (int i = 0; i < width; i++)
  {
    for (int j = 0; j < height; j++)
    {
      if (matrix[i, j] != 0)
      {
        //ApplyFloodFill(i, j, 0, matrix, matrix_out);
        int centroid = CalculateCentroid(matrix_out);
        totalVals++;
        centroids = (int *)realloc(centroids, totalVals * sizeof(int));
        centroids[totalVals - 1] = centroid;
      }
    }
  }

  free(centroids);
}

void ApplyFloodFill(int x_node, int y_node, float limit_val, float matrix_in[MATRIX][MATRIX], float matrix_out[MATRIX][MATRIX])
{
  if (matrix_in[x_node][y_node] == limit_val)
    return;

  matrix_out[x_node][y_node] = matrix_in[x_node][y_node];
  matrix_in[x_node][y_node] = limit_val;

  ApplyFloodFill(max(x_node - 1, 0), y_node, limit_val, matrix_in, matrix_out);
  ApplyFloodFill(min(x_node + 1, MATRIX - 1), y_node, limit_val, matrix_in, matrix_out);
  ApplyFloodFill(x_node, max(y_node - 1, 0), limit_val, matrix_in, matrix_out);
  ApplyFloodFill(x_node, min(y_node + 1, MATRIX - 1), limit_val, matrix_in, matrix_out);
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

int CalculateDeltas(float matrix[MATRIX][MATRIX])
{

  int actual_top = 0;
  int actual_bottom = 0;

  for (int i = 0; i < 32; i++)
  {
    for (int j = 0; j < 32; j++)
    {
      if (matrix[i, j] != 0)
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
    return actual_top - top;
  }

  top = actual_top;
  bottom = actual_bottom;

  return 0;
}