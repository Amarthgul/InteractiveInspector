# Interactive Inspector | iOS

This repo inherits much of the concepts from [this repo](https://github.com/cs-isamiul/Interactive-Anatomy-Visualization-Demo),
but took a different implemntation and is built for iOS platform. 

## Add interactive models into the scene

Models by default are displayed but not selectable. 

To make models selectable, several steps need to be performed. 

1. Attach a `Mesh Collider` to the mesh (**to the mesh, not the pre-fab**).
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
click on the mesh (or select and press F2) in scene hierachy to change the mesh name 
will alter the name disaplyed during runtime. 