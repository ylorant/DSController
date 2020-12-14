#include <Arduino.h>

#define MODE_NORMAL 1
#define MODE_PROBE 2

#define RX_LED 17
#define TX_LED 30

String readData;
String command;
uint8_t btnPressed;
uint8_t mode;
uint8_t buttonValues[22];
int i;
int codeLastPos;
String controllerCode;

void configureMode(uint8_t newMode);
void sendKeyCommand(uint8_t keyIndex, uint8_t status);

void setup()
{
    // Set the default mode as control mode (the computer controls the DS)
    configureMode(MODE_NORMAL);

    Serial.setTimeout(5);
    Serial.begin(9600);
}

/* Main loop */
void loop()
{
    if (Serial.available() > 0) {
        readData = Serial.readStringUntil('\n');
        command = readData.substring(0, 3);

        if(command.compareTo("VER") == 0) {
            Serial.write("1.0\n");
        } else if(command.compareTo("MOD") == 0) {
            mode = readData.charAt(4) - 48;

            if(mode == MODE_NORMAL || mode == MODE_PROBE) {
                configureMode(mode);
            }
        } else if(command.compareTo("CCD") == 0) {
            codeLastPos = readData.length();
            controllerCode = readData.substring(4, codeLastPos);
        } else if(command.compareTo("KEY") == 0 && mode == MODE_NORMAL) {
            btnPressed = readData.charAt(6) == '1' ? LOW : HIGH;

            switch(readData.charAt(4)) {
                case 'A': sendKeyCommand(0, btnPressed); break;
                case 'B': sendKeyCommand(1, btnPressed); break;
                case 'X': sendKeyCommand(2, btnPressed); break;
                case 'Y': sendKeyCommand(3, btnPressed); break;
                case 'L': sendKeyCommand(4, btnPressed); break;
                case 'R': sendKeyCommand(5, btnPressed); break;
                case 'T': sendKeyCommand(6, btnPressed); break;// S(t)art
                case 'C': sendKeyCommand(7, btnPressed); break; // Sele(c)t
                case 'U': sendKeyCommand(8, btnPressed); break;// Up
                case 'D': sendKeyCommand(9, btnPressed); break;// Down
                case 'E': sendKeyCommand(10, btnPressed); break;// L(e)ft
                case 'I': sendKeyCommand(11, btnPressed); break;// R(i)ght
            }
        }
    }

    if(mode == MODE_PROBE) {
        for(i = 2; i < 22; i++) {
            // Ignore LED
            if(i == 17) {
                continue;
            }

            btnPressed = digitalRead(i);

            if(btnPressed != buttonValues[i]) {
                buttonValues[i] = btnPressed;
                Serial.write(i + 48);
            }
        }
    }
}

void sendKeyCommand(uint8_t keyIndex, uint8_t status)
{
    uint8_t port = controllerCode.charAt(keyIndex) - 63;
    digitalWrite(port, status);
}

/**
 * Configures the mode and reassigns all the pins
 */
void configureMode(uint8_t newMode)
{
    int i;
    uint8_t newPinMode;

    mode = newMode;

    if(mode == MODE_NORMAL) {
        newPinMode = OUTPUT;
        digitalWrite(30, HIGH);
    } else if(mode == MODE_PROBE) {
        newPinMode = INPUT_PULLUP;
        digitalWrite(30, LOW);

        for(i = 0; i < 22; i++) {
            buttonValues[i] = HIGH;
        }
    }

    // Setting up all pins as required, and if they're outputs, setting their
    // initial level as high (DS buttons are normally closed ?)
    for(i = 2; i < 11; i++) {
        pinMode(i, newPinMode);
        if(newPinMode == OUTPUT) {
            digitalWrite(i, HIGH);
        }
    }

    for(i = 14; i < 17; i++) {
        pinMode(i, newPinMode);
        if(newPinMode == OUTPUT) {
            digitalWrite(i, HIGH);
        }
    }

    for(i = 18; i < 21; i++) {
        pinMode(i, newPinMode);
        if(newPinMode == OUTPUT) {
            digitalWrite(i, HIGH);
        }
    }
}
