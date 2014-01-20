Aurora
======

My take on Ambilight made with C# and C++/CLI

http://youtu.be/3cwZmXn_8CU
<iframe width="560" height="315" src="http://www.youtube.com/embed/3cwZmXn_8CU" frameborder="0" allowfullscreen></iframe>



Running on your computer
========================

You will be able to run the simulator, which captures byte stream that would be otherwise sent through the USB.
To start the simulation, press "sim" button in the lower-left corner of the interface, as shown below:
<img src="http://i.imgur.com/kt4lM.png" />
 
Contact me if you have a device, such as a strand of LED modules "Adalight" and would like to use it with my software.


Setting up Teensy in Windows 8
==============================

I don't have links to arduino-1.0.2 and teensyduino.exe, but they are available on line. This guide is written from my memory.

1. Install arduino-1.0.2
2. Go to Windows 8 Metro Control Panel, "More PC Settings", then to Advanced Startup. Find the option which restarts the computer.
3. Select option that allows for unsigned drivers to be installed.
4. Run teensyduino.exe as administrator. Install the Teensy drivers
5. Make sure arduino-1.0.2 has the Teensy drivers selected
6. In arduino-1.0.2 verify the firmware and upload it to Teensy
7. In Tensy app, check Automatic Mode. This way, whenever a button is pressed, Teensy restarts.
8. When Teensy is restarted, it should flash red, green and then blue. It should do it every time a button is pressed (see step 6)
