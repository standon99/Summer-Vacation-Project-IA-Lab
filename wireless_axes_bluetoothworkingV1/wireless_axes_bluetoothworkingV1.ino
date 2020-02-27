#include "BluetoothSerial.h"

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

BluetoothSerial SerialBT;
const int fwd1 = 14;
const int rvs1 = 15;
const int fwd2 = 32;
const int rvs2 = 33;
const int pwmFreq = 25000;
const int encoder1 = 13;
const int encoder2 = 27;
const int batOut = 21;
const int pushbutton = 26;
int ctr = 0;
//int encoderValue;
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


// New Variables:

// Variable Definitions
int itr = 0;
bool stat = false;
int tStat;
byte message_i2c;
byte sent_data;
//int forward = 9;
//int reverse = 10;
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
int ctr2 = 1;// was = 0
int x;
String message;
const int nbSlider = 6;
int val[7] = {500, 500, 500, 500, 500, 500, 500};
int requestedPos[nbSlider];
int desiredPos[nbSlider];
int digit1;
int digit2;
int digit3;
int digit4;
int digit5, digit6, digit7, digit8, digit9, digit10, digit11, digit12;
int id;
bool moveSlider[nbSlider] = {false, false, false, false, false, false};
int old_id = 7;
int mode = 0;
int yAxis[2] = { 0, 1 }, xAxis[2] = { 3, 2 }, zAxis[2] = { 5, 4 };
int disc_val;
int encoderValue = 0;
int timeNow;


const float minForce = 256;
const float maxForce = 100;
const int nbData = maxForce;
float data[nbData];
bool mov = true;
bool isFollowing;
const int nbAxe = 3;
int nbStepAxe[nbAxe];
int masterEncoderVals[10];
int MasterSwitches[10];
int switch_stat;

bool serial_stat;
int interrupt_cnt = 0;
int times[2];

// Data sets for use in mapping haptics
float data0[nbData] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10 , 10 , 10 , 10, 10, 10 , 10, 10, 0, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0}; //{0, 1, 2, 3, 4, 4, 4, 0};
float data1[nbData] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10 , 10 , 10 , 10, 10, 10 , 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0}; // 10 replaced 4
float data2[nbData] = {0, 0, 0, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 , 10 , 10 , 10, 200, 200, 200 , 200, 200, 200, 200, 200, 200, 200, 200, 200, 200 , 200, 200, 200, 200, 200, 200, 200, 200, 200, 200 , 200, 200, 200, 200, 200, 200, 200, 200, 200, 200 , 200, 200, 200, 200, 200, 200, 200, 200, 200, 200 , 200, 200, 200, 200, 200, 200, 200, 200, 200 , 200, 200, 200, 200, 200, 200, 200, 200, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 0, 0}; //{0, 1, 2, 3, 4, 4, 4, 2, 1, 0};
bool corr_loc[3][2] = {{false, false},
  {false, false},
  {false, false}
};

int mssgRcvCount = 0;
int myAddress;
int val1;
int val2;
int hapticsStat;
int xStep;
int yStep;
int zStep;


/*
  ledcWrite(0, 255);
  ledcWrite(4, 255);
  delay (500);
  ledcWrite(0, 0);
  ledcWrite(1, 255);
  delay(100);
  ledcWrite(1, 0);

  ledcAttachPin(fwd1, 0);
  ledcAttachPin(rvs1, 1);
  ledcAttachPin(fwd2, 2);
  ledcAttachPin(rvs2, 3);
*/

void setAddress(String address) {
  myAddress = address.toInt();
}


