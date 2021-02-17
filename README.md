# collier 
A simple windows service for automating crypto miners.  

I mine Ether with my 3080 when its not doing anything else, and I've found
that if I manually stop it to play games I often forget to restart it.  
This service is meant to monitor GPU load, whether or not I'm gaming, or if 
if I'm idle and start / stop the miner when necessary.  A longer term goal is
to hook up a widget based UI to interact with the service.  
