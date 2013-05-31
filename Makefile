SHELL=/bin/bash

export:
	xbuild /p:Configuration=Unity /p:PostBuildEvent= Tabletop.io.Gui.sln
	xbuild /p:Configuration=Unity-iOS /p:PostBuildEvent= Tabletop.io.Gui.sln

clean:
	rm -rf bin/
	rm -rf obj/
	rm -rf editor/bin/
	rm -rf editor/obj

