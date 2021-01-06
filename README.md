2021-jan-03 - 20:27 - update
original oculus c++ dll for monogame can be found here https://github.com/cpt-max/OculusRift4MonoGame . but i didn't map the oculus touch controls yet. if i would do it, i would probably reference the code of the ab3d.OculusWrap on how it's done and you can find the ab3d.OculusWrap here https://github.com/ab4d/Ab3d.OculusWrap . Personally, i normally always use the ab3d.dxengine.OculusWrap for better graphics and soon, i will use the rest of what my ab3d.dxengine purchased license can offer for objects loading inside of C# without using monogame. But i like to test some stuff out in monogame from time to time and until i learn how to release a program using my commercial license of the ab3d.dxengine, i will be able to have a nicer Virtual Reality universe than what users see of how i coded the SCCoreSystems repository. the SCCoreSystems repository was a tryout of instanced objects and "instanced" physics engine and virtual reality desktops and all of it running on threads even the rendering to the console or wpf windows. It was a rookie tryout to make something work after reading the rastertek c# scripts found here as reference https://github.com/Dan6040/SharpDX-Rastertek-Tutorials . I don't even know yet how to make digital fire with a shader and rastertek type of terrain and grass yet, from using the rastertek c# repo as reference. Doing it in unity3d is so much easier.

# monogame-with-physics---VR-and-non-VR

Project 1: Oculus Rift c++ dll wrapper rebuilt in monogame 3.7.1.189 - i just rebuilt the original repo here https://github.com/cpt-max/OculusRift4MonoGame and made it work in monogame 3.7.1.189. Anyone can do that. it's super easy. There are no jitter physics in this one. it's a simple rebuild.

<img src="https://i.ibb.co/jb7vmNV/oculusv-wrappermonogame3-8-1-189.png" alt="oculusv-wrappermonogame3-8-1-189" border="0">

Project 2: Oculus Rift c++ dll wrapper with monogame 3.8.0.1641 with jitter physics... its a normal rebuild. Anyone can do that.
<img src="https://i.ibb.co/93RMfF5/my-rebuild-monogame-jitter-physics.png" alt="my-rebuild-monogame-jitter-physics" border="0">

Project 3: jitter physics in monogame 3.8.0.1641 with he original oculusrift.dll . The oculus touch mapping isnt done yet.
I just combined the oculus rift c++ dll with monogame 3.8.0.1641 with the original jitter physics and added an example of instanced
cubes using the jitter physics... you will notice that the first jitter physics scene has normal jitter physics monogame instantiation of light gray cubes, but added a drawinstanced example to make use of instanced rendering using the jitter physics with the position and rotation sent to the shader and accessed by instanced objects and those are green.
physics engine position as the normal instantiated jitter physics demo code. achieving instanced objects in monogame was easier than i thought but i had issues in my shader. I used the instanced exemples explained here https://community.monogame.net/t/hardware-instancing-shader/12259/2 or here https://community.monogame.net/t/hardware-instancing-shader/12259/3 if i remember correctly, and i used the pasteBin and unity3d forums way of capturing the vectors forward/right/up
for a quaternion from the user DeAngelo that posted how it worked back then on the unity forums. i will have to see if i screenshoted the forum post when he posted that, all credits to him back then because it was the only place i had found how to retrieve directions out of a quaternion but i cannot find that post anymore and use the www as reference.

Edit 2021-jan-05: something is wrong with the cubes visual from the monogame directX window but inside of the headset VR view, it is showing the terrain at the correct position? i will try and fix this issue when i have the time.
<img src="https://i.ibb.co/m62dhkV/oculus-Riftc-dllwith-Monogame3-8-0-1641.png" alt="oculus-Riftc-dllwith-Monogame3-8-0-1641" border="0">

It is stupidly easy to code something like that but i couldn't find any VR Monogame shared project with physics included, back when i started coding in 2016. But last year about july 2020 approx, i stopped working on the SCCoreSystems repository draft, in order to try Monogame again... i started with this very simple project, but the oculus touch aren't working here yet with the oculusRift.dll c++ wrapper. So i went ahead and attempted again to bring the ab3d.oculusWrap inside of monogame with and without using the ab3d.dxengine.oculusWrap which is of better quality and you can find the repository here https://github.com/ninekorn/SCCoreSystemsMono for that project using the ab3d.oculusWrap inside of Monogame with the physics engine jitter. Switching from BepuV1 and BepuV2 or jitter is a piece of cake thing to do and i will see when i will have the time to code that. But Jitter is MIT licensed and the BepuV1 and BepuV2 physics engine have the Apache 2.0 license and the repositories are here https://github.com/bepu/bepuphysics1 and here https://github.com/bepu/bepuphysics2 . I will soon upload a virtual reality SCCoreSystems solution or repos with the BepuV1 and BepuV2 working inside of virtual reality for a basic started project to anyone who wants to attempt programming with their oculus rifts headsets in C#. I am starting to get more and more interested in trying to include Python and C++ physics engines in order to use their physics engine as wrappers for C#, just as if a it was an "Oculus Rift Wrapper". I thought my engine could push 15k physics rigidbody in the same scene, but that was fake performance as i was only trying to modify the jitter physics settings in the function

public void Step(float totalTime, bool multithread, float timestep, int maxSteps)

but i thought i was getting somewhere with less lag with 15k physics objects in one scene, but those 15k objects were actually from 3 to 8 jitter physics engines running at the same time with each of their rigidbodies unable to collide yet with any physics engine objects from other physics engines instances. so divide 15k by 8 or even 12 physics engines jitter running at the same time, and only about 1250 to 1850 collide with each other... I had the time to explain that somewhere in my programming notes on my computer. But, i can have 1 terrain that collides with each of those physics objects if i had 1 terrain to each physics engines instances of jitter. My attempts at more than 1 physics engines of jitter working in the same program can be found in my SCCoreSystems repository here https://github.com/ninekorn/SCCoreSystems-rerelease .

thank you for reading me.
steve chassé



