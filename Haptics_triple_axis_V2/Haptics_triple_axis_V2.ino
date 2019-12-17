#include <Arduino.h>
bool mov = true;
int old_id = 7;
const int nbSlider = 6;
const int numReadings = 20;
int readIndex = 0, steps;
int tabPos[nbSlider][numReadings];
float pos[nbSlider];
int slider[nbSlider] = {A0, A1, A2, A3, A4, A5};
int motorSwitch[6] = {7, 3, 26, 45, 8, 46};
int motorPinPlus[6] = {5, 9, 28, 40, A8, 48};
int motorPinMinus[6] = {2, 6, 38, 30, A13, 44};
const int nbAxe = 3;
int ROT_B[nbAxe] = {21, 3, 19};
int ROT_A[nbAxe] = {20, 2, 18};
int buttonSwitch[nbAxe] = {17, 10, 22};
const float minForce = 256;
const float maxForce = 50;
const int resolution = minForce;
const int maxStep = maxForce;
float listPosf0[resolution + 1];
int listPos0[resolution + 1];
float listPosf1[resolution + 1];
int listPos1[resolution + 1];
float listPosf2[resolution + 1];
int listPos2[resolution + 1];
const int nbData = maxForce;
//float data0[nbData] = {3, 6, 9, 12, 15, 18, 21, 60, 63, 66, 69, 72, 75, 78, 81};
//float data1[nbData] = {3, 6, 9, 12, 15, 18, 21, 60, 63, 66, 69, 72, 75, 78, 81};
//float data2[nbData] =  {3, 6, 9, 12, 15, 18, 21, 60, 63, 66, 69, 72, 75, 78, 81};
float data0[nbData] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10 , 10 , 10 , 10, 10, 10 , 10, 10, 0, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0}; //{0, 1, 2, 3, 4, 4, 4, 0};
float data1[nbData] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10 , 10 , 10 , 10, 10, 10 , 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0}; // 10 replaced 4
float data2[nbData] = {0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 , 0 , 0 , 0, 200, 200 , 200, 200, 200, 200, 200, 200, 200, 200, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0}; //{0, 1, 2, 3, 4, 4, 4, 2, 1, 0};

