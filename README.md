# Compiler (Basic Configure)
This project is about compiler including EBNF input system and parsing analysis tools and AJ language that system programming language. <br/>
IDE project is not supported more so don't build the projects in the Application folder. <br/>

Build the projects CommandPrompt.Builder and CommandPrompt.Util. The projects is the top layer projects of the system. <br/>
To use the compiler for AJ language in the VS Code follow the below sequence. <br/>

1. Build the projects CommandPrompt.Builder and CommandPrompt.Util.
2. Add to the environment variable the path that there is a ajbuild program. (ajbuild program is created after build CommandPrompt.Builder project.)
3. Add to the environment variable the path that there is a ajutil program. (ajutil program is created after build CommandPrompt.Util project.)
4. Execute the cmd and input ajbuild and ajutil. if you get screen as below then the configure is successed.

![image](https://user-images.githubusercontent.com/69152847/167346391-8e345c1f-5666-4e77-934e-67adecdccc1d.png)

5. Create solution file by input "ajbuild new --output [path to create sln file] sln". (ex: ajbuild new --output c:\ajtest sln). <br/>
6. Create the start project file by input "ajbuild new --output [path to create ajproj file] start". (ex: ajbuild new --output c:\ajtest\test start). <br/>
7. Add project file to the solution by input "ajbuild sln [solution path] add [the full path of project file to create])". (ex: ajbuild sln c:\ajtest add c:\ajtest\test). <br/>
8. If you completed for now then your solution folder will be as below.
  
![image](https://user-images.githubusercontent.com/69152847/167367389-b5eb6c3f-c35d-49d9-a86c-14378dc247e7.png)

9. Ok read the folder (the folder existing sln file) from VS Code.
10. Create the tasks.json file.
11. Type the below content (if your solution path is different change the content of red underline.)

![image](https://user-images.githubusercontent.com/69152847/167354844-e521ad39-3248-479b-82ff-4f5770f46a07.png)

12. Add main.aj file (file name does not matter) in the project path as below.

![image](https://user-images.githubusercontent.com/69152847/167367680-e72684d5-1904-49a3-894b-a84393eef56c.png)

13. Build with ctrl + shift + b. if the result is as below then all configure is successed.

![image](https://user-images.githubusercontent.com/69152847/167359849-f62a91e0-bdd3-40d4-9559-eb2bc3db2f35.png)

14. Good, now let's programming with AJ language. Before start need a few library sources. 
Download from this (https://drive.google.com/file/d/1l8_JHT4MmAtxzy4Gr4zsD225vS5ZgVhL/view?usp=sharing)
After download add to the project folder. the result is as below.

![image](https://user-images.githubusercontent.com/69152847/167375176-b86da848-3c18-4c85-9516-022db487b2db.png)

15. AJ language is similar C#. click main.aj and let's type as below and build with ctrl + shift + b.

![image](https://user-images.githubusercontent.com/69152847/167423068-5b4e22d8-4f3b-4964-87de-702030f62be8.png)

16. Good, as above if syntax error or semantic error is fired the position that error fired shows.




