(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/PathfinderAttackSimulator/bin/Release/netstandard2.0"

(**
Web Application for PathfinderAttackSimulator
=============================================

<div class="row">
  <div class="span1"></div>
  <div class="span6">
      <pre> This area is still under construction! </pre>
  </div>
  <div class="span1"></div>
</div>

Most people i play with do not have any experience in programming, thats why they shy away from using this toolbox.
So i started working on a web application so everyone can easily use all tools without having to download a code editor.
The web application is not yet finished, but i want to present you the progress so far. If you have feedback or just feel 
like i have no idea what i am doing, feel free to open an issue [here](https://github.com/Freymaurer/PathfinderWebApp/issues).
All of the following is build on a [SAFE stack](https://safe-stack.github.io/) framework

<br>
![PathfinderAppOverview](img\PathfinderWebApp1.PNG)
<br>


Above you can see the general layout. A click on "Open new Attack Calculator tab"(1) will add a new "Attack Calculator"(2) element to the page.
A click on the "hide/show"(3) button will open up the attack calculator tab.

<br>
![PathfinderAppAttackCalculatorTabOverview](img\PathfinderWebApp2.PNG)
<br>

In the area of number 4 will be a display of your selected character,weapons, size and modifications. But to see something there you'll need to first select everything. 
You can do this using the buttons at the bottom (5). This will open up a searchbar and a result table from which you can choose.

<br>
![PathfinderAppAttackCalculatorSearchBar](img\PathfinderWebApp3.PNG)
<br>

To the right of every search result you can then find a button to add the character to the area 4. In the same manner one can search for weapons and modifications.
In the end it will look like this: 

<br>
![PathfinderAppAttackCalculatorSearchBar](img\PathfinderWebApp4.PNG)
<br>

There are still several points i need to adress until i will host this:

* Enable the use of modifications that need additional variables
* Enable the option to add own characters, weapons, modifications
* Add the remaining toolbox features
* Enable the option to download a .txt document with all additionally added characters, weapons and modifications,
which can then be uploaded to the application to automatically load all previously created characters, weapons and modifications.



*)
