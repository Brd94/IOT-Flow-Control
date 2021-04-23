
#include <Wire.h>
#include <Adafruit_AMG88xx.h>

#define PIN_ADD 8
#define PIN_SUB 7

Adafruit_AMG88xx amg;

float pixels[AMG88xx_PIXEL_ARRAY_SIZE];

int current_delta = 0;

bool amg_status;

void setup()
{
  Serial.begin(38400);

  pinMode(PIN_ADD, OUTPUT);
  pinMode(PIN_SUB, OUTPUT);
  digitalWrite(PIN_ADD, LOW);
  digitalWrite(PIN_SUB, LOW);

  current_delta = 20;
}

long last_acq;
long pass;

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

void loop()
{

  SyncDeltas();

  if (!amg_status)
  {
    amg_status = amg.begin();

    if (!amg_status)
    {
      Serial.println("!! ERRORE !! - Sensore non trovato");
    }

    delay(1000);
  }
  else
  {

    //Serial.println("BeginSignal");

    //Serial.println(current_delta);
    // digitalWrite(7, HIGH);
    // delay(50);
    // digitalWrite(7, LOW);
    // delay(50);
    // digitalWrite(8, HIGH);
    // delay(50);
    // digitalWrite(8, LOW);
    // delay(50);

    //Serial.println("EndSignal");

    //read all the pixels

    amg.readPixels(pixels);
    last_acq = millis();
    //     for(int i=1; i<=AMG88xx_PIXEL_ARRAY_SIZE; i++){
    //      if(pixels[i-1] > 30)
    //        out += "█ ";
    //      else if(pixels[i-1] > 25)
    //        out += "▓ ";
    //      else if(pixels[i-1] > 20)
    //        out += "▒ ";
    //       else
    //        out += "░ ";
    //      if( i%8 == 0 ) out+= "\n";
    //    }

    Serial.println("BeginMatrix");

    for (int i = 1; i <= AMG88xx_PIXEL_ARRAY_SIZE; i++)
    {
      Serial.print(pixels[i - 1]);
      Serial.print(" ");
      if (i % 8 == 0)
        Serial.println();
    }

    Serial.println("EndMatrix");

    Serial.println("BeginTerm");

    Serial.println(amg.readThermistor());

    Serial.println("EndTerm");

    pass = millis() - last_acq;

    if (pass < 100)
      delay(100 - pass);
  }
}
