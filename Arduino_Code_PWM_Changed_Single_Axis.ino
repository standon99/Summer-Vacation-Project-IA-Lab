
#include <PID_v1.h>

const int analogInPin = A0;
const int forward = 9; // was 9
const int reverse = 10; // was 10
const int thirteen = 13;
const int five = 5;
const int enc = 8;
const int encInt = 7;
const int button = 12;
int sensorValue = 0;        // value read from the pot
int outputValue = 0;        // value output to the PWM (analog out)
int count = 600;
int curve;
int input, output, Kp, Ki, Kd;
bool buttonPress;
//double Setpoint, Input, Output;
//double Kp=2, Ki=50, Kd=50;
//PID myPID(&Input, &Output, &Setpoint, Kp, Ki, Kd, DIRECT);

// Analog input pin A0
int slider_Pos = A0;
int cur_Pos, digit1, digit2, digit3, digit4, id, pos_val;

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
int Input, Output;
void sliderToVal(int val) {
  int pos = analogRead(slider_Pos);
  int value;
  if (abs(pos - val) > 1)
  {
    value = abs(pos - val) * 5 + 460;

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

void setup() {
  Serial.begin(9600);
  Serial.flush();

  pinMode(A0, INPUT);

  pinMode (forward, OUTPUT);
  pinMode (reverse, OUTPUT);
  pinMode (thirteen, OUTPUT);
  pinMode (five, OUTPUT);
  pinMode (button, INPUT_PULLUP);
  pinMode(enc, INPUT_PULLUP);
  pinMode(encInt, INPUT_PULLUP);
  TCCR1A = 0;   // stop Timer 1
  TCCR1B = 0;
  TCCR1C = 0;
  TCCR1A = bit (COM1A1) | bit (WGM11);
  TCCR1B = bit (WGM13) | bit (WGM12) | bit (CS10);
  ICR1 = 600; // set frequency. 1000 is 16khz 500 32khz. //////////// up to here sets nine and ten frequency to 26khz


  //analogWrite(reverse, 0);
  //analogWrite(forward, 0);  // set these to 300 to 600 where needed with the other one digital write LOW, but pull low if at ends of fader
 
  
  double Setpoint, Input, Output;
  
  //PID(&Input, &Output, &pos_val, Kp, Ki, Kd, DIRECT);
  //SetSampleTime(1);
 // SetOutputLimits(500, 600);
}

void loop()
{
  cur_Pos = analogRead(A0);
  Serial.println(analogRead(A0));

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
  }

  //Compute();
  //PID(&Input, &Output, &pos_val, Kp, Ki, Kd, Direction)
  // Send slider to required position
  sliderToVal(pos_val);
}
