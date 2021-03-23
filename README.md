# collier 
A simple windows service for automating crypto miners.  

I mine Ether with my 3080 when its not doing anything else, and I've found that if I manually stop it to play games I often forget to restart it.  The service allows you to set a list of process paths that are treated as gaming activity.  If they show up as using the GPU, the miner will be stopped so you can game.  When those processes are no longer running, the miner will automatically start back up again.  

Originally I thought the app would need to be much more sophisticated, like asking the GPU for its load and detecting when the user is idle or active.  However that didn't really result in a usable program.  Many things are using the GPU than you would expect (for example, Windows Terminal), and when polling the GPU randomly when not gaming you would see brief spikes of load.  This meant you'd have to poll over a period of time to really understand whether or not the GPU was in use.  This felt like over engineering (and might see remnants of that in the service code), so I settled on a set a process path based approach.  This made sense since most of my games live under very few locations.  

Right now the service only supports the t-rex miner (which needs to be installed separately) and a single GPU since this meets my requirements.  

I'm also working on a simple react native UWP application to show mining statistics, miner state and a short tail of the log.  This is intended to keep as a widget on my second monitor so I have easy data available at a glance.  It should be noted that t-rex comes with a web server built in that can provdie a lot of this functionality.  But I really didn't want my UX in the browser and this gave me a project to check out the new stuff in .NET 5 and play with react-native.  
