functions:

#############

UPDATE_VERSION == increment version of the assembly, file and manifest by:
	Debug: revision +1
	Release: build +1 and revision=0
	exception for other build configs

USE IN PRE-BUILD, 

syntax:
BuildAssist.exe UPDATE_VERSION $(ConfigurationName) $(ProjectDir)\Properties\AssemblyInfo.cs $(ProjectDir)\Properties\app.manifest

Notes on params:
	$(ConfigurationName) => macro replaced with build config name
	
#############

RELEASE_PUBLISH == roll a release build into a special separate dir and compress into a versioned zip file.
	Debug: does not do anything
	Release: performs the operation
	exception for other build configs

USE IN POST-BUILD, 

syntax:
BuildAssist.exe RELEASE_PUBLISH $(ConfigurationName) $(ProjectDir)\Properties\AssemblyInfo.cs $(TargetDir) [PublishDirPath]

Notes on params:
	$(TargetDir) => macro for full path to output dir for current build config, ex. C:\MyCoolProg\CoolProject\bin\Release
	[PublishDirPath] => path to the dir where zip will be placed

#############