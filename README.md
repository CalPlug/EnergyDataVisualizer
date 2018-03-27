# PlugLoad Periscope
This program is a demonstration application that uses an Apple iPad (and other ARKit enabled iOS devices) to produce an augmented reality (AR) environment to allow uses to visualize usage and energy consumption for househoold appliances.

Created by Nikita Tsvetkov, Viet Ly, and Kim Phan

Project Mentors: Sergio Gago, Ph.D., Michael Klopfer, Ph.D., Joy Pixley, Ph.D., and Prof. G.P. Li

University of California, Irvine - California Plug Load Research Center (CalPlug)

March 27, 2018

Copyright (C) The Regents of the University of California, 2018

# Installation
Setting up this application is a two part process. 

First, read the `front_end_doc.pdf` contained in the `front_end` folder. The instructions in that document will
help you set up the front end. Once the necessary development software is ready, clone this project, and open up the front end solution Percept.sln in Visual Studio. This should be renamed to PlugLoadPeriscope.sln at some point. Build the code, and run the application on an iOS device. If using the free provisioning option, then the iOS device should be plugged into the Apple computer. When plugged in, the device should be listed as one of the possible devices to run code on, alongside virtual devices that are simulated. Note that virtual simulation does not work for this project, since it uses the camera hardware. Select the device, and run the code. After running the code, that application will persist on that device (it doesn't need to be connected to a Visual Studio session.)

Next, read the `Setting_up_AWS_Backend` located in the `back_end` folder to set up the plumbing of the device. An AWS account will be necessary. This account will need to contain the services and deployed cloud formation stack which are detailed in the backend pdf.

# Running the application
Refer to `front_end_doc.pdf` in the `front_end` folder for further instructions.
Look towards the end to see pictures of the demo in live action.
