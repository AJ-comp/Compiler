AJPGS
====
This project is about compiler including EBNF input system and parsing analysis tools and AJ language that system programming language. <br/>
<b>IDE project is not supported more so don't build the projects in the Application folder.<b/> <br/>

Basic Configure
----
> Build the projects CommandPrompt.Builder and CommandPrompt.Util. The projects is the top layer projects of the system. <br/>
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

![image](https://user-images.githubusercontent.com/69152847/167998745-13121dd5-a51a-4289-bc2d-cef44ea4007f.png)

12. Add main.aj file (file name does not matter) in the project path as below.

![image](https://user-images.githubusercontent.com/69152847/167367680-e72684d5-1904-49a3-894b-a84393eef56c.png)

13. Build with ctrl + shift + b. if the result is as below then all configure is successed.

![image](https://user-images.githubusercontent.com/69152847/167359849-f62a91e0-bdd3-40d4-9559-eb2bc3db2f35.png)

14. Good, now let's programming with AJ language. Before start need a few library sources. 
Download from this (https://drive.google.com/file/d/1l8_JHT4MmAtxzy4Gr4zsD225vS5ZgVhL/view?usp=sharing)
After download add to the project folder. the result is as below.

![image](https://user-images.githubusercontent.com/69152847/167375176-b86da848-3c18-4c85-9516-022db487b2db.png)

15. AJ language is similar C#. So syntax highlighting is compatible with C#. Click the language mode button of right bottom and change to C# as below.

![image](https://user-images.githubusercontent.com/69152847/167998433-4bcd8044-f1ac-47ab-82a9-55b4b08a8f3b.png)
* * *

Display problems
----
OK, if you did basic configure successfully you can use Problems feature of VS Code. click main.aj file in the solution explorer and let's type as below.

```C#
using System;

namespace Main;

public class TestMain
{
    public int a;
    public int b;
    
    public void Test()
    {
        int i=0;
        if(i > 10) i++;
        TTest();
        
        return 10;
    }
}
```

The above code has 2 problems. The first is TTest function is used even though is not declared. The second is Test function returns value 10 even though the return type of Test function is void. AJ Compiler will catch this problems and display it. Build with ctrl + shift + b and you can see problems that AJ compiler catch as below.

![image](https://user-images.githubusercontent.com/69152847/167423068-5b4e22d8-4f3b-4964-87de-702030f62be8.png)

Good, as above if syntax error or semantic error is fired the position that error fired shows.
* * *

Generate LLVM IR
----
AJ Compiler can generate LLVM IR if the code correct.
Let's write the code as below in main.aj file.

```C#
using System;

namespace Main;

public class TestMain
{
    public int a;
    public int b;

    public void Main()
    {
        Test2();
        Test();
    }

    public int Test2()
    {
        return 10;
    }

    public void Test()
    {
        Main.TestMain c;

        int i = 10;
        int j = i++;

        if(true) i++;
        else i--;

        if(true) i++;

        int z = i / j;

        if(false) j = j%10;
        else j = j * 10;

        while(true)
        {
            i++;
            if(i > 20) continue;
            else if(j > 20) break;

        return;
        }
    }
}
```

The above code correct according to AJ grammar. if you build with ctrl + shift + b then you can see to generate main.aj.ll file as below.

![image](https://user-images.githubusercontent.com/69152847/206813072-daabd784-a9e7-4e63-843c-26670869ec01.png)

The content of main.aj.ll file is as below. Display original source file for llvm ir.

![image](https://user-images.githubusercontent.com/69152847/206813710-73cea74b-8e8e-4554-83c0-6c1b0fbb8711.png)
* * *

Generate assembly code
---
If you did generate LLVM IR and you have a llc.exe program (this can get through LLVM project) you can generate assembly code with LLVM IR.

1. Move to terminal window of VS code.
2. Input the command llc.exe main.aj.ll and you can get assembly file as below. (if you don't have llc.exe this work will be failed.)

![image](https://user-images.githubusercontent.com/69152847/206820166-e4c33edc-6a14-4920-825c-016b6273d5f4.png)
* * *


Publications
----
The below video is that about this project. <br/>
https://www.youtube.com/watch?v=oMhS3l0DyLo&t=187s <br/>
https://www.youtube.com/watch?v=m1ylQsQRPF8&t=29s <br/>
https://www.youtube.com/watch?v=QwPlyto8JAA&t=22s <br/>
