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
int val[7] = {500, 500, 500, 500, 500, 500, 500};
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
int yAxis[2] = { 0, 1 }, xAxis[2] = { 3, 2 }, zAxis[2] = { 5, 4 };
int disc_val;

const float minForce = 256;
const float maxForce = 50;
const int nbData = maxForce;
float data[nbData];
bool mov = true;
bool isFollowing;
const int nbAxe = 3;
int nbStepAxe[nbAxe];

// Data sets for use in mapping haptics
float data0[nbData] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10 , 10 , 10 , 10, 10, 10 , 10, 10, 0, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0}; //{0, 1, 2, 3, 4, 4, 4, 0};
float data1[nbData] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10 , 10 , 10 , 10, 10, 10 , 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0}; // 10 replaced 4
float data2[nbData] = {0, 0, 0, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 , 10 , 10 , 10, 10, 200 , 200, 200, 200, 200, 200, 200, 200, 200, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 0, 0}; //{0, 1, 2, 3, 4, 4, 4, 2, 1, 0};
bool corr_loc[3][2] = {{false, false},
  {false, false},
  {false, false}
};
void newHapticFunc(int k)
{
  int currPosition = analogRead(A0);
  //float data[nbData] = {0,5,10,100,100,10,5,0};

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

  int intervals = 1023 / (nbData - 1);
  int intervalCount = 0, Cnt = 0;

  Cnt = currPosition / intervals;
  intervalCount = intervals * Cnt;
  int currData = data[Cnt];
  intervals = 1023 / (currData * 5);
  if (currData == 0) intervals = 1023 / 2;
  //intervalCount = 0
  if (intervals > 20) mov = false;
  if (currPosition > (intervalCount + (intervals / 2) + 3) && !(currPosition > 1022 || currPosition < 1) && mov)
  {
    digitalWrite(9, LOW);
    digitalWrite(10, HIGH);
  }
  else if (currPosition < (intervalCount + (intervals / 2) - 3) && !(currPosition > 1022 || currPosition < 1) && mov)
  {
    digitalWrite(9, HIGH);
    digitalWrite(10, LOW);
  }
  else
  {
    digitalWrite(9, LOW);
    digitalWrite(10, LOW);
  }
  mov = true;
}

void discreteAxes(int steps)
{
  int currPosition = analogRead(A0);
  int value;
  int Cnt2 = currPosition / (1023 / steps);
  int intervalCount2 = (1023 / steps) * Cnt2;
  int val2 = (intervalCount2 + ((1023 / steps) / 2));

  if (currPosition > (intervalCount2 + ((1023 / steps) / 2) + 25))
  {
    digitalWrite(9, HIGH);
    digitalWrite(10, LOW);
  }
  else if (currPosition < (intervalCount2 + ((1023 / steps) / 2) - 25))
  {
    digitalWrite(10, HIGH);
    digitalWrite(9, LOW);
  }
  else
  {
    digitalWrite(10, LOW);
    digitalWrite(9, LOW);
  }
}

