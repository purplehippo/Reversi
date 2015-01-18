/*
    @author:    Jo Faircloth
    @date:      18 January 2015
    @comment:   Handles client/server chatter between Play and BroadcastHub, via AngularJS
                Game allows for a single game, with one opponent

Reference: http://nickberardi.com/signalr-and-angularjs/
*/

var width = height = 8;

//// add $scope etc to array, so if minified we do not lose dependency on $scope as parameter
var app = angular.module('reversiGame', ['ngSanitize']);
app.controller('gameController', function($scope) {
    $scope.tiles = initialiseBoard();
    $scope.boardWidth = width * 54;
    $scope.messages = {};
    $scope.panels = {};
    $scope.messages.chat = [];
    $scope.messages.game = [];
    $scope.messages.player = [];
    $scope.messages.opponent = "";
    $scope.messages.board = "";
    $scope.panels.playerData = false;
    $scope.validMoves = [];

    var _hub = $.connection.broadcastHub;

    // handle the click event from the ui tile
    // play will return with broadcast(s) to determine message or board update
    $scope.tileSelected = function (tile) {
        if (typeof tile === "undefined") { return; }
        var that = this;
        return _hub.server.play(tile.id).done(function (data) {
            console.log("play()");
            $scope.validMoves = "22";
            $scope.$apply();
        });
    };
    // handle the click event from the join game button
    // sets this user to wanting to play, and seeks another to play against
    $scope.joinGame = function () {
        return _hub.server.findOpponent().done(function () { });
    };
    // handle the click event from the chat
    // immediately returns the msg as a broadcast - could possibly be handled 
    //      here, rather than in separate listener
    $scope.sendChatMessage = function () {
        return _hub.server.sendChatMessage('xyz', $scope.chatMessage).done(function () {

        });
    };

    // update the game grid to 'flip' counters in changedTileIds list
    // replaces classes according to state
    _hub.client.UpdateGrid = function (changedTileIds) {
        // dictionary(tileid, state)
        for (var t in changedTileIds) {
            if (changedTileIds.hasOwnProperty(t)) {
                $element('#tile' + t)
                    .removeClass("tileEMPTY titleWHITE tileBLACK", 10, "easeInQuad")
                    .addClass("tile" + changedTileIds[t], 1000, "easeInQuad");
            }
        }
    }
    // update scores / next turn message
    _hub.client.UpdateToolbox = function (black, white, message) {
        $scope.blackScore = black;
        $scope.whiteScore = white;
        $scope.nextPlayer = message;
    }
    // update the 'Your Messages' messages (invalid move, etc)
    _hub.client.UpdatePlayerMessages = function (message, state) {
        console.log('updateGameMessages()');
        $scope.messages.player.push(message);
        $scope.$apply();
    };
    // notify of no available opponents
    _hub.client.NoOpponents = function () {
        $scope.messages.opponent = "Sorry, there are currently no opponents available to play.";
        $scope.$apply();
    }
    // notify of found opponent and game status
    _hub.client.FoundOpponent = function (data) {
        // name, state
        $scope.messages.opponent = data.name + ", you have connected to a game with " + data.opponentName + ".  You are " + data.state + ".";
        $scope.panels.playerData = true;
        $scope.chatTextPlaceholder = "Send message to " + data.opponentName;
        $scope.$apply();
    }
    // notify that opponent disconnected, and you win :)
    _hub.client.OpponentDisconnected = function (data) {
        // name
        $scope.messages.opponent = "Your opponent " + data.name + " disconnected.  YOU WIN!!";
        $scope.$apply();
    }
    // display information on number of boards in games, and players connected
    _hub.client.UpdatePlayers = function (data) {
        // noGamesPlaying, noPlayers
        $scope.messages.board = "Number of boards: " + data.noGamesPlaying + "<br />Number of players: " + data.noPlayers;
        $scope.$apply();
    }
    // update valid moves
    _hub.client.ShowValidMoves = function (validTiles, forMe) {
        // save for when we switch on / off show
        if (validTiles.length > 0) {
            validMoves = validTiles;
            validMovesForMe = forMe;
        }
        // list of ids (validTiles), if next state is this player's state (forMe)
        ShowMoves(validMoves, validMovesForMe, true);
    }
    // update chat messages
    _hub.client.ReceiveMessage = function (time, from, message) {
        $scope.messages.chat.push('<b>' + time + ' ' + from + '</b>: ' + message + '<br />');
        $scope.chatMessage = "";
        $scope.$apply();

        //$('.chatMessages').animate({
        //    scrollTop: $('.chatMessages').get(0).scrollHeight
        //}, 1000);

    }

    // connect to the broadcastHub, enabling interaction with SignalR interface
    $.connection.hub.start().done(function () {
        var usr = prompt('Please enter your player name');
        $scope.messages.welcome = "Welcome, " + usr;
        $scope.user = usr;
        _hub.server.newPlayer($scope.user);
        _hub.server.initialise();
        console.log("hub started");
    }).fail(function () {
        console.log("hub failed to start");
    });

});

