# WSLIPConf
Auto Configure Port Forwarding From WSL

This program automatically detects the WSL IP address and sets up netsh portproxy commands for the IPv4 address.  

This is useful for running test projects in VS Code under WSL that serve and listen.  This will allow you to use your Windows-based browser or other front-end projects while running the back-end project in WSL.

__Binaries are included for x64 in the _binaries_ folder.

Just download the code, compile it, and start the project.  It should be relatively self-explanatory. 

You might need to run 'git submodule init' and 'git submodule update' from the root of the project folder to grab the MessageBoxEx project.

If for any reason the window gets messed up and you can't fix it, go into the registry under HKEY_CURRENT_USER\Software\Nathaniel Moschkin\WSLIPConf and delete the window geometry key.

![](https://raw.githubusercontent.com/ironywrit/wslip/master/docs/screen1.png)


