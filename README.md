Wurm Assistant (2.x)
====================
This program expands [Wurm Online][] game experience by providing handy tools for assisting with more tricky game activities.

For more information, please visit:

* [Wurm Assistant Blog][blog website]
* [Wurm Assistant Official Discussion][official thread]

This project is licensed under [GPL]. Some 3rd party components/libraries may be licensed under different terms.

Contributions and Credits
-------------
http://blog.aldurcraft.com/WurmAssistant/page/Contributors-and-Supporters

About the code
--------------
WA2 is written in C# using .NET 4.0 Windows Forms framework. Solution requires Visual Studio 2012+ due to async usage (Express edition is fine).

Repository is 100% self-contained, all the dependencies are either projects in the solution or libraries in _lib dir. Build robot is also part of this solution and build events are macroed to work for any location. However, it may be necessary to reaquire Microsoft.Bcl.Async package.

This version is discontinued. Work on version 3.0 has moved to Codeplex:
-link-to-be-added-

Even older version (1.x) of this project is available here:
https://github.com/PeterAldur/WurmAssistant

[3rdpartycomponents]: http://blog.aldurcraft.com/WurmAssistant/page/Contributors-and-Supporters
[Wurm Online]: http://wurmonline.com/
[blog website]: http://blog.aldurcraft.com/wurmassistant/
[official thread]: http://forum.wurmonline.com/index.php?/topic/68031-windows-tool-wurm-assistant-wa2-alpha-released/
[GPL]: http://www.gnu.org/licenses/gpl.html
[email]: aldurcraft@gmail.com
[Microsoft.BCL]: http://nuget.org/packages/Microsoft.Bcl.Async/
[irrKlang]: http://www.ambiera.com/irrklang/
[SQLite]: http://www.sqlite.org/
[Notification Window]: http://www.codeproject.com/Articles/277584/Notification-Window
