#### Summer Vacation Research Project 2019 - 2020 

### Faculty of Information Technology - Immersive Analytics Lab - Monash University 2019

## Siddhant:

# C# Unity Script:
 * Smoothing of input (Kalman filter/moving mean) --- DONE --- 2/12/19
 * Only send data if present position is different to previous position --- DONE --- 28/11/19
 * Fix so both hands can be used --- DONE --- IO EXCEPTION ERROR --- FIXED --- 29/11/19
 * PID in Arduino --- MODIFIED VERSION INCORPORATED --- 2/12/19
 * Restrict amount of data sent through serial port (no unnecessary data sends).
 * Map movement distance of fingers to movement of fader in a 1:1 ratio --- DONE (normalization function altered) --- 2/12/19
 * Gesture Recognition --- Pinch is now registered (index finger and thumb tips pushed together) --- 3/12/2019
 * Gesture Recognition --- Can now terminate gesture tracking by closing fist --- 4/12/2019
 * Gesture Recognition --- Tracking center of hand gesture area
 * Gain Function --- Increase precision the higher you go (min 100 and max 300 due to accuracy issues) --- DRAFTED --- 4/12/2019
 * Haptics --- Both axis discretization and mapping of data sets implemented. Note at this stage data sets are entered directly into          Arduino script file
 * Two hand tracking is also operational, both hands may be tracked to manipulate both faders assigned to each axis.
 
# Arduino Code:
* PWM pins are operating at 26 kHz
* Modified PID-like control system in order to smooth movements of fader
* Works with single axis only
* Accuracy set at 1 (out of 1023 positions along the slider)
* New faders in use, these faders are communicating with one another using I2C.
* Haptics are now included in the new faders code as well- operating without vibrations/jitter. If moved too fast, however, these can often crash.
