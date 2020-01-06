#include <Wire.h>
bool stat = false;
bool isMaster;
int indexRead, i2cAddress;
int indexPin = A1;

//////////////////////////////////

//////////////////////////////////

void setup()
{
  isMaster = false;
  indexRead = analogRead(indexPin);
  i2cAddress  = map(indexRead, 0, 1023, 1, 127);
  Wire.begin(32);  // I2c begin as slave.
  Wire.onReceive(receiveEvent); // register event
  Wire.onRequest(requestEvent);
}

void loop()
{
  //Wire.begin(32);
  //Wire.onReceive(receiveEvent);
  if (stat)
  {
    wiggle();
    stat = false;
  }
  /////!!!!!! CHECK WHICH PIN WE NEED TO READ POSITION FROM
}

// function that executes whenever data is received from master
// this function is registered as an event, see setup()
void receiveEvent()
{
  stat = true;
  wiggle();
  while (1 < Wire.available()) // loop through all but the last
  {
    int pos = Wire.read(); // receive byte as a character
  }
}

void requestEvent()
{
  while (1 < Wire.available()) // loop through all but the last
  {
    //int currPos = analogRead(A0);
    String currPos = "7";
    byte data[4];
    for (byte i = 0; i < 4; i++) {
      data[i] = (byte)currPos.charAt(i);
    }
    Wire.write(data, 8);
  }
}

void wiggle()
{
  digitalWrite(9, HIGH);
  digitalWrite(10, LOW);

  delay(100);
  digitalWrite(9, LOW);
  digitalWrite(10, HIGH);

  delay(100);
  digitalWrite(9, HIGH);
  digitalWrite(10, LOW);

  delay(100);
  digitalWrite(9, LOW);
  digitalWrite(10, HIGH);

  delay(100);
  digitalWrite(9, LOW);
  digitalWrite(10, LOW);
}
