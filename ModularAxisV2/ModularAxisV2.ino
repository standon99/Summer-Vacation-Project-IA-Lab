#include <Wire.h>
int itr = 0;
bool stat = false;
int tStat;
byte message_i2c;
byte sent_data;
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
int ctr = 1;// was = 0
int x;
String message;
const int nbSlider = 6;
int val[nbSlider] = {500, 500, 500, 500, 500, 500};
int requestedPos[nbSlider];
int desiredPos[nbSlider];
int digit1;
int digit2;
int digit3;
int digit4;
int id;
bool moveSlider[nbSlider] = {false, false, false, false, false, false};
int old_id = 7;
int mode = 0;

void sliderToVal(int val) {
  int pos = analogRead(A0);
  pos = map(pos, 0, 1023, 0, 255);
  int value;
  value = abs(pos - val) * 2 + 470; // was 450
  if (abs(pos - val) > 1) // was 2
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

void receiveEvent(int howMany)
{
  if (!isMaster)
  {
    byte reqPos = Wire.read();
    int Pos = (int)reqPos;
    sliderToVal(reqPos);
  }
}

void requestEvent()
{
  if (!isMaster)
  {
    int pos = analogRead(A0);
    message_i2c = map(pos, 0, 1023, 0, 255);
    Wire.write(message_i2c);
  }
}
void setup() {
  pinMode(forward, OUTPUT);
  pinMode(reverse, OUTPUT);
  pinMode(setIndexString, OUTPUT);
  int usbRead = analogRead(A5);
  if (usbRead == 1023)  // is there 5v from the usb on analog pin5? if so, this is the master.
  {
    isMaster = true;
    digitalWrite(setIndexString, LOW);  // turns on fet to send 5v at top of index string.
    Serial.begin(2000000);
    Wire.begin();
    delay(7000);
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

  // Set PWM to required values for smooth slider movement
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
  if (isMaster)
  {
    if (Serial.available() > 0) {
      message = Serial.readStringUntil('\n');
      //old_id = id;
      id = message[0] - '0';
      if (id >= 0 && id <= 5) {
        mode = 0;
        digit1 = message[2] - '0';
        digit2 = message[3] - '0';
        digit3 = message[4] - '0';
        digit4 = message[5] - '0';
        val[id] = digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4;
        val[id] = abs(val[id]) % 1024;
        moveSlider[id] = true;
        desiredPos[id] = val[id];
      }
    }
    for (int i = 0; i < (nDevices+1); i+2)
    {
      if (mode == 0)
      {
        if (i == 0)
        {
          sent_data = map(val[i], 0, 1023, 0, 255);
          sliderToVal(sent_data);
        }
        
          sent_data = map(val[i], 0, 1023, 0, 255);
          Wire.beginTransmission(addresses[i]);
          Wire.write(sent_data);
          tStat = Wire.endTransmission();
        
      }
      //Serial.println(tStat);
      //Serial.println("SENT DATA");
      //delay(500);
      Wire.requestFrom(addresses[i], 1);   // request 1 byte from slave arduino (8)
      byte MasterReceive = Wire.read();    // receive a byte from the slave arduino and store in MasterReceive
      requestedPos[i] = MasterReceive;
      //Serial.println(MasterReceive);

      if (itr % 150 == 0)
      {
        Serial.println("--------------------------------");
        for (int v = 0; v < nDevices; v++)
        {
          Serial.println(requestedPos[v]);
        }
      }
      //delay(1000);
    }
  }
  itr++;
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