void sliderToVal(int val) {
  int pos = analogRead(A0);
  int value;
  value = abs(pos - val) * 2 + 450; // was 450
  if (abs(pos - val) > 4) // was 2
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
  if (!isMaster && (Wire.available() == 2))
  {
    int place = 0;
    byte recData[2];

    while (Wire.available())
    {
      recData[place] = Wire.read();
      place++;
    }
    int full_val = (recData[1] << 8) + recData[0];
    if (full_val < 1024)
    {
      sliderToVal(full_val);
    }
    else if ((full_val >= 1500) && (full_val <= 1600))
    {
      int disc_val = full_val - 1500;
      discreteAxes(disc_val);
    }
    else if (full_val >= 2000 && full_val < 2010)
    {
      int data_set = full_val - 2000;
      newHapticFunc(data_set);
    }
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
    digitalWrite(setIndexString, HIGH);  // turns on fet to send 5v at top of index string.
    Serial.begin(2000000);
    Wire.begin();
    //Wire.setClock(4000000);
    delay(7000);
    getAddresses();
    Serial.println(usbRead);
  }
  else //slave mode
  {
    isMaster = false;
    digitalWrite(setIndexString, LOW);
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
  /*
    for (int k = 0; k < (nDevices + 1); k++)
    {
    Serial.println(addresses[k], DEC);
    }
  */
  if (isMaster)
  {
    if (Serial.available() > 0) {
      message = Serial.readStringUntil('\n');
      old_id = id;
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
      else if (id == 6) {
        for (int i = 0; i < nbSlider; i++) {
          digitalWrite(9, LOW);
          digitalWrite(10, LOW);
          moveSlider[i] = false;
          mode = 0;
          isFollowing = false;
        }
      }
      else if (id == 7 || old_id == 7) {
        mode = 1;
        //indice0 = 0;
        //indice1 = 0;
        //indice2 = 0;
        //fillHaptic();
        ///////////////////
        if (isMaster)
        {
          newHapticFunc(2);
        }
        for (int k = 1; k < (nDevices + 1); k++)
        {
          if (k == 1)
          {
            int num = 2000 + 2;
            byte hb = highByte(num);
            byte lb = lowByte(num);
            Wire.beginTransmission(addresses[nDevices]);
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }

          else if (k == 2)
          {
            int num = 2000;
            byte hb = highByte(num);
            byte lb = lowByte(num);
            if (nDevices > 3) {
              Wire.beginTransmission(addresses[3]);
            }
            else {
              Wire.beginTransmission(addresses[1]);
            }
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }

          else if (k == 3)
          {
            int num = 2000;
            byte hb = highByte(num);
            byte lb = lowByte(num);
            if (nDevices > 3) {
              Wire.beginTransmission(addresses[4]);
            }
            else {
              Wire.beginTransmission(addresses[2]);
            }
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }

          else if (k == 4)
          {
            int num = 2000 + 1;
            byte hb = highByte(num);
            byte lb = lowByte(num);
            if (nDevices > 3) {
              Wire.beginTransmission(addresses[1]);
            }
            else {
              Wire.beginTransmission(addresses[3]);
            }
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }

          else if (k == 5)
          {
            int num = 2000 + 1;
            byte hb = highByte(num);
            byte lb = lowByte(num);
            if (nDevices > 3) {
              Wire.beginTransmission(addresses[2]);
            }
            else {
              Wire.beginTransmission(addresses[4]);
            }
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }
        }
      }
      else if (id == 8 || old_id == 8) {

        mode = 1;
        // indice0 = 0;
        //indice1 = 0;
        //indice2 = 0;
        for (int i = 0; i < nbAxe; i++) {
          digit1 = message[2 + 5 * i] - '0';
          digit2 = message[3 + 5 * i] - '0';
          digit3 = message[4 + 5 * i] - '0';
          digit4 = message[5 + 5 * i] - '0';
          nbStepAxe[i] = digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4;
          nbStepAxe[i] += 1500;
          //nbStepAxe[i] = abs(nbStepAxe[i]) % 256;
        }
        //fillRegularStep(nbStepAxe[0], nbStepAxe[1], nbStepAxe[2]);
        //discreteAxes(nbStepAxe[0], nbStepAxe[1], nbStepAxe[2]); // x, y, z
        discreteAxes(nbStepAxe[2] - 1500);
        for (int k = 1; k < (nDevices + 1); k++)
        {
          if (k == 1)
          {
            byte hb = highByte(nbStepAxe[2]);
            byte lb = lowByte(nbStepAxe[2]);
            Wire.beginTransmission(addresses[nDevices]);
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }

          else if (k == 2)
          {
            byte hb = highByte(nbStepAxe[0]);
            byte lb = lowByte(nbStepAxe[0]);
            if (nDevices > 3) {
              Wire.beginTransmission(addresses[3]);
            }
            else {
              Wire.beginTransmission(addresses[1]);
            }
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }

          else if (k == 3)
          {
            byte hb = highByte(nbStepAxe[0]);
            byte lb = lowByte(nbStepAxe[0]);
            if (nDevices > 3) {
              Wire.beginTransmission(addresses[4]);
            }
            else {
              Wire.beginTransmission(addresses[2]);
            }
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }

          else if (k == 4)
          {
            byte hb = highByte(nbStepAxe[1]);
            byte lb = lowByte(nbStepAxe[1]);
            if (nDevices > 3) {
              Wire.beginTransmission(addresses[1]);
            }
            else {
              Wire.beginTransmission(addresses[3]);
            }
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();
          }

          else if (k == 5)
          {
            byte hb = highByte(nbStepAxe[1]);
            byte lb = lowByte(nbStepAxe[1]);
            if (nDevices > 3) {
              Wire.beginTransmission(addresses[2]);
            }
            else {
              Wire.beginTransmission(addresses[4]);
            }
            Wire.write(lb);
            Wire.write(hb);
            tStat = Wire.endTransmission();

          }
        }
      }
      /*
        else if (id == 9) {
        followedSlider = message[3] - '0';
        followedSlider = followedSlider % 6;
        followingSlider = message[4] - '0';
        followingSlider = followingSlider % 6;
        sign = message[5] - '0';
        digit1 = message[7] - '0';
        digit2 = message[8] - '0';
        digit3 = message[9] - '0';
        digit4 = message[10] - '0';
        dist = digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4;
        isFollowing = true;
        mode = 1;
        }
      */
    }
    /*
      for (int b = 0; b < nbSlider; b++)
      {
      sliderToVal(b, desiredPos[b]);
      }
    */

    for (int i = 0; i < (nDevices + 1); i++)
    {
      if (mode == 0)
      {
        int devices_diff = nDevices - 3;
        if (i == 0)
        {
          int master_data = val[zAxis[0]];
          sliderToVal(master_data);
        }
        else if (i == 1)
        {
          // sent_data = map(val[zAxis[1]], 0, 1023, 0, 255);
          byte hb = highByte(val[zAxis[1]]);//val[zAxis[1]]
          byte lb = lowByte(val[zAxis[1]]);//val[zAxis[1]]
          Wire.beginTransmission(addresses[nDevices]);
          Wire.write(lb);
          Wire.write(hb);
          tStat = Wire.endTransmission();
        }
        else if (i == 2)
        {
          //sent_data = map(val[xAxis[0]], 0, 1023, 0, 255);
          byte hb = highByte(val[xAxis[0]]);
          byte lb = lowByte(val[xAxis[0]]);
          if (nDevices > 3) {
            Wire.beginTransmission(addresses[3]);
          }
          else {
            Wire.beginTransmission(addresses[1]);
          }
          Wire.write(lb);
          Wire.write(hb);
          tStat = Wire.endTransmission();
        }
        else if (i == 3)
        {
          // sent_data = map(val[xAxis[1]], 0, 1023, 0, 255);
          byte hb = highByte(val[xAxis[1]]);
          byte lb = lowByte(val[xAxis[1]]);
          if (nDevices > 3) {
            Wire.beginTransmission(addresses[4]);
          }
          else {
            Wire.beginTransmission(addresses[2]);
          }
          Wire.write(lb);
          Wire.write(hb);
          tStat = Wire.endTransmission();
        }
        else if (i == 4)
        {
          // sent_data = map(val[yAxis[0]], 0, 1023, 0, 255);
          byte hb = highByte(val[yAxis[0]]);
          byte lb = lowByte(val[yAxis[0]]);
          if (nDevices > 3)
          {
            Wire.beginTransmission(addresses[1]);
          }
          else
          {
            Wire.beginTransmission(addresses[3]);
          }
          Wire.write(lb);
          Wire.write(hb);
          tStat = Wire.endTransmission();
        }
        else if (i == 5)
        {
          // sent_data = map(val[yAxis[1]], 0, 1023, 0, 255);
          byte hb = highByte(val[yAxis[1]]);
          byte lb = lowByte(val[yAxis[1]]);
          if (nDevices > 3)
          {
            Wire.beginTransmission(addresses[2]);
          }
          else
          {
            Wire.beginTransmission(addresses[4]);
          }
          Wire.write(lb);
          Wire.write(hb);
          tStat = Wire.endTransmission();
        }
        /*
          else if (i == (nDevices + 1))
          {
          sent_data = map(val[xAxis[1]], 0, 1023, 0, 255);
          Wire.beginTransmission(addresses[i]);
          Wire.write(sent_data);
          tStat = Wire.endTransmission();
          }
        */
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
        Serial.println(map(analogRead(A0), 0, 1023, 0, 255));
        for (int v = 1; v < nDevices + 1; v++)
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
