# Interactive Inspector | Mobile & Web | Adaptive UI 

<p align="center">
	<img src="https://raw.githubusercontent.com/Amarthgul/InteractiveInspector/main/Assets/Resources/Screenshots/CoverVer01.png" width="512">
</p>

This repo inherits much of the concepts from [this repo](https://github.com/cs-isamiul/Interactive-Anatomy-Visualization-Demo),
but took a different implemntation and is built for iOS platform. 

This branch (adaptive UI) is different from the main as it is built to adaptive different devices and resoultions  
at the price of no longer supporting fly-in and fly-out animations. Idealy, this version would be able to work on
both iOS as an application and on browser as an WebGL program. 

## Connect and install on iPad:

Please ensure the version/release you download is built for iOS, the following steps will not work if the browser version is
downloaded. Also note that since Xcode does not support Windows, this part also won't work on a Windows PC. 

Download the release package and open the xCode project. In `Project navigator`, select `Unity-iPhone`, which will open an 
attribute editor. In the `Signing & Capabilities` tab, check `Automatically manage signing` and select the right team 
(a team with Apple developer account). 

Connect the device (current version is designed for only iPad, iPhone can use them but the UI will look way off) to the Mac with Xcode open. 
If this is the first time the device is being used for testing app, you need to enable developer mode, which can be found in the `Settings > Privacy`.
Enable the developer mode on this device. 
In Xcode, you should be able to click the top panel and see a drop down menu which contains the currently connected device, select that device and 
hit start. After Xcode completes, the app should open automatically on you device. 

## Basic operations - iOS

* Touch and move with a single finger around the screen to rotate around the model 

* 2 fingers moving together to pan around the model

* Pinching 2 fingers to zoom in or out

* Tap on a part of the model to select and highlight it

## Basic operation - Web browser

* Press and move the left mouse button to rotate around the model 

* Press and move the right mouse button to pan around the model

* Use the middle scroll wheel to zoom in or out

* Click on a part of the model to select and highlight it

## Common operations 

* The bottom panel provide access to the general settings, text description, and appearamce. 

* Settings 

  The setting panel allows the user change how they interact with the app. By default the model will start to rotate if no user interaction is 
  detected for a while, this can be disabled or enabled in the settings. The user can also enable automatic voiceover so that the text description
  is read whenever text panel is invoked. Light theme changes the UI of the application to bright white colors. And the user can also enable/disable 
  multiple selection to designate whether or not they want to select several objects at the same time. 

* Description

  Description proivdes a short text description of the object the user is curretly selecting, also an option to read the text out loud. 

* Appearance

  The user could edit the color and transparency of the objects they selected, or isolate the object by decrease the transparency of other objects. 


## Add interactive models into the scene

Models by default are displayed but not selectable. 

To make models selectable, several steps need to be performed. 

1. Attach a `Mesh Collider` to the mesh (**to the mesh, not the pre-fab**, collider does not work on prefab).
Then click on `TapHandler` in the scene, in the Inspector, add a new element 
in the `Can Be Tapped` list, drag and drop the new mesh into the slot, 
this will make the mesh selectable. 

2. If desired, create and add a new shader in the `Highlight Shaders` list to
the corresponding element number. This shader will be used as the object's highlight
selection effect. Please note that this shader should have a rim and fill light attribute,
otherwise the application may have unexpected outcome when user change the rima nd fill color
of the selected. 

3. Using the same method, add a vocie over audio clip into the project and drag it in `Voice Overs` 
list, and fill in the corresponding `Descriptions` list. The former one is for the audio voiceover 
when object is selected, the later for the displayed text.


### Changing the displayed name of the object

The displayed name of the selected object is the name of the mesh. Thus, simply double
click on the mesh (or select the object in scene hierarchy and press `F2`) in scene hierachy to change the mesh name 
will alter the name disaplyed during runtime. 


## Dev Log 

* July 12th 

  Added a breahting animation to the help instructions, further tidied up the code. Optimized the color picking algorithm
  so that it works properly on different screen resolutions. 

* July 5th 

  Auto voiceover now function properly, when the text description panel is on, whenever a new object is selected, 
  its voice description will be played automaitcally. 

  Help instruction can now be clicked outside to hide, and the highlight has animations to signify which key to use. 

* June 29th 
  
  Changed the selection method, by default the user can only select 1 object. There is now a multiple slection
  checkbox in the settings, with which the user can select multiple objects. 

* June 26th 

  Completely reworked the color picker, hand calculated the 3 different resolutions. 
  As a result, the web version can now be controlled by mouse, with little to no UI misalignment. 

* June 15th 

  The bottom panel now disaplys permanently (can still be toggled by unchecking the always show option in inspector). 
  When nothing is selected and a panel box is invoked, it will say select to begin. 

  Select highlight now uses the Buckeye theme from the brand guideline page. 

* June 8th

  Now supports light theme (UIs become bright white while texts and icons becomes dark), 
  UI also adapts to different screen size and orientation (limited to iPad). 


* May 30th 

  Fixed the issue where momentum will lead the camera to rotate fanatically around the vertical axis. 

  Added some portrait mode suport. 


* May 23rd 

  UIs have been reworked to be adaptive, which tries to detect the resoultion of the current screen and move to the 
  best place. 

  Selection and text/voice has been reworked to always reflect the infomation of the newest selected object. 

* May 15th 
  
  Created the first release. Release package is an xCode project that can be used to build the app
  on iPad Air 4th Gen. iPhone is not supported in this build. 