// NEED TO ADJUST PWM
void sliderToVal(int val1, int val2) {
  int pos1 = analogRead(A2);
  int pos2 = analogRead(A3);
  Serial.println("pos1");
  Serial.println(pos1);
  Serial.println("pos2");
  Serial.println(pos2);
  //int value;

  int mapVal1 = map(val1, 0, 127, 0, 511);///
  int mapVal2 = map(val2, 0, 127, 0, 511);///

  int value1 = abs(pos1 - val1) * 0.15 + 240; // was 450
  if (value1 > 255) value1 = 255;
  int value2 = abs(pos2 - val2) * 0.15 + 240;
  if (value2 > 255) value2 = 255;
  Serial.println("Val 2");
  Serial.println(value2);
  Serial.println("Val 1");
  Serial.println(value1);
  if (abs(pos1 - val1) > 80) // was 2
  {
    if (pos1 > val1)
    {
      ledcWrite(0, 0);
      ledcWrite(1, value1);
      //digitalWrite(33, LOW);
      ///analogWrite(10, value);//
      //digitalWrite(32, HIGH);
    }
    else
    {
      ledcWrite(1, 0);
      ledcWrite(0, value1);
      //digitalWrite(32, LOW);
      //analogWrite(33, value);///
      //digitalWrite(33, HIGH);
    }
  }
  else
  {
    ledcWrite(1, 0);
    ledcWrite(0, 0);
    //digitalWrite(32, LOW);
    //digitalWrite(33, LOW);
  }

  if (abs(pos2 - val2) > 80) // was 2
  {
    if (pos2 > val2)
    {
      ledcWrite(2, 0);
      ledcWrite(3, value2);
      //digitalWrite(14, LOW);
      ///analogWrite(10, value);//
      //digitalWrite(15, HIGH);
    }
    else
    {
      ledcWrite(3, 0);
      ledcWrite(2, value2);
      //digitalWrite(15, LOW);
      //analogWrite(33, value);///
      //digitalWrite(14, HIGH);
    }
  }
  else
  {
    ledcWrite(3, 0);
    ledcWrite(2, 0);
    //digitalWrite(14, LOW);
    //digitalWrite(15, LOW);
  }
}

