/* NOTES:
    For the existing faders, the settings below provide average results. When the new faders arrive, change the sliderToVal function as below:

    void sliderToVal(int val) {
  int pos = analogRead(slider_Pos);
  int value;
  if (abs(pos - val) > 4)
  {
    value = abs(pos - val) * 2 + 450;

    if (value > 600)
    {
      value = 600;
    }

    if (pos > val)
    {
      analogWrite(forward, value);
      digitalWrite(reverse, LOW);
    }
    else
    {
      digitalWrite(forward, LOW);
      analogWrite(reverse, value);
    }
  }
  else
  {
    digitalWrite(forward, LOW);
    digitalWrite(reverse, LOW);
  }
  }
  Also remove the PWM setup code in teh setup loop and replace it with the relevant code for the Arduino model in use.
*/

const int analogInPin = A0;
const int forward = 9; // was 9
const int reverse = 10; // was 10
const int thirteen = 13;
const int five = 5;
const int enc = 8;
const int encInt = 7;
const int button = 12;
//int sensorValue = 0;        // value read from the pot
//int outputValue = 0;        // value output to the PWM (analog out)
///int count = 600;
//int curve;
//int input, output, Kp, Ki, Kd;
//bool buttonPress;
//double Setpoint, Input, Output;
//double Kp=2, Ki=50, Kd=50;
//PID myPID(&Input, &Output, &Setpoint, Kp, Ki, Kd, DIRECT);

// Analog input pin A0
// int slider_Pos = A0;
int cur_Pos, digit1, digit2, digit3, digit4, id, pos_val;

// Old Fader Axes pinouts
const int nbSlider = 6;
int slider[nbSlider] = {A0, A1, A2, A3, A4, A5};
int motorSwitch[6] = {7, 3, 26, 45, 8, 46};
int motorPinPlus[6] = {5, 9, 28, 40, A8, 48};
int motorPinMinus[6] = {2, 6, 33, 30, A13, 44};

String message;

/*
  int PID_funct(int error_val, float time_val)
  {
  // These values can be altered to fine-tune the function
  Kp = 10;
  Ki = 10;
  Kd = 10;
  new_time = millis();
  dt = new_time - time_val;
  integral = integral + error * dt;
  derivative = error * dt;
  output = (Kp*error) + (Ki * integral) + (Kd * derivative));
  return output;
  }

*/


