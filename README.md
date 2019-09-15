# broadridge

## The Platform
Net Core 3.0 preview. https://dotnet.microsoft.com/download/dotnet-core/3.0

## IDE
VS 2019

## Configuration
The current version has 2 parameters that must be specified in the appsettings.json file.

{

  "maxparallelism": 4,  //the maximum count of files that might be processed in parallel
  
  "txtprocessorwriterthreshold": 32768 // Back pressure and flow control for txt processor 
  
}

## Comments

1. Solution contains several projects to decompose code according to improve maintenability
2. The fileparser.host contains console application. An example of run is fileparser.host.exe "../../../../testdirectory"  "result.txt"
The Source Control contains little files to process
3. Net Core 3.0 is used just to try the new promising idea of using SPAN\Memory\Pipelines. I need more time to decrease the memory footprint using 
this technology. I believe many improvements might be done.
4. The solution only has one test project with one unit test inside. Real implementation would require extra test project per each project with code with many unit tests inside.


   