void SerialWriteIfChange()
{
  if (fader1Val != fader1Old || fader2Val != fader2Old || encoderValue != encoderCountOld || buttonpress != oldPress)
  {
    if (ctr2 % 1 == 0)
    {
      SerialBT.print(myAddress);
      SerialBT.print(",");
      SerialBT.print(fader1Val);
      SerialBT.print(",");
      SerialBT.print(fader2Val);
      SerialBT.print(",");
      // SerialBT.print (encoderCount);
      //SerialBT.print (",");
      SerialBT.print(buttonpress);
      SerialBT.print(",");
      SerialBT.print(encoderValue);
      //SerialBT.print (",");
      //SerialBT.println (bat);
      SerialBT.println();
    }
    fader1Old = fader1Val;
    fader2Old = fader2Val;

    encoderCountOld = encoderValue;
    oldPress = buttonpress;

    //    sliderToVal(desiredPos[0]);
    ctr2++;
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

void newHapticFunc(int k, int j) // k is the data set the first slider is mapping, and j is the data set the second slider is mapping
{
  int currPosition1 = analogRead(A2);
  int currPosition2 = analogRead(A3);
  //float data[nbData] = {0,5,10,100,100,10,5,0};

  // Map different data sets depending on the value of k
  if (k == 0)
  {
    for (int y = 0; y < nbData; y++)
    {
      data[y] = data0[y];
    }
  }
  else if (k == 1)
  {
    for (int y = 0; y < nbData; y++)
    {
      data[y] = data1[y];
    }
  }
  else if (k == 2)
  {
    for (int y = 0; y < nbData; y++)
    {
      data[y] = data2[y];
    }
  }

  // Define number of intervals needed
  int intervals = 4025 / (nbData - 1);
  int intervalCount = 0, Cnt = 0;

  // Iterate through past all the points already passed by the slider
  Cnt = currPosition1 / intervals;
  intervalCount = intervals * Cnt;
  // Find data value corresponding to current fader position
  int currData = data[Cnt];
  intervals = 4025 / (currData * 5);
  if (currData == 0) intervals = 4025 / 2;
  //intervalCount = 0
  if (intervals > 20) mov = false;

  // Snap to the nearest position-
  if (currPosition1 > (intervalCount + (intervals / 2) + 10) && !(currPosition1 > 4025 || currPosition1 < 1) && mov)
  {
    ledcWrite(0, 0);
    ledcWrite(1, 255);
  }
  else if (currPosition1 < (intervalCount + (intervals / 2) - 10) && !(currPosition1 > 4025 || currPosition1 < 1) && mov)
  {
    ledcWrite(0, 255);
    ledcWrite(1, 0);
  }
  else
  {
    ledcWrite(0, 0);
    ledcWrite(1, 0);
  }
  mov = true;

  //float data[nbData] = {0,5,10,100,100,10,5,0};

  // Map different data sets depending on the value of k
  if (j == 0)
  {
    for (int y = 0; y < nbData; y++)
    {
      data[y] = data0[y];
    }
  }
  else if (j == 1)
  {
    for (int y = 0; y < nbData; y++)
    {
      data[y] = data1[y];
    }
  }
  else if (j == 2)
  {
    for (int y = 0; y < nbData; y++)
    {
      data[y] = data2[y];
    }
  }

  // Define number of intervals needed
  intervals = 4025 / (nbData - 1);
  intervalCount = 0, Cnt = 0;

  // Iterate through past all the points already passed by the slider
  Cnt = currPosition2 / intervals;
  intervalCount = intervals * Cnt;
  // Find data value corresponding to current fader position
  currData = data[Cnt];
  intervals = 4025 / (currData * 5);
  if (currData == 0) intervals = 4025 / 2;
  //intervalCount = 0
  if (intervals > 20) mov = false;

  // Snap to the nearest position-
  if (currPosition2 > (intervalCount + (intervals / 2) + 10) && !(currPosition2 > 4025 || currPosition2 < 1) && mov)
  {
    ledcWrite(2, 0);
    ledcWrite(3, 255);
  }
  else if (currPosition2 < (intervalCount + (intervals / 2) - 10) && !(currPosition2 > 4025 || currPosition2 < 1) && mov)
  {
    ledcWrite(2, 255);
    ledcWrite(3, 0);
  }
  else
  {
    ledcWrite(2, 0);
    ledcWrite(3, 0);
  }
  mov = true;
}

void updateEncoder() {
  //SerialBT.print ("Called");

  interrupt_cnt++;
  int timeDiff;
  int inputB = ledcRead(encoder1); // Don't need to test both encoder inputs because A is already high, this interrupt was called on the rising edge.
  //Serial.println(inputB);
  int state = 0, counter = 0;
  int no = 20;
  int stat[no + 1];
  times[interrupt_cnt % 2] = millis();
  if ((interrupt_cnt % 2) == 0)
  {
    timeDiff = millis() - times[1];
  }
  else
  {
    timeDiff = millis() - times[0];
  }

  if (timeDiff > 20) {
    state = 1;
  }
  else state = 0;
  if (state == 1)
  {
    if (inputB == LOW)
    {
      encoderValue++;  // A leads B - B is low
      if (encoderValue > 24)
      {
        encoderValue = 0;
      }
    }
    else
    {
      encoderValue--;          // B leads A - B was already high.
      if (encoderValue <= -1)
      {
        encoderValue = 24;
      }
    }
    state = 0;
  }

  //encoderValue = 70;
  //Serial.println(encoderValue); //  Right hand connector test ok.
}


void discreteAxes(int steps1, int steps2)
{
  Serial.println("CALLED");
  int currPosition1 = analogRead(A2);
  int currPosition2 = analogRead(A3);

  int value;
  int Cnt2 = currPosition1 / (int)(4024 / steps1);
  int intervalCount2 = (4025 / steps1) * Cnt2;
  int val2 = (intervalCount2 + ((4024 / steps1) / 2));
  Serial.println(intervalCount2);

  if (currPosition1 > (intervalCount2 + ((4024 / steps1) / 2) + 60))
  {
    ledcAttachPin(0, 255);
    ledcAttachPin(1, 0);
   // Serial.println("Here1");
  }
  else if (currPosition1 < (intervalCount2 + ((4024 / steps1) / 2) - 60))
  {
    ledcAttachPin(1, 255);
    ledcAttachPin(0, 0);
    //Serial.println("Here2");
  }
  else
  {
    ledcAttachPin(1, 0);
    ledcAttachPin(0, 0);
    //Serial.println("IN ELSE");
  }
/*
  Cnt2 = currPosition2 / (4025 / steps2);
  intervalCount2 = (4025 / steps2) * Cnt2;
  val2 = (intervalCount2 + ((4025 / steps2) / 2));

  if (currPosition2 > (intervalCount2 + ((4025 / steps2) / 2) + 60))
  {
    ledcAttachPin(2, 255);
    ledcAttachPin(3, 0);
  }
  else if (currPosition2 < (intervalCount2 + ((4025 / steps2) / 2) - 60))
  {
    ledcAttachPin(3, 255);
    ledcAttachPin(2, 0);
  }
  else
  {
    ledcAttachPin(2, 0);
    ledcAttachPin(3, 0);
  }

*/
}


int asciiReader(char symbol)
{
  int number;
  int asciiNo = symbol;
  number = asciiNo - 48;
  return number;
}


void setup() {
  Serial.begin(2000000);
  SerialBT.begin("FaderAxisSINGLE_ESP"); //Change this for each unit
  pinMode(encoder1, INPUT_PULLUP);
  pinMode(encoder2, INPUT_PULLUP);
  pinMode(pushbutton, INPUT_PULLUP);
  pinMode(13, OUTPUT);
  attachInterrupt(digitalPinToInterrupt(encoder2), updateEncoder, RISING);////////////////////////////////

  //analogReadResolution(4); // can set this all the way to 12bits of resolution
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

void loop() {
  SerialBT.println("SECOND AXIS HERE");
  //sliderToVal(502, 10);
  //Serial.println(encoderValue);
  //Serial.println(analogRead(A3));
  //Serial.println(analogRead(A2));
  bat = analogRead(batVoltage);
  //buttonpress = digitalRead(pushbutton);

  // Rotary Encoder Switch
  if (digitalRead(pushbutton)) {
    buttonpress = 0;
  }
  else {
    buttonpress = 1;
  }

  ledcWrite(4, map(bat, 230, 273, 10, 255 ) );  // pwm output for battery level led
  ReadAndAverageInputs();

  SerialWriteIfChange();
  //ctr2++;

  //delay(3);
  //Serial.println("In UPDATE Loop");
  if (SerialBT.available() > 0) {
    //digitalWrite(13, HIGH);
    if (mssgRcvCount == 0)
    {
      message = Serial.readStringUntil('\n');
      myAddress = message[0] - '0';
      mssgRcvCount++;

    }
    else
    {
      Serial.println("Serial Availible");
      //digitalWrite(13, HIGH);
      message = SerialBT.readStringUntil('\n');
      old_id = id;

      mode = 0;
      //      digit1 = message[0] - '0';
      //      digit2 = message[1] - '0';
      //      digit3 = message[2] - '0';
      //      digit4 = message[3] - '0';
      //      digit5 = message[4] - '0';
      //      digit6 = message[5] - '0';
      //      digit7 = message[6] - '0';
      //      digit8 = message[7] - '0';
      //      digit9 = message[8] - '0';
      //      digit10 = message[9] - '0';
      //      digit11 = message[10] - '0';
      //      digit12 = message[11] - '0';
      digit1 = asciiReader(message[0]);
      digit2 = asciiReader(message[1]);
      digit3 = asciiReader(message[2]);
      digit4 = asciiReader(message[3]);
      digit5 = asciiReader(message[4]);
      digit6 = asciiReader(message[5]);
      digit7 = asciiReader(message[6]);
      digit8 = asciiReader(message[7]);
      digit9 = asciiReader(message[8]);
      digit10 = asciiReader(message[9]);
      digit11 = asciiReader(message[10]);
      digit12 = asciiReader(message[11]);
      //digit13= asciiReader(message[12]); ///
      //digit12 = asciiReader(message[12]);
      Serial.println(digit1);
      Serial.println(digit2);
      Serial.println(digit3);
      Serial.println(digit4);
      Serial.println(digit5);
      Serial.println(digit6);
      Serial.println(digit7);
      Serial.println(digit8);
      Serial.println(digit9);
      Serial.println(digit10);
      Serial.println(digit11);
      //Serial.println(digit12);
      Serial.println("-----------");
      val1 = digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4;
      val2 = digit5 * 1000 + digit6 * 100 + digit7 * 10 + digit8;
      Serial.println(val1);
      Serial.println(val2);
      Serial.println(analogRead(A2));
      Serial.println(analogRead(A3));
      //Serial.println(digit9);
      //Serial.println(digit2);
      //Serial.println(message[9].toInt());
      hapticsStat = digit9;
      Serial.println("HAPTICS STAT");
      Serial.println(hapticsStat);
      xStep = digit10;
      yStep = digit11;
      zStep = digit12;
      /*
        if (hapticsStat == 0)
        {
        //val1 = 100; val2 = 100;
        sliderToVal(val1, val2);
        }
        else if (hapticsStat == 1)
        {
        newHapticFunc(2, 2); // Must change for each ESP
        }
        else if (hapticsStat == 2)
        {
        discreteAxes(zStep); // must change from unity side
        }
      */
    }
  }
  if (hapticsStat == 0)
  {
    //val1 = 100; val2 = 100;
    sliderToVal(val1, val2);
  }
  else if (hapticsStat == 1)
  {
    newHapticFunc(2, 2); // Must change for each ESP
  }
  else if (hapticsStat == 2)
  {
    discreteAxes(4, 4); // must change from unity side
  }
}
