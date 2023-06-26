# Interactive Inspector | iOS | Adaptive UI 

<p align="center">
	<img src="https://raw.githubusercontent.com/Amarthgul/InteractiveInspector/main/Assets/Resources/Screenshots/CoverVer01.png" width="512">
</p>

This repo inherits much of the concepts from [this repo](https://github.com/cs-isamiul/Interactive-Anatomy-Visualization-Demo),
but took a different implemntation and is built for iOS platform. 

This branch (adaptive UI) is different from the main as it is built to adaptive different devices and resoultions  
at the price of no longer supporting fly-in and fly-out animations. Idealy, this version would be able
to work on both iOS and on browser as an WebGL application. 

## Run on iPad:

Download the release package and open the xCode project (does not work on Windows). In `Project navigator`, select
`Unity-iPhone`, which will open an attribute editor. In the `Signing & Capabilities` tab, check `Automatically manage signing`
and select the right team (with developer account). 

Connect the device (current version supports only iPad) and enable the developer mode on the device. In Xcode, you should be 
able to click the top panel and see a drop down menu which contains the currently connected device, select that device and 
hit start. After Xcode completes, the app should start automatically on you device. 


## Basic operations

* Touch and move with a single finger around the screen to rotate around the model 

* 2 fingers moving together to pan around the model

* Pinching 2 fingers to zoom in or out

* Tap on a part of the model to select and highlight it

* Once an object is selected, the bottom menu will be activated (technically user can still access the 
  menu items when nothing is selected, but it will show nothing but a prompt: "Select an item to begin"). 

  * The left-most icon is for settings. By default, the camera starts rotating around if there is no 
  user input for a while, you could disable that in the settings. One can also enable auto voiceover 
  so that invoking description panel will auto play the audio. 

  The light mode turns the app into light theme, with the background and most UI elements becoming white. 

  The setting panel also offers touch sensitivity control and a reset button. 

  * The middle icon shows the text description of the object currently seleted (if any). When multiple
  objects are selected, the first on the list will be displayed. 

  * The icon at the right is for changing the appearance of the model(s). This offers control over
  the opacity of the objects, and their hightlight colors. 

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