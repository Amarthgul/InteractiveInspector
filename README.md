# Interactive Inspector | iOS

<p align="center">
	<img src="https://raw.githubusercontent.com/Amarthgul/InteractiveInspector/main/Assets/Resources/Screenshots/CoverVer01.png" width="512">
</p>

This repo inherits much of the concepts from [this repo](https://github.com/cs-isamiul/Interactive-Anatomy-Visualization-Demo),
but took a different implemntation and is built for iOS platform. 

## Basic operations

* Touch and move with a single finger around the screen to rotate around the model 

* 2 fingers moving together to pan around the model

* Pinching 2 fingers to zoom in or out

* Tap on a part of the model to select and highlight it

* Once an object is selected, a pop up menu will show up at the bottom. 

  * The left-most icon is for settings. By default, the camera starts rotating around if there is no 
  user input for 5 seconds, you could disable that in the settings. Similarly, auto-voiceover can also
  be disabled. 

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
selection effect. 

3. Using the same method, create and add a vocie over audio clip in `Voice Overs` 
list and `Descriptions` list. The former one is for the audio voiceover when object
is selected, the later for the displayed text.


### Changing the displayed name of the object

The displayed name of the selected object is the name of the mesh. Thus, simply double
click on the mesh (or select the object in scene hierarchy and press `F2`) in scene hierachy to change the mesh name 
will alter the name disaplyed during runtime. 