float minData;
float maxData;
int indice0, indice1, indice2 = 0;
int mode = 0;
volatile int lastEncoded[nbAxe] = {0, 0, 0};
volatile long encoderValue[nbAxe] = {0, 0, 0};
int digit1;
int digit2;
int digit3;
int digit4;
int val[nbSlider] = {0, 0, 0, 0, 0, 0};
int nbStepAxe[nbAxe] = {0, 0, 0};
int id;
int followingSlider;
int followedSlider;
int dist;
int sign;
String message;
bool moveSlider[nbSlider] = {false, false, false, false, false, false};
bool isFollowing = false;
float data[nbData];
void setup() {
  Serial.begin(2000000);
  /*
    for (int i = 0; i < nbData; i++)
    {
    data0[i] = random(0,10);
    }
    for (int i = 0; i < nbData; i++)
    {
    data1[i] = random(0,10);
    }
    for (int i = 0; i < nbData; i++)
    {
    data2[i] = random(0,10);
    }
  */
  //---------------------------------------------- Set PWM frequency for D4 & D13 ------------------------------
  TCCR0B = TCCR0B & B11111000 | B00000001;    // set timer 0 divisor to     1 for PWM frequency of 62500.00 Hz
  //---------------------------------------------- Set PWM frequency for D11 & D12 -----------------------------
  TCCR1B = TCCR1B & B11111000 | B00000001;    // set timer 1 divisor to     1 for PWM frequency of 31372.55 Hz
  //---------------------------------------------- Set PWM frequency for D9 & D10 ------------------------------
  TCCR2B = TCCR2B & B11111000 | B00000001;    // set timer 2 divisor to     1 for PWM frequency of 31372.55 Hz
  //---------------------------------------------- Set PWM frequency for D2, D3 & D5 ---------------------------
  TCCR3B = TCCR3B & B11111000 | B00000001;    // set timer 3 divisor to     1 for PWM frequency of 31372.55 Hz
  //---------------------------------------------- Set PWM frequency for D6, D7 & D8 ---------------------------
  TCCR4B = TCCR4B & B11111000 | B00000001;    // set timer 4 divisor to     1 for PWM frequency of 31372.55 Hz
  //---------------------------------------------- Set PWM frequency for D44, D45 & D46 ------------------------
  TCCR5B = TCCR5B & B11111000 | B00000001;    // set timer 5 divisor to     1 for PWM frequency of 31372.55 Hz



  for (int i = 0; i < nbSlider; i++)
  {
    pinMode(slider[i], INPUT);
    pinMode(motorSwitch[i], OUTPUT);
    pinMode(motorPinPlus[i], OUTPUT);
    pinMode(motorPinMinus[i], OUTPUT);
  }

  for (int i = 0; i < nbAxe; i++)
  {
    pinMode(ROT_B[i], INPUT);
    pinMode(ROT_A[i], INPUT);
    pinMode(buttonSwitch[i], INPUT);
    digitalWrite(ROT_B[i], HIGH);
    digitalWrite(ROT_A[i], HIGH);
    digitalWrite(buttonSwitch[i], HIGH);
  }
  //attachInterrupt(0, updateEncoder1, CHANGE);
  //attachInterrupt(1, updateEncoder1, CHANGE);
  // attachInterrupt(4, updateEncoder2, CHANGE);
  // attachInterrupt(5, updateEncoder2, CHANGE);
  // attachInterrupt(2, updateEncoder0, CHANGE);
  // attachInterrupt(3, updateEncoder0, CHANGE);
}
//________________________________________________________________________________________________
void fillStep(int axe, float currentForce) {
  if (axe == 0) {
    for (int i = 0; i < int(currentForce / maxForce); i++) {
      listPosf0[indice0 + 1] = listPosf0[indice0] + 1024 / currentForce; //add the next position
      indice0 += 1;
    }
  }
  if (axe == 1) {
    for (int i = 0; i < int(currentForce / maxForce); i++) {
      listPosf1[indice1 + 1] = listPosf1[indice1] + 1024 / currentForce; //add the next position
      indice1 += 1;
    }
  }
  if (axe == 2) {
    for (int i = 0; i < int(currentForce / maxForce); i++) {
      listPosf2[indice2 + 1] = listPosf2[indice2] + 1024 / currentForce; //add the next position
      indice2 += 1;
    }
  }
}
void mindata() {
  minData = data0[0];
  for (int i = 0; i < nbData; i++) {
    if (data0[i] < minData) {
      minData = data0[i];
    }
  }
}
void maxdata() {
  maxData = data0[0];
  for (int i = 0; i < nbData; i++) {
    if (data0[i] > maxData) {
      maxData = data0[i];
    }
  }
}
void fillHaptic() {
  //fill axe 0 - X (haptic)
  listPosf0[0] = 0;
  mindata();
  maxdata();
  for (int i = 0; i < maxStep; i++) {
    float currentForce = map(data0[i], minData, maxData, minForce, maxForce); //the corresponding force to the data
    fillStep(0, currentForce); //fill the positions where the slider don't move for one of the 10 parts
    if (listPosf0[indice0] + 1024 / currentForce <= (i + 1) * 1024 / maxForce) {
      listPosf0[indice0 + 1] = listPosf0[indice0] + 1024 / currentForce;
      indice0 += 1;
    }
  }
  listPosf0[indice0 + 1] = 1023;
  for (int i = 0; i < indice0 + 2; i++) {
    listPos0[i] = listPosf0[i];
  }
  //fill axe 1 - Y (haptic)
  listPosf1[0] = 0;
  for (int i = 0; i < maxStep; i++) {
    float currentForce = map(data1[i], minData, maxData, minForce, maxForce); //the corresponding force to the data
    fillStep(1, currentForce); //fill the positions where the slider don't move for one of the 10 parts
    if (listPosf1[indice1] + 1024 / currentForce <= (i + 1) * 1024 / maxForce) {
      listPosf1[indice1 + 1] = listPosf1[indice1] + 1024 / currentForce;
      indice1 += 1;
    }
  }
  listPosf1[indice1 + 1] = 1023;
  for (int i = 0; i < indice1 + 2; i++) {
    listPos1[i] = listPosf1[i];
  }
  //fill axe 2 - Z (haptic)
  listPosf2[0] = 0;
  for (int i = 0; i < maxStep; i++) {
    float currentForce = map(data2[i], minData, maxData, minForce, maxForce); //the corresponding force to the data
    fillStep(2, currentForce); //fill the positions where the slider don't move for one of the 10 parts
    if (listPosf2[indice2] + 1024 / currentForce <= (i + 1) * 1024 / maxForce) {
      listPosf2[indice2 + 1] = listPosf2[indice2] + 1024 / currentForce;
      indice2 += 1;
    }
  }
  listPosf2[indice2 + 1] = 1023;
  for (int i = 0; i < indice2 + 2; i++) {
    listPos2[i] = listPosf2[i];
  }
}
//________________________________________________________________________________________________
void fillRegularStep(int nbStepX, int nbStepY, int nbStepZ) {
  indice0 = nbStepX + 1;
  indice1 = nbStepY + 1;
  indice2 = nbStepZ + 1;
  for (int i = 0; i < nbStepX; i++) {
    listPos0[i] = i * (1024 / nbStepX);
  }
  for (int i = 0; i < nbStepY; i++) {
    listPos1[i] = i * (1024 / nbStepY);
  }
  for (int i = 0; i < nbStepZ; i++) {
    listPos2[i] = i * (1024 / nbStepZ);
  }
  listPos0[indice0 - 1] = 1023;
  listPos1[indice1 - 1] = 1023;
  listPos2[indice2 - 1] = 1023;
  listPos0[indice0] = 1023;
  listPos1[indice1] = 1023;
  listPos2[indice2] = 1023;
}
//________________________________________________________________________________________________
void scale(int id) {
  int value;
  int sUncert = 60;
  int scale = 50;
  if (id == 0 || id == 1) {
    int val = analogRead(slider[id]);
    for (int i = 0; i < indice0 + 1; i++) {
      value = 1023 - (scale * data0[i]);
      if (val >= listPos0[i] - (listPos0[i] - listPos0[i - 1]) / 2 && val < listPos0[i] + (listPos0[i + 1] - listPos0[i]) / 2) { //from which integer of listPos the slider the closer
        //go to this position
        if (val > listPos0[i] + sUncert) {
          analogWrite(motorSwitch[id], value);
          digitalWrite(motorPinPlus[id], HIGH);
          digitalWrite(motorPinMinus[id], LOW);
        }
        else if (val < listPos0[i] - sUncert) {
          analogWrite(motorSwitch[id], value);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], HIGH);
        }
        else {
          analogWrite(motorSwitch[id], value);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], LOW);
        }
      }
    }
  }
  else if (id == 2 || id == 3) {
    int val = analogRead(slider[id]);
    for (int i = 0; i < indice1 + 1; i++) {
      value = 1023 - (scale * data0[i]);
      if (val >= listPos1[i] - (listPos1[i] - listPos1[i - 1]) / 2 && val < listPos1[i] + (listPos1[i + 1] - listPos1[i]) / 2) { //from which integer of listPos the slider the closer
        //go to this position
        if (val > listPos1[i] + sUncert) {
          analogWrite(motorSwitch[id], value);
          digitalWrite(motorPinPlus[id], HIGH);
          digitalWrite(motorPinMinus[id], LOW);
        }
        else if (val < listPos1[i] - sUncert) {
          analogWrite(motorSwitch[id], value);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], HIGH);
        }
        else {
          analogWrite(motorSwitch[id], value);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], LOW);
        }
      }
    }
  }
  else if (id == 4 || id == 5) {
    int val = analogRead(slider[id]);

    for (int i = 0; i < indice2 + 1; i++) {
      value = 1023 - (scale * data0[i]);
      if (val >= listPos2[i] - (listPos2[i] - listPos2[i - 1]) / 2 && val < listPos2[i] + (listPos2[i + 1] - listPos2[i]) / 2) { //from which integer of listPos the slider the closer
        //go to this position
        if (val > listPos2[i] + sUncert) {
          //analogWrite(motorSwitch[id], value);
          digitalWrite(motorSwitch[id], HIGH);
          digitalWrite(motorPinPlus[id], HIGH);
          digitalWrite(motorPinMinus[id], LOW);
        }
        else if (val < listPos2[i] - sUncert) {
          //analogWrite(motorSwitch[id], value);
          digitalWrite(motorSwitch[id], HIGH);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], HIGH);
        }
        else {
          //analogWrite(motorSwitch[id], value);
          digitalWrite(motorSwitch[id], LOW);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], LOW);
        }
      }
    }
  }
}
//followingSlider follows followedSlider by a distance of dist (dist can be positive or negative)
void follow(int followedSlider, int followingSlider, int dist) {
  if (followedSlider != followingSlider) {
    int val = analogRead(slider[followedSlider]);
    if (dist < 1024 && dist > -1024) {
      if (val <= 1023 - dist && val >= 0 - dist) {
        sliderToVal(followingSlider, val + dist);
      }
      else if (val > 1023 - dist && dist >= 0) {
        sliderToVal(followingSlider, 1023);
      }
      else if (val < 0 - dist && dist <= 0) {
        sliderToVal(followingSlider, 0);
      }
    }
  }
}
//________________________________________________________________________________________________

