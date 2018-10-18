# serialkeymapper
SerialKeyMapper is a tiny Windows application that can run in the background, and will convert any characters received on the specified serial port into keystrokes sent to any active application. It was designed to enable an Arduino to control video playback on a PC - the Arduino would read sensor input and generate Serial.println() messages corresponding to hotkeys that controlled the application.

e.g. In Powerpoint, the keyboard hotkey "S" starts or stops the current show. When SerialKeyMonitor is running, an Arduino connected via USB to a PC can execute the following command:
```
Serial.println('s');
```
which will be intercepted and translated into an event trigger as if the keyboard letter 'S' had been pressed, starting the Powerpoint slideshow.
