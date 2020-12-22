# DSController

This software allows to remote control a Nintendo DS by a PC using a keyboard,
or a controller using a transfer software like joy2key.

This software is to use with an arduino mounted inside the console, using the
provided program loaded into it.

# Installation

## Arduino

### Wiring

*Note: Everything below considers a Nintendo DS Fat.*

The arduino part has been developed originally for an Arduino/Sparkfun Pro Micro. It can probably be adapted on other Arduino-compatible boards, such as the Arduino Pro Mini. However, the current program assumes a direct connection to the control program, so using something as a FTDI or UART to the board may need some adaptation.

The pinout to connect each port can be whatever pins you can fit between pin 2 and 21 (A3), the programs having been made to be pin-agnostic. Pinout diagram for the Pro Micro can be found [here](https://cdn.sparkfun.com/assets/9/c/3/c/4/523a1765757b7f5c6e8b4567.png). The probe function of the control program will help you define the controller code for the button-pin mapping you made on the actual console. On the console, you should connect the Pro Micro pins using Kynar cables to the given test pads on the console motherboard :

| Test pad name | Button |
| ------------- | ------ |
| P00           | A      |
| P01           | B      |
| R00           | X      |
| R01           | Y      |
| P02           | Select |
| P03           | Start  |
| P06           | Up     |
| P07           | Down   |
| P05           | Left   |
| P04           | Right  |
| P08           | R      |
| P09           | L      |

For the position of those test pads on the Nintendo DS motherboard, you can refer yourself on the [motherboard picture](https://commons.wikimedia.org/wiki/File:Nintendo-DS-Mk1-Motherboard-Top.jpg) available on Wikimedia Commons.

The placement of the board itself should be just above the earphone jack and below the WiFi board, that way it leaves space for a Loopy capture card above. Some cutting of the bottom shell will be necessary for the board, USB port and cables. See [this picture](https://i.imgur.com/w6i4FmJ.jpg) for an installation example (here using thick double sided tape). 

*Note: Using this configuration, you might have problems screwing back the right-side motherboard screw. Consider screwing it back on before sticking the board.*

### Flashing

Once wired in the console (or before wiring, as you want), you can flash the board using PlatformIO (an alternative IDE to the Arduino IDE, integrated into Visual Studio Code and Atom). Install it through Atom, Visual Studio Code or just install the CLI version through your distro's package system if you're on Linux, open the `arduino` folder in your IDE and flash the board.

Alternatively, you can build and flash the board through CLI just by going into the `arduino` directory and issuing the following command :

```bash
platformio run --target upload
```

## Control program

### Getting from a release

Just download the release zip file matching your operating system in the Releases section and extract it on your computer. It should contain an executable. Nothing else is required, as it has been statically compiled to avoid any dependency.

For Windows builds, there will be, along with the program file and it's resource file, some Batch scripts to help you set everything up without having to open and execute manually commands.

### Compiling from source

To compile the control program's source code, you must start by installing the .NET SDK. Instructions on how to install it should be available on [Microsoft's documentation](https://docs.microsoft.com/en-us/dotnet/core/install/).

Minimum version of the .NET SDK should be .NET Core 3.0 (at least it's the version it was developed with).

Once installed you can build the project by setting yourself in the project directory and by running the following command :

```bash
dotnet publish -c Release
```

You can also build it statically (so it won't require any dependency to run) using the following command (replace `{platform}` by your actual platform, tested with `win-x86`, `win-x64` and `linux-x64`) :

```bash
dotnet publish -r {platform} -c Release /p:PublishSingleFile=true
```

## Configuration/Usage

Prior to using DSController, the control program has to be configured to work correctly with the board, and also to configure the key mapping.

*Note: all the following commands assume that you're currently in the same folder where the executable is*.

### Generating a blank configuration file

The first thing to do is to generate a blank configuration file to populate. To do this, issue this command:

```bash
./DSController -s > config.json
```

*Note: For Windows users who downloaded the release archive, you can execute the `generate-config.bat` included in it, that will avoid you from opening a command prompt.*

This command will ask the control program for a blank configuration file and will output it into the `config.json` file. This is the default file name that is looked up for configuration.

### Listing connected devices

The next step is telling the control program what is the device to control. This is made by setting the `device` key in the configuration file (at that point you might want to open it in a text editor).

To know what to put there, the control program can help you. You can scan the computer to search for the correct connected device by running this:

```bash
./DSController -l
```

*Note: For Windows users using the release archive, you can use the `list-ports.bat` script for that step.*

This will output a list of all connected devices, there should be only one for most cases. Copy its name into the `device` key in your configuration file. If there is more than one, try each one to find the correct device.

### Generating a controller code

Once you have the device set in your configuration file, the next step is to probe the Arduino controller to get its controller code. This is not a DRM thing, it has been made for ease of installation.

The controller code is what maps the actual buttons to the pins of the microcontroller, so the installer can set the pins in whatever order suits him most and still have the program work.

To probe the Arduino, just start the DSController program with the `-p` option:

```bash
./DSController -c config.json -p
```

*Note: Here the configuration file is specified, but as it's the default location, it can be omitted. However, if you use a different file, you have to include it.*

*Note: For Windows users using the release archive, you can use the `probe-buttons.bat` script for that step.*

The program will ask you to press each button in sequence, then will output the controller code to put in the `controllerCode` configuration setting of your config file.

### Using it

Finally to use it, just start the control program with the config file parameter (or without, if you use the default config file location):

```bash
./DSController -c config.json
```

*Note: Windows users, you can just double-click the program in the case you're using the default config file.*

A window should open, and if everything is right, it should show a green background with the message "Connected." on it. If there is an error, the window should tell what the error is with a red background.