void sliderToVal(int id, int val) {
  int pos = analogRead(slider[id]);
  int value;
  if (abs(pos - val) > 4)
  {
    if (id == 2 || id == 3) {
      value = abs(pos - val) * 10 + 1000;
    }
    if (id == 0 || id == 1) {
      value = abs(pos - val) * 10 + 850;
    }
    if (id == 4 || id == 5) {
      value = abs(pos - val) * 10 + 850;
    }
    if (value > 1023)
    {
      value = 1023;
    }

    if (pos < val)
    {
      analogWrite(motorPinPlus[id], value);
      digitalWrite(motorPinMinus[id], LOW);
      //analogWrite(motorSwitch[id], value);
      digitalWrite(motorSwitch[id], HIGH);
    }
    else
    {
      digitalWrite(motorPinPlus[id], LOW);
      analogWrite(motorPinMinus[id], value);
      //analogWrite(motorSwitch[id], value);
      digitalWrite(motorSwitch[id], HIGH);
    }
  }
  else
  {
    digitalWrite(motorPinPlus[id], LOW);
    digitalWrite(motorPinMinus[id], LOW);
    //analogWrite(motorSwitch[id], value);
    digitalWrite(motorSwitch[id], LOW);
    moveSlider[id] = false;
  }
}

void newHapticFunc()
{
  for (int k = 0; k < 6; k++)
  {
    int currPosition = analogRead(slider[k]);
    //float data[nbData] = {0,5,10,100,100,10,5,0};
    if (k == 0 || k == 1)
    {
      for (int y = 0; y < nbData; y++)
      {
        data[y] = data0[y];
      }
    }
    else if (k == 2 || k == 3)
    {
      for (int y = 0; y < nbData; y++)
      {
        data[y] = data1[y];
      }
    }
    else if (k == 4 || k == 5)
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
    if (k == 3) Serial.println(currData); Serial.println("                ");
    intervals = 1023 / (currData*5);
    if (currData == 0) intervals = 1023 / 2;
    //intervalCount = 0
    if (intervals > 20) mov = false;
    if (currPosition > (intervalCount + (intervals / 2) + 3) && !(currPosition > 1022 || currPosition < 1) && mov)
    {
      digitalWrite(motorSwitch[k], HIGH);
      digitalWrite(motorPinPlus[k], HIGH);
      digitalWrite(motorPinMinus[k], LOW);
      Serial.println("LEFT");
    }
    else if (currPosition < (intervalCount + (intervals / 2) - 3) && !(currPosition > 1022 || currPosition < 1) && mov)
    {
      digitalWrite(motorSwitch[k], HIGH);
      digitalWrite(motorPinPlus[k], LOW);
      digitalWrite(motorPinMinus[k], HIGH);
      Serial.println("RIGHT");
    }
    else
    {
      digitalWrite(motorSwitch[k], LOW);
      digitalWrite(motorPinPlus[k], LOW);
      digitalWrite(motorPinMinus[k], LOW);
    }
    mov = true;
  }
}


