# SculptVR

![demo1](https://user-images.githubusercontent.com/53326572/215682195-1de544be-b4b1-4690-89ab-2dbd801d74e0.gif)
![demo2](https://user-images.githubusercontent.com/53326572/215682860-33310c80-0010-487d-a043-0b667c30b347.gif)

Check the video [Demo.mp4](Demo.mp4) for more demonstrations.

## General Information

Note: This project is MASSIVE, as it includes the Oculus Developer SDK Unity Toolchain. If you just wish to use the application, it is highly recommended you either download the unity build below or only download the folder ScupltVR
Google Drive Build Location: https://drive.google.com/file/d/1BMry69IqH-KVFxrCFwlrZD1ZU_L_gXX4/view?usp=sharing

### Team Members:

Cole Davidson & Runqiu Guo

### Third Party Assets:

Make sure to document the name and source of any third party assets such as 3D models, textures, or any other content used that was not solely written by you.  Include sufficient detail for the instructor or TA to easily find them, such as a download link.

- Floor Texture By [3dTextures.me](https://3dtextures.me/)
- Easel model By [mubble](https://opengameart.org/users/mubble)
- Swamp Paint, Ice Painting, and Desert Painting images by [Jap](https://opengameart.org/users/jap)
- Low Poly Pottery Models by [FabinhoSC](https://opengameart.org/users/fabinhosc)
- Cabinet and Door models by [Quaternius](https://quaternius.com)
- Ship Painting is public domain, but the editing was done by [eleazzaar](https://opengameart.org/users/eleazzaar)
- Chisel model is also public domain, no credited author, but model was hosted by [Creazilla](https://creazilla.com/nodes/67717-bog-standard-chisel-3d-model)
- Oculus Integration Package By Oculus

All models used under the CC License

### Project Description:

Our Project is a VR application for modeling turnwheel pottery. We represent the clay using Volumetric data, then render it using the marching cubes algorithm. Marching Cubes is quite intensive, even for a Desktop computer, so we utilize Oculus Link to improve performance.

To model the clay, the user directly manipulates the volumetric data. This is done via several interaction techniques, some of which can be used in tandem.

#### ShrinkyBrush
The de facto way to sculpt in this application is through this technique. When enabled, the user has an additional wand attached to the end of each controller. When the user squeezes the grip button, the brush tip will grow and a sphere will appear on the wand. This was developed as a way to explore using grip strength as an additional DOF in VR controls.

#### Chisel
This technique is meant to clean up unwanted protrusions on the pot surface. It is an asymmetric bimanual pointing technique, where a ray is sent from the dominant hand through the non-dominant hand into the scene. We chose to use both hands to point since it reduces the magnitude of error that commonly comes with pointing techniques; The user must move their primary hand a greater distance in order to influence the pointing direction. In addition, by using hand position to determine the point direction, we were also able to apply the PRISM technique to both controllers to increase the pointing sensitivity; jitter doesn't affect the pointing direction at all and the user must move their hands swiftly to rapidly change the pointing direction.

#### Flight Stick
This interaction technique is used to change the axis of rotation; it has no effect on the pot's orientation, just how it rotates. By default, the axis of rotation is the up vector, as it would be in real life. However, by grabbing the joystick using the grasp action, you can reposition the axis of rotation to match the orientation of the joystick. Our original plan was to use Spindle + wheel to manipulate the axis of rotation, but we found Flight Stick far more intuitive to use.
Provide a general overview of your project and describe each of its functions.

#### Bare Hands
This interaction technique is intuitive: With the help of hand tracking functionalities from the Oculus Integration SDK, users can use their hands to touch and manipulate the shape of pots. In the hand tracking scene, you can switch between controllers/hands by simply picking up/putting down the controllers.

#### ShrinkyBrush for Hands
An extended version of the original shrinky brush on hand tracking. We generate brush tips (sphere colliders) between the users’ thumb tips and index tips. The users may manipulate the size of brush tips by moving their thumb and index fingers.

### Instructions:

Provide instructions for running and testing your project's various features.  These should clearly describe how to use each of your project's functions so that the instructor can understand how to use it properly.

#### Starting the Application
This project was built into a Windows application instead of an Android apk, due to the use of oculus link. We found that we could start the application using the following process.
Connect the Quest to the Computer & Enable Oculus Link
Navigate to the Build Folder.
Open the SculptVR Unity Application
Put the headset back on. You should be inside the application.

#### Default Mode
By default, there is a point grabber assigned to each controller; it works just like the one we made in class for Assignment 4. This is used to enable and disable other features in the 3D menu. When the line renderer is green, press the trigger to enable/disable the feature.

#### ShrinkyBrush
When enabled, 2 wands will appear in front of both controllers. By squeezing the left and right grasp buttons, the radius of the sphere will increase. If you squeeze the button as tightly as possible, the brush tip will expand as large as it can, and if you don’t touch it at all, it will disappear. Varying levels of pressure in between will change the size.

If enabled, to disable just use the point grabber to select the shrinky brush button again.

#### Chisel
Chisel is bimanual asymmetric, so you must be careful when enabling it. When you select the Chisel button, the hand which presses the button is assigned the role of dominant hand. So if you are left handed, select it using the left hand, and use your right hand if you are right handed. You can disable it using either hand however.

There are two different controls on the Chisel. By pushing the dominant hand’s joystick left or right, you can rotate the pot, so you don’t need to walk around the turnwheel. This is sensitivity dependent, so to move faster, press the joystick further.

The chisel also uses PRISM for precision aiming, which can take a lot of getting used to. If you find that your hands are drifting too far away from the controller, you can reset using the primary button on the controller (x in left handed mode and a in right handed mode). You may notice that the line doesn't move when you are aiming at the pot; don't be alarmed. Since the clay is a point cloud, the chisel is automatically rounding your line to the nearest surface point that the line passes through. This may mean if you move your hand in small amounts, the line will still round to the same location.

Since you are modifying the volumentric function, we have placed a little blue cube which serves as a reticle at the currently selected point. Sometimes this will be completely hidden by the surface mesh, and other times it will not. 

Also, when you are aiming at the pot, you should see a line pointing from the dominant hand to the pot’s surface; this indicates which point in the volumetric data you will manipulate. When you see this line, if you release the grasp button, the pot will get chipped at the location which you are pointing. The action is mapped to release and not pressed so users can cancel their action by pointing away from the pot if they change their mind.

#### FlightStick
FlightStick uses a grasp grabber to select the joystick handle. When your controller is touching the handle, squeeze the grasp button to select the handle. After that, all you must do is move your hand to move the joystick accordly. The joystick will follow your hand until you release the grasp button, even if you move your hand off the joystick. FlightStick can be enabled and disabled just like all other controls; point near the FlightStick (not the red orb) to enable/disable.

#### Start Table
This button can be activated using the Point Grabber. Pressing it will toggle the table rotation.

#### Reset Button
This button can also be activated using the Point Grabber. Pressing it will reset the clay to a block.

#### ChangingScenes
To switch between handtracking and controllers, we built 2 separate scenes. To change the scene, in either room, turn around and point your controller at the door. When the line is green, press the trigger and you will be swapped to the other room.

#### Hand tracking
In order to make the hand tracking in the project work properly, you need to first make sure you have already toggled on the hand tracking and auto switch in your quest: Go to Settings → Device → Hand and Controllers, then select the toggle for hand tracking and auto switch for hand tracking. To use the hand tracking feature in the project, first go to the hand tracking scene. Afterwards, put down your controllers and wait until you can see both of your hands. Later you will be able to manipulate the pot with your hand gestures.

#### ShrinkyBrush for Hands
To toggle on this feature, first go to the hand tracking scene. Then press button B/Y to turn on the brush on the right/left hand. Then put down your controllers and after a while you can see both of your hands have blue spheres between thumbs and index fingers. Moving those two fingers will allow you to change the size of the brush tip. You can use the brush to sculpt the pot like the original ShrinkyBrush mentioned earlier. To toggle off this feature, simply pinch your thumb with index finger (i.e. make the blue sphere extremely small). You can also turn it off by pressing the A/X button if you are in the controller mode.

## License

Material for [CSCI 5619 Fall 2021](https://canvas.umn.edu/courses/268490) by [Evan Suma Rosenberg](https://illusioneering.umn.edu/) is licensed under a [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-nc-sa/4.0/).

The intent of choosing CC BY-NC-SA 4.0 is to allow individuals and instructors at non-profit entities to use this content.  This includes not-for-profit schools (K-12 and post-secondary). For-profit entities (or people creating courses for those sites) may not use this content without permission (this includes, but is not limited to, for-profit schools and universities and commercial education sites such as Coursera, Udacity, LinkedIn Learning, and other similar sites).