void sliderToVal(int val, int id) {
  int pos = analogRead(slider[id]);
  int value;
  if (abs(pos - val) > 7)
  {
    value = abs(pos - val) * 2 + 900;

    if (value > 600)
    {
      value = 600;
    }

    if (pos < val)
    {
      digitalWrite(motorPinPlus[id], HIGH);
      digitalWrite(motorPinMinus[id], LOW);
      //analogWrite(motorSwitch[id], value);
      digitalWrite(motorSwitch[id], HIGH);
    }
    else
    {
      digitalWrite(motorPinPlus[id], LOW);
      digitalWrite(motorPinMinus[id], HIGH);
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
  }
}

/*
  void sliderToVal(int val, int id) {
  int pos = analogRead(slider[id]);
  if (abs(pos - val) > 150) {
    if (pos > val) {
      digitalWrite(motorPinPlus[id], LOW);
      digitalWrite(motorPinMinus[id], HIGH);
    }
    else {
      digitalWrite(motorPinPlus[id], HIGH);
      digitalWrite(motorPinMinus[id], LOW);
    }
    digitalWrite(motorSwitch[id], HIGH);
  }
  else {
    digitalWrite(motorPinPlus[id], LOW);
    digitalWrite(motorPinMinus[id], LOW);
    analogWrite(motorSwitch[id], 0);
    //[id] = false;
  }
  }
*/

//-------------------------------------------------------------------------------------------------------------
const float minForce = 256;
const float maxForce = 8;
const int resolution = minForce;
const int maxStep = maxForce;
float listPosf0[resolution + 1];
int listPos0[resolution + 1];
float listPosf1[resolution + 1];
int listPos1[resolution + 1];
float listPosf2[resolution + 1];
int listPos2[resolution + 1];
const int nbData = maxForce;
float data0[nbData] = {0, 1, 2, 3, 4, 4, 4, 4};
float data1[nbData] = {0, 4, 0, 4, 0, 4, 0, 4};
float data2[nbData] = {0, 1, 2, 3, 4, 2, 1, 0};
float minData;
float maxData;
int indice0, indice1, indice2 = 0;
int mode = 0;

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
  if (id == 0 || id == 1) {
    int val = analogRead(slider[id]);
    for (int i = 0; i < indice0 + 1; i++) {
      if (val >= listPos0[i] - (listPos0[i] - listPos0[i - 1]) / 2 && val < listPos0[i] + (listPos0[i + 1] - listPos0[i]) / 2) { //from which integer of listPos the slider the closer
        //go to this position
        if (val > listPos0[i] + 3) {
          analogWrite(motorSwitch[id], 255);
          digitalWrite(motorPinPlus[id], HIGH);
          digitalWrite(motorPinMinus[id], LOW);
        }
        else if (val < listPos0[i] - 3) {
          analogWrite(motorSwitch[id], 255);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], HIGH);
        }
        else {
          analogWrite(motorSwitch[id], 0);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], LOW);
        }
      }
    }
  }
  else if (id == 2 || id == 3) {
    int val = analogRead(slider[id]);
    for (int i = 0; i < indice1 + 1; i++) {
      if (val >= listPos1[i] - (listPos1[i] - listPos1[i - 1]) / 2 && val < listPos1[i] + (listPos1[i + 1] - listPos1[i]) / 2) { //from which integer of listPos the slider the closer
        //go to this position
        if (val > listPos1[i] + 3) {
          analogWrite(motorSwitch[id], 255);
          digitalWrite(motorPinPlus[id], HIGH);
          digitalWrite(motorPinMinus[id], LOW);
        }
        else if (val < listPos1[i] - 3) {
          analogWrite(motorSwitch[id], 255);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], HIGH);
        }
        else {
          analogWrite(motorSwitch[id], 0);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], LOW);
        }
      }
    }
  }
  else if (id == 4 || id == 5) {
    int val = analogRead(slider[id]);
    for (int i = 0; i < indice2 + 1; i++) {
      if (val >= listPos2[i] - (listPos2[i] - listPos2[i - 1]) / 2 && val < listPos2[i] + (listPos2[i + 1] - listPos2[i]) / 2) { //from which integer of listPos the slider the closer
        //go to this position
        if (val > listPos2[i] + 3) {
          analogWrite(motorSwitch[id], 255);
          digitalWrite(motorPinPlus[id], HIGH);
          digitalWrite(motorPinMinus[id], LOW);
        }
        else if (val < listPos2[i] - 3) {
          analogWrite(motorSwitch[id], 255);
          digitalWrite(motorPinPlus[id], LOW);
          digitalWrite(motorPinMinus[id], HIGH);
        }
        else {
          analogWrite(motorSwitch[id], 0);
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
//-------------------------------------------------------------------------------------------------------------



void setup() {
  Serial.begin(2000000);
  //Serial.flush();

  //pinMode(A0, INPUT);
  for (int i = 0; i < 6; i++) {
    pinMode(slider[i], INPUT);
    pinMode(motorSwitch[i], OUTPUT);
    pinMode(motorPinPlus[i], OUTPUT);
    pinMode(motorPinMinus[i], OUTPUT);
  }
  //pinMode (forward, OUTPUT);
  //pinMode (reverse, OUTPUT);
  //pinMode (thirteen, OUTPUT);
  //pinMode (five, OUTPUT);
  //pinMode (button, INPUT_PULLUP);
  // pinMode(enc, INPUT_PULLUP);
  //pinMode(encInt, INPUT_PULLUP);
  //TCCR1A = 0;   // stop Timer 1
  //TCCR1B = 0;
  //TCCR1C = 0;
  //TCCR1A = bit (COM1A1) | bit (WGM11);
  //TCCR1B = bit (WGM13) | bit (WGM12) | bit (CS10);
  //ICR1 = 600; // set frequency. 1000 is 16khz 500 32khz. //////////// up to here sets nine and ten frequency to 26khz


  //analogWrite(reverse, 0);
  //analogWrite(forward, 0);  // set these to 300 to 600 where needed with the other one digital write LOW, but pull low if at ends of fader


  //double Setpoint, Input, Output;

  //PID(&Input, &Output, &pos_val, Kp, Ki, Kd, DIRECT);
  //SetSampleTime(1);
  // SetOutputLimits(500, 600);

  //---------------------------------------------- Set PWM frequency for D4 & D13 ------------------------------

  TCCR0B = TCCR0B & B11111000 | B00000001;    // set timer 0 divisor to     1 for PWM frequency of 62500.00 Hz
  //TCCR0B = TCCR0B & B11111000 | B00000010;    // set timer 0 divisor to     8 for PWM frequency of  7812.50 Hz
  //  TCCR0B = TCCR0B & B11111000 | B00000011;    <// set timer 0 divisor to    64 for PWM frequency of   976.56 Hz (Default)
  //TCCR0B = TCCR0B & B11111000 | B00000100;    // set timer 0 divisor to   256 for PWM frequency of   244.14 Hz
  //TCCR0B = TCCR0B & B11111000 | B00000101;    // set timer 0 divisor to  1024 for PWM frequency of    61.04 Hz


  //---------------------------------------------- Set PWM frequency for D11 & D12 -----------------------------

  TCCR1B = TCCR1B & B11111000 | B00000001;    // set timer 1 divisor to     1 for PWM frequency of 31372.55 Hz
  //TCCR1B = TCCR1B & B11111000 | B00000010;    // set timer 1 divisor to     8 for PWM frequency of  3921.16 Hz
  // TCCR1B = TCCR1B & B11111000 | B00000011;    // set timer 1 divisor to    64 for PWM frequency of   490.20 Hz
  //TCCR1B = TCCR1B & B11111000 | B00000100;    // set timer 1 divisor to   256 for PWM frequency of   122.55 Hz
  //TCCR1B = TCCR1B & B11111000 | B00000101;    // set timer 1 divisor to  1024 for PWM frequency of    30.64 Hz

  //---------------------------------------------- Set PWM frequency for D9 & D10 ------------------------------

  TCCR2B = TCCR2B & B11111000 | B00000001;    // set timer 2 divisor to     1 for PWM frequency of 31372.55 Hz
  //TCCR2B = TCCR2B & B11111000 | B00000010;    // set timer 2 divisor to     8 for PWM frequency of  3921.16 Hz
  //TCCR2B = TCCR2B & B11111000 | B00000011;    // set timer 2 divisor to    32 for PWM frequency of   980.39 Hz
  //  TCCR2B = TCCR2B & B11111000 | B00000100;    // set timer 2 divisor to    64 for PWM frequency of   490.20 Hz
  //TCCR2B = TCCR2B & B11111000 | B00000101;    // set timer 2 divisor to   128 for PWM frequency of   245.10 Hz
  //TCCR2B = TCCR2B & B11111000 | B00000110;    // set timer 2 divisor to   256 for PWM frequency of   122.55 Hz
  //TCCR2B = TCCR2B & B11111000 | B00000111;    // set timer 2 divisor to  1024 for PWM frequency of    30.64 Hz


  //---------------------------------------------- Set PWM frequency for D2, D3 & D5 ---------------------------

  TCCR3B = TCCR3B & B11111000 | B00000001;    // set timer 3 divisor to     1 for PWM frequency of 31372.55 Hz
  //TCCR3B = TCCR3B & B11111000 | B00000010;    // set timer 3 divisor to     8 for PWM frequency of  3921.16 Hz
  //  TCCR3B = TCCR3B & B11111000 | B00000011;    // set timer 3 divisor to    64 for PWM frequency of   490.20 Hz
  //TCCR3B = TCCR3B & B11111000 | B00000100;    // set timer 3 divisor to   256 for PWM frequency of   122.55 Hz
  //TCCR3B = TCCR3B & B11111000 | B00000101;    // set timer 3 divisor to  1024 for PWM frequency of    30.64 Hz


  //---------------------------------------------- Set PWM frequency for D6, D7 & D8 ---------------------------

  TCCR4B = TCCR4B & B11111000 | B00000001;    // set timer 4 divisor to     1 for PWM frequency of 31372.55 Hz
  //TCCR4B = TCCR4B & B11111000 | B00000010;    // set timer 4 divisor to     8 for PWM frequency of  3921.16 Hz
  //  TCCR4B = TCCR4B & B11111000 | B00000011;    // set timer 4 divisor to    64 for PWM frequency of   490.20 Hz
  //TCCR4B = TCCR4B & B11111000 | B00000100;    // set timer 4 divisor to   256 for PWM frequency of   122.55 Hz
  //TCCR4B = TCCR4B & B11111000 | B00000101;    // set timer 4 divisor to  1024 for PWM frequency of    30.64 Hz


  //---------------------------------------------- Set PWM frequency for D44, D45 & D46 ------------------------

  TCCR5B = TCCR5B & B11111000 | B00000001;    // set timer 5 divisor to     1 for PWM frequency of 31372.55 Hz
  //TCCR5B = TCCR5B & B11111000 | B00000010;    // set timer 5 divisor to     8 for PWM frequency of  3921.16 Hz
  //  TCCR5B = TCCR5B & B11111000 | B00000011;    // set timer 5 divisor to    64 for PWM frequency of   490.20 Hz
  //TCCR5B = TCCR5B & B11111000 | B00000100;    // set timer 5 divisor to   256 for PWM frequency of   122.55 Hz
}

void loop()
{
  //int slider[nbSlider] = {A0, A1, A2, A3, A4, A5};

  //cur_Pos = analogRead(A0);
  //Serial.println(analogRead(A0));
  //pos_val = 750;
  // Read input from Serial Port (sent from Unity)
  if (Serial.available() > 0) {
    message = Serial.readStringUntil('\n');
    id = message[0] - '0';
    digit1 = message[2] - '0';
    digit2 = message[3] - '0';
    digit3 = message[4] - '0';
    digit4 = message[5] - '0';
    pos_val = digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4;
    pos_val = abs(pos_val) % 1024;
    // Send slider to required position
    //sliderToVal(pos_val, id);
    sliderToVal(pos_val, id);
  }
  //id = 3;

  //Serial.println(pos_val);
  //int read_val = analogRead(A3);
  //Serial.println(read_val);
  //Compute();
  //PID(&Input, &Output, &pos_val, Kp, Ki, Kd, Direction)
  //delay(500);

}
