/***************************************************************************
  This is a library for the AMG88xx GridEYE 8x8 IR camera

  This sketch tries to read the pixels from the sensor

  Designed specifically to work with the Adafruit AMG88 breakout
  ----> http://www.adafruit.com/products/3538

  These sensors use I2C to communicate. The device's I2C address is 0x69

  Adafruit invests time and resources providing this open source code,
  please support Adafruit andopen-source hardware by purchasing products
  from Adafruit!

  Written by Dean Miller for Adafruit Industries.
  BSD license, all text above must be included in any redistribution
 ***************************************************************************/

#include <Wire.h>
#include <Adafruit_AMG88xx.h>

Adafruit_AMG88xx amg;

float pixels[AMG88xx_PIXEL_ARRAY_SIZE];

void setup()
{
  Serial.begin(38400);
  Serial.println(F("AMG88xx pixels"));

  bool status;

  // default settings
  status = amg.begin();
  if (!status)
  {
    Serial.println("Could not find a valid AMG88xx sensor, check wiring!");
    while (1)
      ;
  }

  Serial.println();

  delay(100); // let sensor boot up
}

long last_acq;
long pass;
void loop()
{
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

  if(pass < 100)
    delay(100 - pass);

}
