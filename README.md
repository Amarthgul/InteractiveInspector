# Interactive Inspector | StarGazer

This branch is for estimating the possibility of an object/objects being gazed upon and
perform different operations. 

There are three main parts in this implementation, a camera gazer, a behavior interface, 
and a behavior definition. 

The `ViewSelect` is the demo scene. 

## Camera StarGazer 

The `StarGazer.cs` script is attached to the camera and is the source of the possibility 
estimation, it will estimate the possibility of each object being gazed upon. 
The possibility is then sent to each object and they will perform actions accordingly. 

The level of confidence that an object is being gazed upon is calculated by `ConfidenceFunction()`,
which enables future algorithms to be implemented easily without affecting the rest of the
contents. 

To add objects for the camera to estimate, create a new field in the `Gazable Object` and
drag the object there. **Note that the object need to possess a behavior class that inherited from 
the `ObjectBehavior` interface**. 

If there are two cameras in the scene, as is the case for some AR/VR applications, check the 
`dualCamera` checkbox in the `StarGazer` component, and drag the second camera into the 
`meinCameraZwei` field. 

## ObjectBehavior Interface

The `ObjectBehavior` interface has but one method:

```c#
public void GazeOperation(float LoC);
```

This is needed as the camera gazer is calling this method to pass the gaze level of confidence. 

## Behavior definition 

For an object to actually perform anything, the behavior must be explicitly defined in a new class
that inherit from `ObjectBehavior` and implements `GazeOperation()`. 

For example, the `ColorBehavior` script changes the sheen color of the objects accroding to the 
level of confidence given. 

<hr /> 

## To Create a New Behavior 

First, create a new class that inherits from `ObjectBehavior`:

```c#
public class YOUR_CLASS_NAME : MonoBehaviour, ObjectBehavior
```

Inside the class, implement the method:

```c#
public void GazeOperation(float LoC) {/* DO_YOUR_THING_HERE */}
```

Remember `LoC` is the level of confidence sent from the camera gazer. 

After the behavior is defined, attach the script to the object. And drag the object into the 
camera Gazer's `Gazable Object` field. 
