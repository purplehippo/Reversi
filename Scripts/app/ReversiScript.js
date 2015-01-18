/*
    @author:    Jo Faircloth
    @date:      January 2015
    @comment:   Handles client/server chatter between Play and BroadcastHub
*/
var validMoves;
var validMovesForMe;

var players = {};

$(function () {
    // connect to our broadcast hub
    var rev = $.connection.broadcastHub;

    // update scores / next turn message
    rev.client.updateToolbox = function (black, white, message) {
        $('#lblBlackScore').html(black);
        $('#lblWhiteScore').html(white);
        $('#lblNextPlayer').html(message);
    }

    // update the game grid to 'flip' counters in changedTileIds list
    // replaces classes according to state
    rev.client.updateGrid = function (changedTileIds) {
        // dictionary(tileid, state)
        for (var t in changedTileIds) {
            if (changedTileIds.hasOwnProperty(t)) {
                $('#tile' + t)
                    .removeClass("tileEMPTY titleWHITE tileBLACK", 10, "easeInQuad")
                    .addClass("tile" + changedTileIds[t], 1000, "easeInQuad");
            }
        }
    }

    // reset messages and re-add from list
    rev.client.updatePlayerMessages = function (messages, state) {
        // log, msg, state
        $('#gameMessages').html('');
        $('#playerMessages').html('');
        for (var m in messages) {
            if (messages.hasOwnProperty(m)) {
                if (messages[m].log == "GAME")
                    $('#gameMessages').append(messages[m].msg);
                else {
                    // if player message is intended for me (based on state)
                    if (messages[m].state == state)
                        $('#playerMessages').append(messages[m].msg);
                }
            }
        }
        // animate the scroll to bottom if overflow
        $('.gameMessages').animate({
            scrollTop: $('.gameMessages').get(0).scrollHeight
        }, 2000);
        $('.playerMessages').animate({
            scrollTop: $('.playerMessages').get(0).scrollHeight
        }, 2000);
    }

    rev.client.updatePlayers = function (data) {
        // noGamesPlaying, noPlayers
        $('#lblBoardPlayers').html('Number of boards: ' + data.noGamesPlaying + '<br />Number of players: ' + data.noPlayers);
    }

    rev.client.noOpponents = function () {
        $('#lblOpponentMessage').html('Sorry, there are currently no opponents available to play.');
    }

    rev.client.foundOpponent = function (data) {
        // name, state
        $('#lblOpponentMessage').html(data.name + ', you have connected to a game with ' + data.opponentName + '.  You are ' + data.state + '.');
        $('#btnFindOpponent').prop('disabled', true);
        $('#playerData').css('display', 'inline-block');
        $('#txtMessage').prop('placeholder', 'Send message to ' + data.opponentName);
    }

    rev.client.opponentDisconnected = function (data) {
        // name
        $('#lblOpponentMessage').html('Your opponent ' + data.name + ' disconnected.  YOU WIN!!');
        //RefreshGrid();
    }

    rev.client.showValidMoves = function (validTiles, forMe) {
        // save for when we switch on / off show
        if (validTiles.length > 0) {
            validMoves = validTiles;
            validMovesForMe = forMe;
        }
        // list of ids (validTiles), if next state is this player's state (forMe)
        ShowMoves(validMoves, validMovesForMe, true);
    }
    rev.client.receiveMessage = function (time, from, message) {
        message = $('<p>').text(message).html();
        $('#userMessages').append('<b>' + time + ' ' + from + '</b>: ' + message + '<br />');

        $('.chatMessages').animate({
            scrollTop: $('.chatMessages').get(0).scrollHeight
            }, 1000);
    }
    // ensure connection to hub; apply click events on tiles and find opponent;
    // get user and initialise data
    $.connection.hub.start()
        .done(function () {
            var usr = prompt('Please enter your player name');
            $('#lblWelcome').html('Welcome, ' + usr);
            rev.server.newPlayer(usr);
            rev.server.initialise();

            // click event for DIVs with ids starting 'tile'
            $("div").click(function () {
                if (!(this.id.lastIndexOf("tile", 0) === 0)) return;
                else {
                    ShowMoves(null, true, false);
                }
                // action the played tile, swap the colours via updateGrid(...)
                rev.server.play(this.id.replace("tile", ""));
            });
            $('#btnFindOpponent').click(function () {
                rev.server.findOpponent();
            });
            $('#chkValidMoves').click(function () {
                if (this.checked) ShowMoves(validMoves, validMovesForMe, true);
                else ShowMoves(null, validMovesForMe, false);
            });
            $('#btnSendMessage').click(function () {
                rev.server.sendChatMessage(usr, $('#txtMessage').val());
                $('#txtMessage').val('').focus();
            });
        }).fail(function () {
            $('#playerMessages').append('<b>ERROR: CONNECTION LOST</b>');
            $('.playerMessages').animate({
                scrollTop: $('.playerMessages').get(0).scrollHeight
            }, 2000);
        });
});

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
function RefreshGrid() {
    $('#board').find("div").each(function () {
        if (this.id.lastIndexOf("tile", 0) === 0) {
            $(this).removeClass("tileWHITE tileBLACK").addClass("tileEMPTY");
        }
    });
    $('#tile27').addClass("tileWHITE");
    $('#tile36').addClass("tileWHITE");
    $('#tile28').addClass("tileBLACK");
    $('#tile35').addClass("tileBLACK");
}


