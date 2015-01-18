# Reversi
A proof of concept game of Othello (aka Reversi), built using MVC5, SignalR, AngularJS, Bootstrap, etc

This project was designed to gain an initial and working understanding of MVC(5) and SignalR.  The frontend initially implemented Razor to access the MVC model and display the grid and counters.  Once that was successfully working, Bootstrap CSS and AngularJS was added, replacing jQuery and the original design.

The master branch is the original (jQuery / Razor front end, with mixed CSS).
The bootstrap branch is the original updated to use (Twitter) Bootstrap 3.
The angular branch is the bootstrap updated to use AngularJS in place of jQuery.

	
There are many things that could be improved, for example:

 -  NOTE: The AngularJS version is incomplete.  It does, however, demonstrate that I am able to learn and implement multiple new frameworks and models.  Some parts are complete (chat), but I was unable to complete over the weekend.
    For example:
      I would want to update the JS prompt dialogue with an Angular modal box - possibly implementing a bespoke directive and the 'ui.bootstrap' component.
      The board's tiles should be moved to their own directive, in order to facilitate 'finding' the right ones to flip counters.


 -  There is a major bug on the board initilisation, for example if a player leaves/another joins or after a game ends.  This is as much to do with the way I have first initialised it, as I've not yet implemented that functionality.  

 -  Some error handling for dropped connections needs fully debugging, as sometimes connections are dropped but the member count (handled by onDisconnected override method in the Hub) does not decrement.

 -  I have struggled to get 'the right' interpretation of front end (AngularJS/jQuery) vs back end code.  My experience is separating concerns from HTML, but AngularJS introduces it back in to JavaScript, and the line appears fuzzy.  For example, MVC generally utilises Razor (my initial solution used the @model directive to access the BoardViewModel), thereby removing the need for AngularJS.

 -  It is not obvious how 'Model' (from MVC) backend code fits with dependencies, such as Interfaces and Abstract classes.

 - There is no IE8 compability (among others!) issues.  'data-' versions of Angular directives helped compatability and HTML validation, but version 1.3 does not support IE8 at all...  I have selected not to use 'data-<directive>'.


With this in mind, I have a working solution which demonstrates an understanding, and an ability to learn and implement, a number of web technologies without specific previous experience in any, and over just a few days.
