# collier 
A simple windows service for automating crypto miners.  

I mine Ether with my 3080 when its not doing anything else, and I've found that if I manually stop it to play games I often forget to restart it.  The service allows you to set a list of process paths that are treated as gaming activity.  If they show up as using the GPU, the miner will be stopped so you can game.  When those processes are no longer running, the miner will automatically start back up again.  

Right now the service only supports the [t-rex miner](https://github.com/trexminer/T-Rex) (which needs to be installed separately), a single GPU and mining ether since this meets my requirements.  

There's a react-native windows front end to view the miner statistics.  It is not necessary to run the front end as t-rex miner has a web interface.  However I didn't want to monitor the miner through the browser and was also looking for a reason to check out react-native windows.  

## Setup
* Download and follow the instructions on the [release page](https://github.com/cwtowns/collier/releases/tag/v1.0.9.0).  
* Edit appsettings.json in the windows service directory and set a value for miner.t-rex.exeLocation
* Create a minersettings.private.json file in the windows service directory for your command line args to the miner.  This includes your address.  For example:

```
{
  "miner": {
    "t-rex": {
      "exeArguments": "-a ethash -o stratum+tcp://us1.ethermine.org:4444 -u 0xF2C12HJF093219432MJF02398423M01284320MFS -p x -d 0"
    }
  }
}
```

Here's an example of the user interface

![user interface](https://i.imgur.com/jdm1sJf.png)

Miner status will change colors depending on configured thresholds in the front end application.  These settings can be configured in the front end install location.  Reach out if you need to do this.  Currently having problems determining the UWP install location post reinstall.  

## Thoughts on React Native for Windows
* They need better debuggging documentation.  For example:
  * how do you debug release builds?  Can you see console.log statements?
  * when should you be using the Visual Studio solution?  
* The app crashes during hot swap.  A lot.  Once the app crashes I don't see a way to relaunch it without asking npm to run it again.  This slows down development.  
* Some things work in debug mode and not in release mode and it isn't clear why.  For example, I developed with crypto.getRandomValues() and it worked fine until I went to publish a release.  In release mode the module is missing.  I assume it's picking this up in debug mode because that runs through Chrome, and I'm getting the browser's version.  Finding this out only when we run a production build is not good.
* Related to the above, [not all node native methods are present](https://github.com/parshap/node-libs-react-native#globals).  This makes using some libraries problematic.  
* AirBnb dropped react-native stating, among other reasons, that they maintained multiple versions of their app code.  Native, Android, and iOS.  This was obvious from my experiment just on windows.  I have 3 native modules in this simple app to deal with file system access in UWP, handle the missing crypto module in release builds, and handle logging to disk since the platform appears to drop console.log entirely from release builds.  

## Thoughts on UWP

* The windows service is not a UWP app, which is the new preferred way for deploying windows applications.  UWP automatically containerizes your app, which is great, but it isn't clear to me how to manage external dependencies.  I don't want to ship the miner with my app as that would require a new build every time a new miner is released.  Can external dependencies be managed via UWP?
* The log files for the front end appear to be MIA post reinstall.  Finding out where the UWP app is installed to and what direcotries it uses is frustrating.  My default administrator account doesn't own any of the apps directories so drilling down into them via explorer isn't possible without changing ownership.  That doesn't give me good feelings.  

### History

Originally I thought the app would need to be much more sophisticated, like asking the GPU for its load and detecting when the user is idle or active.  However that didn't really result in a usable program.  Many things are using the GPU than you would expect (for example, Windows Terminal), and when polling the GPU randomly when not gaming you would see brief spikes of load.  This meant you'd have to poll over a period of time to really understand whether or not the GPU was in use.  This felt like over engineering (and might see remnants of that in the service code), so I settled on a set a process path based approach.  This made sense since most of my games live under very few locations.  
