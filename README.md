2021-jan-03 - 20:27 - update
original oculus c++ dll for monogame can be found here https://github.com/cpt-max/OculusRift4MonoGame . but i didn't map the oculus touch controls yet. if i would do it, i would probably reference the code of the ab3d.OculusWrap on how it's done and you can find the ab3d.OculusWrap here https://github.com/ab4d/Ab3d.OculusWrap . Personally, i normally always use the ab3d.dxengine.OculusWrap for better graphics and soon, i will use the rest of what my ab3d.dxengine purchased license can offer for objects loading inside of C# without using monogame. But i like to test some stuff out in monogame from time to time and until i learn how to release a program using my commercial license of the ab3d.dxengine, i will be able to have a nicer Virtual Reality universe than what users see of how i coded the SCCoreSystems repository. the SCCoreSystems repository was a tryout of instanced objects and "instanced" physics engine and virtual reality desktops and all of it running on threads even the rendering to the console or wpf windows. It was a rookie tryout to make something work after reading the rastertek c# scripts found here as reference https://github.com/Dan6040/SharpDX-Rastertek-Tutorials . I don't even know yet how to make digital fire with a shader and rastertek type of terrain and grass yet, from using the rastertek c# repo as reference. Doing it in unity3d is so much easier.

# monogame-with-physics---VR-and-non-VR

Project 1: Oculus Rift c++ dll wrapper rebuilt in monogame 3.7.1.189 - i just rebuilt the original repo here https://github.com/cpt-max/OculusRift4MonoGame and made it work in monogame 3.7.1.189. Anyone can do that. it's super easy. There are no jitter physics in this one. it's a simple rebuild.

<img src="https://i.ibb.co/jb7vmNV/oculusv-wrappermonogame3-8-1-189.png" alt="oculusv-wrappermonogame3-8-1-189" border="0">

Project 2: Oculus Rift c++ dll wrapper with monogame 3.8.0.1641 with jitter physics... its a normal rebuild. Anyone can do that.
<img src="https://i.ibb.co/93RMfF5/my-rebuild-monogame-jitter-physics.png" alt="my-rebuild-monogame-jitter-physics" border="0">

Project 3: jitter physics in monogame 3.8.0.1641
I tried using the repo here https://github.com/mattleibow/jitterphysics but it didn't work out of the box. so i rebuilt it in monogame 3.8 3.8.0.1641 but using the Jitter physics from the google archive here https://code.google.com/archive/p/jitterphysics/ .  It doesn't include the fixes of github user mattleibow yet on some jitter physics code. The Jitter physics is MIT license and the jitter physics license is written all over the scripts inside of the jitter.dll. The mattleibow repo is probably still working but somehow i wasn't able to make it work after a while anymore. and instanced object with the jitter physics.
I just combined the oculus rift c++ dll with monogame 3.8.0.1641 with the original jitter physics and added an example of instanced
cubes using the jitter physics... you will notice that the first scene has normal jitter physics monogame instantiation of objects, and i spawned a couple of light gray cubes and at the exact identical place as them are green "instanced" cubes using the same jitter
physics engine position as the normal instantiated jitter physics demo code. achieving instanced objects in monogame was easier than i thought but i had issues in my shader. I used the instanced monogame scripts explained here https://community.monogame.net/t/hardware-instancing-shader/12259/2 or here https://community.monogame.net/t/hardware-instancing-shader/12259/3 if i remember correctly, and i used the pasteBin and unity3d forums way of capturing the vectors forward/right/up
for a quaternion. I think it was the user DeAngelo that posted how it worked back then on the unity forums. i will have to see if i screenshoted the forum post when he posted that, all credits to him back then because it was the only place i had found how to retrieve directions out of a quaternion.

<img src="https://i.ibb.co/m62dhkV/oculus-Riftc-dllwith-Monogame3-8-0-1641.png" alt="oculus-Riftc-dllwith-Monogame3-8-0-1641" border="0">

thank you for reading me.
steve chassé



