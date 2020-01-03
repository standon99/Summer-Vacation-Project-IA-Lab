#include <Wire.h>
// SLAVE CODE
bool stat = false;
byte message;
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
    Wire.onReceive(receiveEvent);
    Wire.onRequest(requestEvent);
  }

  TCCR1A = 0;   // stop Timer 1
  TCCR1B = 0;
  TCCR1C = 0;
  TCCR1A = bit (COM1A1) | bit (WGM11);
  TCCR1B = bit (WGM13) | bit (WGM12) | bit (CS10);
  ICR1 = 600; // set frequency. 1000 is 16khz 500 32khz. //////////// up to here sets nine and ten frequency to 26khz

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
  //wiggle();
  /* if (stat == true)
    {
     wiggle();
     stat = false;
    }
  */
  //delay(500);
}
void receiveEvent(int howMany)
{
  byte reqPos = Wire.read();
  int Pos = (int)reqPos;
  sliderToVal(reqPos);
}

void requestEvent()
{
  int pos = analogRead(A0);
  message = map(pos, 0, 1023, 0, 255);
  Wire.write(message);
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

void sliderToVal(int val) {
  int pos = analogRead(A0);
  pos = map(pos, 0, 1023, 0, 255);
  int value;
  value = abs(pos - val) * 2 + 450;
  if (abs(pos - val) != 0)
  {
    if (pos < val)
    {
      digitalWrite(9, LOW);
      analogWrite(10, value);
    }
    else
    {
      digitalWrite(10, LOW);
      analogWrite(9, value);
    }
  }
  else
  {
    digitalWrite(9, LOW);
    digitalWrite(10, LOW);
  }
}
