# WSLIPConf
Auto Configure Port Forwarding From WSL

This program automatically detects the WSL IP address using the Linux _ip_ command, and sets up _netsh portproxy_ commands for the IPv4 address.  

This is useful for running test projects in VS Code under WSL that serve and listen.  This will allow you to use your Windows-based browser or other front-end projects while running the back-end project in WSL.

__Binaries are included for x64 in the _binaries_ folder.

Just download the code, compile it, and start the project.  It should be relatively self-explanatory. 

You might need to run 'git submodule init' and 'git submodule update' from the root of the project folder to grab the MessageBoxEx project.

If for any reason the window gets messed up and you can't fix it, go into the registry under __HKEY_CURRENT_USER\Software\Nathaniel Moschkin\WSLIPConf__ and delete the window geometry key.

## Sample Screens

### Main Screen

_(Currently only V4ToV4 bindings are supported)_

![](docs/image1.png)

### Binding Configuration
![](docs/image2.png)

### About Box 
![](docs/image3.png)