/// TODO:  !!  to enable click receive, valid move display and tile turn
app.directive("tile", function () {

});
    
// daft little funciton to initialise board
function initialiseBoard() {
    var tiles = [], tile;
    var middle = width / 2;
    var initialWhite = (middle - 1).toString() + (middle).toString() +
        "," + (middle).toString() + (middle - 1).toString();
    var initialBlack = (middle - 1).toString() + (middle - 1).toString() +
        "," + (middle).toString() + (middle).toString();
    var k = 0;
    for (var i = 0; i < width; i++) {
        for (var j = 0; j < height; j++) {
            tile = { id: i.toString() + j.toString(), state: "EMPTY", ref: 'tile' + k };

            if (initialBlack.indexOf(tile.id) > -1)
                tile.state = "WHITE";
            if (initialWhite.indexOf(tile.id) > -1)
                tile.state = "BLACK";

            k++;
            tiles.push(tile);
        }
    }
    return tiles;
}

///
/// bit of a nasty function, incorporates jQuery in order to 
/// find and change the class for a valid move
function ShowMoves(tiles, forMe, display) {
    if (forMe) {
        if (display && $('#chkValidMoves')[0].checked) {
            for (var v in tiles) {
                if (tiles.hasOwnProperty(v)) {
                    $('#tile' + tiles[v]).find('span').addClass('valid');
                }
            }
        } else {
            $('#board').find("div").each(function () {
                $(this).find('span').removeClass('valid');
            });
        }
    }
}



//var validMoves;
//var validMovesForMe;

//var players = {};

//$(function () {
//    // reset messages and re-add from list
//    rev.client.updatePlayerMessages = function (messages, state) {
//        // log, msg, state
//        $('#gameMessages').html('');
//        $('#playerMessages').html('');
//        for (var m in messages) {
//            if (messages.hasOwnProperty(m)) {
//                if (messages[m].log == "GAME")
//                    $('#gameMessages').append(messages[m].msg);
//                else {
//                    // if player message is intended for me (based on state)
//                    if (messages[m].state == state)
//                        $('#playerMessages').append(messages[m].msg);
//                }
//            }
//        }
//        // animate the scroll to bottom if overflow
//        $('.gameMessages').animate({
//            scrollTop: $('.gameMessages').get(0).scrollHeight
//        }, 2000);
//        $('.playerMessages').animate({
//            scrollTop: $('.playerMessages').get(0).scrollHeight
//        }, 2000);
//    }


//    // ensure connection to hub; apply click events on tiles and find opponent;
//    // get user and initialise data
//    $.connection.hub.start()
//        .done(function () {

//            // click event for DIVs with ids starting 'tile'
//            $("div").click(function () {
//                if (!(this.id.lastIndexOf("tile", 0) === 0)) return;
//                else {
//                    ShowMoves(null, true, false);
//                }
//                // action the played tile, swap the colours via updateGrid(...)
//                rev.server.play(this.id.replace("tile", ""));
//            });
//            $('#chkValidMoves').click(function () {
//                if (this.checked) ShowMoves(validMoves, validMovesForMe, true);
//                else ShowMoves(null, validMovesForMe, false);
//            });
//        }).fail(function () {
//            $('#playerMessages').append('<b>ERROR: CONNECTION LOST</b>');
//            $('.playerMessages').animate({
//                scrollTop: $('.playerMessages').get(0).scrollHeight
//            }, 2000);
//        });
//});

//function ShowMoves(tiles, forMe, display) {
//    if (forMe) {
//        if (display && $('#chkValidMoves')[0].checked) {
//            for (var v in tiles) {
//                if (tiles.hasOwnProperty(v)) {
//                    $('#tile' + tiles[v]).find('span').addClass('valid');
//                }
//            }
//        } else {
//            $('#board').find("div").each(function () {
//                $(this).find('span').removeClass('valid');
//            });
//        }
//    }
//}
