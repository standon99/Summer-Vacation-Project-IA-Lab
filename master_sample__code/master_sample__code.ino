#include <Wire.h>
// MASTER CODE
int tStat;
byte sent_data = 0;

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

void setup() {
  pinMode(forward, OUTPUT);
  pinMode(reverse, OUTPUT);
  pinMode(setIndexString, OUTPUT);
  int usbRead = analogRead(A5);
  if (usbRead == 1023)  // is there 5v from the usb on analog pin5? if so, this is the master.
  {
    isMaster = true;
    digitalWrite(setIndexString, LOW);  // turns on fet to send 5v at top of index string.
    Serial.begin(9600);
    Wire.begin();
    delay (5000);
    getAddresses();
    Serial.println(usbRead);
  }
  else //slave mode
  {
    isMaster = false;
    digitalWrite(setIndexString, HIGH);
    delay(500); // index string fet off.
    indexRead = analogRead(indexPin);
    i2cAddress  = map(indexRead, 0, 1023, 1, 127);
    Wire.begin(i2cAddress);  // I2c begin as slave.
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

void loop() {
  // put your main code here, to run repeatedly:
  // if ((int)sent_data > 245) {
  //   sent_data = 0;
  // }
  //else sent_data++;
  /*
    if (!Serial.available())
    {
    getAddresses();
    }
  */
  for (int i = 0; i < nDevices; i++)
  {
    sent_data = 70;
    Wire.beginTransmission(32);
    Wire.write(sent_data);
    tStat = Wire.endTransmission();
    //Serial.println(tStat);
    Serial.println("SENT DATA");
    //delay(500);
    Wire.requestFrom(32, 1);                          // request 1 byte from slave arduino (8)
    byte MasterReceive = Wire.read();                // receive a byte from the slave arduino and store in MasterReceive
    Serial.println(MasterReceive);
    Serial.println(MasterReceive == '100');
    //delay(1000);
  }
}
