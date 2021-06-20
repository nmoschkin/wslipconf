# WSLIPConf
Auto Configure Port Forwarding From WSL

Will automatically detect the WSL IP address and set up netsh portproxy commands for the IPv4 address.  

This is useful for running test projects in VS Code under WSL that serve and listen.  This will allow you to use your Windows-based browser or other front-end projects while running the back-end project in WSL.

Just download the code, compile it, and start the project.  It should be relatively self-explanitory. 

If for any reason the window gets messed up and you can't fix it, go into the registry under HKEY_CURRENT_USER\Software\Nathaniel Moschkin\WSLIPConf and delete the window geometry key.


