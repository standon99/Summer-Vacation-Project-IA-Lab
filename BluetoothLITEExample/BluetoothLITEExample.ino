
#include "BluetoothSerial.h"

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

BluetoothSerial SerialBT;
const int fwd1 = 14;
const int rvs1 = 15;
const int fwd2 = 32;
const int rvs2 = 33;
const int recieved = 13;
const int pwmFreq = 25000;
const int encoder1 = 13;
const int encoder2 = 27 ;
const int batOut = 21;
const int pushbutton = 26;
int encoderValue;
//const int pwmResolution  = 8;
int fader1 = A2;
int fader2 = A3;
int fader1Val = 0;
int fader2Val = 0;
int fader1Old = 0;
int fader2Old = 0;
int batVoltage = 35;
long debounceMillis = 0;
int encoderCount  = 0;
int encoderCountOld = 0;
int buttonpress = 0;
int oldPress;
const int numReadings = 15;

int readings[numReadings];      // the readings from the analog input
int readIndex = 0;              // the index of the current reading
int total = 0;                  // the running total
int average = 0;
int readings2[numReadings];      // the readings from the analog input
int readIndex2 = 0;              // the index of the current reading
int total2 = 0;                  // the running total
int average2 = 0;
int bat;

void setup() {

  SerialBT.begin("FaderAxis1"); //Change this for each unit
  pinMode(encoder1, INPUT_PULLUP);
  pinMode(encoder2, INPUT_PULLUP);
  pinMode(pushbutton, INPUT_PULLUP);
  pinMode(recieved, OUTPUT);
 // attachInterrupt(digitalPinToInterrupt(encoder2), encoderRead, FALLING);

  analogReadResolution(4); // can set this all the way to 12bits of resolution
  ////pwmsetup

  for (int i = 0; i < 4; i++)
  {
    ledcSetup(i, pwmFreq, 8);
  }
  ledcSetup (4, pwmFreq, 8); //led outpin
  ledcAttachPin(fwd1, 0);
  ledcAttachPin(rvs1, 1);
  ledcAttachPin(fwd2, 2);
  ledcAttachPin(rvs2, 3);
  ledcAttachPin(batOut, 4);
  ledcWrite(0, 255);
  ledcWrite(4, 255);
  delay (500);
  ledcWrite(0, 0);
  ledcWrite(1, 255);
  delay(100);
  ledcWrite(1, 0);

  for (int i = 0; i < numReadings; i++) {
    readings[i] = 0;
    readings2[i] = 0;
  }

}

int asciiReader(char message)
{
  int number;
  int asciiNo = message;
  number = asciiNo - 48;
  return number;
}


void loop() {


  bat = analogRead(batVoltage);
  buttonpress = digitalRead(pushbutton);
  ledcWrite(4, map(bat, 230, 273, 10, 255 ) );  // pwm output for battery level led
  ReadAndAverageInputs();
  SerialWriteIfChange();
 if (SerialBT.available()) {
    //Serial.write(SerialBT.read());
    digitalWrite(recieved, HIGH);
    SerialBT.println(asciiReader((char)SerialBT.read()));
  }
  delay(3);
digitalWrite(recieved, LOW);
}

void SerialWriteIfChange()
{
  if (fader1Val != fader1Old || fader2Val != fader2Old || encoderCount != encoderCountOld || buttonpress != oldPress)
  {
    SerialBT.print(fader1Val);
    SerialBT.print (",");
    SerialBT.print(fader2Val);
    SerialBT.print (",");
    SerialBT.print (encoderCount);
    SerialBT.print (",");
    SerialBT.print (buttonpress);
    SerialBT.print (",");
    SerialBT.println (bat);

    fader1Old = fader1Val;
    fader2Old = fader2Val;
    encoderCountOld = encoderCount;
    oldPress = buttonpress;
  }
}

void ReadAndAverageInputs() {
  total = total - readings[readIndex];
  total2 = total2 - readings2[readIndex];
  readings[readIndex] = analogRead(fader1) / 4;
  readings2[readIndex] = analogRead(fader2) / 4;
  total = total + readings[readIndex];
  total2 = total2 + readings2[readIndex];
  readIndex ++;
  if (readIndex >= numReadings) {
    readIndex = 0;
  }
  fader1Val =  total / numReadings;
  fader2Val = total2 / numReadings;
}


void encoderRead()
{
  unsigned long currentMillis = millis();
  int encRead = digitalRead(encoder1);
  if (currentMillis  - debounceMillis > 20 ) // adds a 20 millisecond dbounce filter for the rotary encoder
  {
    debounceMillis = currentMillis;
    int encRead2 = digitalRead(encoder1);
    if (encRead && encRead2)
    {
      encoderCount --;
      if (encoderCount > 24) encoderCount = 0;
    }
    else if (!encRead && !encRead2)
    {
      encoderCount ++;
      if (encoderCount < 0) encoderCount = 24;
    }


  }


}
