# Monkey Skull Interactive | iOS

This repo inherits much of the concepts from [this repo](https://github.com/cs-isamiul/Interactive-Anatomy-Visualization-Demo),
but took a different implemntation and platform. 

## Add models into the scene

To add models into the scene, attach a `Mesh Collider` to the mesh (**to the mesh, not the pre-fab**).
Then click on `TapHandler` in the scene, in the Inspector, add a new element in the `Can Be Tapped` list, 
drag the new mesh into the slot, this will make the mesh selectable. 
