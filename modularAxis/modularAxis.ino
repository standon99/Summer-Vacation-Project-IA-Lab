#include <Wire.h>
int forward = 9 ;
int reverse = 10;
int setIndexString = 5;
int indexPin = A1;
int indexRead;
bool isMaster;
int i2cAddress;
int addresses[6];
int readVal[6];
byte address;
byte error;
int nDevices;
int ctr = 0;
int x;

void setup()
{
  pinMode(forward, OUTPUT);
  pinMode(reverse, OUTPUT);
  pinMode(setIndexString, OUTPUT);
  wiggle();
  delay(10000); // 10 seconds to make sure that power and usb both plugged in if this is master.

  int usbRead = analogRead(A5);
  if (usbRead == 1023)  // is there 5v from the usb on analog pin5? if so, this is the master.
  {
    isMaster = true;
    digitalWrite(setIndexString, LOW);  // turns on fet to send 5v at top of index string.
    wiggle();
    wiggle();
    wiggle();
    Serial.begin(9600);
    Wire.begin();
    delay (1000);
    getAddresses();
    Serial.println(usbRead);
  }
  else
  {
    Serial.begin(9600);
    isMaster = false;
    digitalWrite(setIndexString, HIGH);
    delay(500); // index string fet off.
    indexRead = analogRead(indexPin);
    i2cAddress  = map(indexRead, 0, 1023, 1, 127);
    Wire.begin(i2cAddress);  // I2c begin as slave.
    //    Wire.onRequest(requestEvent);

    //Serial.println(i2cAddress);

    Wire.onReceive(printer);
  }
}

void getAddresses()
{
  for (address = 1; address < 127; address++ )
  {
    // The i2c_scanner uses the return value of
    // the Write.endTransmisstion to see if
    // a device did acknowledge to the address.
    Wire.beginTransmission(address);
    error = Wire.endTransmission();

    if (error == 0)
    {
      Serial.print("I2C device found at address 0x");
      if (address < 16)
        Serial.print("0");
      Serial.print(address, HEX);
      addresses[ctr] = address;
      ctr++;
      Serial.println("  !");
      nDevices++;
    }
    else if (error == 4)
    {
      Serial.print("Unknown error at address 0x");
      if (address < 16)
        Serial.print("0");
      Serial.println(address, HEX);
    }
  }
  if (nDevices == 0)
    Serial.println("No I2C devices found\n");
  else
    Serial.println(nDevices);
  Serial.println("done\n");

  // delay(5000);           // wait 5 seconds for next scan
}

void loop()
{
  for (int i = 0; i < nDevices; i++)
  {
    if (isMaster)
    {
      Wire.beginTransmission(addresses[i]);
      Wire.write(5);
      Serial.println(addresses[i]);
      Wire.endTransmission(addresses[i]);
      //Wire.requestFrom(addresses[i], 3);
      //readVal[i] = Wire.read();
      //Serial.println(readVal[i]);
    }
    else
    {
      //Wire.onReceive(printer());
      //Serial.println("In slave device");
      //wiggle();
      //wiggle();
      delay(500);
    }
    wiggle();
    delay(500);
  }
}

int printer(int bytes)
{
  x = Wire.read();
  return x;
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
