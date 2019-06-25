# DependenciesExplorer
Simple f# console application to see visually ( with d3 ) the nuget dependencies of a source code ( in c# projects)

## How it works 
There is a branch with the executable file ([executable-file](https://github.com/hiroCat/DependenciesExplorer/tree/executable-file)) that has the self-contained console application.
If we launch the executable and then paste the path to our source code we are going to be on our way.
We are going to get two questions regarding if we want to include test projects or not and if we want to include external dependencies or not.
Once we clicked ok we are going to get the number of projects / number of tests projects and a browser should open up with a D3 diagram.
Here are some examples.
![Capture1](https://user-images.githubusercontent.com/5578838/60126899-44173580-978f-11e9-8c87-d7d2c095ff05.PNG)
![Capture12](https://user-images.githubusercontent.com/5578838/60126900-44173580-978f-11e9-8ae4-4719fc6a0b5a.PNG)

## TODO/Known issues
I has a LOT of limitations/work in progress but it's a start, so let's list them.
- Windows only :( (for the executable file, since it runs on a modified nuget for the graphics)
- Only csharp projects ( fsharp projects it's a working progress) 
- Not a right way to use the Xplot library ( currently using a local nuget because there is no stable nuget generated on his repo)
- Using nuget (yeah I didn't realize that I wasn't using paket since late on, to change tho !)
- All together (it's a bit of a mess, to split in the future!)
- The test/no test validation is done checking the xunit/nunit paket so may not be accurate.
- Maybe other stuff that surely I haven't tried
