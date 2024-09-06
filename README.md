# Unity Ultimate UI Scroll chooser
This is a re-make of this package
https://assetstore.unity.com/packages/tools/input-management/scroll-flow-190674#description

It was poorly made, but looked really nice, so i decided to optimize it and add more features!

![scroll](https://github.com/user-attachments/assets/b6ffceec-9080-4aeb-b219-376b0910c837)

## Why this is better, than inital asset?
### Optimization differences
1. Removed all **.GetComponent** from Update method
2. No **.GetChild** calls
3. No more unsafe value retrieving from object name
4. Removed recursive unused properties
5. Overall better architecture and code-style


### New features
1. Added event Action<int> ValueChanged in ScrollMechanic.cs
2. Added Color property 
3. Setter for current value (**.SetCurrentTarget(int firstTarget)**)
4. Transparency support

## Unresolved issues 
1. Very fast scroll can glitch a little for a frame. Not lethal, but its annoying 
2. Old input system support only (i moved all inputs to separate method, so it can be easily be reworked, but im just lazy)