//
//void sliderToVal(int id, int val) {
//  val = abs(val % 1024);
//  int pos = analogRead(slider[id]);
//  if (abs(pos - val) > 20) {
//    if (pos > val) {
//      digitalWrite(motorPinPlus[id], HIGH);
//      digitalWrite(motorPinMinus[id], LOW);
//    }
//    else {
//      digitalWrite(motorPinPlus[id], LOW);
//      digitalWrite(motorPinMinus[id], HIGH);
//    }
//    analogWrite(motorSwitch[id], 255);
//  }
//  else {
//    digitalWrite(motorPinPlus[id], LOW);
//    digitalWrite(motorPinMinus[id], LOW);
//    analogWrite(motorSwitch[id], 0);
//    moveSlider[id] = false;
//  }
//}
//________________________________________________________________________________________________
void loop() {
  //Serial.println(analogRead(A0));
  for (int i = 0; i < nbSlider; i++) {
    tabPos[i][readIndex] = analogRead(slider[i]);
    pos[i] = 0;
  }
  readIndex = (readIndex + 1) % numReadings;
  for (int i = 0; i < nbSlider; i++) {
    for (int j = 0; j < numReadings; j++) {
      pos[i] += tabPos[i][j];
    }
    pos[i] /= numReadings;
  }
  for (int i = 0; i < nbSlider; i++) {
    Serial.print(round(pos[i]));
    Serial.print(" ");
  }
  for (int i = 0; i < 3; i++) {
    Serial.print(encoderValue[i]);
    Serial.print(" ");
    if (digitalRead(buttonSwitch[i])) {
      Serial.print("0");
    }
    else {
      Serial.print("1");
    }
    Serial.print(" ");
  }
  for (int i = 0; i < nbSlider; i++) {
    Serial.print(moveSlider[i]);
    Serial.print(" ");
  }
  Serial.print("\n");
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
    }
    else if (id == 6) {
      for (int i = 0; i < nbSlider; i++) {
        analogWrite(motorSwitch[i], 0);
        digitalWrite(motorPinPlus[i], LOW);
        digitalWrite(motorPinMinus[i], LOW);
        moveSlider[i] = false;
        mode = 0;
        isFollowing = false;
      }
    }
    else if (id == 7 || old_id == 7) {
      //mode = 1;
      //indice0 = 0;
      //indice1 = 0;
      //indice2 = 0;
      //fillHaptic();
      ///////////////////
      newHapticFunc();
    }
    else if (id == 8) {
      /*
        mode = 1;
        indice0 = 0;
        indice1 = 0;
        indice2 = 0;
        for (int i = 0; i < nbAxe; i++) {
        digit1 = message[2 + 5 * i] - '0';
        digit2 = message[3 + 5 * i] - '0';
        digit3 = message[4 + 5 * i] - '0';
        digit4 = message[5 + 5 * i] - '0';
        nbStepAxe[i] = digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4;
        nbStepAxe[i] = abs(nbStepAxe[i]) % 256;
        }
        fillRegularStep(nbStepAxe[0], nbStepAxe[1], nbStepAxe[2]);
      */
      digit1 = 3;
      digit2 = 3;
      digit3 = 3;
      for (int k = 0; k < 6; k++)
      {
        int currPosition = analogRead(slider[k]);
        if (k == 0 || k == 1)
        {
          steps = digit1;
          //Serial.println(digit1);
        }
        else if (k == 2 || k == 3)
        {
          steps = digit2;
        }
        else if (k == 4 || k == 5)
        {
          steps = digit3;
        }
        int Cnt2 = currPosition / steps;
        int intervalCount = steps * Cnt2;
        if (currPosition > (intervalCount + ((1023/steps)/2) + 3))
        {
          digitalWrite(motorSwitch[k], HIGH);
          digitalWrite(motorPinPlus[k], HIGH);
          digitalWrite(motorPinMinus[k], LOW);
          Serial.println("LEFT");
        }
        else if (currPosition < (intervalCount + ((1023/steps)/2) - 3))
        {
          digitalWrite(motorSwitch[k], HIGH);
          digitalWrite(motorPinPlus[k], LOW);
          digitalWrite(motorPinMinus[k], HIGH);
          Serial.println("RIGHT");
        }
        else
        {
          digitalWrite(motorSwitch[k], LOW);
          digitalWrite(motorPinPlus[k], LOW);
          digitalWrite(motorPinMinus[k], LOW);
        }
      }
    }
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
  }
  if (mode == 0) {
    for (int i = 0; i < 6; i++) {
      if (moveSlider[i]) {
        sliderToVal(i, val[i]);
      }
    }
  }
  else if (mode == 1) {
    for (int i = 0; i < nbSlider; i++) {
      scale(i);
    }
  }
  if (isFollowing) {
    if (sign == 1) {
      follow(followedSlider, followingSlider, dist);
    }
    else if (sign == 0) {
      follow(followedSlider, followingSlider, -dist);
    }
  }




}
//________________________________________________________________________________________________
void updateEncoder0() {
  int MSB = digitalRead(ROT_A[0]);
  int LSB = digitalRead(ROT_B[0]);
  int encoded = (MSB << 1) | LSB;
  int sum = (lastEncoded[0] << 2) | encoded;
  if (sum == 0b1101 || sum == 0b0100 || sum == 0b0010 || sum == 0b1011) encoderValue[0] ++;
  if (sum == 0b1110 || sum == 0b0111 || sum == 0b0001 || sum == 0b1000) encoderValue[0] --;
  lastEncoded[0] = encoded;
}
void updateEncoder1() {
  int MSB = digitalRead(ROT_A[1]);
  int LSB = digitalRead(ROT_B[1]);
  int encoded = (MSB << 1) | LSB;
  int sum = (lastEncoded[1] << 2) | encoded;
  if (sum == 0b1101 || sum == 0b0100 || sum == 0b0010 || sum == 0b1011) encoderValue[1] ++;
  if (sum == 0b1110 || sum == 0b0111 || sum == 0b0001 || sum == 0b1000) encoderValue[1] --;
  lastEncoded[1] = encoded;
}
void updateEncoder2() {
  int MSB = digitalRead(ROT_A[2]);
  int LSB = digitalRead(ROT_B[2]);
  int encoded = (MSB << 1) | LSB;
  int sum = (lastEncoded[2] << 2) | encoded;
  if (sum == 0b1101 || sum == 0b0100 || sum == 0b0010 || sum == 0b1011) encoderValue[2] ++;
  if (sum == 0b1110 || sum == 0b0111 || sum == 0b0001 || sum == 0b1000) encoderValue[2] --;
  lastEncoded[2] = encoded;
}
