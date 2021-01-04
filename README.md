# monogame-with-physics---VR-and-non-VR

Project 1: Oculus Rift c++ dll wrapper rebuilt in monogame 3.7.1.189 - i just rebuilt the original repo here https://github.com/cpt-max/OculusRift4MonoGame and made it work in monogame 3.7.1.189. Anyone can do that. it's super easy.


Project 2: Oculus Rift c++ dll wrapper with monogame 3.8.0.1641 with jitter physics and instanced object with the jitter physics.
I just combined the oculus rift c++ dll with monogame 3.8.0.1641 with the original jitter physics and added an example of instanced
cubes using the jitter physics... you will notice that the first scene has normal jitter physics monogame instantiation of objects, and i spawned a couple of light gray cubes and at the exact identical place as them are green "instanced" cubes using the same jitter
physics engine position as the normal instantiated jitter physics demo code. achieving instanced objects in monogame was easier than i thought but i had issues in my shader. I used the instanced monogame scripts explained here https://community.monogame.net/t/hardware-instancing-shader/12259/2 or here https://community.monogame.net/t/hardware-instancing-shader/12259/3 if i remember correctly, and i used the pasteBin and unity3d forums way of capturing the vectors forward/right/up
for a quaternion. I think it was the user DeAngelo that posted how it worked back then on the unity forums. i will have to see if i screenshoted the forum post when he posted that, all credits to him back then because it was the only place i had found how to retrieve directions out of a quaternion.

Project 3: jitter physics in monogame 3.8.0.1641 - no VR
I tried using the repo here https://github.com/mattleibow/jitterphysics but it didn't work out of the box. so i rebuilt it in monogame 3.8 3.8.0.1641 but using the Jitter physics from the google archive here https://code.google.com/archive/p/jitterphysics/ .  It doesn't include the fixes of github user mattleibow yet on some jitter physics code. The Jitter physics is MIT license and the jitter physics license is written all over the scripts inside of the jitter.dll. The mattleibow repo is probably still working but somehow i wasn't able to make it work after a while anymore.

thank you for reading me.
steve chassé



