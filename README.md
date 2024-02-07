# Interactive Inspector | StarGazer

This branch is for estimating the possibility of an object/objects being gazed upon and
perform different operations. 

There are three main parts in this implementation, a camera gazer, a behavior interface, 
and a behavior definition. 

## Camera StarGazer 

The `StarGazer.cs` script is the source of the possibility estimation, it will estimate 
the possibility of each object it receives being gazed upon. The possibility is then sent
to each object and they will perform actions accordingly. 

The level of confidence that an object is being gazed upon is calculated by `ConfidenceFunction()`,
which enables future algorithms to be implemented easily. 

To add objects for the camera to estimate, create a new field in the `Gazable Object` and
drag the object there. **Note that the object need to possess a behavior inherited from the 
`ObjectBehavior` class**. 

## ObjectBehavior Class

The `ObjectBehavior` class has but one function:

```c#
	public void GazeOperation(float LoC);
```

As such, every 

## Behavior definition 

For an object to actually perform anything, the behavior must be explicitly defined in a new class
that inherit from `ObjectBehavior` and implements `GazeOperation()`. 

For example, the `ColorBehavior` script changes the sheen color of the objects accroding to the 
level of confidence given. 
