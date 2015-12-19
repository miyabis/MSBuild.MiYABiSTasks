# MSBuild.MiYABiSTasks

[![Build status](https://ci.appveyor.com/api/projects/status/kew6mx25v90xylxc?svg=true)](https://ci.appveyor.com/project/miyabis/seleniumtestassist)
[![NuGet](https://img.shields.io/nuget/v/MiYABiS.MSBuild.svg)](https://www.nuget.org/packages/MiYABiS.MSBuild/)

It is a task collection that adds an insufficient amount in the tasking facility of the MSBuild standard. 

All tasks that can be used with MiYABiS Build Tasks

Task | Description
------ | -------
CopyEx | The file and the folder are copied.
CreateItemFromFile | The list of the item is read from the text file specifying the character-code.
FolderDiff | Files that exist under the control of two folders are compared.
TFFolderDiff  | It runs the comparison using TF.exe.

## Copy Task

It is a task of enhancing the function of the Copy task of the MSBuild standard. 

>When DestinationFolder Property is specified, the path configuration of the subordinate who removed the path name from the file name of the copy origin can be copied as it is.  

>When SkipUnchanged Property is True, the file not copied is not included in the output parameter.

## CreateItemFromFile Task

It is a task of enhancing the function of the ReadLinesFromFile task. 
>It is possible to read by specifying the character-code of the file.

## FolderDiff Task

It is only addition of the last updated date of presence and the file of the file that compares. 
Content of the file is not compared. 

## TFFolderDiff Task

It runs the comparison using TF.exe.
